using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_EnergizedSpark
    {
        // 1) Dummy FeatNames for each attunement
        private static readonly FeatName[] AttunementFeats =
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
        private static readonly Dictionary<FeatName, DamageKind> SparkAttunements = new()
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

        private static readonly Dictionary<FeatName, string> AttunementDescriptions = new()
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

        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var sparkTrait = ModTraits.TEnergizedSpark;
            // Register attunement feats
            foreach (var fn in AttunementFeats)
            {
                var desc    = AttunementDescriptions.TryGetValue(fn, out var d) ? d : "";
                ModManager.AddFeat(new TrueFeat(fn, 1, "", desc, new[] { sparkTrait }, null));
            }

            // Define the Spark feat
            var spark = new TrueFeat(
                ExemplarFeatNames.FeatEnergizedSpark,
                1,
                "Energized Spark",
                "{b}Feat 1, Exemplar{/b}\nThe energy of your spirit manifests as crackling lightning, the chill of winter, or the power of an element. " +
                "Choose one of the following traits when you select this feat and attune that damage type for any spirit damage you deal.\n\n" +
                "{i}Special{/i} You can select this feat multiple times, choosing a different damage type each time.",
                new[] { ExemplarBaseClass.TExemplar, sparkTrait },
                null)
            .WithOnSheet(sheet =>
            {
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "EnergizedSpark:Attunement",
                        name: "Energized Spark Damage Type",
                        level: 1,
                        eligible: ft => AttunementFeats.Contains(ft.FeatName)
                    )
                );
            })
            // Tag the chosen type on a persistent QEffect
            .WithPermanentQEffect(null, qf =>
            {
                qf.Id = ExemplarIkonQEffectIds.QEnergizedSpark;
                // store the selected kind in the effect's Key
                var selected = AttunementFeats.FirstOrDefault(fn => qf.Owner.HasFeat(fn));
                if (SparkAttunements.TryGetValue(selected, out var kind))
                    qf.Key = kind.ToString();
                else
                    qf.Key = DamageKind.Untyped.ToString();
            });

            ModManager.AddFeat(spark);
        }
    }
}
