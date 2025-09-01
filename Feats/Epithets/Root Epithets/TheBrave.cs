using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class TheBrave
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Feat(
            ExemplarFeats.TheBrave,
            "Your deeds show fearlessness: when a beast surfaces, you're there to fight it; when someone's lost in the dark, you're first to the rescue. ",
            "You're trained in Athletics. After you Spark Transcendence, your body carries you forward, allowing you to Stride up to half your Speed in a straight line toward one enemy of your choice as a free action. " +
                "Once you have used this ability on a given enemy, you can't use it against that enemy again for 10 minutes.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Athletics);
        })
        .WithPermanentQEffect("After you Spark Transcendence, your body carries you forward, allowing you to Stride up to half your Speed in a straight line toward one enemy of your choice as a free action. " +
            "Once you have used this ability on a given enemy, you can't use it against that enemy again for 10 minutes.", q =>
        {
            q.AfterYouTakeAction = async (q, action) =>
            {
                if (action.ActionId == ExemplarActions.SparkTranscendence)
                {
                    var target = await q.Owner.Battle.AskToChooseACreature(q.Owner,
                            q.Owner.Battle.AllCreatures.Where(cr => cr.EnemyOf(q.Owner) && q.Owner.Occupies.HasLineOfEffectToIgnoreLesser(cr.Occupies) != CoverKind.Blocked &&
                            !cr.HasEffect(ExemplarQEffects.TheBraveUsedOnTarget)), IllustrationName.QuestionMark, "Choose a target to Stride towards.", "Stride towards this creature.", "Pass");
                    if (target != null)
                    {
                        if (await q.Owner.StrideAsync("Choose a square to Stride to.", false, false, target.Occupies, true, false, true))
                        {
                            target.AddQEffect(new QEffect()
                            {
                                Id = ExemplarQEffects.TheBraveUsedOnTarget
                            });
                        }
                    }
                }
            };
        });
    }
}
