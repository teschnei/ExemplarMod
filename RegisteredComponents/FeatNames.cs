using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;

public static class ExemplarFeats
{
    public static readonly FeatName ExemplarClass = ModManager.RegisterFeatName("ExemplarClass", "Exemplar");
    public static readonly FeatName HumbleStrikes = ModManager.RegisterFeatName("HumbleStrikes", "Humble Strikes");
    //Ikons
    public static readonly FeatName BarrowsEdge = ModManager.RegisterFeatName("BarrowsEdge", "Barrow's Edge");
    public static readonly FeatName BandsOfImprisonment = ModManager.RegisterFeatName("BandsOfImprisonment", "Bands of Imprisonment");
    public static readonly FeatName EyeCatchingSpot = ModManager.RegisterFeatName("EyeCatchingSpot", "Eye Catching Spot");
    public static readonly FeatName FetchingBangles = ModManager.RegisterFeatName("FetchingBangles", "Fetching Bangles");
    public static readonly FeatName GazeSharpAsSteel = ModManager.RegisterFeatName("GazeSharpAsSteel", "Gaze Sharp As Steel");
    public static readonly FeatName GleamingBlade = ModManager.RegisterFeatName("GleamingBlade", "Gleaming Blade");
    public static readonly FeatName GleamingBladeWeapon = ModManager.RegisterFeatName("GleamingBladeWeapon", "Gleaming Blade (Weapon)");
    public static readonly FeatName GleamingBladeUnarmed = ModManager.RegisterFeatName("GleamingBladeUnarmed", "Gleaming Blade (Unarmed)");
    public static readonly FeatName HandsOfTheWildling = ModManager.RegisterFeatName("HandsOfTheWildling", "Hands Of The Wildling");
    public static readonly FeatName HandsOfTheWildlingWeapon = ModManager.RegisterFeatName("HandsOfTheWildlingWeapon", "Hands Of The Wildling (Weapon)");
    public static readonly FeatName HandsOfTheWildlingUnarmed = ModManager.RegisterFeatName("HandsOfTheWildlingUnarmed", "Hands Of The Wildling (Unarmed)");
    public static readonly FeatName HornOfPlenty = ModManager.RegisterFeatName("HornOfPlenty", "Horn Of Plenty");
    public static readonly FeatName MirroredAegis = ModManager.RegisterFeatName("MirroredAegis", "Mirrored Aegis");
    public static readonly FeatName MortalHarvest = ModManager.RegisterFeatName("MortalHarvest", "Mortal Harvest");
    public static readonly FeatName NobleBranch = ModManager.RegisterFeatName("NobleBranch", "Noble Branch");
    public static readonly FeatName PeltOfTheBeast = ModManager.RegisterFeatName("PeltOfTheBeast", "Pelt Of The Beast");
    public static readonly FeatName ScarOfTheSurvivor = ModManager.RegisterFeatName("ScarOfTheSurvivor", "Scar Of The Survivor");
    public static readonly FeatName ShadowSheath = ModManager.RegisterFeatName("ShadowSheath", "Shadow Sheath");
    public static readonly FeatName SkinHardAsHorn = ModManager.RegisterFeatName("SkinHardAsHorn", "Skin Hard As Horn");
    public static readonly FeatName SkybearersBelt = ModManager.RegisterFeatName("SkybearersBelt", "Skybearer's Belt");
    public static readonly FeatName Starshot = ModManager.RegisterFeatName("Starshot", "Starshot");
    public static readonly FeatName ThousandLeagueSandals = ModManager.RegisterFeatName("ThousandLeagueSandals", "Thousand League Sandals");
    public static readonly FeatName TitansBreaker = ModManager.RegisterFeatName("TitansBreaker", "Titans Breaker");
    public static readonly FeatName TitansBreakerWeapon = ModManager.RegisterFeatName("TitansBreakerWeapon", "Titans Breaker (Weapon)");
    public static readonly FeatName TitansBreakerUnarmed = ModManager.RegisterFeatName("TitansBreakerUnarmed", "Titans Breaker (Unarmed)");
    public static readonly FeatName UnfailingBow = ModManager.RegisterFeatName("UnfailingBow", "Unfailing Bow");
    public static readonly FeatName VictorsWreath = ModManager.RegisterFeatName("VictorsWreath", "Victor's Wreath");

    //Ikon subfeats
    public static readonly FeatName PeltOfTheBeastCold = ModManager.RegisterFeatName("PeltOfTheBeastCold", "Cold");
    public static readonly FeatName PeltOfTheBeastElectricity = ModManager.RegisterFeatName("PeltOfTheBeastElectricity", "Electricity");
    public static readonly FeatName PeltOfTheBeastFire = ModManager.RegisterFeatName("PeltOfTheBeastFire", "Fire");
    public static readonly FeatName PeltOfTheBeastPoison = ModManager.RegisterFeatName("PeltOfTheBeastPoison", "Poison");
    public static readonly FeatName PeltOfTheBeastSonic = ModManager.RegisterFeatName("PeltOfTheBeastSonic", "Sonic");
    public static readonly FeatName SkinHardAsHornBludgeoning = ModManager.RegisterFeatName("SkinHardAsHornBludgeoning", "Bludgeoning");
    public static readonly FeatName SkinHardAsHornPiercing = ModManager.RegisterFeatName("SkinHardAsHornPiercing", "Piercing");
    public static readonly FeatName SkinHardAsHornSlashing = ModManager.RegisterFeatName("SkinHardAsHornSlashing", "Slashing");

    //Root epithets
    public static readonly FeatName TheBrave = ModManager.RegisterFeatName("TheBrave", "The Brave");
    public static readonly FeatName TheCunning = ModManager.RegisterFeatName("TheCunning", "The Cunning");
    public static readonly FeatName TheDeft = ModManager.RegisterFeatName("TheDeft", "The Deft");
    public static readonly FeatName TheMournful = ModManager.RegisterFeatName("TheMournful", "The Mournful");
    public static readonly FeatName TheProud = ModManager.RegisterFeatName("TheProud", "The Proud");
    public static readonly FeatName TheRadiant = ModManager.RegisterFeatName("TheRadiant", "The Radiant");

    //Dominion epithets
    public static readonly FeatName BornOfTheBonesOfTheEarth = ModManager.RegisterFeatName("BornOfTheBonesOfTheEarth", "Born of the Bones of the Earth");
    public static readonly FeatName DancerInTheSeasons = ModManager.RegisterFeatName("DancerInTheSeasons", "Dancer in the Seasons");
    public static readonly FeatName OfVerseUnbroken = ModManager.RegisterFeatName("OfVerseUnbroken", "Of Verse Unbroken");
    public static readonly FeatName PeerlessUnderHeaven = ModManager.RegisterFeatName("PeerlessUnderHeaven", "Peerless under Heaven");
    public static readonly FeatName RestlessAsTheTides = ModManager.RegisterFeatName("RestlessAsTheTides", "Restless as the Tides");
    public static readonly FeatName WhoseCryIsThunder = ModManager.RegisterFeatName("WhoseCryIsThunder", "Whose Cry is Thunder");

    //Level 1 Feats
    public static readonly FeatName EnergizedSpark = ModManager.RegisterFeatName("EnergizedSpark", "Energized Spark");

    public static readonly FeatName EnergizedSparkAir = ModManager.RegisterFeatName("EnergizedSparkAir", "Air (Slashing)");
    public static readonly FeatName EnergizedSparkCold = ModManager.RegisterFeatName("EnergizedSparkCold", "Cold");
    public static readonly FeatName EnergizedSparkEarth = ModManager.RegisterFeatName("EnergizedSparkEarth", "Earth (Bludgeoning)");
    public static readonly FeatName EnergizedSparkElectricity = ModManager.RegisterFeatName("EnergizedSparkElectricity", "Electricity");
    public static readonly FeatName EnergizedSparkFire = ModManager.RegisterFeatName("EnergizedSparkFire", "Fire");
    public static readonly FeatName EnergizedSparkMetal = ModManager.RegisterFeatName("EnergizedSparkMetal", "Metal (Slashing)");
    public static readonly FeatName EnergizedSparkPoison = ModManager.RegisterFeatName("EnergizedSparkPoison", "Poison");
    public static readonly FeatName EnergizedSparkSonic = ModManager.RegisterFeatName("EnergizedSparkSonic", "Sonic");
    public static readonly FeatName EnergizedSparkVitality = ModManager.RegisterFeatName("EnergizedSparkVitality", "Vitality (Positive)");
    public static readonly FeatName EnergizedSparkVoid = ModManager.RegisterFeatName("EnergizedSparkVoid", "Void (Negative)");
    public static readonly FeatName EnergizedSparkWater = ModManager.RegisterFeatName("EnergizedSparkWater", "Water (Bludgeoning)");
    public static readonly FeatName EnergizedSparkWood = ModManager.RegisterFeatName("EnergizedSparkWood", "Wood (Piercing)");

    public static readonly FeatName SanctifiedSoul = ModManager.RegisterFeatName("SanctifiedSoul", "Sanctified Soul");
    public static readonly FeatName SanctifiedSoulHoly = ModManager.RegisterFeatName("EnergizedSparkHoly", "Holy (Good)");
    public static readonly FeatName SanctifiedSoulUnholy = ModManager.RegisterFeatName("EnergizedSparkUnholy", "Unholy (Evil)");

    public static readonly FeatName TwinStars = ModManager.RegisterFeatName("TwinStars", "Twin Stars");
    public static readonly FeatName VowOfMortalDefiance = ModManager.RegisterFeatName("VowOfMortalDefiance", "Vow of Mortal Defiance");

    //Level 2 Feats
    public static readonly FeatName HurlAtTheHorizon = ModManager.RegisterFeatName("HurlAtTheHorizon", "Hurl At The Horizon");
    public static readonly FeatName LeapTheFalls = ModManager.RegisterFeatName("LeapTheFalls", "Leap The Falls");
    public static readonly FeatName LightningSwap = ModManager.RegisterFeatName("LightningSwap", "Lightning Swap");
    public static readonly FeatName RedGoldMortality = ModManager.RegisterFeatName("RedGoldMortality", "Red-Gold Mortality");

    //Level 4 Feats
    public static readonly FeatName OnlyTheWorthy = ModManager.RegisterFeatName("OnlyTheWorthy", "Only the Worthy");
    public static readonly FeatName SteelOnSteel = ModManager.RegisterFeatName("SteelOnSteel", "Steel On Steel");
    public static readonly FeatName ThroughTheNeedlesEye = ModManager.RegisterFeatName("ThroughTheNeedlesEye", "Through The Needle's Eye");

    //Level 6 Feats
    public static readonly FeatName BindingSerpentsCelestialArrow = ModManager.RegisterFeatName("BindingSerpentsCelestialArrow", "Binding Serpents Celestial Arrow");
    public static readonly FeatName FlowOfWar = ModManager.RegisterFeatName("FlowOfWar", "Flow Of War");
    public static readonly FeatName MotionlessCutter = ModManager.RegisterFeatName("MotionlessCutter", "Motionless Cutter");
    public static readonly FeatName ReactiveStrike = ModManager.RegisterFeatName("ReactiveStrike", "Reactive Strike");

    //Level 8 Feats
    public static readonly FeatName AdditionalIkon = ModManager.RegisterFeatName("AdditionalIkon", "Additional Ikon");
    public static readonly FeatName BattleHymnToTheLost = ModManager.RegisterFeatName("BattleHymnToTheLost", "Battle Hymn to the Lost");
    public static readonly FeatName RaiseIsland = ModManager.RegisterFeatName("RaiseIsland", "Raise Island");
    public static readonly FeatName RejoiceInSolsticeStorm = ModManager.RegisterFeatName("RejoiceInSolsticeStorm", "Rejoice in Solstice Storm");
}
