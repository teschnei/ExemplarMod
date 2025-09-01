using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class TheProud
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Feat(
            ExemplarFeats.TheProud,
                "Whether out of overconfidence, a desire to protect your comrades, or the unslakable thirst for glory, you invite challengers to strike you down.",
                "You are trained in Intimidation. After you Spark Transcendence, you can boast to one enemy within 6 squares (30 ft) to draw its attention; " +
                "this effect has the auditory, emotion, mental, and linguistic traits. Until the start of your next turn, the target takes a –1 status penalty " +
                "to attack rolls, damage rolls, and skill checks against creatures other than you, and it gains a +1 status bonus to these rolls when targeting you.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Intimidation);
        })
        .WithPermanentQEffect("After you Spark Transcendence, you can boast to one enemy within 30 feet to draw its attention; " +
                "this effect has the auditory, emotion, mental, and linguistic traits. Until the start of your next turn, the target takes a –1 status penalty " +
                "to attack rolls, damage rolls, and skill checks against creatures other than you, and it gains a +1 status bonus to these rolls when targeting you.",
                q =>
        {
            q.AfterYouTakeAction = async (q, action) =>
            {
                if (action.ActionId == ExemplarActions.SparkTranscendence)
                {
                    var target = await q.Owner.Battle.AskToChooseACreature(q.Owner,
                        q.Owner.Battle.AllCreatures.Where(cr => cr.EnemyOf(q.Owner) && cr.DistanceTo(q.Owner) <= 6 && !cr.IsImmuneTo(Trait.Auditory) && !cr.IsImmuneTo(Trait.Mental) && !cr.IsImmuneTo(Trait.Emotion)),
                        IllustrationName.QuestionMark, "The Proud: Choose a creature to boast to.", "Choose this creature to boast to.", "Don't boast to anyone");
                    if (target != null)
                    {
                        target.AddQEffect(new QEffect("The Proud",
                                    "You receive a bonus to attacks, damage, and skill checks against ${}, but a penalty to attacks, damage, and skill checks against anyone else.",
                                    ExpirationCondition.CountsDownAtStartOfSourcesTurn, q.Owner, IllustrationName.Rage)
                        {
                            CountsAsADebuff = true,
                            BonusToAttackRolls = (_, _, target) => new Bonus(target == q.Owner ? 1 : -1, BonusType.Status, "The Proud", false),
                            BonusToSkillChecks = (_, _, target) => new Bonus(target == q.Owner ? 1 : -1, BonusType.Status, "The Proud", false),
                            BonusToDamage = (_, _, target) => new Bonus(target == q.Owner ? 1 : -1, BonusType.Status, "The Proud", false)
                        });
                    }
                }
            };
        });
    }
}
