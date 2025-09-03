using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class EyeCatchingSpot
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Ikon(new Feat(
            ExemplarFeats.EyeCatchingSpot,
            "A fetching beauty spot under an eye or a smile as warm as the sun distracts foes and captures hearts alike.",
            "{b}Usage{/b} imbued on the face\n\n" +
            "{b}Immanence{/b} (mental, visual) Your beauty becomes supernaturally enhanced, distracting foes and imposing a –1 circumstance penalty to melee attack rolls against you.\n\n" +
            $"{{b}}Transcendence — Captivating Charm {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (concentrate, emotion, mental, transcendence, visual)\n" +
            "You focus your attention on a creature within 30 feet, overwhelming its senses. The creature must succeed at a Will save against your class DC or be fascinated by you until the start of your next turn. The condition ends if you use a hostile action against the target, but not if you use one against its allies.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonBody],
            null
        ).WithIllustration(IllustrationName.Blinded), q =>
        {
            q.AddGrantingOfTechnical(cr => cr.EnemyOf(q.Owner), qe =>
            {
                qe.BonusToAttackRolls = (qe, action, target) => action.HasTrait(Trait.Attack) && action.HasTrait(Trait.Melee) && target == q.Owner ? new Bonus(-1, BonusType.Circumstance, "Eye-Catching Spot") : null;
            });
        }, q =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                IllustrationName.DarkHeart,
                "Captivating Charm",
                [Trait.Concentrate, Trait.Emotion, Trait.Mental, Trait.Visual, ExemplarTraits.Transcendence],
                "You focus your attention on a creature within 30 feet, overwhelming its senses. The creature must succeed at a Will save against your class DC or be fascinated by you until the start of your next turn. The condition ends if you use a hostile action against the target, but not if you use one against its allies.",
                Target.Ranged(6)
            ).WithActionCost(2)
            .WithSavingThrow(new SavingThrow(Defense.Will, q.Owner.ClassDC()))
            .WithEffectOnEachTarget(async (action, caster, target, result) =>
            {
                if (result < CheckResult.Success)
                {
                    target.AddQEffect(new QEffect("Fascinated",
                        "You are fascinated and take a -2 status penalty to Perception and skill checks, and you can't use concentrate actions unless they (or their intended consequence) are related to the subject of your fascination.",
                        ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                        caster,
                        IllustrationName.Seek)
                    {
                        Tag = caster,
                        BonusToSkillChecks = (skill, action, target) => new Bonus(-2, BonusType.Status, "Fascinated"),
                        BonusToPerception = q => new Bonus(-2, BonusType.Status, "Fascinated"),
                        AfterYouTakeHostileAction = (q, action) => q.ExpiresAt = ExpirationCondition.Immediately
                    }.AddGrantingOfTechnical((q, cr) => cr.EnemyOf(q.Owner), q =>
                    {
                        q.PreventTargetingBy = action => action.Owner == target && q.Owner != caster && action.HasTrait(Trait.Concentrate) ? "You can only concentrate on the subject of your fascination." : null;
                    }));
                }
            }));
        }).IkonFeat;
    }
}

