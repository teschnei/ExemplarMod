using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Root;

public class TheProud
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Epithet(
            ExemplarFeats.TheProud,
                "Whether out of overconfidence, a desire to protect your comrades, or the unslakable thirst for glory, you invite challengers to strike you down.",
                "You are trained in Intimidation. After you Spark Transcendence, you can boast to one enemy within 6 squares (30 ft) to draw its attention; " +
                "this effect has the auditory, emotion, mental, and linguistic traits. Until the start of your next turn, the target takes a –1 status penalty " +
                "to attack rolls, damage rolls, and skill checks against creatures other than you, and it gains a +1 status bonus to these rolls when targeting you.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithTranscendPossibility("After you Spark Transcendence, you can boast to one enemy within 6 squares (30 ft) to draw its attention; " +
                "this effect has the auditory, emotion, mental, and linguistic traits. Until the start of your next turn, the target takes a –1 status penalty " +
                "to attack rolls, damage rolls, and skill checks against creatures other than you, and it gains a +1 status bonus to these rolls when targeting you.", (exemplar, action) =>
            new ActionPossibility(new CombatAction(exemplar, IllustrationName.Demoralize, "The Proud", [Trait.Auditory, Trait.Emotion, Trait.Mental, Trait.Linguistic],
                "You boast to one enemy within 6 squares (30 ft) to draw its attention; " +
                "this effect has the auditory, emotion, mental, and linguistic traits. Until the start of your next turn, the target takes a –1 status penalty " +
                "to attack rolls, damage rolls, and skill checks against creatures other than you, and it gains a +1 status bonus to these rolls when targeting you.",
                Target.Ranged(6))
                .WithActionCost(0)
                .WithEffectOnEachTarget(async (action, self, target, _) =>
                {
                    target.AddQEffect(new QEffect("The Proud",
                                "You receive a bonus to attacks, damage, and skill checks against ${}, but a penalty to attacks, damage, and skill checks against anyone else.",
                                ExpirationCondition.CountsDownAtStartOfSourcesTurn, self, IllustrationName.Rage)
                    {
                        CountsAsADebuff = true,
                        BonusToAttackRolls = (_, _, target) => new Bonus(target == self ? 1 : -1, BonusType.Status, "The Proud", false),
                        BonusToSkillChecks = (_, _, target) => new Bonus(target == self ? 1 : -1, BonusType.Status, "The Proud", false),
                        BonusToDamage = (_, _, target) => new Bonus(target == self ? 1 : -1, BonusType.Status, "The Proud", false)
                    });
                })
            )
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Intimidation);
        });
    }
}
