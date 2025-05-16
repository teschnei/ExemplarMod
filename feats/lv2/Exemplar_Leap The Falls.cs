using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_LeapTheFalls
    {

        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var leap = new TrueFeat(
                ExemplarFeatNames.FeatLeapTheFalls,
                2,
                "Leap The Falls",
                "{b}Immanence{/b} You imbue a body Ikon, and then you gain the Powerful Leap and [Quick jump not implemented] Quick Jump skill feats, allowing you to jump further and faster, even if you do not meet the prerequisites for them.",
                new[] { ExemplarBaseClass.TExemplar },
                null
            )
            .WithOnSheet(sheet =>
            {
                //list of all feats that have the Ikon trait
                var ikonFeats = sheet.AllFeats
                    .Where(f => f.Traits.Contains(ModTraits.Ikon))
                    .ToList();
                // 1) Add the feat to the character sheet
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "LeapTheFalls",
                        name: "Leap The Falls",
                        level: 2,
                        eligible: ft => ikonFeats.Contains(ft) && ft.Traits.Contains(ModTraits.Ikon) &&
                            ft.FeatName != ExemplarFeatNames.FeatLeapTheFalls && 
                            ft.HasTrait(ModTraits.BodyIkon)
                    )
                );
            })
            .WithOnSheet(sheet =>
            {
                // 2) Add the two skill feats to the character sheet
                if (!sheet.HasFeat(FeatName.PowerfulLeap))
                    sheet.GrantFeat(FeatName.PowerfulLeap);
                //current quickjump does not exist.
            });
            ModManager.AddFeat(leap);
        }
    }
}
