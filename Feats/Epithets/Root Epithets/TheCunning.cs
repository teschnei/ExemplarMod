// file: Exemplar_EpithetTheCunning.cs
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using System.Linq;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;

namespace Dawnsbury.Mods.Classes.Exemplar
{
    public class Exemplar_EpithetTheCunning
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var theCunning = new TrueFeat(
                ExemplarFeats.TheCunning,
                3,
                "The Cunning",
                "Why race a hare across a meadow, or a salmon up a waterfall? Why face a titan in a test of strength? " +
                "Wouldn't it be better to best your foes with a bit of creativity? After all, the stories that echo throughout history are always those where wits and trickery, rather than raw talent or power, win the day. " +
                "You are trained in Deception. After you Spark Transcendence, you can Create a Diversion or Feint as a free action.",
                new[] { ExemplarTraits.RootEpithet },
                null
            )
            .WithOnSheet(sheet =>
            {
                // Grant training in Deception
                sheet.GrantFeat(FeatName.Deception);
            })
            .WithPermanentQEffect(null, qf =>
            {
                qf.AfterYouTakeAction = async (selfQf, action) =>
                {
                    // Only once per turn, and only after a Transcendence ability
                    if (!action.HasTrait(ExemplarTraits.Transcendence)
                    || selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QTheCunningUsedThisTurn))
                        return;

                    // Provide two zero-cost actions: Diversion or Feint
                    qf.ProvideMainAction = qf2 =>
                    {
                        // Only once per turn, and only after a Transcendence ability
                        if (!action.HasTrait(ExemplarTraits.Transcendence)
                        || selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QTheCunningUsedThisTurn))
                            return null;
                        // 1) Create a Diversion
                        //I am not sure if createADiverion is working as intended.
                        var diversion = CommonStealthActions.CreateCreateADiversion(qf2.Owner)
                        .WithActionCost(0);

                        // 2) Feint
                        var feint = CombatManeuverPossibilities.CreateFeintAction(qf2.Owner)
                        .WithActionCost(0);

                        //first make a map for these two.
                        var actions = new[]
                        {
                            diversion,
                            feint
                        };
                        // Return both as choices. 
                        // Depending on your engine version you may need to swap in the correct
                        // “multi-action” wrapper (e.g. ChoiceActionPossibility or Po ssibilityPool).
                        return new SubmenuPossibility(IllustrationName.Feint, "The Cunning: Choose an action")
                        {
                            Subsections = [

                                new PossibilitySection("Select Ikon to Empower")
                                {
                                    Possibilities = actions.Select(q =>
                                    {

                                        return (Possibility) new ActionPossibility(q
                                        .WithEffectOnEachTarget(async (act, self, target, result) =>
                                        {
                                            // Mark as used so you can't re-inject this until your next turn
                                            self.AddQEffect(new QEffect(
                                                "The Cunning: Used this turn",
                                                "You've used your free Diversion or Feint this turn.",
                                                ExpirationCondition.CountsDownAtStartOfSourcesTurn,
                                                self,
                                                IllustrationName.Feint
                                            )
                                            {
                                                Id = ExemplarIkonQEffectIds.QTheCunningUsedThisTurn,
                                                Key = "The Cunning: Used this turn",
                                                Value = 1
                                            });
                                        })
                                        );
                                    }).ToList()
                                }
                            ]
                        };


                    };
                };
            });

            ModManager.AddFeat(theCunning);
        }
    }
}
