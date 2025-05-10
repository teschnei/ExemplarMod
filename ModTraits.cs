using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Exemplar;

public static class ModTraits
{
    public static readonly Trait TEnergizedSpark = ModManager.RegisterTrait("EnergizedSpark");
    public static readonly Trait Ikon = ModManager.RegisterTrait(
        "Ikon",
        new TraitProperties("Ikon", true)
        {
            IsClassTrait = false
        });

    public static readonly Trait Transcendence = ModManager.RegisterTrait(
        "Transcendence",
        new TraitProperties("Transcendence", true)
        {
            IsClassTrait = false
        });
    // Add more custom traits here if needed later
}
