// file: Exemplar_SanctifiedSoul.cs
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using System.Collections.Generic;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_SanctifiedSoul
    {
        // 1) Dummy FeatNames for each attunement
        private static readonly FeatName[] AttunementFeats =
        {
            ModManager.RegisterFeatName("SanctifiedSoulHoly",         "Sanctified Soul: Holy"),
            ModManager.RegisterFeatName("SanctifiedSoulUnholy",        "Sanctified Soul: Unholy")
        };
        // Map feats to DamageKind
        private static readonly Dictionary<FeatName, DamageKind> SparkAttunements = new()
        {
            { AttunementFeats[0], DamageKind.Good},
            { AttunementFeats[1], DamageKind.Evil }
        };

        private static readonly Dictionary<FeatName, string> AttunementDescriptions = new()
        {
            { AttunementFeats[0], "Makes your spirit damage Holy, suffused with dawn's first light that cleaves through darkness." },
            { AttunementFeats[1], "Makes your spirit damage Unholy, as though borne on a brimstone-laced whisper." }
        };

        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var sanctifiedTrait = ModTraits.sanctifiedTrait;
            // Register attunement feats
            foreach (var fn in AttunementFeats)
            {
                var desc = AttunementDescriptions.TryGetValue(fn, out var d) ? d : "";
                ModManager.AddFeat(new TrueFeat(fn, 1, "", desc, new[] { sanctifiedTrait }, null));
            }

            var sanctifiedSoul = new TrueFeat(
                ExemplarFeatNames.FeatSanctifiedSoul,
                1,
                "Sanctified Soul",
                "You've drawn a line in the sand in the cosmic struggle between good and evil and chosen a side. " +
                "You gain either the holy trait or the unholy trait. All your exemplar abilities that deal spirit damage gain the sanctified trait, " +
                "allowing you to apply your chosen trait to better affect your enemies.",
                new[] { ExemplarBaseClass.TExemplar },
                null
            )
            .WithOnSheet(sheet =>
            {
                // Choose holy or unholy alignment for your spirit-damage abilities
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "SanctifiedSoulSide",
                        name: "Sanctified Soul: Side",
                        level: 1,
                        eligible: ft => AttunementFeats.Contains(ft.FeatName)
                    )
                );
            })
            .WithPermanentQEffect(null, qf =>
            {
                qf.Id = ExemplarIkonQEffectIds.QSanctifiedSoul;
                //store the selected kind in the effect's Key.
                var selected = AttunementFeats.FirstOrDefault(fn => qf.Owner.HasFeat(fn));
                if (SparkAttunements.TryGetValue(selected, out var kind))
                    qf.Key = kind.ToString();
                else
                    qf.Key = DamageKind.Untyped.ToString();
            });

            ModManager.AddFeat(sanctifiedSoul);
        }
    }
}
