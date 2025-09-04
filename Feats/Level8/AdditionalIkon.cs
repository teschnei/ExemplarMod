using System.Collections.Generic;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class AdditionalIkon
{
    [FeatGenerator(6)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new TrueFeat(
            ExemplarFeats.AdditionalIkon,
            8,
            "Your story has grown rich enough that three ikons can't contain its full complexity.",
            "You gain a fourth ikon, which can be of any type.",
            [ExemplarTraits.Exemplar],
            null
        )
        .WithOnSheet(sheet =>
        {
            // Let the player pick one more Ikon feat
            sheet.AddSelectionOptionRightNow(
                new SingleFeatSelectionOption(
                    key: "AdditionalIkon",
                    name: "Additional Ikon",
                    level: 8,
                    eligible: ft => ft.HasTrait(ExemplarTraits.Ikon)
                )
            );
        });
    }
}
