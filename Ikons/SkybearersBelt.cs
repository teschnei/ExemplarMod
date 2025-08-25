using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class SkybearersBelt
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("SkybearersBelt", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Skybearer's Belt", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonBelt)
            .WithRuneProperties(new RuneProperties("Ikon", IkonRuneKind.SkybearersBelt, "This girdle wraps around your waist, magnifying your strength to the point you feel you could carry the sky itself.",
            "", item =>
            {
                item.Traits.AddRange([ExemplarTraits.Ikon, Trait.Divine]);
            })
            .WithCanBeAppliedTo((Item rune, Item item) =>
            {
                if (!item.HasTrait(Trait.Worn) || item.WornAt != Trait.Belt)
                {
                    return "Must be a worn belt.";
                }
                return null;
            }));
        });
        ItemName freeItem = ModManager.RegisterNewItemIntoTheShop("Ordinary Belt", itemName =>
        {
            return new Item(itemName, IllustrationName.GrapplersBelt, "Belt", 1, 0, Trait.DoNotAddToShop)
            .WithDescription("An ordinary belt.")
            .WithWornAt(Trait.Belt);
        });
        yield return new Ikon(new Feat(
            ExemplarFeats.SkybearersBelt,
            "This girdle wraps around your waist, magnifying your strength to the point you feel you could carry the sky itself.\n\n",
            "{b}Usage{/b} worn belt\n\n" +
            "{b}Immanence{/b} Strength flows forth. You can attempt to Disarm, Grapple, Shove, or Trip creatures up to two sizes larger than you, and you gain a +1 circumstance bonus to checks for these actions and to your saving throws to resist these actions.\n\n" +
            $"{{b}}Transcendence — Bear Allies' Burdens {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (transcendence)\n" +
            "You move with a speed belying your strength, carrying your allies as easily as straw dolls. You Stride. At any point you are adjacent to a willing ally during the Stride, you can pick that ally up, and you can deposit them into a space adjacent to you at any other point during your movement. " +
            "You ignore the ally's Bulk while carrying them during your Stride. You can Climb, Fly, or Swim instead of Striding if you have the corresponding movement type.",
            [ExemplarTraits.Ikon],
            null
        ).WithIllustration(IllustrationName.GrapplersBelt), q =>
        {
            q.BonusToAttackRolls = (q, action, target) => Ikon.GetIkonItemWorn(q.Owner, ikonRune) != null &&
                (action.ActionId == ActionId.Disarm || action.ActionId == ActionId.Grapple || action.ActionId == ActionId.Shove || action.ActionId == ActionId.Trip) ?
                    new Bonus(1, BonusType.Circumstance, "Skybearer's Belt", true) : null;
            q.BonusToDefenses = (q, action, defense) => Ikon.GetIkonItemWorn(q.Owner, ikonRune) != null &&
                (action?.ActionId == ActionId.Disarm || action?.ActionId == ActionId.Grapple || action?.ActionId == ActionId.Shove || action?.ActionId == ActionId.Trip) ?
                    new Bonus(1, BonusType.Circumstance, "Skybearer's Belt", true) : null;
        }, q =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                IllustrationName.GrapplersBelt,
                "Bear Allies' Burdens",
                [ExemplarTraits.Transcendence],
                "You move with a speed belying your strength, carrying your allies as easily as straw dolls. You Stride. At any point you are adjacent to a willing ally during the Stride, you can pick that ally up, and you can deposit them into a space adjacent to you at any other point during your movement. " +
                "You ignore the ally's Bulk while carrying them during your Stride. You can Climb, Fly, or Swim instead of Striding if you have the corresponding movement type.",
                Target.Tile((Creature cr, Tile t) => t.LooksFreeTo(cr), null).WithPathfindingGuidelines((Creature cr) => new PathfindingDescription
                {
                    Squares = cr.Speed
                }).WithAdditionalTargetingRequirement((self, _) =>
                {
                    if (Ikon.GetIkonItemWorn(self, ikonRune) == null)
                    {
                        return Usability.NotUsable("You must be wearing the Skybearer's Belt");
                    }
                    return Usability.Usable;
                })
            ).WithActionCost(2)
            .WithEffectOnChosenTargets(async (action, self, targets) =>
            {
                Tile tile = targets.ChosenTile!;
                var path = Pathfinding.ReconstructPathFromEarlierFloodfill(self.Occupies, tile);
                HashSet<Tile> pathWithAdjacentTiles = [.. path];
                foreach (var t in path)
                {
                    //There has to be a better way to do this...
                    pathWithAdjacentTiles.UnionWith(self.Battle.Map.AllTiles.Where(tile => tile.IsAdjacentTo(t)).ToHashSet());
                }
                List<Creature> alreadyAsked = new();
                List<(Creature, Tile)> moves = new();
                var bear = new QEffect()
                {
                    StateCheckWithVisibleChanges = async q =>
                    {
                        var adjacentAllies = q.Owner.Battle.AllCreatures.Where(cr => cr.FriendOf(q.Owner) && cr.IsAdjacentTo(q.Owner));
                        foreach (var ally in adjacentAllies)
                        {
                            if (!alreadyAsked.Contains(ally))
                            {
                                alreadyAsked.Add(ally);
                                if (await q.Owner.AskForConfirmation(ally.Illustration, $"Pick up {ally.Name}?", "Yes", "No"))
                                {
                                    var result = await q.Owner.Battle.SendRequest(new AdvancedRequest(q.Owner, $"Select where to drop off {ally.Name}", pathWithAdjacentTiles.Select(tile => Option.ChooseTile($"Drop off {ally.Name} here.", tile, async () =>
                                    {
                                        moves.Add((ally, tile));
                                    }, 0) as Option).ToList()));
                                    await result.ChosenOption.Action();
                                }
                            }
                        }
                    }
                };
                self.AddQEffect(bear);
                await self.MoveToUsingEarlierFloodfill(tile, action, new MovementStyle()
                {
                    MaximumSquares = self.Speed
                });
                self.RemoveAllQEffects(q => q == bear);
                foreach (var (friend, target) in moves)
                {
                    await friend.MoveTo(target, action, new MovementStyle()
                    {
                        MaximumSquares = self.Speed + 2,
                        ForcedMovement = true
                    });
                }
            }));
        })
        .WithRune(ikonRune)
        .WithFreeWornItem(freeItem)
        .IkonFeat;
    }
}

