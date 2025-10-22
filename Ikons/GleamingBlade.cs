using System.Collections.Generic;
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
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class GleamingBlade
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Ikon(new Feat(
            ExemplarFeats.GleamingBlade,
            "This blade glitters with such sharpness it seems to cut the very air in front of it.",
            "{b}Usage{/b} a weapon in the sword or knife group, or a melee unarmed attack that deals slashing damage\n\n" +
            "{b}Immanence{/b} Strikes with the {i}gleaming blade{/i} deal 2 additional spirit damage per weapon damage die.\n\n" +
            $"{{b}}Transcendence — Flowing Spirit Strike {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (spirit, transcendence)\nMake two Strikes with the {{i}}gleaming blade{{/i}}, each against the same target and using your current multiple attack penalty. If the {{i}}gleaming blade{{/i}} doesn't have the agile trait, the second Strike takes a –2 penalty. If both attacks hit, you combine their damage, which is all dealt as spirit damage. You add any precision damage only once. Combine the damage from both Strikes and apply resistances and weaknesses only once. This counts as two attacks when calculating your multiple attack penalty.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.GleamingBlade), (ikon, q) =>
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
            var ikonItem = Ikon.GetHeldIkon(q.Owner, ikon);
            var action = new CombatAction(
                q.Owner,
                ExemplarIllustrations.GleamingBlade,
                "Flowing Spirit Strike",
                [ExemplarTraits.Spirit, ExemplarTraits.Transcendence, Trait.AlwaysHits, Trait.IsHostile],
                "Make two Strikes with the {i}gleaming blade{/i}, each against the same target and using your current multiple attack penalty. If the {i}gleaming blade{/i} doesn't have the agile trait, the second Strike takes a –2 penalty. If both attacks hit, you combine their damage, which is all dealt as spirit damage. You add any precision damage only once. Combine the damage from both Strikes and apply resistances and weaknesses only once. This counts as two attacks when calculating your multiple attack penalty.",
                Target.Reach(ikonItem!).WithAdditionalConditionOnTargetCreature(new IkonWieldedTargetingRequirement(ikon, "gleaming blade"))
            )
            .WithActionCost(2)
            .WithEffectOnChosenTargets(async (action, self, targets) =>
            {
                var penalty = new QEffect("Gleaming Blade penalty", "[this condition has no description]", ExpirationCondition.Never, self, IllustrationName.None)
                {
                    BonusToAttackRolls = (_, action, target) => (!action.HasTrait(Trait.Agile)) ? new Bonus(-2, BonusType.Untyped, "Gleaming Blade penalty") : null
                };

                StrikeModifiers strikeModifiers = new StrikeModifiers();
                //TODO: it's probably better to use a QEffect with Convert for this
                //TODO: check Sneak Attack (should only apply once)
                strikeModifiers.ReplacementDamageKind = Ikon.GetBestDamageKindForSpark(self, targets.ChosenCreature!);
                var map = self.Actions.AttackedThisManyTimesThisTurn;
                var strike = self.CreateStrike(ikonItem!, map, strikeModifiers).WithActionCost(0);
                await self.MakeStrike(strike, targets.ChosenCreature!);
                self.AddQEffect(penalty);
                await self.MakeStrike(strike, targets.ChosenCreature!);
                self.RemoveAllQEffects(q => q == penalty);
            });
            if (ikonItem != null)
            {
                var tooltipStrike = q.Owner.CreateStrike(ikonItem, q.Owner.Actions.AttackedThisManyTimesThisTurn).WithActionCost(0);
                action.WithTargetingTooltip((action, target, _) => CombatActionExecution.BreakdownAttackForTooltip(tooltipStrike, target).TooltipDescription);
            }
            return new ActionPossibility(action);
        })
        .WithValidItem(item =>
        {
            if (item.WeaponProperties == null)
            {
                return "Must be a weapon.";
            }
            else if (!((item.HasTrait(Trait.Sword)) || (item.HasTrait(Trait.Knife)) || (item.HasTrait(Trait.Unarmed) && !item.HasTrait(Trait.Ranged) && item.DetermineDamageKinds().Contains(DamageKind.Slashing)) || (item.ItemName == ItemName.HandwrapsOfMightyBlows)))
            {
                return "Must be a sword or a knife, or a melee unarmed attack that deals slashing damage.";
            }
            return null;
        })
        .IkonFeat;
    }
}
