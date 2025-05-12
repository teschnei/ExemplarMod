

using System;
using System.Collections.Generic;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Modding;

public static class ExemplarIkonQEffectIds
{
    //general effects
    public static readonly QEffectId FirstShiftFree = ModManager.RegisterEnumMember<QEffectId>("FirstShiftFree");
    public static readonly QEffectId TranscendenceTracker = ModManager.RegisterEnumMember<QEffectId>("TranscendenceTracker");
    
    //barrows
    public static readonly QEffectId QEmpoweredBarrowsEdge = ModManager.RegisterEnumMember<QEffectId>("BarrowsEdge");
    public static readonly QEffectId QBarrowsEdgeDamageTracker = ModManager.RegisterEnumMember<QEffectId>("BarrowsEdgeDamageTracker");

    //bands
    public static readonly QEffectId QEmpoweredBandsOfImprisonment = ModManager.RegisterEnumMember<QEffectId>("BandsOfImprisonment");

    //gleaming
    public static readonly QEffectId QEmpoweredGleamingBlade = ModManager.RegisterEnumMember<QEffectId>("GleamingBlade");

    //mirrored aegis
    public static readonly QEffectId QEmpoweredMirroredAegis = ModManager.RegisterEnumMember<QEffectId>("MirroredAegis");
    public static readonly QEffectId QMirroredAegisAura = ModManager.RegisterEnumMember<QEffectId>("MirroredAegisAura");

    //Eyecatching
    public static readonly QEffectId QEmpoweredEyeCatchingSpot = ModManager.RegisterEnumMember<QEffectId>("EmpoweredEyeCatchingSpot");

    //mortal harvest
    public static readonly QEffectId QEmpoweredMortalHarvest = ModManager.RegisterEnumMember<QEffectId>("MortalHarvest");

    //sandels
    public static readonly QEffectId QEmpoweredThousandLeagueSandals = ModManager.RegisterEnumMember<QEffectId>("ThousandLeagueSandals");

    //Gaze
    public static readonly QEffectId QEmpoweredGazeSharpAsSteel = ModManager.RegisterEnumMember<QEffectId>("GazeSharpAsSteel");
    public static readonly QEffectId QGazeMomentUnending = ModManager.RegisterEnumMember<QEffectId>("GazeMomentUnending");

    //Hands
    public static readonly QEffectId QEmpoweredHandsOfTheWildling = ModManager.RegisterEnumMember<QEffectId>("HandsOfTheWildling");

    
    //Scars
    public static readonly QEffectId QEmpoweredScarOfTheSurvivor = ModManager.RegisterEnumMember<QEffectId>("ScarOfTheSurvivor");

    //bangles
    public static readonly QEffectId QFetchingBanglesAura = ModManager.RegisterEnumMember<QEffectId>("FetchingBanglesAura");
    public static readonly QEffectId QEmpoweredFetchingBangles = ModManager.RegisterEnumMember<QEffectId>("FetchingBangles");

    //horn
    public static readonly QEffectId QEmpoweredHornOfPlenty = ModManager.RegisterEnumMember<QEffectId>("HornOfPlenty");

    //pelt
    public static readonly QEffectId QEmpoweredPeltOfTheBeast = ModManager.RegisterEnumMember<QEffectId>("PeltOfTheBEast");
    public static readonly QEffectId QPeltAura = ModManager.RegisterEnumMember<QEffectId>("PeltAura");

    //skin hard as horn
    public static readonly QEffectId QEmpoweredSkinHardAsHorn = ModManager.RegisterEnumMember<QEffectId>("Skin Hard As Horn");
    public static readonly QEffectId QSkinHornAura = ModManager.RegisterEnumMember<QEffectId>("Skin Horn Aura");

    //starshot
    public static readonly QEffectId QEmpoweredStarshot = ModManager.RegisterEnumMember<QEffectId>("Starshot");

    //noblebranch
    public static readonly QEffectId QEmpoweredNobleBranch = ModManager.RegisterEnumMember<QEffectId>("Noble Branch");
    public static readonly QEffectId QNobleBranchDamageTracker = ModManager.RegisterEnumMember<QEffectId>("NobleBranchDamageTracker");
    //titans break
    public static readonly QEffectId QEmpoweredTitansBreaker = ModManager.RegisterEnumMember<QEffectId>("TitansBreaker");
    //unfailingbow
    public static readonly QEffectId QEmpoweredUnfailingBow = ModManager.RegisterEnumMember<QEffectId>("UnfailingBow");

    //non-ikon feats
    public static readonly QEffectId QEnergizedSpark =
        ModManager.RegisterEnumMember<QEffectId>("EnergizedSpark");
    public static readonly QEffectId QHurlAtTheHorizon =
        ModManager.RegisterEnumMember<QEffectId>("HurlAtTheHorizon");
    public static readonly QEffectId QLeapTheFalls =
        ModManager.RegisterEnumMember<QEffectId>("LeapTheFalls");
    public static readonly QEffectId QSteelOnSteel =
        ModManager.RegisterEnumMember<QEffectId>("SteelOnSteel");
        
    //BELOW HERE IS ONLY REQUIRED FOR NEW IKONS, EFFECTS ARE NOT NEEDED.
    public static readonly HashSet<QEffectId> EmpoweredIkonIds = new()
    {
        QEmpoweredBarrowsEdge,
        QEmpoweredBandsOfImprisonment,
        QEmpoweredGleamingBlade,
        QEmpoweredMirroredAegis,
        QEmpoweredEyeCatchingSpot,
        QEmpoweredMortalHarvest,
        QEmpoweredThousandLeagueSandals,
        QEmpoweredFetchingBangles,
        QEmpoweredScarOfTheSurvivor,
        QEmpoweredHandsOfTheWildling,
        QEmpoweredGazeSharpAsSteel,
        QEmpoweredHornOfPlenty,
        QEmpoweredPeltOfTheBeast,
        QEmpoweredSkinHardAsHorn,
        QEmpoweredStarshot,
        QEmpoweredNobleBranch,
        QEmpoweredTitansBreaker,
        QEmpoweredUnfailingBow
    };
    // Add more Ikon IDs as you add more Ikons
    public static QEffectId GetEmpowermentIdForIkon(string ikonName)
        {
            return ikonName switch
            {
                "Barrow's Edge" => QEmpoweredBarrowsEdge,
                "Bands of Imprisonment" => QEmpoweredBandsOfImprisonment,
                "Gleaming Blade" => QEmpoweredGleamingBlade,
                "Mirrored Aegis" => QEmpoweredMirroredAegis,
                "Eye Catching Spot" => QEmpoweredEyeCatchingSpot,
                "Mortal Harvest" => QEmpoweredMortalHarvest,
                "Thousand League Sandals" => QEmpoweredThousandLeagueSandals,
                "Fetching Bangles" =>QEmpoweredFetchingBangles,
                "Scar Of The Survivor" =>QEmpoweredScarOfTheSurvivor,
                "Hands Of The Wildling" =>QEmpoweredHandsOfTheWildling,
                "Gaze Sharp As Steel" =>QEmpoweredGazeSharpAsSteel,
                "Horn Of Plenty" => QEmpoweredHornOfPlenty,
                "Pelt Of The Beast" => QEmpoweredPeltOfTheBeast,
                "Skin Hard As Horn" => QEmpoweredSkinHardAsHorn,
                "Starshot" => QEmpoweredStarshot,
                "Noble Branch" => QEmpoweredNobleBranch,
                "Titans Breaker"=> QEmpoweredTitansBreaker,
                "Unfailing Bow"=> QEmpoweredUnfailingBow,
                _ => throw new Exception($"Unknown Ikon name: {ikonName}"),

                // Add more mappings as you add more Ikons
            };
        }
}
