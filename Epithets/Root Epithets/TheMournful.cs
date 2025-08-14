// file: Exemplar_EpithetTheMournful.cs
using System.Linq;
using Microsoft.Xna.Framework;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
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
    public class Exemplar_EpithetTheMournful
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var theMournful = new TrueFeat(
                ExemplarFeatNames.EpithetTheMournful,
                3,
                "The Mournful",
                "To be a hero is to endure countless hardships and stand where others have fallen, shouldering dreams and destinies in their stead. " +
                "Though this weight may reach your eyes, you bear this burden so that those under you can live smiling. " +
                "You are trained in Diplomacy. After you Spark Transcendence, your act resonates with bittersweet poignancy, making one enemy of your choice within 30 feet who witnessed the act dazzled as tears or memories dance in their eyes. " +
                "The enemy remains dazzled until the start of your next turn, and then they are immune to this effect for 10 minutes.",
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
                    // Only once per turn, and only after a Transcendence ability
                    if (!action.HasTrait(ModTraits.Transcendence)
                        || selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QTheMournfulUsedThisTurn))
                        return;

                    // Inject the free “Dazzle” action
                    qf.ProvideMainAction = qf2 =>
                    {
                        if(selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QTheMournfulUsedThisTurn))
                            return null;
                            
                        var dazzle = new CombatAction(
                            qf2.Owner,
                            IllustrationName.Dazzled,
                            "The Mournful: Dazzle",
                            new[] { ModTraits.Epithet, ModTraits.Transcendence },
                            "Dazzle one enemy within 30 feet who witnessed your Transcendence— they become Dazzled until the start of your next turn, then immune to this effect for 10 minutes.",
                            Target.Distance(6)
                        ).WithActionCost(0);

                        dazzle.WithEffectOnEachTarget(async (act, caster, target, result) =>
                        {
                            // Prevent re-targeting the same creature
                            if (target.HasEffect(ExemplarIkonQEffectIds.QTheMournfulImmune))
                            {
                                target.Overhead("They are already immune.", Color.Orange);
                                return;
                            }

                            // Apply the Dazzled effect
                            target.AddQEffect(QEffect.Dazzled().WithExpirationAtStartOfSourcesTurn(caster,1));

                            // Then apply 10-minute immunity (≈100 counts of 6s)
                            target.AddQEffect(new QEffect(
                                "Immune to The Mournful",
                                "You cannot be Dazzled by The Mournful again for 10 minutes.",
                                ExpirationCondition.CountsDownAtEndOfYourTurn,
                                caster,
                                IllustrationName.Protection
                            )
                            {
                                Id    = ExemplarIkonQEffectIds.QTheMournfulImmune,
                                Value = 100
                            });

                            // Mark usage for this turn
                            caster.AddQEffect(new QEffect(
                                "The Mournful Used",
                                "You have used The Mournful this turn.",
                                ExpirationCondition.ExpiresAtStartOfYourTurn,
                                caster,
                                IllustrationName.Protection
                            )
                            {
                                Id    = ExemplarIkonQEffectIds.QTheMournfulUsedThisTurn,
                                Value = 1
                            });
                        });

                        return new ActionPossibility(dazzle);
                    };
                };
            });

            ModManager.AddFeat(theMournful);
        }
    }
}
