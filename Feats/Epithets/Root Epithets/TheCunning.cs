using System.Collections.Generic;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Root;

public class TheCunning
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        ModManager.RegisterActionOnEachActionPossibility(action =>
        {
            if (action.Owner.HasEffect(ExemplarQEffects.TheCunning) &&
                (action.ActionId == ActionId.CreateADiversion || action.ActionId == ActionId.Feint))
            {
                action.ActionCost = 0;
            }
        });

        yield return new Feat(
            ExemplarFeats.TheCunning,
            "Why race a hare across a meadow, or a salmon up a waterfall? Why face a titan in a test of strength? Wouldn't it be better to best your foes with a bit of creativity? " +
            "After all, the stories that echo throughout history are always those where wits and trickery, rather than raw talent or power, win the day.",
            "You are trained in Deception. After you Spark Transcendence, you can Create a Diversion or Feint as a free action.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Deception);
        })
        .WithPermanentQEffect("After you Spark Transcendence, you can Create a Diversion or Feint as a free action.", q =>
        {
            q.AfterYouTakeAction = async (q, action) =>
            {
                if (action.HasTrait(ExemplarTraits.Transcendence))
                {
                    q.Owner.AddQEffect(new QEffect()
                    {
                        Id = ExemplarQEffects.TheCunning,
                        AfterYouTakeAction = async (q, action) =>
                        {
                            q.ExpiresAt = ExpirationCondition.Immediately;
                        }
                    });
                }
            };
        });
    }
}
