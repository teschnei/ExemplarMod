// ExemplarFeatNames.cs
using Dawnsbury.Modding;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;
using System.Collections.Generic;

namespace Dawnsbury.Mods.Classes.Exemplar
{
    public static class ExemplarFeatNames
    {
        public static readonly FeatName IkonBarrowsEdge =
            ModManager.RegisterFeatName("IkonBarrowsEdge", "Barrow's Edge");

        public static readonly FeatName IkonBandsOfImprisonment =
            ModManager.RegisterFeatName("IkonBandsOfImprisonment", "Bands of Imprisonment");

        public static readonly FeatName IkonGleamingBlade =
            ModManager.RegisterFeatName("IkonGleamingBlade", "Gleaming Blade");

        public static readonly FeatName IkonMirroredAegis =
            ModManager.RegisterFeatName("IkonMirroredAegis", "Mirrored Aegis");

        public static readonly FeatName IkonEyeCatchingSpot =
            ModManager.RegisterFeatName("IkonEyeCatchingSpot", "Eye Catching Spot");

        public static readonly FeatName IkonMortalHarvest =
            ModManager.RegisterFeatName("IkonMortalHarvest", "Mortal Harvest");

        public static readonly FeatName IkonThousandLeagueSandals =
            ModManager.RegisterFeatName("IkonThousandLeagueSandals", "Thousand League Sandals");

        public static readonly FeatName IkonGazeSharpAsSteel =
            ModManager.RegisterFeatName("IkonGazeSharpAsSteel", "Gaze Sharp As Steel");

        public static readonly FeatName IkonHandsOfTheWildling =
            ModManager.RegisterFeatName("IkonHandsOfTheWildling", "Hands Of The Wildling");
        public static readonly FeatName IkonScarOfTheSurvivor =
            ModManager.RegisterFeatName("IkonScarOfTheSurvivor", "Scar Of The Survivor");
        public static readonly FeatName IkonFetchingBangles =
            ModManager.RegisterFeatName("IkonFetchingBangles", "Fetching Bangles");

        public static readonly FeatName IkonHornOfPlenty =
            ModManager.RegisterFeatName("IkonHornOfPlenty", "Horn Of Plenty");

        public static readonly FeatName IkonPeltOfTheBeast =
            ModManager.RegisterFeatName("IkonPeltOfTheBeast", "Pelt Of The Beast");
        public static readonly FeatName IkonSkinHardAsHorn =
            ModManager.RegisterFeatName("IkonSkinHardAsHorn", "Skin Hard As Horn");
        public static readonly FeatName IkonStarshot =
            ModManager.RegisterFeatName("IkonStarshot", "Starshot");
        public static readonly FeatName IkonNobleBranch =
            ModManager.RegisterFeatName("IkonNobleBranch", "Noble Branch");
        public static readonly FeatName IkonTitansBreaker =
            ModManager.RegisterFeatName("IkonTitansBreaker", "Titans Breaker");
        internal static readonly FeatName IkonUnfailingBow =
            ModManager.RegisterFeatName("IkonUnfailingBow", "Unfailing Bow");

        //non-ikon feats
        public static readonly FeatName FeatReactiveStrike =
            ModManager.RegisterFeatName("FeatReactiveStrike", "Reactive Strike");
        public static readonly FeatName FeatEnergizedSpark =
            ModManager.RegisterFeatName("FeatEnergizedSpark", "Energized Spark");
        public static readonly FeatName FeatHurlAtTheHorizon =
            ModManager.RegisterFeatName("FeatHurlAtTheHorizon", "Hurl At The Horizon");
        public static readonly FeatName FeatLeapTheFalls =
            ModManager.RegisterFeatName("FeatLeapTheFalls", "Leap The Falls");
        public static readonly FeatName FeatSteelOnSteel =
            ModManager.RegisterFeatName("FeatSteelOnSteel", "Steel On Steel");
        public static readonly FeatName FeatThroughTheNeedlesEye =
            ModManager.RegisterFeatName("IkonThroughTheNeedlesEye", "Through The Needle's Eye");
        public static readonly FeatName FeatFlowOfWar =
            ModManager.RegisterFeatName("FlowOfWar", "Flow Of War");
        public static readonly FeatName FeatAdditionalIkon =
            ModManager.RegisterFeatName("AdditionalIkon", "Additional Ikon");
        public static readonly FeatName FeatSanctifiedSoul =
            ModManager.RegisterFeatName("SanctifiedSoul", "Sanctified Soul");


        //List of root epithets
        public static readonly FeatName EpithetTheBrave =
            ModManager.RegisterFeatName("EpithetTheBrave", "The Brave");
        public static readonly FeatName EpithetTheCunning =
            ModManager.RegisterFeatName("EpithetTheCunning", "The Cunning");
        public static readonly FeatName EpithetTheMournful =
            ModManager.RegisterFeatName("EpithetTheMournful", "The Mournful");
        public static readonly FeatName EpithetTheProud =
            ModManager.RegisterFeatName("EpithetTheProud", "The Proud");
        public static readonly FeatName EpithetTheRadiant =
            ModManager.RegisterFeatName("EpithetTheRadiant", "The Radiant");
        // …and so on for every Ikon

        //List of Dominion epithets
        public static readonly FeatName EpithetDancerInTheSeasons =
            ModManager.RegisterFeatName("EpithetDancerInTheSeasons", "The Dancer In The Seasons");
        public static readonly FeatName EpithetPeerlessUnderHeaven =
            ModManager.RegisterFeatName("EpithetPeerlessUnderHeaven", "The Peerless Under Heaven");
        public static readonly FeatName EpithetRestlessAsTheTides =
            ModManager.RegisterFeatName("EpithetRestlessAsTheTides", "The Restless As The Tides");
        public static readonly FeatName EpithetWhoseCryIsThunder =
            ModManager.RegisterFeatName("EpithetWhoseCryIsThunder", "The Whose Cry Is Thunder");
        // 1) Dummy FeatNames for EnergizedSparks. Vitality and Void are not implemented.
        public static readonly FeatName[] AttunementFeats =
        {
            ModManager.RegisterFeatName("EnergizedSparkAir",         "Energized Spark: Air (Slashing)"),
            ModManager.RegisterFeatName("EnergizedSparkCold",        "Energized Spark: Cold"),
            ModManager.RegisterFeatName("EnergizedSparkEarth",       "Energized Spark: Earth (Bludgeoning)"),
            ModManager.RegisterFeatName("EnergizedSparkElectricity", "Energized Spark: Electricity"),
            ModManager.RegisterFeatName("EnergizedSparkFire",        "Energized Spark: Fire"),
            ModManager.RegisterFeatName("EnergizedSparkMetal",       "Energized Spark: Metal (Slashing)"),
            ModManager.RegisterFeatName("EnergizedSparkPoison",      "Energized Spark: Poison"),
            ModManager.RegisterFeatName("EnergizedSparkSonic",       "Energized Spark: Sonic"),
            // ModManager.RegisterFeatName("EnergizedSparkVitality",    "Energized Spark: Vitality"),
            // ModManager.RegisterFeatName("EnergizedSparkVoid",        "Energized Spark: Void"),
            ModManager.RegisterFeatName("EnergizedSparkWater",       "Energized Spark: Water (Bludgeoning)"),
            ModManager.RegisterFeatName("EnergizedSparkWood",        "Energized Spark: Wood (Piercing)")
        };

        // Map feats to DamageKind
        public static readonly Dictionary<FeatName, DamageKind> SparkAttunements = new()
        {
            { AttunementFeats[0], DamageKind.Slashing},
            { AttunementFeats[1], DamageKind.Cold },
            { AttunementFeats[2], DamageKind.Bludgeoning },
            { AttunementFeats[3], DamageKind.Electricity },
            { AttunementFeats[4], DamageKind.Fire },
            { AttunementFeats[5], DamageKind.Slashing },
            { AttunementFeats[6], DamageKind.Poison },
            { AttunementFeats[7], DamageKind.Sonic },
            // { AttunementFeats[8], DamageKind.Vitality },
            // { AttunementFeats[9], DamageKind.Void },
            { AttunementFeats[8], DamageKind.Bludgeoning },
            { AttunementFeats[9], DamageKind.Piercing }
        };

        public static readonly Dictionary<FeatName, string> AttunementDescriptions = new()
        {
            { AttunementFeats[0], "Makes your spirit damage slashing, as though carried on razor-sharp wind." },
            { AttunementFeats[1], "Makes your spirit damage cold, chilling and numbing your foe." },
            { AttunementFeats[2], "Makes your spirit damage bludgeoning, like a crushing earth strike." },
            { AttunementFeats[3], "Makes your spirit damage electricity, crackling through armor." },
            { AttunementFeats[4], "Makes your spirit damage fire, scorching whatever it hits." },
            { AttunementFeats[5], "Makes your spirit damage slashing, like a forged metal blade." },
            { AttunementFeats[6], "Makes your spirit damage poison, inflicting potent toxins." },
            { AttunementFeats[7], "Makes your spirit damage sonic, tearing with deafening force." },
            { AttunementFeats[8], "Makes your spirit damage bludgeoning, like a tidal water surge." },
            { AttunementFeats[9], "Makes your spirit damage piercing, like a sharpened wooden spear." }
        };
    }
}
