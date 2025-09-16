using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class GleamingBlade
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("GleamingBlade", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Gleaming Blade", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonSwordKnife)
            .WithRuneProperties(new RuneProperties("Ikon", IkonRuneKind.Ikon, "This blade glitters with such sharpness it seems to cut the very air in front of it.",
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
                else if ((!weapon.HasTrait(Trait.Sword)) && (!weapon.HasTrait(Trait.Knife)))
                {
                    return "Must be a Sword or a Knife.";
                }
                return null;
            }));
        });

        yield return new Ikon(new Feat(
            ExemplarFeats.GleamingBlade,
            "This blade glitters with such sharpness it seems to cut the very air in front of it.",
            "{b}Usage{/b} a weapon in the sword or knife group, or a melee unarmed attack that deals slashing damage\n\n" +
            "{b}Immanence{/b} Strikes with the {i}gleaming blade{/i} deal 2 additional spirit damage per weapon damage die.\n\n" +
            $"{{b}}Transcendence — Flowing Spirit Strike {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (spirit, transcendence)\nMake two Strikes with the {{i}}gleaming blade{{/i}}, each against the same target and using your current multiple attack penalty. If the {{i}}gleaming blade{{/i}} doesn't have the agile trait, the second Strike takes a –2 penalty. If both attacks hit, you combine their damage, which is all dealt as spirit damage. You add any precision damage only once. Combine the damage from both Strikes and apply resistances and weaknesses only once. This counts as two attacks when calculating your multiple attack penalty.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.GleamingBlade), q =>
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
                ExemplarIllustrations.GleamingBlade,
                "Flowing Spirit Strike",
                [ExemplarTraits.Transcendence],
                "Make two Strikes with the {i}gleaming blade{/i}, each against the same target and using your current multiple attack penalty. If the {i}gleaming blade{/i} doesn't have the agile trait, the second Strike takes a –2 penalty. If both attacks hit, you combine their damage, which is all dealt as spirit damage. You add any precision damage only once. Combine the damage from both Strikes and apply resistances and weaknesses only once. This counts as two attacks when calculating your multiple attack penalty.",
                Target.Reach(Ikon.GetIkonItem(q.Owner, ikonRune)!).WithAdditionalConditionOnTargetCreature(new IkonWieldedTargetingRequirement(ikonRune, "gleaming blade"))
            )
            .WithActionCost(2)
            .WithEffectOnChosenTargets(async (action, self, targets) =>
            {
                var blade = Ikon.GetIkonItem(q.Owner, ikonRune)!;
                var penalty = new QEffect("Gleaming Blade penalty", "[this condition has no description]", ExpirationCondition.Never, self, IllustrationName.None)
                {
                    BonusToAttackRolls = (_, action, target) => (!action.HasTrait(Trait.Agile)) ? new Bonus(-2, BonusType.Untyped, "Gleaming Blade penalty") : null
                };

                StrikeModifiers strikeModifiers = new StrikeModifiers();
                //TODO: it's probably better to use a QEffect with Convert for this
                strikeModifiers.ReplacementDamageKind = Ikon.GetBestDamageKindForSpark(self, targets.ChosenCreature!);
                var map = self.Actions.AttackedThisManyTimesThisTurn;
                var strike = self.CreateStrike(blade, map, strikeModifiers).WithActionCost(0);
                await self.MakeStrike(strike, targets.ChosenCreature!);
                self.AddQEffect(penalty);
                await self.MakeStrike(strike, targets.ChosenCreature!);
                self.RemoveAllQEffects(q => q == penalty);
            }));
        })
        .WithRune(ikonRune)
        .IkonFeat;
    }
}
