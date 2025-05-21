using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_EpithetPeerlessUnderTheHeavens
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var peerless = new TrueFeat(
                ExemplarFeatNames.EpithetPeerlessUnderHeaven,
                7,
                "Peerless under Heaven",
                "For as long as there have been gods, they have made war, and you aim to stand atop the pile when the fighting's over. " +
                "When you critically succeed on a Strike, divine skill at arms guides your weapon, granting you the critical specialization effect for the weapon group. " +
                "If you already had access to the critical specialization effect for this weapon, your weapon gains the additional critical specialization effect of the grievous rune.\n\n" +
                "After you Spark Transcendence, your impeccable battle form strikes fear. An enemy of your choice within 30 feet must succeed at a Will save against your class DC or be frightened 1. " +
                "That creature is then temporarily immune to this effect for 10 minutes. This is an emotion, fear, mental, and visual effect.",
                new[] { ModTraits.DominionEpithet },
                null
            )
            .WithPermanentQEffect(null, qf =>
            {
                //maybe this works?
                qf.YouHaveCriticalSpecialization = (selfQf, item, action, defender) =>
                {
                    return true;
                };
                qf.AfterYouTakeAction = async (selfQf, action) =>
                {
                    // Critical Specialization on a critical Strike
                    if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess)
                    {
                        // TODO: implement granting of critical specialization effect for this weapon group
                        // CommonAbilityEffects.CriticalSpecializationEffect(action);
                    }

                    // Frighten effect on Transcendence
                    if (action.HasTrait(ModTraits.Transcendence))
                    {
                        qf.ProvideMainAction = qf2 =>
                        {
                            var fear = new CombatAction(
                                qf2.Owner,
                                IllustrationName.Fear,
                                "Peerless under Heaven: Strike Fear",
                                new[] { ModTraits.Epithet, ModTraits.Transcendence },
                                "An enemy within 30 feet must succeed at a Will save against your class DC or be frightened 1 (immune for 10 minutes).",
                                Target.Distance(30)
                            )
                            .WithActionCost(0)
                            .WithSavingThrow(new SavingThrow(Defense.Will, qf2.Owner.ClassOrSpellDC()))
                            .WithEffectOnEachTarget(async (act, caster, target, result) =>
                            {
                                if (result == CheckResult.CriticalFailure || result == CheckResult.Failure)
                                {
                                    // Prevent re-targeting the same creature
                                    if (target.HasEffect(ExemplarIkonQEffectIds.QTheMournfulImmune))
                                    {
                                        target.Occupies.Overhead("They are already immune.", Color.Orange);
                                        return;
                                    }

                                    // Apply frightened 1 with 10-minute immunity
                                    target.AddQEffect(QEffect.Frightened(1));

                                    //apply 10 minute immunity. (100 counts of 6s)
                                    target.AddQEffect(new QEffect(
                                        "Immune to Peerless under Heaven: Strike Fear",
                                        "You are immune to the Peerless under Heaven: Strike Fear effect.",
                                        ExpirationCondition.CountsDownAtEndOfYourTurn,
                                        caster,
                                        IllustrationName.Protection
                                    )
                                    {
                                        Id = ExemplarIkonQEffectIds.QPeerlessUnderHeavenImmune,
                                        Value = 100
                                    });
                                }
                            });

                            //cleanup empowerment and track exhaustion.


                            return new ActionPossibility(fear);
                        };
                    }
                };
            });

            ModManager.AddFeat(peerless);
        }
    }
}
