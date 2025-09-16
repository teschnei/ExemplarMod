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

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Dominion;

public class PeerlessUnderHeaven
{
    [FeatGenerator(7)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Epithet(
            ExemplarFeats.PeerlessUnderHeaven,
                "For as long as there have been gods, they have made war, and you aim to stand atop the pile when the fighting's over.",
                "When you critically succeed on a Strike, divine skill at arms guides your weapon, granting you the critical specialization effect for the weapon group. " +
                //"If you already had access to the critical specialization effect for this weapon, your weapon gains the additional critical specialization effect of the grievous rune.
                "\n\n" +
                "After you Spark Transcendence, your impeccable battle form strikes fear. An enemy of your choice within 30 feet must succeed at a Will save against your class DC or be frightened 1. " +
                "That creature is then immune to this effect. This is an emotion, fear, mental, and visual effect.",
            [ExemplarTraits.DominionEpithet],
            null
        )
        .WithTranscendPossibility("After you Spark Transcendence, your impeccable battle form strikes fear. An enemy of your choice within 30 feet must succeed at a Will save against your class DC or be frightened 1. " +
                "That creature is then immune to this effect. This is an emotion, fear, mental, and visual effect.", (exemplar, action) =>
            new ActionPossibility(new CombatAction(exemplar, IllustrationName.Demoralize, "Peerless Under Heaven", [Trait.Emotion, Trait.Fear, Trait.Mental, Trait.Visual],
                "Your impeccable battle form strikes fear. An enemy of your choice within 30 feet must succeed at a Will save against your class DC or be frightened 1. " +
                "That creature is then immune to this effect.",
                Target.Ranged(6).WithAdditionalConditionOnTargetCreature((self, target) =>
                    target.HasEffect(ExemplarQEffects.PeerlessUnderHeavenUsedOnTarget) ? Usability.NotUsableOnThisCreature("already affected once") : Usability.Usable
                ))
                .WithActionCost(0)
                .WithSavingThrow(new SavingThrow(Defense.Will, exemplar.ClassDC()))
                .WithEffectOnEachTarget(async (action, self, target, result) =>
                {
                    if (result <= CheckResult.Failure)
                    {
                        target.AddQEffect(QEffect.Frightened(1));
                    }
                    target.AddQEffect(new QEffect()
                    {
                        Id = ExemplarQEffects.PeerlessUnderHeavenUsedOnTarget
                    });
                })
            )
        )
        .WithPermanentQEffect(null, q =>
        {
            //TODO: after someone creates the grievous rune: loop over YouHaveCriticalSpecialization (skipping itself) to determine if crit. spec. was already granted, and temporarily grant the grievous rune if so
            q.YouHaveCriticalSpecialization = (_, _, _, _) => true;
        });
    }
}
