using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class BarrowsEdge
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Ikon(new Feat(
            ExemplarFeats.BarrowsEdge,
            "This blade subtly rattles in its scabbard, as if it wants to be unsheathed so it can consume violence.",
            "{b}Usage{/b} melee weapon that deals slashing or piercing damage\n\n" +
            "{b}Immanence{/b} The {i}barrow's edge{/i} deals 1 additional spirit damage per weapon damage die to a creature it Strikes. If the creature is below half its maximum Hit Points, the weapon deals 3 additional spirit damage per weapon damage die instead.\n\n" +
            $"{{b}}Transcendence — Drink of my Foes {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (healing, transcendence, vitality)\n{{b}}Requirements{{/b}} Your last action was a successful Strike with the {{i}}barrow's edge{{/i}}.\nYour blade glows as it absorbs your foe's vitality. You regain Hit Points equal to half the damage dealt.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.BarrowsEdge), (ikon, q) =>
        {
            q.AddExtraKindedDamageOnStrike = (action, target) =>
            {
                if (ikon.IsIkonItem(action.Item))
                {
                    int dice = action.Item!.WeaponProperties?.DamageDieCount ?? 0;
                    return new KindedDamage(DiceFormula.FromText($"{(target.HP <= target.MaxHP / 2 ? 3 * dice : dice)}", "Barrow's Edge"), Ikon.GetBestDamageKindForSpark(action.Owner, target));
                }
                return null;
            };

            q.AddGrantingOfTechnical(cr => cr.EnemyOf(q.Owner), qe =>
            {
                qe.AfterYouTakeDamage = async (qe, damage, _, action, _) =>
                {
                    if (action?.Owner == q.Owner)
                    {
                        q.Tag = damage;
                    }
                };
            });
        }, (ikon, q) =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                ExemplarIllustrations.BarrowsEdge,
                "Drink of my Foes",
                [Trait.Healing, ExemplarTraits.Transcendence, Trait.Positive],
                "Your blade glows as it absorbs your foe's vitality. You regain Hit Points equal to half the damage dealt.",
                Target.Self().WithAdditionalRestriction(self =>
                {
                    var barrow = Ikon.GetHeldIkon(q.Owner, ikon);
                    if (barrow == null)
                    {
                        return "You must be wielding the {i}barrow's edge{/i}.";
                    }
                    var lastAction = self.Actions.ActionHistoryThisTurn.LastOrDefault();
                    if (lastAction == null || !lastAction.HasTrait(Trait.Strike) ||
                        lastAction.CheckResult < CheckResult.Success ||
                        !(ikon.IsIkonItem(lastAction.Item)))
                    {
                        return "Your last action must be a successful Strike with the {i}barrow's edge{/i}.";
                    }
                    return null;
                })
            ).WithActionCost(1)
            .WithEffectOnSelf(async (action, caster) =>
            {
                await caster.HealAsync(((int)(q.Tag ?? 0) / 2).ToString(), action);
            }));
        })
        .WithValidItem(item =>
        {
            if (item.WeaponProperties == null)
            {
                return "Must be a weapon.";
            }
            else if (item.WeaponProperties.DamageKind.ToString() != "Slashing" && item.WeaponProperties.DamageKind.ToString() != "Piercing")
            {
                return "Must be a Slashing or a Piercing Weapon.";
            }
            return null;
        })
        .IkonFeat;
    }
}
