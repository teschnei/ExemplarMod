using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class Starshot
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("Starshot", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Starshot", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonRanged)
            .WithRuneProperties(new RuneProperties("Ikon", IkonRuneKind.Starshot, "You might be the only one capable of stringing this bow or pulling this trigger; either way, the ikon's shots are packed with explosive power, striking like falling stars.",
            "", item =>
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
            ExemplarFeats.Starshot,
            "You might be the only one capable of stringing this bow or pulling this trigger; either way, the ikon's shots are packed with explosive power, striking like falling stars.",
            "{b}Usage{/b} a ranged weapon\n\n" +
            "{b}Immanence{/b} Strikes with the {i}starshot{/i} deal an additional 1 spirit splash damage per weapon damage die.\n\n" +
            $"{{b}}Transcendence — Giant-Felling Comet {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (spirit, transcendence)\nYou shoot the {{i}}starshot{{/i}}, causing a detonation in a 5-foot burst within 60 feet. " +
            "Each creature in the area must succeed at a basic Reflex save against your class DC or take spirit damage equal to your normal Strike damage with the {i}starshot{/i}. " +
            "Creatures larger than you take a -2 circumstance penalty to their saving throws. This shot requires any ammunition that would normally be required.",
            [ExemplarTraits.Ikon],
            null
        ).WithIllustration(IllustrationName.MagicFang), q =>
        {
            q.AfterYouTakeAction = async (q, action) =>
            {
                if (action.Item?.Runes.Any(rune => rune.ItemName == ikonRune) ?? false && action.HasTrait(Trait.Strike))
                {
                    int dice = action.Item.WeaponProperties?.DamageDieCount ?? 0;

                    IEnumerable<Creature> creatures = action.ChosenTargets.ChosenCreature?.Occupies.Neighbours.Creatures ?? [];
                    foreach (var creature in creatures)
                    {
                        await CommonSpellEffects.DealDirectSplashDamage(action, DiceFormula.FromText(dice.ToString(), "Starshot"), creature, Ikon.GetBestDamageKindForSpark(action.Owner, creature));
                    }
                }
            };
        },
        q =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                IllustrationName.Quarterstaff,
                "Giant-Felling Comet",
                [ExemplarTraits.Transcendence, ExemplarTraits.Ikon],
                "You shoot the {i}starshot{/i}, causing a detonation in a 5-foot burst within 60 feet. Each creature in the area must succeed at a basic Reflex save against your class DC or take spirit damage equal to your normal Strike damage with the {i}starshot{/i}. Creatures larger than you take a -2 circumstance penalty to their saving throws. This shot requires any ammunition that would normally be required.",
                Target.Burst(12, 1).WithAdditionalRequirementOnCaster(self =>
                {
                    var starshot = Ikon.GetIkonItem(q.Owner, ikonRune);
                    if (starshot == null)
                    {
                        return Usability.NotUsable("You must be wielding the {i}starshot{/i}.");
                    }
                    if (((starshot.HasTrait(Trait.Reload1) || starshot.HasTrait(Trait.Reload2)) && starshot.EphemeralItemProperties.NeedsReload) ||
                        (starshot.HasTrait(Trait.Repeating) && starshot.EphemeralItemProperties.AmmunitionLeftInMagazine <= 0))
                    {
                        return Usability.NotUsable("Your {i}starshot{/i} must be loaded.");
                    }
                    return Usability.Usable;
                })
            )
            .WithActionCost(2)
            .WithSavingThrow(new SavingThrow(Defense.Reflex, q.Owner.ClassDC()))
            .WithEffectOnChosenTargets(async (action, self, targets) =>
            {
                var starshot = Ikon.GetIkonItem(self, ikonRune);
                var strike = StrikeRules.CreateStrike(self, starshot!, RangeKind.Ranged, 0);
                foreach (var target in targets.AllCreaturesInArea)
                {
                    var checkResult = targets.CheckResults[target];
                    await CommonSpellEffects.DealDirectDamage(action, strike.TrueDamageFormula!, target, checkResult, Ikon.GetBestDamageKindForSpark(self, target));
                }
                if (starshot!.HasTrait(Trait.Reload1) || starshot.HasTrait(Trait.Reload2))
                {
                    starshot.EphemeralItemProperties.NeedsReload = true;
                }
                if (starshot.HasTrait(Trait.Repeating))
                {
                    starshot.EphemeralItemProperties.AmmunitionLeftInMagazine--;
                }
            }));
        })
        .WithRune(ikonRune)
        .IkonFeat;
    }
}
