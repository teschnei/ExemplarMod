using System.Threading.Tasks;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using System.Linq;
using System.Collections.Generic;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_EpithetDancerInTheSeasons
    {

        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var dancer = new TrueFeat(
                ExemplarFeatNames.EpithetDancerInTheSeasons,
                7,
                "Dancer in the Seasons",
                "You flourish in spring and idle in summer, give in fall and take in winter. You gain the Energized Spark feat for your choice of cold, fire, void, or wood. " +
                "When you critically succeed on a Strike, you can Step as a free action in a whirl of leaves, snow, blossoms, or shimmering heat. " +
                "The season changes, rotating each time you use this ability.\n\n" +
                "When you Spark Transcendence, you gain temporary Hit Points equal to half your level as you are reinvigorated by the changing of the seasons; " +
                "these temporary Hit Points last until the start of your next turn.",
                new[] { ModTraits.DominionEpithet },
                null
            )
            .WithOnSheet(sheet =>
            {

                var allowed = new HashSet<FeatName>
                    {
                        ExemplarFeatNames.AttunementFeats[1],
                        ExemplarFeatNames.AttunementFeats[4],//fire,
                        // ExemplarFeatNames.FeatEnergizedSparkVoid,
                        ExemplarFeatNames.AttunementFeats[9],//wood,
                    };
                    
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "Seasonal Spark Type",             // internal ID
                        name: "Choose your Seasonal Spark damage type", // UI label
                        level: 7,                                  // level
                        eligible: ft => ft.HasTrait(ModTraits.TEnergizedSpark)
                            && allowed.Contains(ft.FeatName)
                    )
                );
            })
            .WithPermanentQEffect(null, qf =>
            {
                qf.AfterYouTakeAction = async (selfQf, action) =>
                {
                    // Free Step on a critical Strike
                    if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess)
                    {
                        qf.ProvideMainAction = qf2 =>
                        {
                            return new ActionPossibility(
                                new CombatAction(
                                    qf2.Owner,
                                    IllustrationName.FreedomOfMovement,
                                    "Dancer in the Seasons: Step",
                                    new[] { ModTraits.Epithet },
                                    "Step as a free action in a whirl of leaves, snow, blossoms, or shimmering heat.",
                                    Target.Self()
                                ).WithActionCost(0).WithEffectOnSelf(async (act, caster) =>
                                {
                                    await selfQf.Owner.StrideAsync("Choose where you want to Step.", allowStep: true, maximumFiveFeet: true);
                                })
                            );
                        };
                    }

                    // Gain temporary HP on Transcendence
                    if (action.HasTrait(ModTraits.Transcendence))
                    {
                        int tempHP = selfQf.Owner.Level / 2;
                        // Ensure tempHP is at least 1
                        tempHP = tempHP < 1 ? 1 : tempHP;
                        // Add the temporary HP effect
                        selfQf.Owner.AddQEffect(new QEffect(
                            "Seasonal Renewal",
                            $"You gain {tempHP} temporary Hit Points as you are reinvigorated by the changing of the seasons.",
                            ExpirationCondition.ExpiresAtStartOfYourTurn,
                            selfQf.Owner
                        )).GainTemporaryHP(tempHP);
                    }
                };
            });

            ModManager.AddFeat(dancer);
        }
    }
}
