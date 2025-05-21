using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Exemplar.Utilities;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Mechanics.Core;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_EpithetRestlessAsTheTides
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var tides = new TrueFeat(
                ExemplarFeatNames.EpithetRestlessAsTheTides,
                7,
                "Restless as the Tides [WIP]",
                "Your dominion is over the ocean, the great source and ultimate taker of lives. " +
                "You gain the Energized Spark feat for your choice of water or cold. " +
                "When you critically succeed on a Strike, water blasts the target and those nearby. " +
                "This deals bludgeoning splash damage equal to the number of weapon damage dice to the target and all creatures within 10 feet of it. This effect has the water trait.\n\n" +
                "When you Spark Transcendence, you can Step, your body carried along by a surging tide. " +
                "If your transcendence affected an enemy, you can instead move that enemy 5 feet in a direction of your choice unless it succeeds at a Fortitude save against your class DC. " +
                "If you move an enemy who started out adjacent to you, you can Step into the space it vacated.",
                new[] { ModTraits.DominionEpithet },
                null
            )
            .WithOnSheet(sheet =>
            {
                // Grant the base Spark feat and filter to water or cold
                sheet.GrantFeat(ExemplarFeatNames.FeatEnergizedSpark);
                var allowed = new HashSet<FeatName>
                {
                    ExemplarFeatNames.AttunementFeats[1], // Cold
                    ExemplarFeatNames.AttunementFeats[8]  // Water
                };
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "RestlessTidesSparkType",
                        name: "Choose your Tides Spark damage type",
                        level: 7,
                        eligible: ft => ft.HasTrait(ModTraits.TEnergizedSpark) && allowed.Contains(ft.FeatName)
                    )
                );
            })
            .WithPermanentQEffect(null, qf =>
            {
                qf.AfterYouTakeAction = async (selfQf, action) =>
                {
                    // Splash effect on critical Strike
                    if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess)
                    {
                        var target = action.ChosenTargets?.ChosenCreature;
                        if (target != null && action.Item?.WeaponProperties != null)
                        {
                            int diceCount = action.Item.WeaponProperties.DamageDieCount;
                            int dieSize = action.Item.WeaponProperties.DamageDieSize;
                            var splashFormula = DiceFormula.FromText($"{diceCount}d{dieSize}", "Tidal Splash");
                            // Deal splash damage to primary target
                            await CommonSpellEffects.DealDirectSplashDamage(action, splashFormula, target, DamageKind.Bludgeoning);
                            // Splash to creatures within 10 feet
                            foreach (var creature in selfQf.Owner.Battle.AllCreatures
                                .Where(c => c.DistanceTo(target) <= 10 && c != target))
                            {
                                await CommonSpellEffects.DealDirectSplashDamage(action, splashFormula, creature, DamageKind.Bludgeoning);
                            }
                        }
                    }

                    // Provide Step or enemy movement on Transcendence
                    if (action.HasTrait(ModTraits.Transcendence))
                    {
                        qf.ProvideMainAction = qf2 =>
                        {
                            // Determine if the last action targeted an enemy
                            bool targetedEnemy = action.ChosenTargets?.ChosenCreature != null
                                && action.ChosenTargets.ChosenCreature.OwningFaction != qf2.Owner.OwningFaction;

                            if (targetedEnemy)
                            {
                                // TODO: implement alternative behavior for moving the targeted enemy
                                // For now, fall back to a simple Step
                            }
                        var actionPossibility = new ActionPossibility(
                            new CombatAction(
                                qf2.Owner,
                                IllustrationName.WaterWalk,
                                "Restless as the Tides: Surge",
                                new[] { ModTraits.Epithet, ModTraits.Transcendence },
                                "Step as a surging tide carries you. If your Transcendence affected an enemy, you may instead attempt to move that enemy 5 feet (basic Fortitude save) and, if that enemy was adjacent, step into its vacated space.",
                                Target.Self()
                            ).WithActionCost(0).WithEffectOnSelf(async (act, caster) =>
                            {
                                await selfQf.Owner.StrideAsync("Choose where you want to Step.", allowStep: true, maximumFiveFeet: true);
                            })

                            );
                            return actionPossibility;
                        };
                    }
                };
            });

            ModManager.AddFeat(tides);
        }
    }
}
