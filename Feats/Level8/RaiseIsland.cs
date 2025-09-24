using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level8;

public class RaiseIsland
{
    [FeatGenerator(8)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new TrueFeat(
            ExemplarFeats.RaiseIsland,
            8,
            "You churn the sea and fish up stone so your enemies together can receive their punishment.",
            "Each enemy in a 30-foot emanation must succeed at a Fortitude save against your class DC or be swept up to 15 feet to " +
            "another location of your choice within the affected area and become off-guard until the start of your next turn. " +
            "Afterwards, you can attempt a melee Strike against one enemy in reach.\n" +
            "This action can only be performed once per combat encounter.",
            [Trait.Concentrate, Trait.Divine, ExemplarTraits.Exemplar],
            null
        )
        .WithActionCost(3)
        .WithPrerequisite(sheet => sheet.HasFeat(ExemplarFeats.BornOfTheBonesOfTheEarth) || sheet.HasFeat(ExemplarFeats.RestlessAsTheTides), "You must have the domain epithet Born of the Bones of the Earth or Restless as the Tides.")
        .WithPermanentQEffect(null, qf =>
        {
            qf.ProvideMainAction = q => q.Owner.HasEffect(ExemplarQEffects.RaiseIslandUsed) ? null : new ActionPossibility(new CombatAction(
                    q.Owner,
                    IllustrationName.ElementalBlastEarth,
                    "Raise Island",
                    [Trait.Concentrate, Trait.Divine],
                    "Each enemy in a 30-foot emanation must succeed at a Fortitude save against your class DC or be swept up to 15 feet to " +
                    "another location of your choice within the affected area and become off-guard until the start of your next turn. " +
                    "Afterwards, you can attempt a melee Strike against one enemy in reach.\n" +
                    "This action can only be performed once per combat encounter.",
                    Target.EnemiesOnlyEmanation(6))
                .WithActionCost(3)
                .WithSavingThrow(new SavingThrow(Defense.Fortitude, q.Owner.ClassDC()))
                .WithEffectOnChosenTargets(async (action, self, targets) =>
                {
                    List<Creature> alreadyAffected = new();
                    var creatures = targets.GetAllTargetCreatures()
                        .Where(cr => targets.CheckResults.TryGetValue(cr, out var checkResult) && checkResult <= CheckResult.Failure).ToList();

                    while (alreadyAffected.Count < creatures.Count)
                    {
                        var options = creatures.Where(cr => !alreadyAffected.Contains(cr)).Select(cr =>
                            new CreatureOption(cr, "Move this target", async () =>
                            {
                                var tiles = targets.ChosenTiles.Where(tile => tile.DistanceTo(cr.Occupies) <= 3);
                                var options = targets.ChosenTiles.Where(tile => tile.DistanceTo(cr.Occupies) <= 3 && self.CanSee(tile)).Select(tile => new TileOption(tile, "Move the target here.", async () =>
                                {
                                    await cr.SingleTileMove(tile, action, new MovementStyle()
                                    {
                                        Shifting = true,
                                        ShortestPath = true,
                                        ForcedMovement = true,
                                        MaximumSquares = 3
                                    });
                                    alreadyAffected.Add(cr);
                                }, float.MinValue, true));
                                await (await self.Battle.SendRequest(new AdvancedRequest(self, "Choose a square to move the target to.", [.. options, new CancelOption(false, float.MinValue)])
                                {
                                    TopBarText = "Choose a square to move the target to.",
                                    TopBarIcon = action.Illustration
                                })).ChosenOption.Action();
                            }, float.MinValue, true)
                        );
                        var result = await self.Battle.SendRequest(new AdvancedRequest(self, "Choose a target to move.", [.. options, new PassViaButtonOption("Don't move any more targets.")])
                        {
                            TopBarText = "Choose a target to move.",
                            TopBarIcon = action.Illustration
                        });
                        if (result.ChosenOption is PassViaButtonOption)
                        {
                            break;
                        }
                        await result.ChosenOption.Action();
                    }
                    foreach (var target in creatures)
                    {
                        target.AddQEffect(QEffect.FlatFooted("Raise Island").WithExpirationAtStartOfSourcesTurn(self, 1));
                    }
                    await CommonCombatActions.StrikeAdjacentCreature(self, null, true);
                    self.AddQEffect(new QEffect()
                    {
                        Id = ExemplarQEffects.RaiseIslandUsed
                    });
                })
            );
        });
    }
}
