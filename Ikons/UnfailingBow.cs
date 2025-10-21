using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class UnfailingBow
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Ikon(new Feat(
            ExemplarFeats.UnfailingBow,
            "The shots fired by this weapon seem guided by divine accuracy, finding the swiftest targets.",
            "{b}Usage{/b} a ranged weapon\n\n" +
            "{b}Immanence{/b} The {i}unfailing bow{/i} deals an additional 1 spirit damage per weapon damage die to creatures it Strikes, or 1d4 additional spirit damage per weapon die on a critical hit.\n\n" +
            $"{{b}}Transcendence — Arrow Splits Arrow {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (transcendence)\n{{b}}Requirements{{/b}} Your previous action was to Strike with the {{i}}unfailing bow{{/i}}.\n" +
            "{b}Effect{/b} You repeat your motions exactly, your attack landing in the same location as your previous shot. You make a Strike against the same target. The result of your d20 roll is the same as the result of the required shot, " +
            "though any penalties (such as your multiple attack penalty) apply normally to this shot and you don't automatically adjust the degree of success if the initial roll was a natural 1 or 20.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.UnfailingBow), (ikon, q) =>
        {
            q.AddExtraKindedDamageOnStrike = (action, target) =>
            {
                if (ikon.IsIkonItem(action.Item))
                {
                    int dice = action.Item?.WeaponProperties?.DamageDieCount ?? 0;
                    if (action.ChosenTargets.CheckResults[target] == CheckResult.Success)
                    {
                        return new KindedDamage(DiceFormula.FromText($"{1 * dice}", "Unfailing Bow"), Ikon.GetBestDamageKindForSpark(action.Owner, target));
                    }
                    return new KindedDamage(DiceFormula.FromText($"{1 * dice}d4", "Unfailing Bow"), Ikon.GetBestDamageKindForSpark(action.Owner, target));
                }
                return null;
            };
            q.AfterYouMakeAttackRoll = (q, breakdownResult) =>
            {
                if (q.Owner.HasEffect(ExemplarQEffects.ArrowSplitsArrow))
                {
                    if (breakdownResult.D20Roll == 20)
                    {
                        if (breakdownResult.ThresholdToDowngrade <= 10)
                        {
                            breakdownResult.CheckResult = breakdownResult.CheckResult.WorsenByOneStep();
                        }
                    }
                }
                else
                {
                    q.Tag = breakdownResult.D20Roll;
                }
            };
        },
        (ikon, q) =>
        {
            var heldIkon = Ikon.GetHeldIkon(q.Owner, ikon);
            var lastAction = q.Owner.Actions.ActionHistoryThisTurn.LastOrDefault();
            var lastIkon = ikon.IsIkonItem(lastAction?.Item) ? lastAction?.Item : null;
            var action = new CombatAction(
                q.Owner,
                ExemplarIllustrations.UnfailingBow,
                "Arrow Splits Arrow",
                [ExemplarTraits.Transcendence, Trait.AlwaysHits, Trait.IsHostile],
                "You repeat your motions exactly, your attack landing in the same location as your previous shot. You make a Strike against the same target. The result of your d20 roll is the same as the result of the required shot, " +
                "though any penalties (such as your multiple attack penalty) apply normally to this shot and you don't automatically adjust the degree of success if the initial roll was a natural 1 or 20.",
                heldIkon == null ? Target.Uncastable("You must be wielding the {i}unfailing bow{/i}.") : heldIkon.DetermineStrikeTarget(RangeKind.Ranged).WithAdditionalConditionOnTargetCreature((self, target) =>
                {
                    var lastAction = self.Actions.ActionHistoryThisTurn.LastOrDefault();
                    if (lastAction == null || !lastAction.HasTrait(Trait.Strike) ||
                        lastAction.CheckResult < CheckResult.Success ||
                        (lastIkon == null))
                    {
                        return Usability.NotUsable("Your last action must be a successful Strike with the {i}unfailing bow{/i}.");
                    }
                    if (((lastIkon.HasTrait(Trait.Reload1) || lastIkon.HasTrait(Trait.Reload2)) && lastIkon.EphemeralItemProperties.NeedsReload) ||
                        (lastIkon.HasTrait(Trait.Repeating) && lastIkon.EphemeralItemProperties.AmmunitionLeftInMagazine <= 0))
                    {
                        return Usability.NotUsable("Your {i}unfailing bow{/i} must be loaded.");
                    }
                    if (lastAction.ChosenTargets.ChosenCreature != target)
                    {
                        return Usability.NotUsableOnThisCreature("You must target the same creature as your previous Strike.");
                    }
                    return Usability.Usable;
                })
            )
            .WithActionCost(1)
            .WithEffectOnChosenTargets(async (action, self, targets) =>
            {
                //See Patches.cs for the guaranteed roll number
                var arrowQ = new QEffect()
                {
                    Id = ExemplarQEffects.ArrowSplitsArrow
                };
                self.AddQEffect(arrowQ);
                var strike = StrikeRules.CreateStrike(self, lastIkon!, lastIkon!.HasTrait(Trait.Ranged) ? RangeKind.Ranged : RangeKind.Melee, self.Actions.AttackedThisManyTimesThisTurn).WithActionCost(0);
                strike.Traits.Add(ExemplarTraits.ArrowGuaranteed);
                strike.ChosenTargets = ChosenTargets.CreateSingleTarget(targets.ChosenCreature!);
                await strike.AllExecute();
                self.RemoveAllQEffects(q => q == arrowQ);
            });
            if (lastIkon != null)
            {
                var tooltipStrike = q.Owner.CreateStrike(lastIkon, q.Owner.Actions.AttackedThisManyTimesThisTurn).WithActionCost(0);
                tooltipStrike.Traits.Add(ExemplarTraits.ArrowGuaranteed);
                action.WithTargetingTooltip((action, target, _) => CombatActionExecution.BreakdownAttackForTooltip(tooltipStrike, target).TooltipDescription);
            }
            return new ActionPossibility(action);
        })
        .WithValidItem(item =>
        {
            if (item.WeaponProperties == null)
            {
                return "Must be a weapon.";
            }
            if (!item.HasTrait(Trait.Ranged))
            {
                return "Must be a ranged weapon.";
            }
            return null;
        })
        .IkonFeat;
    }
}
