using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Root;

public class TheBrave
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Epithet(
            ExemplarFeats.TheBrave,
            "Your deeds show fearlessness: when a beast surfaces, you're there to fight it; when someone's lost in the dark, you're first to the rescue. ",
            "You're trained in Athletics. After you Spark Transcendence, your body carries you forward, allowing you to Stride up to half your Speed in a straight line toward one enemy of your choice as a free action. " +
                "Once you have used this ability on a given enemy, you can't use it against that enemy again for 10 minutes.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithTranscendPossibility("After you Spark Transcendence, your body carries you forward, allowing you to Stride up to half your Speed in a straight line toward one enemy of your choice as a free action. " +
            "Once you have used this ability on a given enemy, you can't use it against that enemy again for 10 minutes.", (exemplar, action) =>
            new ActionPossibility(new CombatAction(exemplar, IllustrationName.WarpStep, "The Brave", [],
                    "Your body carries you forward, allowing you to Stride up to half your Speed in a straight line toward one enemy of your choice. " +
                    "Once you have used this ability on a given enemy, you can't use it against that enemy again for 10 minutes.",
                    Target.Ranged(100).WithAdditionalConditionOnTargetCreature((user, cr) => cr.HasEffect(ExemplarQEffects.TheBraveUsedOnTarget) ? Usability.NotUsableOnThisCreature("already targeted once") : Usability.Usable))
                .WithActionCost(0)
                .WithEffectOnEachTarget(async (action, self, target, _) =>
                {
                    if (await self.StrideAsync("Choose a square to Stride to.", false, false, target.Occupies, true, false, true))
                    {
                        target.AddQEffect(new QEffect()
                        {
                            Id = ExemplarQEffects.TheBraveUsedOnTarget
                        });
                    }
                    else
                    {
                        action.RevertRequested = true;
                    }
                })
            )
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Athletics);
        });
    }
}
