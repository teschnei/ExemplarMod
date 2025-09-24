using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;

public static class ExemplarQEffects
{
    public static readonly QEffectId ShiftImmanence = ModManager.RegisterEnumMember<QEffectId>("ShiftImmanence");
    //Any QEffect that should be removed when an Ikon loses empowerment
    public static readonly QEffectId IkonExpansion = ModManager.RegisterEnumMember<QEffectId>("IkonExpansion");

    public static readonly QEffectId PeltOfTheBeastAttunement = ModManager.RegisterEnumMember<QEffectId>("PeltOfTheBeastAttunement");
    public static readonly QEffectId SkinHardAsHornAttunement = ModManager.RegisterEnumMember<QEffectId>("SkinHardAsHornAttunement");
    public static readonly QEffectId RaiseTheWalls = ModManager.RegisterEnumMember<QEffectId>("RaiseTheWalls");

    //Level 1 Feats
    public static readonly QEffectId EnergizedSpark = ModManager.RegisterEnumMember<QEffectId>("EnergizedSpark");
    public static readonly QEffectId SanctifiedSoul = ModManager.RegisterEnumMember<QEffectId>("SanctifiedSoul");
    public static readonly QEffectId VowOfMortalDefiance = ModManager.RegisterEnumMember<QEffectId>("VowOfMortalDefiance");
    public static readonly QEffectId VowOfMortalDefianceUsed = ModManager.RegisterEnumMember<QEffectId>("VowOfMortalDefianceUsed");

    //Level 2 Feats
    public static readonly QEffectId LightningSwap = ModManager.RegisterEnumMember<QEffectId>("LightningSwap");

    //Level 6 Feats
    public static readonly QEffectId FlowOfWarUsed = ModManager.RegisterEnumMember<QEffectId>("FlowOfWarUsed");

    //Level 8 Feats
    public static readonly QEffectId BattleHymnToTheLostUsed = ModManager.RegisterEnumMember<QEffectId>("BattleHymnToTheLostUsed");
    public static readonly QEffectId RaiseIslandUsed = ModManager.RegisterEnumMember<QEffectId>("RaiseIslandUsed");
    public static readonly QEffectId RejoiceInSolsticeStormUsed = ModManager.RegisterEnumMember<QEffectId>("RejoiceInSolsticeStormUsed");

    //Epithets
    public static readonly QEffectId TheBraveUsedOnTarget = ModManager.RegisterEnumMember<QEffectId>("TheBraveUsedOnTarget");
    public static readonly QEffectId TheDeft = ModManager.RegisterEnumMember<QEffectId>("TheDeft");
    public static readonly QEffectId TheMournfulUsedOnTarget = ModManager.RegisterEnumMember<QEffectId>("TheMournfulUsedOnTarget");
    public static readonly QEffectId TheRadiantUsedOnTarget = ModManager.RegisterEnumMember<QEffectId>("TheRadiantUsedOnTarget");
    public static readonly QEffectId PeerlessUnderHeavenUsedOnTarget = ModManager.RegisterEnumMember<QEffectId>("PeerlessUnderHeavenUsedOnTarget");
    public static readonly QEffectId OfVerseUnbrokenUsedOnTarget = ModManager.RegisterEnumMember<QEffectId>("OfVerseUnbrokenUsedOnTarget");
}

public static class ExemplarTileQEffects
{
    public static readonly TileQEffectId BornOfTheBonesOfTheEarthTerrain = ModManager.RegisterEnumMember<TileQEffectId>("BornOfTheBonesOfTheEarthTerrain");

}

public static class ExemplarLongTermEffects
{
    public static readonly LongTermEffectId HornOfPlentyDailyElixir = ModManager.RegisterEnumMember<LongTermEffectId>("HornOfPlentyDailyElixir");
}
