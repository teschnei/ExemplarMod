using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class MortalHarvest
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("MortalHarvest", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Mortal Harvest", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonSickleAxeFlailPolearm)
            .WithRuneProperties(new RuneProperties("Ikon", IkonRuneKind.Ikon, "This weapon, once used for felling trees or crops, now harvests lives instead.",
            "", item =>
            {
                item.Traits.AddRange([ExemplarTraits.Ikon, Trait.Divine]);
            })
            .WithCanBeAppliedTo((Item rune, Item weapon) =>
            {
                if (weapon.WeaponProperties == null)
                {
                    return "Must be a weapon.";
                }
                if ((!weapon.HasTrait(Trait.Axe)) && (!weapon.HasTrait(Trait.Flail)) && (!weapon.HasTrait(Trait.Polearm)) && (!weapon.HasTrait(Trait.Sickle)))
                {
                    return "Must be a Sickle, Axe, Flail, or Polearm group.";
                }
                return null;
            }));
        });

        yield return new Ikon(new Feat(
            ExemplarFeats.MortalHarvest,
            "This weapon, once used for felling trees or crops, now harvests lives instead.",
            "{b}Usage{/b} a sickle or any weapon from the axe, flail, or polearm group\n\n" +
            "{b}Immanence{/b} The {i}mortal harvest{/i} deals 1 persistent spirit damage per weapon damage die to creatures it Strikes.\n\n" +
            $"{{b}}Transcendence — Reap the Field {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (transcendence)\n{{b}}Requirements{{/b}} Your previous action was a successful Strike with the mortal harvest.\n " +
            "{b}Effect{/b} Time seems to lag as you blur across the battlefield, deciding the fate of many in a moment. " +
            "Stride up to half your Speed and make another melee Strike with the {i}mortal harvest{/i} against a different creature. " +
            "This Strike uses the same multiple attack penalty as your previous Strike, but counts toward your multiple attack penalty as normal.",
            [ExemplarTraits.Ikon],
            null
        ).WithIllustration(IllustrationName.Scythe), q =>
        {
            q.YouDealDamageWithStrike = (qe, action, diceFormula, target) =>
            {
                if (action.Item?.Runes.Any(rune => rune.ItemName == ikonRune) ?? false)
                {
                    target.AddQEffect(QEffect.PersistentDamage($"{action.Item?.WeaponProperties?.DamageDieCount}".ToString(), Ikon.GetBestDamageKindForSpark(action.Owner, target)));
                }
                return diceFormula;
            };
        },
        q =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                IllustrationName.Scythe,
                "Reap the Field",
                [ExemplarTraits.Transcendence],
                "Stride up to half your Speed and make another Strike with the {i}mortal harvest{/i} against a different creature. This Strike uses the same multiple attack penalty as your previous Strike, but counts toward your multiple attack penalty as normal.",
                Target.Self().WithAdditionalRestriction(self =>
                {
                    var harvest = Ikon.GetIkonItem(q.Owner, ikonRune);
                    if (harvest == null)
                    {
                        return "You must be wielding the Mortal Harvest.";
                    }
                    var lastAction = self.Actions.ActionHistoryThisTurn.LastOrDefault();
                    if (lastAction == null || !lastAction.HasTrait(Trait.Strike) ||
                        lastAction.CheckResult < CheckResult.Success ||
                        (lastAction.Item != harvest))
                    {
                        return "Your last action must be a successful Strike with the {i}mortal harvest{/i}.";
                    }
                    return null;
                })
            )
            .WithActionCost(1)
            .WithEffectOnSelf(async (act, self) =>
            {
                var prev = self.Actions.ActionHistoryThisEncounter.LastOrDefault();
                var pen = 5;
                if (prev?.HasTrait(Trait.Agile) ?? false)
                {
                    pen = 4;
                }

                // Stride up to half Speed
                await self.StrideAsync("Choose where to stride (half Speed).", allowPass: false, maximumHalfSpeed: true);

                //Cleanup: currently this is to offset the MAP penalty.
                q.BonusToAttackRolls = (eff, act, defender) =>
                    q.Owner == self ? new Bonus(pen, BonusType.Untyped, "") : null;

                //TODO: only allow striking with the Mortal Harvest itself
                await CommonCombatActions.StrikeAdjacentCreature(self, null);
                q.BonusToAttackRolls = null;
            }));
        })
        .WithRune(ikonRune)
        .IkonFeat;
    }
}
