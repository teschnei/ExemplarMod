using System;
using System.Collections.Generic;
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
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level8;

public class RejoiceInSolsticeStorm
{
    [FeatGenerator(8)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new TrueFeat(
            ExemplarFeats.RejoiceInSolsticeStorm,
            8,
            "You hold your arms out, and the fury of the seasons comes to your jubilant embrace.",
            "A storm spirals out from you, dealing 5d8 damage (see below) to each creature in a 30-foot emanation, " +
            "with a basic Reflex save against your class DC. At 10th level and every 2 levels thereafter, the damage increases by 1d8. " +
            "The emanation is quartered into four non-overlapping cones, each of a different season, " +
            "which are arranged clockwise from spring, to summer, to fall, to winter, beginning with spring to the east. " +
            "Each cone has different traits, damage type, and a different effect to a creature that critically fails its saving throw; " +
            "a creature large enough to be in multiple seasons can choose which it is affected by.\n" +
            "This action can only be performed once per combat encounter.\n\n" +
            "{b}• Spring{/b} (electricity) Spring lightning deals electricity damage. Creatures who critically fail are left numb, becoming clumsy 2 until the end of their next turn.\n" +
            "{b}• Summer{/b} (water) A summer monsoon deals bludgeoning damage. Creatures who critically fail are knocked prone by hurricane winds.\n" +
            "{b}• Fall{/b} (emotion, mental, wood) Falling leaves deal slashing damage. Creatures who critically fail are gripped by melancholy, becoming off-guard until the end of their next turn.\n" +
            "{b}• Winter{/b} (cold) A blizzard deals cold damage. Creatures who critically fail are stupefied 2 until the end of their next turn as the cold numbs their senses.\n",
            [Trait.Concentrate, Trait.Divine, ExemplarTraits.Exemplar, Trait.Manipulate],
            null
        )
        .WithActionCost(2)
        .WithPrerequisite(sheet => sheet.HasFeat(ExemplarFeats.WhoseCryIsThunder) || sheet.HasFeat(ExemplarFeats.DancerInTheSeasons), "You must have the domain epithet Whose Cry is Thunder or Dancer in the Seasons.")
        .WithPermanentQEffect(null, qf =>
        {
            qf.ProvideMainAction = q => q.Owner.HasEffect(ExemplarQEffects.RejoiceInSolsticeStormUsed) ? null : new ActionPossibility(new CombatAction(
                    q.Owner,
                    IllustrationName.ResistEnergy,
                    "Rejoice in Solstice Storm",
                    [Trait.Concentrate, Trait.Divine, Trait.Manipulate],
                    $"A storm spirals out from you, dealing {S.HeightenedVariable((q.Owner.Level / 2) + 1, 5)}d8 damage (see below) to each creature in a 30-foot emanation, " +
                    "with a basic Reflex save against your class DC. " +
                    "The emanation is quartered into four non-overlapping cones, each of a different season, " +
                    "which are arranged clockwise from spring, to summer, to fall, to winter, beginning with spring to the east. " +
                    "Each cone has different traits, damage type, and a different effect to a creature that critically fails its saving throw; " +
                    "a creature large enough to be in multiple seasons can choose which it is affected by.\n" +
                    "This action can only be performed once per combat encounter.\n\n" +
                    "{b}• Spring{/b} (electricity) Spring lightning deals electricity damage. Creatures who critically fail are left numb, becoming clumsy 2 until the end of their next turn.\n" +
                    "{b}• Summer{/b} (water) A summer monsoon deals bludgeoning damage. Creatures who critically fail are knocked prone by hurricane winds.\n" +
                    "{b}• Fall{/b} (emotion, mental, wood) Falling leaves deal slashing damage. Creatures who critically fail are gripped by melancholy, becoming off-guard until the end of their next turn.\n" +
                    "{b}• Winter{/b} (cold) A blizzard deals cold damage. Creatures who critically fail are stupefied 2 until the end of their next turn as the cold numbs their senses.\n",
                    Target.SelfExcludingEmanation(6))
                .WithActionCost(2)
                .WithShortDescription("A storm spirals out from you, dealing 5d8 damage to each creature in a 30-foot emanation, " +
                    "with a basic Reflex save against your class DC. At 10th level and every 2 levels thereafter, the damage increases by 1d8. " +
                    "The emanation is quartered into four non-overlapping cones, each of a different season, " +
                    "which are arranged clockwise from spring, to summer, to fall, to winter, beginning with spring to the east. " +
                    "Each cone has different traits, damage type, and a different effect to a creature that critically fails its saving throw; " +
                    "a creature large enough to be in multiple seasons can choose which it is affected by.\n" +
                    "This action can only be performed once per combat encounter.\n\n")
                .WithSavingThrow(new SavingThrow(Defense.Reflex, q.Owner.ClassDC()))
                .WithEffectOnChosenTargets(async (action, self, targets) =>
                {
                    string damage = $"{(q.Owner.Level / 2) + 1}d8";
                    foreach (var target in targets.GetAllTargetCreatures())
                    {
                        if (targets.CheckResults.TryGetValue(target, out var checkResult))
                        {
                            var direction = GetDirectionFromCaster(self.Occupies, target.Occupies);
                            await CommonSpellEffects.DealBasicDamage(GetSimpleAction(self, direction), self, target, checkResult, damage, GetDamageKind(direction));
                            if (checkResult == CheckResult.CriticalFailure)
                            {
                                target.AddQEffect(GetCritFailEffect(direction));
                            }
                        }
                    }
                    self.AddQEffect(new QEffect()
                    {
                        Id = ExemplarQEffects.RejoiceInSolsticeStormUsed
                    });
                })
            );
        });
    }

    static Direction GetDirectionFromCaster(Tile casterTile, Tile targetTile)
    {
        int casterToTargetX = targetTile.X - casterTile.X;
        int casterToTargetY = targetTile.Y - casterTile.Y;

        if (Math.Abs(casterToTargetX) > Math.Abs(casterToTargetY) ||
                (Math.Abs(casterToTargetX) == Math.Abs(casterToTargetY) &&
                 casterToTargetX > 0 ^ casterToTargetY > 0))
        {
            return casterToTargetX > 0 ? Direction.East : Direction.West;
        }
        else
        {
            return casterToTargetY > 0 ? Direction.South : Direction.North;
        }
    }

    static DamageKind GetDamageKind(Direction direction) => direction switch
    {
        Direction.East => DamageKind.Electricity,
        Direction.South => DamageKind.Bludgeoning,
        Direction.West => DamageKind.Slashing,
        Direction.North => DamageKind.Cold,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), "Unexpected direction")
    };

    static QEffect GetCritFailEffect(Direction direction) => direction switch
    {
        Direction.East => QEffect.Clumsy(2),
        Direction.South => QEffect.Prone(),
        Direction.West => QEffect.FlatFooted("Rejoice in Solstice Storm").WithExpirationAtEndOfOwnersNextTurn(),
        Direction.North => QEffect.Stupefied(2),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), "Unexpected direction")
    };

    static CombatAction GetSimpleAction(Creature owner, Direction direction) => direction switch
    {
        Direction.East => CombatAction.CreateSimple(owner, "Rejoice in Solstice Storm (Spring)", [Trait.Electricity]),
        Direction.South => CombatAction.CreateSimple(owner, "Rejoice in Solstice Storm (Summer)", [Trait.Water]),
        Direction.West => CombatAction.CreateSimple(owner, "Rejoice in Solstice Storm (Fall)", [Trait.Emotion, Trait.Mental, Trait.Wood]),
        Direction.North => CombatAction.CreateSimple(owner, "Rejoice in Solstice Storm (Winter)", [Trait.Cold]),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), "Unexpected direction")
    };
}
