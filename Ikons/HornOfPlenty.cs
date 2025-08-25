using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class HornOfPlenty
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName hornOfPlenty = ModManager.RegisterNewItemIntoTheShop("HornOfPlenty", itemName =>
        {
            return new Item(itemName, IllustrationName.BagOfHolding2, "Horn Of Plenty", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.Ikon)
            .WithStoresItem((container, subitem) =>
            {
                if (!subitem.HasTrait(Trait.Potion) && !subitem.HasTrait(Trait.Elixir))
                    return "You can only store potions and elixirs in the horn of plenty.";
                //make sure it is not too bulky
                // var currentBulk = container.StoredItems.Sum(i => i.w);
                // if (currentBulk + subitem.Bulk > 1)
                //     return "Horn of Plenty can hold up to 1 Bulk only.";
                return null; // OK to store
            });
        });
        yield return new Ikon(new Feat(
            ExemplarFeats.HornOfPlenty,
            "Whether a bag, gourd, wallet, cornucopia, or similar food receptacle, this ikon recalls the harvest and hearth.",
            "The receptacle can store up to 1 Bulk of potions and elixirs, but no other items. It can't be opened except by the ikon's immanence and transcendence abilities.\n" +
            "Each day during your daily preparations, the ikon produces one temporary elixir of life.\n\n" +
            "{b}Immanence{/b} The {i}horn of plenty{/i} shimmers, allowing access to the stored consumables inside. You can Interact to draw a consumable and drink it in a single action while " +
            "your divine spark rests within the horn. Other creatures can't access the contents unless you allow them to.\n\n" +
            $"{{b}}Transcendence — Feed the Masses {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (transcendence)\n" +
            "The {i}horn of plenty{/i} allows you to transfer the effects of potions and elixirs to your allies. " +
            "You Interact to draw a consumable from the horn and then Interact to drink it. Rather than nourishing yourself, the item's effects are transferred to a willing ally within 60 feet, as if " +
            "they had consumed it themself.  If the consumable restores Hit Points, you can choose to divide the amount it restores between you and the ally freely (after rolling dice to determine the amount).",
            [ExemplarTraits.Ikon],
            null
        ).WithIllustration(IllustrationName.BagOfHolding2)
        .WithOnCreature(self =>
        {
            var horn = self.CarriedItems.FirstOrDefault(i => i.BaseItemName == hornOfPlenty);
            if (horn != null && !horn.StoredItems.Any(item => item.Tag == horn))
            {
                var elixir = Items.CreateNew(ItemName.MinorElixirOfLife);
                elixir.Price = 0;
                elixir.Tag = horn;
                horn.StoredItems.Add(elixir);
            }
            if (!self.LongTermEffects?.Effects.Any(lte => lte.Id == ExemplarLongTermEffects.HornOfPlentyDailyElixir) ?? true)
            {
                self.LongTermEffects?.Add(new LongTermEffect()
                {
                    Id = ExemplarLongTermEffects.HornOfPlentyDailyElixir,
                    Duration = LongTermEffectDuration.UntilLongRest
                });
            }
        }), q =>
        {
            q.ProvideContextualAction = qe =>
            {
                var hornActions = qe.Owner.CarriedItems.FirstOrDefault(i => i.BaseItemName == hornOfPlenty)?.StoredItems.Select(item =>
                {
                    return new ActionPossibility(new CombatAction(qe.Owner, item.Illustration, item.Name, [Trait.Manipulate, Trait.Basic],
                    $"You Interact to draw the {item.Name} and drink it.", Target.Self())
                    .WithEffectOnChosenTargets(async (action, self, targets) =>
                    {
                        //TODO: swap hands, etc... so tedious
                        var horn = self.CarriedItems.FirstOrDefault(i => i.BaseItemName == hornOfPlenty);
                        horn!.StoredItems.Remove(item);
                        //TODO: horn.ItemModifications?

                        if (item.WhenYouDrink != null)
                        {
                            await item.WhenYouDrink.Invoke(action, self);
                        }
                    })) as Possibility;
                });
                return new SubmenuPossibility(IllustrationName.BagOfHolding2, "Horn of Plenty")
                {
                    Subsections =
                    [
                        new PossibilitySection("HornOfPlenty")
                        {
                            Possibilities = hornActions?.ToList() ?? []
                        }
                    ]
                };
            };
        },
        q =>
        {
            var hornActions = q.Owner.CarriedItems.FirstOrDefault(i => i.BaseItemName == hornOfPlenty)?.StoredItems.Select(item =>
            {
                return new ActionPossibility(new CombatAction(q.Owner, item.Illustration, item.Name, [Trait.Manipulate, ExemplarTraits.Transcendence],
                $"(something)", Target.RangedFriend(8))
                .WithActionCost(1)
                .WithEffectOnChosenTargets(async (action, self, targets) =>
                {
                    //TODO: swap hands again... maybe
                    var horn = self.CarriedItems.FirstOrDefault(i => i.BaseItemName == hornOfPlenty);
                    horn!.StoredItems.Remove(item);
                    //TODO: horn.ItemModifications?

                    if (item.WhenYouDrink != null)
                    {
                        await item.WhenYouDrink.Invoke(action, targets.ChosenCreature!);
                    }
                    //TODO: do something about the hp split? maybe just overhealing goes into self? is there a free-form entry box?
                })) as Possibility;
            });
            return new SubmenuPossibility(IllustrationName.BagOfHolding2, "Feed the Masses")
            {
                Subsections =
                [
                    new PossibilitySection("HornOfPlenty")
                    {
                        Possibilities = hornActions?.ToList() ?? []
                    }
                ]
            };
        })
        .WithRune(hornOfPlenty)
        .IkonFeat;
    }
}
