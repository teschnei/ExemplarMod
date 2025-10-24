using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level1;

public class TwinStars
{
    [FeatGenerator(1)]
    public static IEnumerable<Feat> GetFeat()
    {
        var flavorText = "Your divine spark embodies a primordial duality, and your ikon splits itself accordingly into two corresponding halves.";
        var rulesText = "{b}Usage{/b} imbued into a one-handed weapon ikon.\n\nWhen combat begins, your weapon splits into two copies of itself, which both gain the twin trait; these copies are identical " +
            "except for one mirrored feature, such as a sun motif on one and a moon motif on another. As these are both manifestations of the same object, your divine spark empowers the " +
            "two halves as if they were a single ikon.";
        yield return new TrueFeat(
            ExemplarFeats.TwinStars,
            1,
            flavorText,
            rulesText,
            [ExemplarTraits.Exemplar, ExemplarTraits.IkonExpansion],
            Ikon.AddExpansionFeat("TwinStars", flavorText, rulesText, [], ikon => ikon.IkonFeat.HasTrait(ExemplarTraits.IkonWeapon), (ikon, feat) =>
            {
                feat.WithOnCreature(creature =>
                {
                    var item = creature.HeldItems.Where(item => ikon.IsIkonItem(item)).FirstOrDefault();
                    Item? twinnedItem = null;
                    // If the item has shadow sheath at all (not necessarily this specific ikon), we'll handle it there instead
                    if (item != null && !(item.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.CustomPermanent && ((mod.Tag as string)?.Contains("ShadowSheath") ?? false))))
                    {
                        item.Traits.Add(ExemplarTraits.Twin);
                        twinnedItem = item.Duplicate();
                        twinnedItem.Traits.AddRange(Trait.EncounterEphemeral, ExemplarTraits.Twin);
                        if (creature.HeldItems.Count == 1)
                        {
                            creature.HeldItems.Add(twinnedItem);
                        }
                        else
                        {
                            creature.CarriedItems.Add(twinnedItem);
                        }
                    }
                    else
                    {
                        item = creature.CarriedItems.Where(item => ikon.IsIkonItem(item)).FirstOrDefault();
                        if (item != null && !(item.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.CustomPermanent && ((mod.Tag as string)?.Contains("ShadowSheath") ?? false))))
                        {
                            item.Traits.Add(ExemplarTraits.Twin);
                            twinnedItem = item.Duplicate();
                            twinnedItem.Traits.AddRange(Trait.EncounterEphemeral, ExemplarTraits.Twin);
                            creature.CarriedItems.Add(twinnedItem);
                        }
                    }
                });
            }, item =>
            {
                if (item.HasTrait(Trait.TwoHanded))
                {
                    return "Twin Stars: must be a one-handed weapon ikon.";
                }
                return null;
            }).ToList()
        );
    }
}
