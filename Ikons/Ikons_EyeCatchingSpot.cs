using System.Linq;
using System.Numerics;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_EyeCatchingSpot
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {

            /*
                Currently doesn't seem to do anything, need to have a better way to give a -1 to melee attacks.
                commented out.
            */
            Feat eyeSpot = new TrueFeat(
                ExemplarFeatNames.IkonEyeCatchingSpot,
                1,
                "Eye-Catching Spot",
                "A fetching beauty spot under an eye or a smile as warm as the sun distracts foes and captures hearts alike.\n\n" +
                "{b}Immanence{/b} Your beauty becomes supernaturally enhanced, distracting foes and imposing a –1 circumstance penalty to melee attack rolls against you.\n\n" +
                "{b}Transcendence — Captivating Charm (two-actions){/b} Concentrate, Emotion, Mental, Transcendence, Visual\n" +
                "You focus your attention on a creature within 30 feet, overwhelming its senses. The creature must succeed at a Will save against your class DC or be fascinated by you until the start of your next turn. The condition ends if you use a hostile action against the target, but not if you use one against its allies.",
                [ModTraits.Ikon],
                null
            )
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: impose –1 circumstance penalty to melee attacks against you
                qf.StateCheck = effect =>
                {
                    // This QEffect is always active when empowered:
                    if (!effect.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredEyeCatchingSpot))
                        return;

                    // Attach a temporary QEffect to any enemy making a melee Strike against you.
                    foreach (var foe in effect.Owner.Battle.AllCreatures.Where(c => c.OwningFaction != effect.Owner.OwningFaction))
                    {
                        if (foe.HasEffect(ExemplarIkonQEffectIds.QEmpoweredEyeCatchingSpot))
                            continue;

                        foe.AddQEffect(new QEffect("Eye Catching Spot", "You are disctracted", ExpirationCondition.Ephemeral, effect.Owner , IllustrationName.RubEyes)
                        {
                            Id = ExemplarIkonQEffectIds.QEmpoweredEyeCatchingSpot,
                            BonusToAttackRolls = (q, action, target) =>
                                action.HasTrait(Trait.Attack) && target == effect.Owner
                                    ? new Bonus(-1, BonusType.Circumstance, "Eye-Catching Spot")
                                    : null
                        });
                    }
                };

                // Transcendence: Captivating Charm
                qf.ProvideMainAction = qf =>
                {
                    if (!qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredEyeCatchingSpot))
                        return null;

                    var action = new CombatAction(
                        qf.Owner,
                        IllustrationName.FearsomeRunestone,
                        "Captivating Charm",
                        new[] { Trait.Concentrate, Trait.Emotion, Trait.Mental, ModTraits.Transcendence, Trait.Visual, ModTraits.Ikon },
                        "Overwhelm a foe's senses. Will save or become fascinated until your next turn.",
                        Target.Ranged(30)
                    )
                    .WithActionCost(2)
                    // Use the built-in saving‐throw helper:
                    .WithSavingThrow(new SavingThrow(
                        Defense.Will, qf.Owner.ClassOrSpellDC()
                    ))
                    .WithEffectOnEachTarget(async (act, caster, target, result) =>
                    {
                        if (result.ToString() == "critical success" || result.ToString() == "success")
                        {
                            target.AddQEffect(new QEffect("Eye Catching Spot", "You are disctracted", ExpirationCondition.Ephemeral, qf.Owner , IllustrationName.RubEyes)
                            {
                                Id = ExemplarIkonQEffectIds.QEmpoweredEyeCatchingSpot,
                                BonusToAttackRolls = (q, action, target) =>
                                    action.HasTrait(Trait.Attack)
                                        ? new Bonus(-1, BonusType.Circumstance, "Eye-Catching Spot")
                                        : null
                            });
                        }

                        // After you cast:
                        caster.RemoveAllQEffects(qx => ExemplarIkonQEffectIds.EmpoweredIkonIds.Contains(qx.Id));
                        caster.AddQEffect(new QEffect("First Shift Free", "Your next Shift Immanence is free")
                        {
                            Id = ExemplarIkonQEffectIds.FirstShiftFree
                        });
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(eyeSpot);
        }
    }
}
