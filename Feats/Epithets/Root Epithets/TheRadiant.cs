using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Root;

public class TheRadiant
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Epithet(
            ExemplarFeats.TheRadiant,
                "Leaders must live bigger lives than any other, shining so brightly that they attract followers, inspire troops, and change the course of kingdoms.",
                "You are trained in Diplomacy. After you Spark Transcendence, you inspire an ally within 30 feet, restoring Hit Points equal to 2 + double your level; this is a mental and emotion effect. " +
                "The ally is then immune to this effect for 10 minutes.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithTranscendPossibility("After you Spark Transcendence, you inspire an ally within 30 feet, restoring Hit Points equal to 2 + double your level; this is a mental and emotion effect. " +
                "The ally is then immune to this effect for 10 minutes.", (exemplar, action) =>
            new ActionPossibility(new CombatAction(exemplar, IllustrationName.Heal, "The Radiant", [Trait.Emotion, Trait.Mental],
                $"You inspire an ally within 30 feet, restoring {2 + (exemplar.Level * 2)} Hit Points. " +
                "The ally is then immune to this effect for 10 minutes.",
                //RangedFriend, but not self
                new CreatureTarget(RangeKind.Ranged, [
                    new MaximumRangeCreatureTargetingRequirement(6),
                    new FriendCreatureTargetingRequirement(),
                    new UnblockedLineOfEffectCreatureTargetingRequirement(),
                    new LegacyCreatureTargetingRequirement((self, target) => target.HasEffect(ExemplarQEffects.TheRadiantUsedOnTarget) ? Usability.NotUsableOnThisCreature("already targeted once") : Usability.Usable)
                ], (_, _, _) => float.MinValue))
                .WithActionCost(0)
                .WithEffectOnEachTarget(async (action, self, target, _) =>
                {
                    await target.HealAsync($"{2 + (self.Level * 2)}", CombatAction.CreateSimple(self, "The Radiant", [Trait.Emotion, Trait.Mental]));
                })
            )
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Diplomacy);
        });
    }
}
