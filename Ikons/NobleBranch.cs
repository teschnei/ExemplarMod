using System;
using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class NobleBranch
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("NobleBranch", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Noble Branch", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonStaff)
            .WithRuneProperties(new RuneProperties("Ikon", IkonRuneKind.Ikon, "This humble stick-like weapon has an elegant simplicity to it, affording you reliable strikes over flashy maneuvers.",
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
                if ((!weapon.HasTrait(Trait.Staff)) && (!weapon.HasTrait(Trait.Spear)) && (!weapon.HasTrait(Trait.Polearm)))
                {
                    return "Must be a staff, bo staff, fighting stick, khakkara, or any weapon in the spear or polearm weapon group";
                }
                return null;
            }));
        });

        yield return new Ikon(new Feat(
            ExemplarFeats.NobleBranch,
            "This humble stick-like weapon has an elegant simplicity to it, affording you reliable strikes over flashy maneuvers.",
            "{b}Usage{/b} a staff, bo staff, fighting stick, khakkara, or any weapon in the spear or polearm weapon group\n\n" +
            "{b}Immanence{/b} The {i}noble branch{/i} deals 2 additional force damage per weapon damage die to creatures it Strikes.\n\n" +
            $"{{b}}Transcendence — Strike, Breathe, Rend {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (force, transcendence)\n{{b}}Requirements{{/b}} Your last action this turn was a successful Strike with the {{i}}noble branch{{/i}}.\n" +
            "{b}Effect{/b} You channel a rending pulse of energy down your weapon in the moment of contact. The target of the Strike takes force damage equal to the {i}noble branch's{/i} weapon damage dice.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.NobleBranch), q =>
        {
            q.AddExtraKindedDamageOnStrike = (action, target) =>
            {
                if (action.Item?.Runes.Any(rune => rune.ItemName == ikonRune) ?? false)
                {
                    int dice = action.Item.WeaponProperties?.DamageDieCount ?? 0;
                    return new KindedDamage(DiceFormula.FromText($"{2 * dice}", "Gleaming Blade"), Ikon.GetBestDamageKindForSpark(action.Owner, target));
                }
                return null;
            };
        },
        q =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                ExemplarIllustrations.NobleBranch,
                "Strike, Breathe, Rend",
                [ExemplarTraits.Transcendence, ExemplarTraits.Ikon],
                "You channel a rending pulse of energy down your weapon in the moment of contact. The target of the Strike takes force damage equal to the {i}noble branch's{/i} weapon damage dice.",
                Target.Self().WithAdditionalRestriction(self =>
                {
                    var branch = Ikon.GetIkonItem(q.Owner, ikonRune);
                    if (branch == null)
                    {
                        return "You must be wielding the {i}noble branch{/i}.";
                    }
                    var lastAction = self.Actions.ActionHistoryThisTurn.LastOrDefault();
                    if (lastAction == null || !lastAction.HasTrait(Trait.Strike) ||
                        lastAction.CheckResult < CheckResult.Success ||
                        (lastAction.Item != branch))
                    {
                        return "Your last action must be a successful Strike with the {i}noble branch{/i}.";
                    }
                    return null;
                })
            )
            .WithActionCost(1)
            .WithEffectOnSelf(async (action, self) =>
            {
                var branch = Ikon.GetIkonItem(q.Owner, ikonRune);
                var lastAction = self.Actions.ActionHistoryThisEncounter.LastOrDefault();
                int damageDieCount = branch?.WeaponProperties?.DamageDieCount ?? 0;
                var damageDieSize = branch?.WeaponProperties?.DamageDieSize ?? 0;
                bool weaponDieIncreased = false;

                foreach (QEffect qe in self.QEffects)
                {
                    if (!weaponDieIncreased && (qe.IncreaseItemDamageDie?.Invoke(qe, branch!) ?? false))
                    {
                        damageDieSize = DamageDiceUtils.IncreaseDamageDiceByOneStep(damageDieSize);
                        weaponDieIncreased = true;
                    }
                    Func<QEffect, Item, bool>? increaseItemDamageDieCount = qe.IncreaseItemDamageDieCount;
                    if (qe.IncreaseItemDamageDieCount?.Invoke(qe, branch!) ?? false)
                    {
                        damageDieCount++;
                    }
                }
                var diceFormula = DiceFormula.FromText($"{damageDieCount}d{damageDieSize}", "Strike, Breathe, Rend");

                await CommonSpellEffects.DealDirectDamage(action, diceFormula, lastAction!.ChosenTargets.ChosenCreature!, CheckResult.Failure, Ikon.GetBestDamageKindForSpark(self, lastAction!.ChosenTargets.ChosenCreature!));
            }));
        })
        .WithRune(ikonRune)
        .IkonFeat;
    }
}
