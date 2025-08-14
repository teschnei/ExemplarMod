// file: Exemplar_EpithetTheRadiant.cs
using Microsoft.Xna.Framework;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.Mechanics;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_EpithetTheRadiant
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var theRadiant = new TrueFeat(
                ExemplarFeatNames.EpithetTheRadiant,
                3,
                "The Radiant",
                "Leaders must live bigger lives than any other, shining so brightly that they attract followers, inspire troops, and change the course of kingdoms. " +
                "You are trained in Diplomacy. After you Spark Transcendence, you inspire an ally within 6 squares (30 ft), restoring Hit Points equal to 2 + double your level; this is a mental and emotion effect. " +
                "The ally is then immune to this effect for 10 minutes.",
                new[] { ModTraits.RootEpithet },
                null
            )
            .WithOnSheet(sheet =>
            {
                // Grant training in Diplomacy
                sheet.GrantFeat(FeatName.Diplomacy);
            })
            .WithPermanentQEffect(null, qf =>
            {
                qf.AfterYouTakeAction = async (selfQf, action) =>
                {
                    // Only trigger once per turn, after any Transcendence use
                    if (!action.HasTrait(ModTraits.Transcendence)
                        || selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QTheRadiantUsedThisTurn))
                        return;

                    qf.ProvideMainAction = qf2 =>
                    {
                        //prevent from using twice in a turn.
                        if (selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QTheRadiantUsedThisTurn))
                            return null;
                        // Build the free Inspire action
                        var inspire = new CombatAction(
                            qf2.Owner,
                            IllustrationName.HealCompanion,
                            "The Radiant: Inspire",
                            new[]
                            {
                                ModTraits.Epithet,
                                ModTraits.Transcendence,
                                Trait.Emotion,
                                Trait.Mental
                            },
                            "Restore Hit Points equal to 2 + double your level; the ally is then immune to this effect for 10 minutes.",
                            Target.RangedFriend(6)  // 6 squares = 30 ft
                        ).WithActionCost(0);

                        inspire.WithEffectOnEachTarget(async (act, caster, target, result) =>
                        {
                            // Prevent re-targeting the same ally within 10 minutes
                            if (target.HasEffect(ExemplarIkonQEffectIds.QTheRadiantImmune))
                            {
                                target.Overhead("They are already inspired recently.", Color.Orange);
                                return;
                            }

                            // Heal: 2 + (2 × your level)
                            int amount = 2 + 2 * caster.Level;
                            await target.HealAsync(amount.ToString(), act);

                            // Grant 10-minute immunity (≈100 × 6s ticks)
                            target.AddQEffect(new QEffect(
                                "Immune to The Radiant",
                                "You cannot be inspired by The Radiant again for 10 minutes.",
                                ExpirationCondition.CountsDownAtEndOfYourTurn,
                                caster,
                                IllustrationName.Protection
                            )
                            {
                                Id    = ExemplarIkonQEffectIds.QTheRadiantImmune,
                                Value = 100
                            });

                            // Mark this use so it can’t reappear until your next turn
                            caster.AddQEffect(new QEffect(
                                "The Radiant Used",
                                "You have used The Radiant this turn.",
                                ExpirationCondition.CountsDownAtStartOfSourcesTurn,
                                caster,
                                IllustrationName.Chaos
                            )
                            {
                                Id    = ExemplarIkonQEffectIds.QTheRadiantUsedThisTurn,
                                Value = 1
                            });
                        });

                        return new ActionPossibility(inspire);
                    };
                };
            });

            ModManager.AddFeat(theRadiant);
        }
    }
}
