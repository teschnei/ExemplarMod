using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;

namespace Dawnsbury.Mods.Classes.Exemplar
{
    public class Exemplar_AdditionalIkon
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var additionalIkon = new TrueFeat(
                ExemplarFeats.AdditionalIkon,
                8,
                "Additional Ikon",
                "Your story has grown rich enough that three ikons can't contain its full complexity. " +
                "You gain a fourth ikon, which can be of any type.",
                [ExemplarTraits.Exemplar],
                null
            )
            .WithOnSheet(sheet =>
            {
                // Let the player pick one more Ikon feat
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "AdditionalIkon",
                        name: "Additional Ikon",
                        level: 8,
                        eligible: ft => ft.HasTrait(ExemplarTraits.Ikon)
                    )
                );
            });

            ModManager.AddFeat(additionalIkon);
        }
    }
}
