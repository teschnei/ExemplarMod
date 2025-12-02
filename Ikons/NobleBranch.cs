using System;
using System.Collections.Generic;
using System.Linq;
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
        yield return new Ikon(new Feat(
            ExemplarFeats.NobleBranch,
            "This humble stick-like weapon has an elegant simplicity to it, affording you reliable strikes over flashy maneuvers.",
            "{b}Usage{/b} a staff, bo staff, fighting stick, khakkara, or any weapon in the spear or polearm weapon group\n\n" +
            "{b}Immanence{/b} The {i}noble branch{/i} deals 2 additional spirit damage per weapon damage die to creatures it Strikes.\n\n" +
            $"{{b}}Transcendence — Strike, Breathe, Rend {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (spirit, transcendence)\n{{b}}Requirements{{/b}} Your last action this turn was a successful Strike with the {{i}}noble branch{{/i}}.\n" +
            "{b}Effect{/b} You channel a rending pulse of energy down your weapon in the moment of contact. The target of the Strike takes spirit damage equal to the {i}noble branch's{/i} weapon damage dice.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.NobleBranch), (ikon, q) =>
        {
            q.AddExtraKindedDamageOnStrike = (action, target) =>
            {
                if (ikon.IsIkonItem(action.Item))
                {
                    int dice = action.Item?.WeaponProperties?.DamageDieCount ?? 0;
                    return new KindedDamage(DiceFormula.FromText($"{2 * dice}", "Gleaming Blade"), Ikon.GetBestDamageKindForSpark(action.Owner, target));
                }
                return null;
            };
        },
        (ikon, q) =>
        {
            var heldIkon = ikon.GetHeldIkon(q.Owner);
            var lastAction = q.Owner.Actions.ActionHistoryThisTurn.LastOrDefault();
            var lastIkon = ikon.IsIkonItem(lastAction?.Item) ? lastAction?.Item : null;
            return new ActionPossibility(new CombatAction(
                q.Owner,
                ExemplarIllustrations.NobleBranch,
                "Strike, Breathe, Rend",
                [ExemplarTraits.Spirit, ExemplarTraits.Transcendence],
                "You channel a rending pulse of energy down your weapon in the moment of contact. The target of the Strike takes spirit damage equal to the {i}noble branch's{/i} weapon damage dice.",
                Target.Self().WithAdditionalRestriction(self =>
                {
                    if (heldIkon == null)
                    {
                        return "You must be wielding the {i}noble branch{/i}.";
                    }
                    var lastAction = self.Actions.ActionHistoryThisTurn.LastOrDefault();
                    if (lastAction == null || !lastAction.HasTrait(Trait.Strike) ||
                        lastAction.CheckResult < CheckResult.Success ||
                        (lastIkon == null))
                    {
                        return "Your last action must be a successful Strike with the {i}noble branch{/i}.";
                    }
                    return null;
                })
            )
            .WithActionCost(1)
            .WithEffectOnSelf(async (action, self) =>
            {
                var lastAction = self.Actions.ActionHistoryThisEncounter.LastOrDefault();
                int damageDieCount = lastIkon?.WeaponProperties?.DamageDieCount ?? 0;
                var damageDieSize = lastIkon?.WeaponProperties?.DamageDieSize ?? 0;
                bool weaponDieIncreased = false;

                foreach (QEffect qe in self.QEffects)
                {
                    if (!weaponDieIncreased && (qe.IncreaseItemDamageDie?.Invoke(qe, lastIkon!) ?? false))
                    {
                        damageDieSize = DamageDiceUtils.IncreaseDamageDiceByOneStep(damageDieSize);
                        weaponDieIncreased = true;
                    }
                    Func<QEffect, Item, bool>? increaseItemDamageDieCount = qe.IncreaseItemDamageDieCount;
                    if (qe.IncreaseItemDamageDieCount?.Invoke(qe, lastIkon!) ?? false)
                    {
                        damageDieCount++;
                    }
                }
                var diceFormula = DiceFormula.FromText($"{damageDieCount}d{damageDieSize}", "Strike, Breathe, Rend");

                await CommonSpellEffects.DealDirectDamage(action, diceFormula, lastAction!.ChosenTargets.ChosenCreature!, CheckResult.Failure, Ikon.GetBestDamageKindForSpark(self, lastAction!.ChosenTargets.ChosenCreature!));
            }));
        })
        .WithValidItem(item =>
        {
            bool tryKhakkhara = ModManager.TryParse<ItemName>("Khakkhara", out var khakkhara);
            bool tryBoStaff = ModManager.TryParse<ItemName>("BoStaff", out var boStaff);
            bool tryFightingStick = ModManager.TryParse<ItemName>("FightingStick", out var fightingStick);
            if (item.WeaponProperties == null)
            {
                return "Must be a weapon.";
            }
            if ((!item.HasTrait(Trait.Staff)) && (!item.HasTrait(Trait.Spear)) && (!item.HasTrait(Trait.Polearm)) &&
                 !(tryKhakkhara && item.BaseItemName == khakkhara) && !(tryBoStaff && item.BaseItemName == boStaff) && !(tryFightingStick && item.BaseItemName == fightingStick))
            {
                return "Must be a staff, bo staff, fighting stick, khakkara, or any weapon in the spear or polearm weapon group";
            }
            return null;
        })
        .IkonFeat;
    }
}
