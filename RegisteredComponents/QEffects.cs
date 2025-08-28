using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core.Mechanics;
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

    //Level 4 Feats
    public static readonly QEffectId SteelOnSteel = ModManager.RegisterEnumMember<QEffectId>("SteelOnSteel");
    public static readonly QEffectId EmpoweredThroughTheNeedlesEye = ModManager.RegisterEnumMember<QEffectId>("EmpoweredThroughTheNeedlesEye");

    //non-ikon epithet
    public static readonly QEffectId TheBraveUsedOnTarget = ModManager.RegisterEnumMember<QEffectId>("TheBraveUsedOnTarget");
    public static readonly QEffectId TheBraveUsedOnSelf = ModManager.RegisterEnumMember<QEffectId>("TheBraveUsedOnSelf");
    public static readonly QEffectId TheCunningUsedThisTurn = ModManager.RegisterEnumMember<QEffectId>("QTheCunningUsedThisTurn");

    public static readonly QEffectId TheMournfulUsedThisTurn = ModManager.RegisterEnumMember<QEffectId>("TheMournfulUsedThisTurn");
    public static readonly QEffectId TheMournfulImmune = ModManager.RegisterEnumMember<QEffectId>("TheMournfulImmune");
    public static readonly QEffectId PeerlessUnderHeavenImmune = ModManager.RegisterEnumMember<QEffectId>("PeerlessUnderHeavenImmune");
    public static readonly QEffectId TheProudUsedThisTurn = ModManager.RegisterEnumMember<QEffectId>("TheProudUsedThisTurn");
    public static readonly QEffectId TheProudEffect = ModManager.RegisterEnumMember<QEffectId>("TheProudEffect");
    public static readonly QEffectId TheRadiantUsedThisTurn = ModManager.RegisterEnumMember<QEffectId>("TheRadiantUsedThisTurn");
    public static readonly QEffectId TheRadiantImmune = ModManager.RegisterEnumMember<QEffectId>("TheRadiantImmune");
}

public static class ExemplarLongTermEffects
{
    public static readonly LongTermEffectId HornOfPlentyDailyElixir = ModManager.RegisterEnumMember<LongTermEffectId>("HornOfPlentyDailyElixir");
}
