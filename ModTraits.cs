using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Exemplar;

public static class ModTraits
{
    public static readonly Trait TEnergizedSpark = ModManager.RegisterTrait("EnergizedSpark");
    public static readonly Trait sanctifiedTrait = ModManager.RegisterTrait("sanctifiedTrait");
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
    //add one to seperate the body ikons.
    public static readonly Trait BodyIkon = ModManager.RegisterTrait(
        "BodyIkon",
        new TraitProperties("Body Ikon", true)
        {
            IsClassTrait = false
        });
    
    //technical traits for hurl at the horizon
    public static readonly Trait hurlAtTheHorizon = ModManager.RegisterTrait(
        "hurlAtTheHorizon",
        new TraitProperties("THurlAtTheHorizon", false)
        {
            IsClassTrait = false
        });
    
    public static readonly Trait Epithet = ModManager.RegisterTrait(
        "Epithet",
        new TraitProperties("Epithet", true)
        {
            IsClassTrait = false
        });
    
    public static readonly Trait RootEpithet = ModManager.RegisterTrait(
        "RootEpithet",
        new TraitProperties("Root Epithet", true)
        {
            IsClassTrait = false
        });



    // keepingTracktraits
    public static readonly Trait Feint = ModManager.RegisterTrait(
        "Feint",
        new TraitProperties("Feint", true)
        {
            IsClassTrait = false
        });
    public static readonly Trait CreateADiversion = ModManager.RegisterTrait(
        "CreateADiversion",
        new TraitProperties("CreateADiversion", true)
        {
            IsClassTrait = false
        });
    // Add more custom traits here if needed later
}
