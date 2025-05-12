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
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Exemplar.Utilities;
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
            ).WithMultipleSelection()
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
                        $"{diceCount * 1}",
                        "Starshot splash"
                    );

                    DamageKind damageKind = DamageKindHelper.GetDamageKindFromEffect(qf.Owner, ExemplarIkonQEffectIds.QEnergizedSpark);   
                    
                    // now call the 4-arg overload:
                    await CommonSpellEffects.DealDirectSplashDamage(
                        action,
                        splashFormula,
                        target,
                        damageKind
                    );

                    if (action.CheckResult > CheckResult.Failure) // If the strike also at least succeeded,
                    {
                        foreach (Creature temp in selfQf.Owner.Battle.AllCreatures.Where(cr =>
                            action.ChosenTargets.ChosenCreature.IsAdjacentTo(cr))) // Loop through all adjacent creatures,
                        {
                            await CommonSpellEffects.DealDirectSplashDamage(action, splashFormula, temp, DamageKind.Bludgeoning);
                        }
                    }
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
                        Target.Burst(12, 1)
                    ).WithActionCost(2);

                    // RulesBlock.GetIconTextFromNumberOfActions(2);
                    
                    // Basic Reflex save against your class DC
                    action.WithSavingThrow(new SavingThrow(
                        Defense.Reflex, qf.Owner.ClassDC()
                    ));

                    action.WithEffectOnEachTarget(async (act, caster, target, result) =>
                    {
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

                        DamageKind damageKind = DamageKindHelper.GetDamageKindFromEffect(qf.Owner, ExemplarIkonQEffectIds.QEnergizedSpark);   

                        //  ▸ 5-arg overload: (CombatAction? power, DiceFormula damage, Creature target, CheckResult checkResult, DamageKind kind)
                        await CommonSpellEffects.DealBasicDamage(
                            act,        // the CombatAction
                            caster,     // who is casting
                            target,     // who to damage
                            result,     // your save result
                            df,         // dice formula
                            damageKind
                        );

                        // 2) Cleanup & exhaustion
                        action.WithEffectOnSelf(async (act, self) =>
                        {
                            IkonEffectHelper.CleanupEmpoweredEffects(self, ExemplarIkonQEffectIds.QEmpoweredStarshot);
                        });

                    }); // (pattern from Mortal Harvest's Transcendence)

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(starshot);
        }
    }
}
