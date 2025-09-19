using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Dominion;

public class RestlessAsTheTides
{
    private class RestlessAsTheTidesTarget : GeneratorTarget
    {
        private List<Creature> Targets;

        public RestlessAsTheTidesTarget(List<Creature> targets)
        {
            Targets = targets;
        }
        public override GeneratedTargetInSequence? GenerateNextTarget()
        {
            if (base.OwnerAction.ChosenTargets.ChosenCreature == null)
            {
                return new GeneratedTargetInSequence(new CreatureTarget(RangeKind.Ranged, [
                    new EnemyCreatureTargetingRequirement(),
                    new LegacyCreatureTargetingRequirement((self, target) => Targets.Contains(target) ? Usability.Usable : Usability.NotUsableOnThisCreature("not Spark Transcendence target"))
                ], (_, _, _) => float.MinValue), " (target to move)", "Don't move any targets");
            }
            else if (base.OwnerAction.ChosenTargets.ChosenTile == null)
            {
                Creature target = base.OwnerAction.ChosenTargets.ChosenCreature;
                return new GeneratedTargetInSequence(Target.Tile((self, tile) => tile.CanIStopMyMovementHere(target) && tile.DistanceTo(target) == 1), " (tile to move the target to)")
                {
                    DisableConfirmNoMoreTargets = true
                };
            }
            else
            {
                return null;
            }
        }
    }

    [FeatGenerator(7)]
    public static IEnumerable<Feat> GetFeat()
    {
        List<FeatName> sparkOptions = [ExemplarFeats.EnergizedSparkWater, ExemplarFeats.EnergizedSparkCold];
        yield return new Epithet(
            ExemplarFeats.RestlessAsTheTides,
                "Your dominion is over the ocean, the great source and ultimate taker of lives.",
                "You gain the Energized Spark feat for your choice of water or cold. " +
                "When you critically succeed on a Strike, water blasts the target and those nearby. " +
                "This deals bludgeoning splash damage equal to the number of weapon damage dice to the target and all enemies within 10 feet of it. This effect has the water trait.\n\n" +
                "When you Spark Transcendence, you can Step, your body carried along by a surging tide. " +
                "If your transcendence affected an enemy, you can instead move that enemy 5 feet in a direction of your choice unless it succeeds at a Fortitude save against your class DC. " +
                "If you move an enemy who started out adjacent to you, you can Step into the space it vacated.",
            [ExemplarTraits.DominionEpithet],
            sparkOptions.Select(spark => new Feat(spark, "", "", [], null)
                .WithEquivalent(sheet => sheet.HasFeat(spark))
                .WithOnSheet(sheet =>
                {
                    sheet.GrantFeat(ExemplarFeats.EnergizedSpark, spark);
                })).ToList()
        )
        .WithTranscendPossibility("When you Spark Transcendence, you can Step, your body carried along by a surging tide. " +
                "If your transcendence affected an enemy, you can instead move that enemy 5 feet in a direction of your choice unless it succeeds at a Fortitude save against your class DC. " +
                "If you move an enemy who started out adjacent to you, you can Step into the space it vacated.", (exemplar, action) =>
                {
                    var targets = action.ChosenTargets.GetTargetCreatures();
                    if (!targets.Contains(exemplar))
                    {
                        return new ActionPossibility(new CombatAction(exemplar, IllustrationName.ElementalBlastWater, "Restless as the Tides", [],
                                "You move an enemy 5 feet in a direction of your choice unless it succeeds at a Fortitude save against your class DC. " +
                                "If you move an enemy who started out adjacent to you, you can Step into the space it vacated.",
                                new RestlessAsTheTidesTarget(targets.ToList()))
                            .WithActionCost(0)
                            .WithSavingThrow(new SavingThrow(Defense.Fortitude, _ => exemplar.ClassDC()))
                            .WithEffectOnChosenTargets(async (action, self, targets) =>
                            {
                                if (targets.ChosenCreature == null || targets.ChosenTile == null)
                                {
                                    action.RevertRequested = true;
                                    return;
                                }
                                var originalTile = targets.ChosenCreature.Occupies;
                                if (targets.CheckResults[targets.ChosenCreature] <= CheckResult.Failure)
                                {
                                    await targets.ChosenCreature.MoveTo(targets.ChosenTile, CombatAction.CreateSimple(self, "Restless as the Tides"), new MovementStyle()
                                    {
                                        ForcedMovement = true,
                                        MaximumSquares = 1
                                    });
                                    if (originalTile.DistanceTo(self) == 1 && await self.AskForConfirmation(IllustrationName.WarpStep, "Step into the targets previous location?", "Yes", "No"))
                                    {
                                        await CommonStealthActions.Step(self, CombatAction.CreateSimple(self, "Step"), originalTile);
                                    }
                                }
                            })
                        );
                    }
                    else
                    {
                        //Copied from the decompiled Step possibility in CreatePossibilities
                        //TODO: maybe possible to get it from Possibilities?
                        return new ActionPossibility(new CombatAction(exemplar, IllustrationName.ElementalBlastWater, "Restless as the Tides (Step)", new Trait[2]
                        {
                            Trait.Move,
                            Trait.Basic
                        }, "Move 5 feet. Unlike most types of movement, Stepping doesn't trigger reactions, such as Attacks of Opportunity, that can be triggered by move actions or upon leaving or entering a square. You can't Step into difficult terrain.", Target.Tile((Creature cr, Tile t) => t.LooksFreeTo(cr), (Creature cr, Tile t) => -2.1474836E+09f).WithPathfindingGuidelines((Creature cr) => new PathfindingDescription
                        {
                            Squares = 1
                        })).WithActionId(ActionId.Step).WithActionCost(0).WithEffectOnChosenTargets(async delegate (CombatAction action, Creature caster, ChosenTargets targets)
                        {
                            await CommonStealthActions.Step(caster, action, targets.ChosenTile!);
                        }));
                    }
                }
        )
        .WithPermanentQEffect(null, q =>
        {
            q.AfterYouTakeAction = async (selfQf, action) =>
            {
                var simpleAction = CombatAction.CreateSimple(selfQf.Owner,
                                "Restless as the Tides",
                                [Trait.Water]);

                if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess && action.ChosenTargets.ChosenCreature != null)
                {
                    foreach (var creature in selfQf.Owner.Battle.AllCreatures.Where(cr => cr.DistanceTo(action.ChosenTargets.ChosenCreature) <= 2 && cr.EnemyOf(selfQf.Owner)))
                    {
                        await CommonSpellEffects.DealDirectSplashDamage(simpleAction,
                            DiceFormula.FromText($"{action.Item?.WeaponProperties?.DamageDieCount ?? 0}"),
                            creature,
                            DamageKind.Bludgeoning);
                    }
                }
            };
        });
    }
}
