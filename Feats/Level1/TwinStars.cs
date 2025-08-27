using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class TwinStars
{
    [FeatGenerator(1)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("TwinStars", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Twin Stars", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonTwinStars)
            .WithRuneProperties(new RuneProperties("Twinned", IkonRuneKind.IkonTwinStars, "Your divine spark embodies a primordial duality, and your ikon splits itself accordingly into two corresponding halves.",
            "", item =>
            {
                item.Traits.Add(ExemplarTraits.Twin);
            })
            .WithCanBeAppliedTo((Item rune, Item weapon) =>
            {
                if (weapon.WeaponProperties == null)
                {
                    return "Must be a weapon.";
                }
                if (weapon.HasTrait(Trait.Ranged) || weapon.HasTrait(Trait.TwoHanded))
                {
                    return "Must be a one-handed melee weapon.";
                }
                if (!weapon.HasTrait(ExemplarTraits.Ikon))
                {
                    return "Must be an ikon.";
                }
                return null;
            }));
        });
        var twinStars = new TrueFeat(
            ExemplarFeats.TwinStars,
            1,
            "Your divine spark embodies a primordial duality, and your ikon splits itself accordingly into two corresponding halves.",
            "{b}Usage{/b} imbued into a one-handed weapon ikon.\n\nWhen combat begins, your weapon splits into two copies of itself, which both gain the twin trait; these copies are identical " +
            "except for one mirrored feature, such as a sun motif on one and a moon motif on another. As these are both manifestations of the same object, your divine spark empowers the " +
            "two halves as if they were a single ikon.",
            [ExemplarTraits.Exemplar, ExemplarTraits.IkonExpansion],
            null
        )
        .WithOnCreature(creature =>
        {
            //WithOnCreatureWhenWorn (for items) doesn't work on weapons, so we have to check here instead
            var item = creature.HeldItems.Where(item => item.Runes.Any(rune => rune.RuneProperties?.RuneKind == IkonRuneKind.IkonTwinStars)).FirstOrDefault();
            if (item != null)
            {
                if (creature.HeldItems.Count == 1)
                {
                    creature.HeldItems.Add(item.Duplicate());
                }
                else
                {
                    creature.CarriedItems.Add(item.Duplicate());
                }
            }
            else
            {
                item = creature.CarriedItems.Where(item => item.Runes.Any(rune => rune.RuneProperties?.RuneKind == IkonRuneKind.IkonTwinStars)).FirstOrDefault();
                if (item != null)
                {
                    creature.CarriedItems.Add(item.Duplicate());
                }
            }
        });
        Ikon.ExtraRunes[twinStars.FeatName] = ikonRune;
        yield return twinStars;
    }
}
