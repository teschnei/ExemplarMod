using System;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Mods.Exemplar.Utilities;
/*
    TODO:
    Currently this completely rewrites the damage type to be spirit / untyped, however
    'If this Strike hits, your additional spirit damage from the ikon's immanence increases to 4 plus an extra die of weapon damage'
    my reading of this means that only the 4 is spirit damage, the rest is weapon damage.
*/
namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_TitansBreaker
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            // Register the Titan's Breaker ikon feat (add FeatName in ExemplarFeatNames.cs)
            var titanBreaker = new TrueFeat(
                ExemplarFeatNames.IkonTitansBreaker,
                1,
                "Titan's Breaker",
                "{b}Immanence{/b} The titan's breaker deals 2 additional spirit damage per weapon damage die to creatures it Strikes. " +
                "Constructs and objects are not immune to this spirit damage, and this spirit damage bypasses hardness equal to your level." +
                "\n\n" +
                "{b}Transcendence — Fracture Mountains (two-actions){/b} Spirit, Transcendence\n" +
                "Make a melee Strike with the titan's breaker. This counts as two attacks for your multiple attack penalty. " +
                "If it hits, you deal increased spirit damage as described.",
                new[] { ModTraits.Ikon },
                null
            ).WithMultipleSelection()
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: extra spirit damage per die
                qf.BonusToDamage = (eff, action, defender) =>
                {
                    // check explicit QEffectId for Titan's Breaker empowerment
                    if (!eff.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredTitansBreaker))
                        return null;
                    if (!action.HasTrait(Trait.Strike) || action.Item?.WeaponProperties == null || defender == null)
                        return null;
                    int dice = action.Item.WeaponProperties.DamageDieCount;
                    return new Bonus(dice * 2, BonusType.Circumstance, "Titan's Breaker Immanence");
                };

                // Transcendence: Fracture Mountains
                qf.ProvideMainAction = qf =>
                {
                    var owner = qf.Owner;
                    // Prevent repeat usage in same turn
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredTitansBreaker))
                        return null;

                    var action = new CombatAction(
                        owner,
                        IllustrationName.Greatclub,
                        "Fracture Mountains",
                        new[] { Trait.Attack, ModTraits.Transcendence, ModTraits.Ikon },
                        "Make a melee Strike with the titan's breaker. Counts as two attacks for your multiple attack penalty. " +
                        "On a hit, you deal increased spirit damage as described.",
                        Target.AdjacentCreature()
                    ).WithActionCost(2);

                    action.WithEffectOnEachTarget(async (act, self, target, result) =>
                    {
                        if (result < CheckResult.Success)
                        {
                            return;
                        }

                        // Determine weapon dice
                        if (act.Item?.WeaponProperties == null)
                            return;
                        int baseDice = act.Item.WeaponProperties.DamageDieCount;
                        int dieSize = (int)act.Item.WeaponProperties.DamageDieSize;

                        // Scale damage by level thresholds
                        int level = self.Level;
                        int flatBonus;
                        int extraDiceCount;
                        if (level >= 18)
                        {
                            flatBonus = 8;
                            extraDiceCount = 3;
                        }
                        else if (level >= 10)
                        {
                            flatBonus = 6;
                            extraDiceCount = 2;
                        }
                        else
                        {
                            flatBonus = 4;
                            extraDiceCount = 1;
                        }

                        // Build formula: (baseDice + extraDiceCount)d(dieSize) + flatBonus
                        int totalDice = baseDice + extraDiceCount;
                        var formula = DiceFormula.FromText($"{totalDice}d{dieSize}+{flatBonus}", "Fracture Mountains");
                        DamageKind damageKind = DamageKindHelper.GetDamageKindFromEffect(qf.Owner, ExemplarIkonQEffectIds.QEnergizedSpark);   
                        
                        await CommonSpellEffects.DealDirectDamage(
                            action,
                            formula,
                            target,
                            result,
                            damageKind
                        );

                        // Cleanup: remove empowerment, grant free Shift, apply exhaustion
                        self.RemoveAllQEffects(q => ExemplarIkonQEffectIds.EmpoweredIkonIds.Contains(q.Id));
                        self.AddQEffect(new QEffect("First Shift Free", "Your next Shift Immanence is free.")
                        { Id = ExemplarIkonQEffectIds.FirstShiftFree });
                        self.AddQEffect(new QEffect(
                                "Spark Exhaustion",
                                "You cannot use another Transcendence this turn.",
                                ExpirationCondition.ExpiresAtStartOfYourTurn,
                                self,
                                IllustrationName.Chaos
                            )
                        { Id = ExemplarIkonQEffectIds.TranscendenceTracker });
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(titanBreaker);
        }
    }
}
