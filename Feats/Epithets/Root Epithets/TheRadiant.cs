using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class TheRadiant
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Feat(
            ExemplarFeats.TheRadiant,
                "Leaders must live bigger lives than any other, shining so brightly that they attract followers, inspire troops, and change the course of kingdoms.",
                "You are trained in Diplomacy. After you Spark Transcendence, you inspire an ally within 30 feet, restoring Hit Points equal to 2 + double your level; this is a mental and emotion effect. " +
                "The ally is then immune to this effect for 10 minutes.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Diplomacy);
        })
        .WithPermanentQEffect("After you Spark Transcendence, you inspire an ally within 30 feet, restoring Hit Points equal to 2 + double your level; this is a mental and emotion effect. " +
            "The ally is then immune to this effect for 10 minutes.", q =>
        {
            q.AfterYouTakeAction = async (q, action) =>
            {
                if (action.ActionId == ExemplarActions.SparkTranscendence)
                {
                    var target = await q.Owner.Battle.AskToChooseACreature(q.Owner,
                        q.Owner.Battle.AllCreatures.Where(cr => cr.FriendOf(q.Owner) && cr.DistanceTo(q.Owner) <= 6 && !cr.IsImmuneTo(Trait.Mental) && !cr.IsImmuneTo(Trait.Emotion) && !cr.HasEffect(ExemplarQEffects.TheRadiantUsedOnTarget)),
                        IllustrationName.QuestionMark, $"The Radiant: Choose a creature to heal {2 + (q.Owner.Level * 2)} HP.", $"Choose this creature to heal {2 + (q.Owner.Level * 2)} HP.", "Don't heal anyone");
                    if (target != null)
                    {
                        await target.HealAsync($"{2 + (q.Owner.Level * 2)}", CombatAction.CreateSimple(q.Owner, "The Radiant", [Trait.Emotion, Trait.Mental]));
                    }
                }
            };
        });
    }
}
