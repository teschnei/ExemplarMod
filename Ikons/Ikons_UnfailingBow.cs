using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Mechanics.Targeting;
using System;
/*
    Skipped, transcendence is hard.
*/
namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_UnfailingBow
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var unfailingBow = new TrueFeat(
                ExemplarFeatNames.IkonUnfailingBow,
                1,
                "Unfailing Bow",
                "The shots fired by this weapon seem guided by divine accuracy, finding the swiftest targets.\n\n" +
                "{b}Immanence{/b} The unfailing bow deals an additional 1 spirit damage per weapon damage die to creatures it Strikes, or 1d4 additional spirit damage per weapon die on a critical hit.\n\n" +
                "{b}Transcendence — Arrow Splits Arrow (one-action){/b} Requirements: Your previous action was to Strike with the unfailing bow. " +
                "Effect: You repeat your motions exactly, your attack landing in the same location as your previous shot. You make a Strike against the same target using the same d20 result.",
                new[] { ModTraits.Ikon },
                null
            )
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: +1 spirit per die; on crit, splash 1d4 per die
                qf.BonusToDamage = (eff, action, defender) =>
                {
                    if (!eff.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredUnfailingBow))
                        return null;
                    if (!action.HasTrait(Trait.Strike) || action.Item == null || defender == null)
                        return null;

                    int dice = action.Item.WeaponProperties?.DamageDieCount ?? 1;
                    return new Bonus(dice, BonusType.Circumstance, "Unfailing Bow Immanence");
                };

                qf.AfterYouDealDamage = async (owner, action, target) =>
                {
                    if (!owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredUnfailingBow))
                        return;
                    if (!action.HasTrait(Trait.Strike) || target == null)
                        return;
                    if (action.CheckResult == CheckResult.CriticalSuccess)
                    {
                        int dice = action.Item?.WeaponProperties?.DamageDieCount ?? 1;
                        var formula = DiceFormula.FromText($"{dice}d4", "Unfailing Bow Crit Splash");

                        DamageKind damageKind = DamageKind.Untyped;
                        var fx = qf.Owner.QEffects
                            .FirstOrDefault(e => e.Id == ExemplarIkonQEffectIds.QEnergizedSpark);
                        if (fx != null &&
                            Enum.TryParse<DamageKind>(fx.Key, out var kind))
                        {
                            damageKind = kind;
                        }

                        await CommonSpellEffects.DealDirectDamage(
                            action, formula, target, action.CheckResult, damageKind
                        );
                    }
                };

                // Transcendence: repeat last shot with same d20 result
                qf.ProvideMainAction = qf =>
                {
                    var owner = qf.Owner;
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredUnfailingBow))
                        return null;

                    var action = new CombatAction(
                        owner,
                        IllustrationName.ArrowProjectile,
                        "Arrow Splits Arrow",
                        new[] { ModTraits.Transcendence, ModTraits.Ikon },
                        "Repeat your last unfailing bow Strike against the same target using the same d20 result.",
                        Target.Self()
                    ).WithActionCost(1);

                    action.WithEffectOnSelf(async (act, self) =>
                    {
                        // Validate previous Strike
                        var prev = self.Actions.ActionHistoryThisEncounter.LastOrDefault();
                        if (prev == null || !prev.HasTrait(Trait.Strike) || prev.Item != self.PrimaryWeaponIncludingRanged)
                        {
                            self.Occupies.Overhead(
                                "Your last action must be a Strike with the unfailing bow.",
                                Microsoft.Xna.Framework.Color.Orange
                            );
                            self.Actions.RevertExpendingOfResources(1, act);
                            return;
                        }

                        var target = prev.ChosenTargets?.ChosenCreature;
                        if (target == null)
                        {
                            self.Occupies.Overhead(
                                "No valid target for repeat shot.",
                                Microsoft.Xna.Framework.Color.Orange
                            );
                            self.Actions.RevertExpendingOfResources(1, act);
                            return;
                        }
                        /*
                            Is there a way to force the the previous hit die to be pushed in? 
                        */

                        // Repeat the strike with forced result
                        var repeat = self.CreateStrike(prev.Item);
                        repeat.ChosenTargets = prev.ChosenTargets;
                        await repeat.AllExecute();

                        // Cleanup: remove empowerment, grant free Shift, exhaustion
                        self.RemoveAllQEffects(q => ExemplarIkonQEffectIds.EmpoweredIkonIds.Contains(q.Id));
                        self.AddQEffect(new QEffect(
                            "First Shift Free",
                            "Your next Shift Immanence is free."
                        )
                        { Id = ExemplarIkonQEffectIds.FirstShiftFree });

                        self.AddQEffect(new QEffect(
                            "Spark Exhaustion",
                            "You cannot use another Transcendence this turn.",
                            ExpirationCondition.ExpiresAtStartOfYourTurn,
                            self
                        )
                        { Id = ExemplarIkonQEffectIds.TranscendenceTracker });
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(unfailingBow);
        }
    }
}
