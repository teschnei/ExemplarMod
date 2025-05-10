using System;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_Starshot
    {
        // 1) Register a new QEffectId so we can check “empowered” state
        public static readonly QEffectId QEmpoweredStarshot =
            ModManager.RegisterEnumMember<QEffectId>("Starshot");

        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            // 2) The Starshot Ikon itself
            var starshot = new TrueFeat(
                ExemplarFeatNames.IkonStarshot,  // make sure this exists in ExemplarFeatNames.cs
                1,
                "Starshot",
                "{b}Immanence{/b} Strikes with the starshot deal an additional 1 spirit splash damage per weapon damage die.\n\n" +
                "{b}Transcendence — Giant-Felling Comet (two-actions){/b} You shoot the starshot, causing a detonation in a 5-foot burst within 60 feet. " +
                "Each creature in the area must succeed at a basic Reflex save against your class DC or take spirit damage equal to your normal Strike damage with the starshot. " +
                "[Not implemented] Creatures larger than you take a –2 circumstance penalty to their saving throws. This shot requires any ammunition that would normally be required.",
                new[] { ModTraits.Ikon },
                null
            )
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: extra spirit splash damage per weapon die
                qf.AfterYouTakeAction = async (selfQf, action) =>
                {
                    if (!selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredStarshot))
                        return;
                    if (!action.HasTrait(Trait.Strike)
                    || action.Item?.WeaponProperties == null
                    || action.ChosenTargets?.ChosenCreature == null)
                        return;

                    var target = action.ChosenTargets.ChosenCreature;
                    int diceCount = action.Item.WeaponProperties.DamageDieCount;
                    int dieSize = action.Item.WeaponProperties.DamageDieSize;
                    // build a formula like “2d8” for two d8s, etc.
                    var splashFormula = DiceFormula.FromText(
                        $"1*{diceCount}",
                        "Starshot splash"
                    );
                    
                    DamageKind damageKind = DamageKind.Untyped;
                    var fx = qf.Owner.QEffects
                        .FirstOrDefault(e => e.Id == ExemplarIkonQEffectIds.QEnergizedSpark);
                    if (fx != null &&
                        Enum.TryParse<DamageKind>(fx.Key, out var kind))
                    {
                        damageKind = kind;
                    }
                    // now call the 4-arg overload:
                    await CommonSpellEffects.DealDirectSplashDamage(
                        action,
                        splashFormula,
                        target,
                        damageKind
                    );
                };  // (pattern from Gleaming Blade's AfterYouTakeAction)

                // Transcendence: Giant-Felling Comet
                qf.ProvideMainAction = qf =>
                {
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredStarshot))
                        return null;

                    var action = new CombatAction(
                        qf.Owner,
                        IllustrationName.MagicFang,
                        "Giant-Felling Comet",
                        new[] { ModTraits.Ikon, ModTraits.Transcendence },
                        "You shoot the starshot, causing a detonation in a 5-foot burst within 60 feet. Each creature must make a Reflex save or take spirit damage equal to your Strike damage.",
                        Target.Ranged(60)
                    ).WithActionCost(2);

                    // Basic Reflex save against your class DC
                    action.WithSavingThrow(new SavingThrow(
                        Defense.Reflex, qf.Owner.ClassOrSpellDC()
                    ));

                    action.WithEffectOnEachTarget(async (act, caster, target, result) =>
                    {
                        if (result < CheckResult.Success)
                            return;

                        // Find your starshot weapon's dice
                        var weapon = caster.HeldItems
                            .FirstOrDefault(i => i.WeaponProperties?.Melee == false && i.HasTrait(Trait.Ranged));
                        if (weapon == null)
                        {
                            caster.Occupies.Overhead("No starshot equipped.", Color.Orange);
                            return;
                        }

                        int diceCount = weapon.WeaponProperties.DamageDieCount;
                        int dieSize = weapon.WeaponProperties.DamageDieSize;
                        var df = DiceFormula.FromText($"{diceCount}d{dieSize}", "Giant-Felling Comet");

                        DamageKind damageKind = DamageKind.Untyped;
                        var fx = caster.QEffects
                            .FirstOrDefault(e => e.Id == ExemplarIkonQEffectIds.QEnergizedSpark);
                        if (fx != null &&
                            Enum.TryParse<DamageKind>(fx.Key, out var kind))
                        {
                            damageKind = kind;
                        }

                        //  ▸ 5-arg overload: (CombatAction? power, DiceFormula damage, Creature target, CheckResult checkResult, DamageKind kind)
                        await CommonSpellEffects.DealDirectDamage(
                            act,        // the CombatAction
                            df,         // dice formula
                            target,     // who to damage
                            result,     // your save result
                            damageKind
                        );

                        // 2) Cleanup & exhaustion
                        action.WithEffectOnSelf(async (act, self) =>
                        {
                            // remove all the “empowered ikon” markers
                            self.RemoveAllQEffects(q => ExemplarIkonQEffectIds.EmpoweredIkonIds.Contains(q.Id));

                            // free next Immanence
                            self.AddQEffect(new QEffect("First Shift Free", "Your next Immanence is free.")
                            {
                                Id = ExemplarIkonQEffectIds.FirstShiftFree
                            });

                            // no further Transcendence this turn
                            self.AddQEffect(new QEffect(
                                    "Spark Exhaustion",
                                    "You cannot use another Transcendence this turn.",
                                    ExpirationCondition.ExpiresAtStartOfYourTurn,
                                    self, IllustrationName.Chaos
                                )
                            {
                                Id = ExemplarIkonQEffectIds.TranscendenceTracker
                            });
                        });

                    }); // (pattern from Mortal Harvest's Transcendence)

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(starshot);
        }
    }
}
