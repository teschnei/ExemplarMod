using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_HurlAtTheHorizon
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            // 2) Define the feat
            var feat = new TrueFeat(
                ExemplarFeatNames.FeatHurlAtTheHorizon,
                2,
                "Hurl At The Horizon",
                "{b}Immanence{/b} Your imbued weapon gains the Thrown 15 feet trait, or if it already has the Thrown trait, its thrown range increases by 10 feet.",
                new[] { ExemplarBaseClass.TExemplar },
                null
            )
            .WithPermanentQEffect(null, qf =>
            {
                // When empowered, adjust your equipped weapon
                qf.StateCheck = effect =>
                {
                    // Check if the weapon is valid
                    var weapon = effect.Owner.HeldItems
                        .FirstOrDefault(item => item.WeaponProperties != null);
                    if (weapon == null)
                        return;
                    // Check if the weapon is a melee weapon and does not have thrown 10 or 20 feet.
                    if (!weapon.HasTrait(Trait.Thrown) || 
                        (!weapon.HasTrait(Trait.Thrown10Feet) && !weapon.HasTrait(Trait.Thrown20Feet)))
                    {
                        // If the weapon doesn't have the Thrown trait, add it
                        weapon.Traits.Add(Trait.Thrown10Feet);
                        weapon.WeaponProperties.Throwable = true;
                    }
                    else if(!weapon.HasTrait(Trait.Thrown20Feet))
                    {
                        // If the weapon already has the Thrown trait, increase its range
                        weapon.Traits.Add(Trait.Thrown20Feet);
                    }
                };
            });

            ModManager.AddFeat(feat);
        }
    }
}
