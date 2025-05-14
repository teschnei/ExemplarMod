using System.Linq;
using System.Security;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
/*
    Todo: Currently this feat does not check if the current ikon is empowered.
    It should check if the current ikon is empowered and then add the thrown trait to the weapon.
*/
namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_HurlAtTheHorizon
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            // Define the feat
            var feat = new TrueFeat(
                ExemplarFeatNames.FeatHurlAtTheHorizon,
                2,
                "Hurl At The Horizon",
                "{b}Immanence{/b} Your imbued weapon gains the Thrown 15 feet trait, or if it already has the Thrown trait, its thrown range increases by 10 feet.",
                new[] { ExemplarBaseClass.TExemplar },
                null
            )
            .WithOnSheet(sheet =>
            {
                //list of all feats that have the Ikon trait
                var ikonFeats = sheet.AllFeats
                    .Where(f => f.Traits.Contains(ModTraits.Ikon))
                    .ToList();
                // Add the feat to the character sheet
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "HurlAtTheHorizon",
                        name: "Hurl At The Horizon",
                        level: 2,
                        eligible: ft => ikonFeats.Contains(ft) && ft.Traits.Contains(ModTraits.Ikon) &&
                            ft.FeatName != ExemplarFeatNames.FeatHurlAtTheHorizon && !ft.HasTrait(ModTraits.BodyIkon)
                    )
                );
                sheet.Tags["temp"] = "Ikon";
            })
            .WithPermanentQEffect(null, qf =>
            {
                // When empowered, adjust your equipped weapon
                qf.StateCheck = effect =>
                {
                    var weapon = effect.Owner.HeldItems
                        .FirstOrDefault(item => item.WeaponProperties != null);
                    if (weapon == null)
                        return;
                    // Check if the effect is empowered already.
                    if (weapon.Traits.Contains(ModTraits.hurlAtTheHorizon))
                        return;

                    // Check if the weapon is a melee weapon and does not have thrown 10 or 20 feet.
                    if (!weapon.HasTrait(Trait.Thrown) ||
                        (!weapon.HasTrait(Trait.Thrown10Feet) && !weapon.HasTrait(Trait.Thrown20Feet)))
                    {
                        // If the weapon doesn't have the Thrown trait, add it
                        weapon.Traits.Add(Trait.Thrown10Feet);
                        weapon.Traits.Add(ModTraits.hurlAtTheHorizon);
                        //give it the empowered trait as to not double give.
                    }
                    else if (!weapon.HasTrait(Trait.Thrown20Feet))
                    {
                        // If the weapon already has the Thrown trait, increase its range
                        weapon.Traits.Add(ModTraits.hurlAtTheHorizon);
                        weapon.Traits.Add(Trait.Thrown20Feet);
                    }
                };
            });

            ModManager.AddFeat(feat);
        }
    }
}
