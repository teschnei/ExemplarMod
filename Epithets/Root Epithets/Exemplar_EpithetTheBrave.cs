// file: Exemplar_EpithetTheBrave.cs
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using System.Linq;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CombatActions;
using System;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_EpithetTheBrave
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var theBrave = new TrueFeat(
                ExemplarFeatNames.EpithetTheBrave,
                3,
                "The Brave",
                "Your deeds show fearlessness: when a beast surfaces, you're there to fight it; when someone's lost in the dark, you're first to the rescue. " +
                "You're trained in Athletics. After you Spark Transcendence, your body carries you forward, allowing you to Stride up to half your Speed in a straight line toward one enemy of your choice as a free action. " +
                "Once you have used this ability on a given enemy, you can't use it against that enemy again for 10 minutes.",
                new[] { ModTraits.RootEpithet },
                null
            )
            .WithOnSheet(sheet =>
            {
                sheet.GrantFeat(FeatName.Athletics);
            })
            .WithPermanentQEffect(null, qf =>
            {
                qf.AfterYouTakeAction = async (selfQf, action) =>
                {

                    // Only trigger on a Transcendence ability also check to see if they have QTheBraveUsedOnTarget
                    if (!action.HasTrait(ModTraits.Transcendence)
                    || selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QTheBraveUsedOnSelf))
                        return;

                    qf.ProvideMainAction = qf2 =>
                    {
                        // Only once per turn, and only after a Transcendence ability
                        if (selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QTheBraveUsedOnSelf))
                            return null;

                        var action = new CombatAction(
                            qf2.Owner,
                            IllustrationName.FreedomOfMovement,
                            "The Brave: Stride",
                            new[] { ModTraits.Epithet, ModTraits.Transcendence },
                            "You can Stride up to half your Speed in a straight line toward one enemy of your choice as a free action.",
                            Target.Distance(10)
                        ).WithActionCost(0);
                        action.WithEffectOnEachTarget(async (act, caster, target, result) =>
                        {
                            if (target.HasEffect(ExemplarIkonQEffectIds.QTheBraveUsedOnTarget))
                            {
                                target.Occupies.Overhead("This has unit has been targetted already.", Color.Orange);
                                return;
                            }

                            if (target == null)
                                return;
                            // Immediately Stride up to half your Speed toward that creature
                            await selfQf.Owner.StrideAsync(
                                $"The Brave: Stride toward {target.Name} (half Speed)",
                                allowPass: true,
                                maximumHalfSpeed: true,
                                strideTowards: target.Occupies
                            );

                            target.AddQEffect(new QEffect("The Brave: Used on target", "You have used The Brave on this target, and cannot use it again for 10 minutes.", ExpirationCondition.CountsDownAtEndOfYourTurn, selfQf.Owner, IllustrationName.FreedomOfMovement)
                            {
                                Id = ExemplarIkonQEffectIds.QTheBraveUsedOnTarget,
                                Value = 100, // 10 minutes
                            });
                            //make sure the caster can't cast it again this turn, add a custom exhaustion effect.
                            caster.AddQEffect(new QEffect("The Brave: Used this turn",
                             "You have used The Brave this turn, and cannot use it again until the start of your next turn.",
                              ExpirationCondition.CountsDownAtEndOfYourTurn,
                              selfQf.Owner,
                              IllustrationName.FreedomOfMovement)
                            {
                                Id = ExemplarIkonQEffectIds.QTheBraveUsedOnSelf,
                                Value = 1,
                            });

                        });

                        return new ActionPossibility(action);
                    };
                };
            });

            ModManager.AddFeat(theBrave);
        }
    }
}
