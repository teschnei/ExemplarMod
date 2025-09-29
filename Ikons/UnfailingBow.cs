using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class UnfailingBow
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("UnfailingBow", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Unfailing Bow", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonRanged)
            .WithRuneProperties(new RuneProperties("ikon", IkonRuneKind.Ikon, "The shots fired by this weapon seem guided by divine accuracy, finding the swiftest targets.",
            "This item grants the {i}immanence{/i} and {i}transcendence{/i} abilities of the Unfailing Bow when empowered.", item =>
            {
                item.Traits.AddRange([ExemplarTraits.Ikon, Trait.Divine]);
            })
            .WithCanBeAppliedTo((Item rune, Item weapon) =>
            {
                if (weapon.WeaponProperties == null)
                {
                    return "Must be a weapon.";
                }
                if (!weapon.HasTrait(Trait.Ranged))
                {
                    return "Must be a ranged weapon.";
                }
                return null;
            }));
        });
        yield return new Ikon(new Feat(
            ExemplarFeats.UnfailingBow,
            "The shots fired by this weapon seem guided by divine accuracy, finding the swiftest targets.",
            "{b}Usage{/b} a ranged weapon\n\n" +
            "{b}Immanence{/b} The {i}unfailing bow{/i} deals an additional 1 force damage per weapon damage die to creatures it Strikes, or 1d4 additional force damage per weapon die on a critical hit.\n\n" +
            $"{{b}}Transcendence — Arrow Splits Arrow {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (transcendence)\n{{b}}Requirements{{/b}} Your previous action was to Strike with the {{i}}unfailing bow{{/i}}.\n" +
            "{b}Effect{/b} You repeat your motions exactly, your attack landing in the same location as your previous shot. You make a Strike against the same target. The result of your d20 roll is the same as the result of the required shot, " +
            "though any penalties (such as your multiple attack penalty) apply normally to this shot and you don't automatically adjust the degree of success if the initial roll was a natural 1 or 20.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.UnfailingBow), q =>
        {
            q.AddExtraKindedDamageOnStrike = (action, target) =>
            {
                if (action.Item?.Runes.Any(rune => rune.ItemName == ikonRune) ?? false)
                {
                    int dice = action.Item.WeaponProperties?.DamageDieCount ?? 0;
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
                            breakdownResult.CheckResult.WorsenByOneStep();
                        }
                    }
                }
                else
                {
                    q.Tag = breakdownResult.D20Roll;
                }
            };
        },
        q =>
        {
            var unfailing = Ikon.GetIkonItem(q.Owner, ikonRune);
            return new ActionPossibility(new CombatAction(
                q.Owner,
                ExemplarIllustrations.UnfailingBow,
                "Arrow Splits Arrow",
                [ExemplarTraits.Transcendence],
                "You repeat your motions exactly, your attack landing in the same location as your previous shot. You make a Strike against the same target. The result of your d20 roll is the same as the result of the required shot, " +
                "though any penalties (such as your multiple attack penalty) apply normally to this shot and you don't automatically adjust the degree of success if the initial roll was a natural 1 or 20.",
                Target.Ranged(unfailing?.WeaponProperties?.MaximumRange ?? 100).WithAdditionalConditionOnTargetCreature((self, target) =>
                {
                    var unfailing = Ikon.GetIkonItem(q.Owner, ikonRune);
                    if (unfailing == null)
                    {
                        return Usability.NotUsable("You must be wielding the {i}unfailing bow{/i}.");
                    }
                    if (((unfailing.HasTrait(Trait.Reload1) || unfailing.HasTrait(Trait.Reload2)) && unfailing.EphemeralItemProperties.NeedsReload) ||
                        (unfailing.HasTrait(Trait.Repeating) && unfailing.EphemeralItemProperties.AmmunitionLeftInMagazine <= 0))
                    {
                        return Usability.NotUsable("Your {i}unfailing bow{/i} must be loaded.");
                    }
                    var lastAction = self.Actions.ActionHistoryThisTurn.LastOrDefault();
                    if (lastAction == null || !lastAction.HasTrait(Trait.Strike) ||
                        lastAction.CheckResult < CheckResult.Success ||
                        (lastAction.Item != unfailing))
                    {
                        return Usability.NotUsable("Your last action must be a successful Strike with the {i}unfailing bow{/i}.");
                    }
                    if (lastAction.ChosenTargets.ChosenCreature != target)
                    {
                        return Usability.NotUsableOnThisCreature("You must target the same creature as your previous Strike.");
                    }
                    return Usability.Usable;
                })
            )
            .WithActionCost(2)
            .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(unfailing!, -1), TaggedChecks.DefenseDC(Defense.AC)))
            .WithNoSaveFor((action, cr) => true)
            .WithEffectOnChosenTargets(async (action, self, targets) =>
            {
                var unfailing = Ikon.GetIkonItem(self, ikonRune);
                var lastAction = self.Actions.ActionHistoryThisTurn.LastOrDefault();
                //See Patches.cs for the guaranteed roll number
                self.AddQEffect(new QEffect()
                {
                    Id = ExemplarQEffects.ArrowSplitsArrow
                }.WithExpirationEphemeral());
                await self.MakeStrike(lastAction!.ChosenTargets.ChosenCreature!, unfailing!);
            }));
        })
        .WithRune(ikonRune)
        .IkonFeat;
    }
}
