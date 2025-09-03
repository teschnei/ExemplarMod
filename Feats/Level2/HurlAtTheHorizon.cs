using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level2;

public class HurlAtTheHorizon
{
    [FeatGenerator(2)]
    public static IEnumerable<Feat> GetFeat()
    {
        var flavorText = "Your weapon flies from your hand as if propelled under its own power.";
        var rulesText = "{b}Usage{/b} imbued into a thrown or melee weapon ikon\n\n The imbued ikon gains the following ability.\n\n" +
            "{b}Immanence{/b} Your weapon gains the thrown 15 feet trait, or increases its thrown distance by 10 feet if it already has the thrown trait.\n\n" +
            "{i}Note: there is no warning for using this on an incompatible item, it will just do nothing{/i}.";
        yield return new TrueFeat(
            ExemplarFeats.HurlAtTheHorizon,
            2,
            flavorText,
            rulesText,
            [ExemplarTraits.Exemplar, ExemplarTraits.IkonExpansion],
            Ikon.AddExpansionFeat("HurlAtTheHorizon", flavorText, rulesText, [], ikon => ikon.IkonFeat.HasTrait(ExemplarTraits.IkonWeapon), (ikon, feat) =>
            {
                feat.WithOnCreature(creature =>
                {
                    var ikonItem = creature.HeldItems.Concat(creature.CarriedItems)
                        .Where(item => item.Runes.Any(rune => rune.ItemName == ikon.Rune)).FirstOrDefault();
                    if (ikonItem != null && ((ikonItem.WeaponProperties?.Throwable ?? false) || !ikonItem.HasTrait(Trait.Ranged)))
                    {
                        creature.AddQEffect(new QEffect()
                        {
                            AfterYouAcquireEffect = async (q, newQ) =>
                            {
                                if (newQ.Id == ikon.EmpoweredQEffectId)
                                {
                                    var oldRange = ikonItem!.WeaponProperties?.RangeIncrement ?? 0;
                                    var oldForcedMelee = ikonItem!.WeaponProperties?.ForcedMelee;
                                    bool addedThrown = false;
                                    if (ikonItem.HasTrait(Trait.Thrown10Feet) || ikonItem.HasTrait(Trait.Thrown20Feet))
                                    {
                                        ikonItem.WeaponProperties?.WithRangeIncrement(oldRange + 2);
                                    }
                                    else if (ikonItem.WeaponProperties != null)
                                    {
                                        addedThrown = true;
                                        ikonItem.Traits.Add(Trait.Thrown10Feet);
                                        ikonItem.WeaponProperties.WithThrownXFeet(3);
                                    }
                                    q.Owner.AddQEffect(new QEffect()
                                    {
                                        Id = ExemplarQEffects.IkonExpansion,
                                        WhenExpires = q =>
                                        {
                                            ikonItem.WeaponProperties?.WithThrownXFeet(oldRange);
                                            if (addedThrown)
                                            {
                                                ikonItem.Traits.Remove(Trait.Thrown10Feet);
                                                ikonItem.WeaponProperties!.Throwable = false;
                                                ikonItem.WeaponProperties!.ForcedMelee = oldForcedMelee ?? false;
                                            }
                                        }
                                    });
                                }
                            }
                        });
                    }
                });
            }).ToList()
        );
    }
}
