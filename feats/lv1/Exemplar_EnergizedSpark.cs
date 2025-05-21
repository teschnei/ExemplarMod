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


        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var sparkTrait = ModTraits.TEnergizedSpark;
            // Register attunement feats
            foreach (var fn in ExemplarFeatNames.AttunementFeats)
            {
                var desc = ExemplarFeatNames.AttunementDescriptions.TryGetValue(fn, out var d) ? d : "";
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
                        eligible: ft => ExemplarFeatNames.AttunementFeats.Contains(ft.FeatName)
                    )
                );
            })
            // Tag the chosen type on a persistent QEffect
            .WithPermanentQEffect(null, qf =>
            {
                qf.Id = ExemplarIkonQEffectIds.QEnergizedSpark;
                // store the selected kind in the effect's Key
                var selected = ExemplarFeatNames.AttunementFeats.FirstOrDefault(fn => qf.Owner.HasFeat(fn));
                if (ExemplarFeatNames.SparkAttunements.TryGetValue(selected, out var kind))
                    qf.Key = kind.ToString();
                else
                    qf.Key = DamageKind.Untyped.ToString();
            });

            ModManager.AddFeat(spark);
        }
    }
}
