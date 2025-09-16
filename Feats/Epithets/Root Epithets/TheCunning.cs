using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Root;

public class TheCunning
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Epithet(
            ExemplarFeats.TheCunning,
            "Why race a hare across a meadow, or a salmon up a waterfall? Why face a titan in a test of strength? Wouldn't it be better to best your foes with a bit of creativity? " +
            "After all, the stories that echo throughout history are always those where wits and trickery, rather than raw talent or power, win the day.",
            "You are trained in Deception. After you Spark Transcendence, you can Create a Diversion or Feint as a free action.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithTranscendPossibility("After you Spark Transcendence, you can Create a Diversion or Feint as a free action.", (exemplar, action) =>
            new SubmenuPossibility(IllustrationName.Feint, "The Cunning")
            {
                Subsections =
                [
                    new PossibilitySection("The Cunning")
                    {
                        Possibilities = [new ActionPossibility(CommonCombatActions.CreateADiversion(exemplar).WithActionCost(0)),
                                         new ActionPossibility(CombatManeuverPossibilities.CreateFeintAction(exemplar).WithActionCost(0))]
                    }
                ]
            }
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Deception);
        });
    }
}
