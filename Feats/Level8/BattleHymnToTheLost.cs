using System.Collections.Generic;
using System.Threading.Tasks;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level8;

public class BattleHymnToTheLost
{
    /*
    public class CreatureAndConeBehindTarget : ConeAreaTarget
    {
        CreatureTarget internalTarget;
        public CreatureAndConeBehindTarget(RangeKind rangeKind, CreatureTargetingRequirement[] creatureTargetingRequirement, Func<Target, Creature, Creature, float> goodness)
            : base(30, (_, _) => float.MinValue)
        {
            internalTarget = new CreatureTarget(rangeKind, creatureTargetingRequirement, goodness);
        }

        public IEnumerable<Option> GenerateTargets(Func<Creature, Creature[], Creature, bool>? additionalRestrictions = null)
        {
            internalTarget.SetOwnerAction(OwnerAction);
            var options = internalTarget.GetTargetingSuitabilityForAllCreatures(OwnerAction.Owner).Where((crusability) =>
            {
                var (cr, usability) = crusability;
                return !usability || (additionalRestrictions?.Invoke(OwnerAction.Owner, OwnerAction.ChosenTargets.ChosenCreatures.ToArray(), cr) ?? true);
            }).Select(crusability =>
            {
                var (cr, usability) = crusability;
                if (usability)
                {
                    HashSet<Tile> tiles = Areas.DetermineTiles(this, OwnerAction.Owner.Occupies, cr.Occupies.ToCenterVector()).TargetedTiles;
                    return Option.ChooseArea(tiles, "Choose this creature as the target.", async delegate
                    {
                        OwnerAction.ChosenTargets.ChosenCreature = cr;
                        OwnerAction.ChosenTargets.ChosenCreatures.Add(cr);
                        OwnerAction.ChosenTargets.ChosenTiles.AddRange(tiles);
                    }, float.MinValue, noConfirmation: true).WithOptionKind(OptionKind.TargetCreature);
                }
                return Option.FakeChooseCreature("Invalid target (" + usability.UnusableReason + ")", cr);
            });
            return options;
        }
    }
    */

    [FeatGenerator(8)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new TrueFeat(
            ExemplarFeats.BattleHymnToTheLost,
            8,
            "Your movements in combat are an artistic dirge to those spirits who could not fight to see another day.",
            "You make a Strike. Regardless of whether the Strike succeeds, spirits of warriors who died gloriously in battle surge out to attack with you. " +
            "They appear in a 30-foot cone if you make a melee Strike or in a 10-foot emanation around your target if you made a ranged Strike. " +
            "Each enemy in the area takes 2d10 damage with a basic Reflex save against your class DC; you choose whether the damage is bludgeoning, " +
            "piercing, or slashing. At 12th level and every 4 levels thereafter, the damage increases by 1d10.\n\n" +
            "As they join the battle, the spirits call your fallen allies back to the fight. " +
            "Any of your allies in the area who are dying regain Hit Points equal to the damage you rolled. This is a divine, healing, positive effect.\n" +
            "This action can only be performed once per combat encounter.",
            [Trait.Concentrate, Trait.Divine, ExemplarTraits.Exemplar],
            null
        )
        .WithActionCost(2)
        .WithPrerequisite(sheet => sheet.HasFeat(ExemplarFeats.OfVerseUnbroken) || sheet.HasFeat(ExemplarFeats.PeerlessUnderHeaven), "You must have the domain epithet Of Verse Unbroken or Peerless Under Heaven.")
        .WithPermanentQEffect(null, qf =>
        {
            qf.ProvideStrikeModifier = item =>
            {
                var strike = qf.Owner.CreateStrike(item, qf.Owner.Actions.AttackedThisManyTimesThisTurn).WithActionCost(0);
                return qf.Owner.HasEffect(ExemplarQEffects.BattleHymnToTheLostUsed) ? null :
                    new CombatAction(qf.Owner,
                        new SideBySideIllustration(item.Illustration, IllustrationName.SongOfStrength),
                        "Battle Hymn to the Lost",
                        [Trait.Concentrate, Trait.Divine],
                        "You make a Strike. Regardless of whether the Strike succeeds, spirits of warriors who died gloriously in battle surge out to attack with you. " +
                        "They appear in a 30-foot cone if you make a melee Strike or in a 10-foot emanation around your target if you made a ranged Strike. " +
                        $"Each enemy in the area takes {S.HeightenedVariable(qf.Owner.Level / 4, 2)}d10 damage with a basic Reflex save against your class DC; you choose whether the damage is bludgeoning, " +
                        "piercing, or slashing.\n\n" +
                        "As they join the battle, the spirits call your fallen allies back to the fight. " +
                        "Any of your allies in the area who are dying regain Hit Points equal to the damage you rolled. This is a divine, healing, positive effect.\n" +
                        "This action can only be performed once per combat encounter.",
                        //TODO: thrown?
                        item.DetermineStrikeTarget(item.HasTrait(Trait.Ranged) ? RangeKind.Ranged : RangeKind.Melee))
                    .WithActionCost(2)
                    .WithActiveRollSpecification(new ActiveRollSpecification(Utility.Attack(strike, item, -1), TaggedChecks.DefenseDC(Defense.AC)))
                    .WithNoSaveFor((action, cr) => true)
                    .WithEffectOnChosenTargets(async (action, self, targets) =>
                    {
                        string damage = $"{(self.Level) / 4}d10";
                        if (targets.ChosenCreature != null)
                        {
                            await self.MakeStrike(targets.ChosenCreature, item);

                            if (!item.HasTrait(Trait.Ranged))
                            {
                                var targetPoint = (targets.ChosenCreature.Occupies.ToCenterVector() - self.Occupies.ToCenterVector()) + targets.ChosenCreature.Occupies.ToCenterVector();
                                var coneTiles = Areas.DetermineTiles(new ConeAreaTarget(6, (_, _) => float.MinValue), targets.ChosenCreature.Occupies, targetPoint).TargetedTiles;
                                foreach (var tile in coneTiles)
                                {
                                    if (tile.PrimaryOccupant != null)
                                    {
                                        await Apply(tile.PrimaryOccupant);
                                    }
                                }
                            }
                            else
                            {
                                var dummyAction = CombatAction.CreateSimple(targets.ChosenCreature, action.Name);
                                var target = new EmanationTarget(2, true);
                                target.SetOwnerAction(dummyAction);
                                var tiles = Areas.DetermineTiles(target).TargetedTiles;
                                foreach (var tile in tiles)
                                {
                                    if (tile.PrimaryOccupant != null)
                                    {
                                        await Apply(tile.PrimaryOccupant);
                                    }
                                }
                            }
                        }
                        self.AddQEffect(new QEffect()
                        {
                            Id = ExemplarQEffects.BattleHymnToTheLostUsed
                        });
                        async Task Apply(Creature target)
                        {
                            if (target.EnemyOf(self))
                            {
                                var checkResult = CommonSpellEffects.RollSavingThrow(target, action, Defense.Reflex, self.ClassDC());
                                await CommonSpellEffects.DealBasicDamage(action, self, target, checkResult, damage, target.WeaknessAndResistance.WhatDamageKindIsBestAgainstMe([DamageKind.Bludgeoning, DamageKind.Piercing, DamageKind.Slashing]));
                            }
                            else if (target.HasEffect(QEffectId.Dying))
                            {
                                var healingAction = CombatAction.CreateSimple(self, action.Name, [Trait.Divine, Trait.Healing, Trait.Positive]);
                                await target.HealAsync(damage, healingAction);
                            }
                        }
                    });
            };
        });
    }
}
