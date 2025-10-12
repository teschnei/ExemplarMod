using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
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

public class TitansBreaker
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("TitansBreaker", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Titan's Breaker", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.Ikon)
            .WithRuneProperties(new RuneProperties("ikon", IkonRuneKind.Ikon, "You wield a weapon whose blows shatter mountains with ease.",
            "This item grants the {i}immanence{/i} and {i}transcendence{/i} abilities of the Titan's Breaker when empowered.", item =>
            {
                item.Traits.AddRange([ExemplarTraits.Ikon, Trait.Divine]);
            })
            .WithCanBeAppliedTo((Item rune, Item weapon) =>
            {
                if (weapon.WeaponProperties == null)
                {
                    return "Must be a weapon.";
                }
                else if ((!weapon.HasTrait(Trait.Club)) && (!weapon.HasTrait(Trait.Hammer)) && (!weapon.HasTrait(Trait.Axe)) && (!weapon.HasTrait(Trait.Unarmed)))
                {
                    return "Must be a Club, Axe, Hammer or Unarmed.";
                }
                return null;
            }));
        });

        yield return new Ikon(new Feat(
            ExemplarFeats.TitansBreaker,
            "You wield a weapon whose blows shatter mountains with ease.",
            "{b}Usage{/b} any melee weapon in the club, hammer, or axe group, or any melee unarmed Strikes that deals bludgeoning damage\n\n" +
            "{b}Immanence{/b} The {i}titan's breaker{/i} deals 2 additional spirit damage per weapon damage die to creatures it Strikes. " +
            /*"Constructs and objects are not immune to this spirit damage, and this*/ "This spirit damage bypasses hardness equal to your level.\n\n" +
            $"{{b}}Transcendence — Fracture Mountains {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (spirit, transcendence)\n" +
            "Your spirit is so dense it takes on tangible force. Make a melee Strike with the {i}titan's breaker{/i}. This counts as two attacks " +
            "when calculating your multiple attack penalty. If this Strike hits, your additional spirit damage from the ikon's immanence increases to 4 plus " +
            "an extra die of weapon damage. If you're at least 10th level, it's increased to 6 spirit damage and two extra dice, and if you're at least 18th level " +
            "it's increased to 8 spirit damage and three extra dice.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.TitansBreaker), q =>
        {
            //Flag for "is this Strike from Fracture Mountains?"
            q.Tag = false;

            q.AddExtraKindedDamageOnStrike = (action, target) =>
            {
                if (action.Item?.Runes.Any(rune => rune.ItemName == ikonRune) ?? false)
                {
                    if ((bool)q.Tag == false)
                    {
                        int dice = action.Item.WeaponProperties?.DamageDieCount ?? 0;
                        return new KindedDamage(DiceFormula.FromText($"{2 * dice}", "Titan's Breaker"), Ikon.GetBestDamageKindForSpark(action.Owner, target));
                    }
                    else
                    {
                        var breaker = action.Item;
                        var damageDieSize = breaker?.WeaponProperties?.DamageDieSize ?? 0;
                        bool weaponDieIncreased = false;

                        foreach (QEffect qe in action.Owner.QEffects)
                        {
                            if (!weaponDieIncreased && (qe.IncreaseItemDamageDie?.Invoke(qe, breaker!) ?? false))
                            {
                                damageDieSize = DamageDiceUtils.IncreaseDamageDiceByOneStep(damageDieSize);
                                weaponDieIncreased = true;
                            }
                        }

                        int flatBonus = 0;
                        int extraDiceCount = 0;
                        if (action.Owner.Level >= 18)
                        {
                            flatBonus = 8;
                            extraDiceCount = 3;
                        }
                        else if (action.Owner.Level >= 10)
                        {
                            flatBonus = 6;
                            extraDiceCount = 2;
                        }
                        else
                        {
                            flatBonus = 4;
                            extraDiceCount = 1;
                        }
                        return new KindedDamage(DiceFormula.FromText($"{extraDiceCount}d{damageDieSize}+{flatBonus}", "Fracture Mountains"), Ikon.GetBestDamageKindForSpark(action.Owner, target));
                    }
                }
                return null;
            };
        },
        q =>
        {
            var breaker = Ikon.GetIkonItem(q.Owner, ikonRune)!;
            var action = new CombatAction(
                q.Owner,
                ExemplarIllustrations.TitansBreaker,
                "Fracture Mountains",
                [ExemplarTraits.Spirit, ExemplarTraits.Transcendence],
                "Your spirit is so dense it takes on tangible force. Make a melee Strike with the {i}titan's breaker{/i}. This counts as two attacks " +
                "when calculating your multiple attack penalty. If this Strike hits, your additional spirit damage from the ikon's immanence increases to 4 plus " +
                "an extra die of weapon damage. If you're at least 10th level, it's increased to 6 spirit damage and two extra dice, and if you're at least 18th level " +
                "it's increased to 8 spirit damage and three extra dice.",
                Target.Reach(Ikon.GetIkonItem(q.Owner, ikonRune)!).WithAdditionalConditionOnTargetCreature(new IkonWieldedTargetingRequirement(ikonRune, "titan's breaker"))
            )
            .WithActionCost(2)
            .WithEffectOnChosenTargets(async (action, self, targets) =>
            {
                //Activate the Immanence's bigger damage mode
                q.Tag = true;
                await self.MakeStrike(targets.ChosenCreature!, breaker);
                self.Actions.AttackedThisManyTimesThisTurn++;
                q.Tag = false;
            });
            if (breaker != null)
            {
                var tooltipStrike = q.Owner.CreateStrike(breaker, q.Owner.Actions.AttackedThisManyTimesThisTurn).WithActionCost(0);
                action.WithActiveRollSpecification(new ActiveRollSpecification(Utility.Attack(tooltipStrike, breaker, -1), TaggedChecks.DefenseDC(Defense.AC)))
                .WithNoSaveFor((action, cr) => true);
            }
            return new ActionPossibility(action);
        })
        .WithRune(ikonRune)
        .IkonFeat;
    }
}
