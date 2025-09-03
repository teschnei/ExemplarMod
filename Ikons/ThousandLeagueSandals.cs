using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class ThousandLeagueSandals
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("ThousandLeagueSandals", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Thousand League Sandals", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonShoes)
            .WithRuneProperties(new RuneProperties("Ikon", IkonRuneKind.Ikon, "Threadbare but trustworthy, your sandals have carried you this far, and they'll carry you much farther still.",
            "", item =>
            {
                item.Traits.AddRange([ExemplarTraits.Ikon, Trait.Divine]);
            })
            .WithCanBeAppliedTo((Item rune, Item item) =>
            {
                if (!item.HasTrait(Trait.Worn) || item.WornAt != Trait.Shoes)
                {
                    return "Must be worn shoes.";
                }
                return null;
            }));
        });
        ItemName freeItem = ModManager.RegisterNewItemIntoTheShop("OrdinaryBoots", itemName =>
        {
            return new Item(itemName, IllustrationName.BootsOfBounding, "Boots", 1, 0, Trait.DoNotAddToShop)
            .WithDescription("An ordinary pair of boots.")
            .WithWornAt(Trait.Shoes);
        });
        yield return new Ikon(new Feat(
            ExemplarFeats.ThousandLeagueSandals,
            "Threadbare but trustworthy, your sandals have carried you this far, and they'll carry you much farther still.",
            "{b}Usage{/b} worn shoes\n\n" +
            "{b}Immanence{/b} Your sandals ease your travels on the path ahead, granting you a +10-foot status bonus to your Speed.\n\n" +
            $"{{b}}Transcendence — Marathon Dash {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (transcendence)\n" +
            "Your feet carry you so quickly they leave a slipstream that speeds your allies on. You Stride. Each ally within 10 feet of you at the start of your movement can Stride as a reaction.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWorn],
            null
        ).WithIllustration(IllustrationName.FreedomOfMovement), q =>
        {
            q.BonusToAllSpeeds = qe => Ikon.GetIkonItemWorn(qe.Owner, ikonRune) != null ? new Bonus(2, BonusType.Circumstance, "Thousand League Sandals") : null;
        }, q =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                IllustrationName.FreedomOfMovement,
                "Marathon Dash",
                [ExemplarTraits.Transcendence],
                "Your feet carry you so quickly they leave a slipstream that speeds your allies on. You Stride. Each ally within 10 feet of you at the start of your movement can Stride as a reaction.",
                Target.Self().WithAdditionalRestriction(self =>
                {
                    if (Ikon.GetIkonItemWorn(self, ikonRune) == null)
                    {
                        return "You must be wearing the Thousand League Sandals";
                    }
                    return null;
                })
            ).WithActionCost(1)
            .WithEffectOnChosenTargets(async (action, self, target) =>
            {
                // Capture which allies were in range at start
                var allies = self.Battle.AllCreatures
                    .Where(a => a.FriendOf(self) && a != self && a.DistanceTo(self) <= 2)
                    .ToList();

                // Your movement
                if (!await self.StrideAsync("Marathon Dash: Choose where to Stride", allowPass: false))
                {
                    action.RevertRequested = true;
                }
                else
                {
                    // Allow each captured ally to Stride as a reaction
                    foreach (var ally in allies)
                    {
                        // Note: this invokes their Stride; game will treat it as a reaction
                        if (await ally.AskToUseReaction("Marathon Dash: Use your reaction to Stride?"))
                        {
                            await ally.StrideAsync("Marathon Dash: Choose where to Stride", allowPass: true);
                        }
                    }
                }
            }));
        })
        .WithRune(ikonRune)
        .WithFreeWornItem(freeItem)
        .IkonFeat;
    }
}

