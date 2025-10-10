using System.Collections.Generic;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class Starshot
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Ikon(new Feat(
            ExemplarFeats.Starshot,
            "You might be the only one capable of stringing this bow or pulling this trigger; either way, the ikon's shots are packed with explosive power, striking like falling stars.",
            "{b}Usage{/b} a ranged weapon\n\n" +
            "{b}Immanence{/b} Strikes with the {i}starshot{/i} deal an additional 1 spirit splash damage per weapon damage die.\n\n" +
            $"{{b}}Transcendence — Giant-Felling Comet {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (spirit, transcendence)\nYou shoot the {{i}}starshot{{/i}}, causing a detonation in a 5-foot burst within 60 feet. " +
            "Each creature in the area must succeed at a basic Reflex save against your class DC or take spirit damage equal to your normal Strike damage with the {i}starshot{/i}. " +
            "Creatures larger than you take a -2 circumstance penalty to their saving throws. This shot requires any ammunition that would normally be required.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.Starshot), (ikon, q) =>
        {
            q.AfterYouTakeAction = async (q, action) =>
            {
                if (ikon.IsIkonItem(action.Item) && action.HasTrait(Trait.Strike))
                {
                    int dice = action.Item?.WeaponProperties?.DamageDieCount ?? 0;

                    IEnumerable<Creature> creatures = action.ChosenTargets.ChosenCreature?.Occupies.Neighbours.Creatures ?? [];
                    foreach (var creature in creatures)
                    {
                        await CommonSpellEffects.DealDirectSplashDamage(action, DiceFormula.FromText(dice.ToString(), "Starshot"), creature, Ikon.GetBestDamageKindForSpark(action.Owner, creature));
                    }
                }
            };
        },
        (ikon, q) =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                ExemplarIllustrations.Starshot,
                "Giant-Felling Comet",
                [ExemplarTraits.Spirit, ExemplarTraits.Transcendence],
                "You shoot the {i}starshot{/i}, causing a detonation in a 5-foot burst within 60 feet. Each creature in the area must succeed at a basic Reflex save against your class DC or take spirit damage equal to your normal Strike damage with the {i}starshot{/i}. Creatures larger than you take a -2 circumstance penalty to their saving throws. This shot requires any ammunition that would normally be required.",
                Target.Burst(12, 1).WithAdditionalRequirementOnCaster(self =>
                {
                    var starshot = Ikon.GetHeldIkon(q.Owner, ikon);
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
                var starshot = Ikon.GetHeldIkon(self, ikon);
                var strike = StrikeRules.CreateStrike(self, starshot!, RangeKind.Ranged, 0);
                foreach (var target in targets.GetAllTargetCreatures())
                {
                    if (targets.CheckResults.TryGetValue(target, out var checkResult))
                    {
                        await CommonSpellEffects.DealDirectDamage(action, strike.TrueDamageFormula!, target, checkResult, Ikon.GetBestDamageKindForSpark(self, target));
                    }
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
