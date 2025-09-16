using System.Linq;
using System.Collections.Generic;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Targeting;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Dominion;

public class DancerInTheSeasons
{
    [FeatGenerator(7)]
    public static IEnumerable<Feat> GetFeat()
    {
        List<FeatName> sparkOptions = [ExemplarFeats.EnergizedSparkCold, ExemplarFeats.EnergizedSparkFire, ExemplarFeats.EnergizedSparkVoid, ExemplarFeats.EnergizedSparkWood];
        yield return new Epithet(
            ExemplarFeats.DancerInTheSeasons,
                "You flourish in spring and idle in summer, give in fall and take in winter.",
                "You gain the Energized Spark feat for your choice of cold, fire, void, or wood. " +
                "When you critically succeed on a Strike, you can Step as a free action in a whirl of leaves, snow, blossoms, or shimmering heat. " +
                "The season changes, rotating each time you use this ability.\n\n" +
                "When you Spark Transcendence, you gain temporary Hit Points equal to half your level.",
            [ExemplarTraits.DominionEpithet],
            sparkOptions.Select(spark => new Feat(spark, "", "", [], null)
                .WithEquivalent(sheet => sheet.HasFeat(spark))
                .WithOnSheet(sheet =>
                {
                    sheet.GrantFeat(ExemplarFeats.EnergizedSpark, spark);
                })).ToList()
        )
        .WithTranscendPossibility("When you Spark Transcendence, you gain temporary Hit Points equal to half your level.", (exemplar, action) =>
            new ActionPossibility(new CombatAction(exemplar, IllustrationName.ResistEnergy, "Dancer In The Seasons", [],
                    $"You gain {exemplar.Level / 2} temporary Hit Points.",
                    Target.Self())
                .WithActionCost(0)
                .WithEffectOnEachTarget(async (action, self, target, _) =>
                {
                    int tempHP = exemplar.Level / 2;
                    // Add the temporary HP effect
                    exemplar.GainTemporaryHP(tempHP);
                })
            )
        )
        .WithPermanentQEffect(null, q =>
        {
            q.AfterYouTakeAction = async (selfQf, action) =>
            {
                // Free Step on a critical Strike
                if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess)
                {
                    await q.Owner.StrideAsync("Dancer in the Seasons: Choose where to Step", true, true, allowPass: true);
                }
            };
        });
    }
}
