using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
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

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class HandsOfTheWildling
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("HandsOfTheWildling", itemName =>
        {
            //TODO: figure out a way to apply this to an unarmed strike of choice
            //TODO: okay there are no free-hand weapons in DD either
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Hands of the Wildling", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonFreeHand)
            .WithRuneProperties(new RuneProperties("ikon", IkonRuneKind.Ikon, "Tattooed fists, savage claws, or even powerful gauntlets - you swing each with the fury of an animal from the woods.",
            "This item grants the {i}immanence{/i} and {i}transcendence{/i} abilities of the Hands of the Wildling when empowered.", item =>
            {
                item.Traits.AddRange([ExemplarTraits.Ikon, Trait.Divine]);
            })
            .WithCanBeAppliedTo((Item rune, Item weapon) =>
            {
                if (weapon.WeaponProperties == null)
                {
                    return "Must be a weapon.";
                }
                return null;
            }));
        });

        yield return new Ikon(new Feat(
            ExemplarFeats.HandsOfTheWildling,
            "Tattooed fists, savage claws, or even powerful gauntlets—you swing each with the fury of an animal from the woods.",
            "{b}Usage{/b} a melee free-hand weapon or a melee unarmed Strike\n\n" +
            "{b}Immanence{/b} Strikes with your {i}hands of the wildling{/i} deal an additional 1 spirit splash damage per weapon damage die. You are immune to this splash damage.\n\n" +
            $"{{b}}Transcendence — Feral Swing {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (spirit, transcendence)\n" +
            "You lash out with both arms, rending all before you. Each creature in a 15-foot cone must succeed at a basic Reflex save against your class DC or take spirit damage equal to your normal Strike damage with your {i}hands of the wildling{/i}. " +
            "You can choose to swing with abandon, which imposes a -2 circumstance penalty to enemies' saving throws, but causes you to become off-guard until the start of your next turn.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.HandsOfTheWildling), q =>
        {
            q.AfterYouTakeAction = async (q, action) =>
            {
                if (action.Item?.Runes.Any(rune => rune.ItemName == ikonRune) ?? false && action.HasTrait(Trait.Strike))
                {
                    int dice = action.Item.WeaponProperties?.DamageDieCount ?? 0;

                    IEnumerable<Creature> creatures = action.ChosenTargets.ChosenCreature?.Occupies.Neighbours.Creatures.Where(cr => cr != q.Owner) ?? [];
                    foreach (var creature in creatures)
                    {
                        await CommonSpellEffects.DealDirectSplashDamage(action, DiceFormula.FromText(dice.ToString(), "Hands of the Wildling"), creature, Ikon.GetBestDamageKindForSpark(action.Owner, creature));
                    }
                }
            };
        }, q =>
        {
            return new SubmenuPossibility(ExemplarIllustrations.HandsOfTheWildling, "Feral Swing")
            {
                Subsections = [
                    new PossibilitySection("Feral Swing")
                    {
                        Possibilities = [
                            CreateFeralSwing(false),
                            CreateFeralSwing(true)
                        ]
                    }
                ]
            };
            ActionPossibility CreateFeralSwing(bool abandon)
            {
                return new ActionPossibility(new CombatAction(
                    q.Owner,
                    ExemplarIllustrations.HandsOfTheWildling,
                    "Feral Swing" + (abandon ? " (with abandon)" : ""),
                    [ExemplarTraits.Spirit, ExemplarTraits.Transcendence],
                    "You lash out with both arms, rending all before you. Each creature in a 15-foot cone must succeed at a basic Reflex save against your class DC or take spirit damage equal to your normal Strike damage with your {i}hands of the wildling{/i}." +
                    (abandon ? " Swinging with abandon imposes a -2 circumstance bonus to enemies' saving throws, but causes you to become off-guard until the start of your next turn." : ""),
                    Target.Cone(3).WithAdditionalRequirementOnCaster(caster =>
                    {
                        var hands = Ikon.GetIkonItem(q.Owner, ikonRune);
                        if (hands == null)
                        {
                            return Usability.NotUsable("You must be wielding the {i}hands of the wildling{/i}");
                        }
                        return Usability.Usable;
                    })
                )
                .WithActionCost(2)
                .WithSavingThrow(new SavingThrow(Defense.Reflex, q.Owner.ClassDC()))
                .WithEffectOnChosenTargets(async (action, self, targets) =>
                {
                    var hands = Ikon.GetIkonItem(self, ikonRune);
                    var penalty = new QEffect("Hands of the Wildling penalty", "", ExpirationCondition.Never, self)
                    {
                        BonusToDefenses = (q, penaltyAction, defense) => defense == Defense.Reflex && penaltyAction == action ? new Bonus(-2, BonusType.Circumstance, "Hands of the Wildling (with abandon)") : null
                    };
                    var strike = StrikeRules.CreateStrike(self, hands!, RangeKind.Melee, 0);
                    foreach (var target in targets.AllCreaturesInArea)
                    {
                        if (abandon)
                        {
                            target.AddQEffect(penalty);
                        }

                        if (targets.CheckResults.TryGetValue(target, out var checkResult))
                        {
                            await CommonSpellEffects.DealDirectDamage(action, strike.TrueDamageFormula!, target, checkResult, Ikon.GetBestDamageKindForSpark(self, target));

                            if (abandon)
                            {
                                target.RemoveAllQEffects(q => q == penalty);
                            }
                        }
                    }
                }));
            }
        })
        .WithRune(ikonRune)
        .IkonFeat;
    }
}
