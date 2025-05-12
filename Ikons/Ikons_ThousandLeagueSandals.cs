using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Exemplar.Utilities;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_ThousandLeagueSandals
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            /*
                This one needs more testing to make sure it's working as intended, it seems to be done?
            */
            var ikon = new TrueFeat(
                ExemplarFeatNames.IkonThousandLeagueSandals,
                1,
                "Thousand-League Sandals",
                "{b}Immanence{/b} Your sandals ease your travels on the path ahead, granting you a +10-foot status bonus to your Speed.\n\n" +
                "{b}Transcendence — Marathon Dash (one-action){/b} Your feet carry you so quickly they leave a slipstream that speeds your allies on. You Stride. Each ally who was within 10 feet of you at the start of your movement can Stride as a reaction.",
                new[] { ModTraits.Ikon },
                null
            ).WithMultipleSelection()
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: +10-foot status bonus to Speed, but only while empowered
                qf.BonusToAllSpeeds = qfSelf =>
                {
                    // Don't apply the speed bonus unless they've shifted (i.e. have the empowered Q-effect)
                    if (!qfSelf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredThousandLeagueSandals))
                        return null;

                    return new Bonus(10, BonusType.Status, "Thousand-League Sandals");
                };


                // Transcendence: Marathon Dash
                qf.ProvideMainAction = qf =>
                {
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredThousandLeagueSandals))
                        return null;

                    var action = new CombatAction(
                        qf.Owner,
                        IllustrationName.FreedomOfMovement,
                        "Marathon Dash",
                        new[] { ModTraits.Ikon, ModTraits.Transcendence },
                        "You Stride. Each ally who was within 10 feet of you at the start of your movement can Stride as a reaction.",
                        Target.Self()
                    ).WithActionCost(1);

                    action.WithEffectOnSelf(async (act, self) =>
                    {
                        // Capture which allies were in range at start
                        var allies = self.Battle.AllCreatures
                            .Where(a => a.OwningFaction == self.OwningFaction && a != self && a.DistanceTo(self) <= 10)
                            .ToList();

                        // Your movement
                        await self.StrideAsync("Marathon Dash: choose where to Stride", allowPass: false);

                        // Allow each captured ally to Stride as a reaction
                        foreach (var ally in allies)
                        {
                            // Note: this invokes their Stride; game will treat it as a reaction
                            if(await ally.AskToUseReaction("Slipstream Reaction: Chose where to stride") )
                            {
                                await ally.StrideAsync("Slipstream : Choose where to Stride", allowPass: true);
                            }
                        }

                        // Clean up empowerment & grant free shift + exhaustion
                        IkonEffectHelper.CleanupEmpoweredEffects(self, ExemplarIkonQEffectIds.QEmpoweredThousandLeagueSandals);
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(ikon);
        }
    }
}
