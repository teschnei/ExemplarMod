using System;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Exemplar.Utilities;

namespace Dawnsbury.Mods.Exemplar
{
    /*
        TODO : 
        1. Test
        2. add +1 to perception.

    */
    public class Ikons_GazeSharpAsSteel
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var ikon = new TrueFeat(
                ExemplarFeatNames.IkonGazeSharpAsSteel,
                1,
                "Gaze Sharp As Steel",
                "{b}Immanence{/b} Your vision sharpens and allows you to sense an enemy's attack almost as soon as it begins, granting you a +1 status bonus to Perception checks and a +2 status bonus to your AC against ranged attacks.\n\n" +
                "{b}Transcendence — A Moment Unending (one-action){/b} Concentrate, Prediction, Transcendence\n" +
                "You take in every movement around you, affording you unparalleled accuracy. Your next successful Strike against an enemy before the end of your next turn deals an additional 1d6 precision damage (2d6 at 10th level, 3d6 at 18th).",
                new[] { ModTraits.Ikon , ModTraits.BodyIkon},
                null
            ).WithMultipleSelection()
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: Perception and AC vs ranged
                
                // qf.BonusToSkillChecks = (skill, eff, defense) =>
                //     eff.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredGazeSharpAsSteel) && skill == 
                //         ? new Bonus(1, BonusType.Status, "Gaze Sharp As Steel")
                //         : null;
                
                qf.BonusToDefenses = (eff, action, defense) =>
                    eff.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredGazeSharpAsSteel)
                    && action != null && action.HasTrait(Trait.Ranged)
                    && defense == Defense.AC
                        ? new Bonus(2, BonusType.Status, "Gaze Sharp As Steel")
                        : null;

                // Transcendence — A Moment Unending
                qf.ProvideMainAction = qf =>
                {
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredGazeSharpAsSteel))
                        return null;

                    var action = new CombatAction(
                        qf.Owner,
                        IllustrationName.RubEyes, // replace with a suitable eye‐focused icon
                        "A Moment Unending",
                        new[] { Trait.Concentrate, Trait.Prediction, ModTraits.Transcendence, ModTraits.Ikon },
                        "Your next successful Strike before end of turn deals extra precision damage.",
                        Target.Self()
                    ).WithActionCost(1);

                    action.WithEffectOnSelf(async (act, self) =>
                    {
                        // 1) Mark the buff
                        self.RemoveAllQEffects(q => q.Id == ExemplarIkonQEffectIds.QGazeMomentUnending);
                        self.AddQEffect(new QEffect("Moment Unending Buff", "Next Strike deals extra precision damage", ExpirationCondition.ExpiresAtStartOfYourTurn, self, IllustrationName.RubEyes)
                        {
                            Id = ExemplarIkonQEffectIds.QGazeMomentUnending,
                            AddExtraStrikeDamage = (strikeAction, defender) =>
                            {
                                if (!strikeAction.HasTrait(Trait.Strike)) 
                                    return null;
                                
                                DamageKind damageKind = DamageKindHelper.GetDamageKindFromEffect(self, ExemplarIkonQEffectIds.QEnergizedSpark);
                                int diceCount = self.Level >= 18 ? 3 : self.Level >= 10 ? 2 : 1;
                                return (DiceFormula.FromText($"{diceCount}d6", "Moment Unending"), damageKind);
                            }
                        });

                        // 2) Cleanup empowerment + free shift + exhaustion
                        self.RemoveAllQEffects(q => ExemplarIkonQEffectIds.EmpoweredIkonIds.Contains(q.Id));
                        self.AddQEffect(new QEffect("First Shift Free", "Your next Shift Immanence is free")
                        {
                            Id = ExemplarIkonQEffectIds.FirstShiftFree
                        });
                        self.AddQEffect(new QEffect("Spark exhaustion", "You cannot use another Transcendence this turn",
                            ExpirationCondition.ExpiresAtStartOfYourTurn, self, IllustrationName.Chaos)
                        {
                            Id = ExemplarIkonQEffectIds.TranscendenceTracker
                        });
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(ikon);
        }
    }
}
