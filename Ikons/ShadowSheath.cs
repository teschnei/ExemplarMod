using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Audio;
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

public class ShadowSheath
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("ShadowSheath", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Shadow Sheath", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonThrown)
            .WithRuneProperties(new RuneProperties("ikon", IkonRuneKind.Ikon, "With an infinite array of darts, throwing knives, or similar weapons, you never need to worry about being unarmed.",
            "This item grants the {i}immanence{/i} and {i}transcendence{/i} abilities of the Shadow Sheath when empowered.", item =>
            {
                item.Traits.AddRange([ExemplarTraits.Ikon, Trait.Divine, Trait.Returning]);
            })
            .WithCanBeAppliedTo((Item rune, Item weapon) =>
            {
                if (weapon.WeaponProperties == null)
                {
                    return "Must be a weapon.";
                }
                if (!weapon.WeaponProperties.Throwable || weapon.HasTrait(Trait.TwoHanded))
                {
                    return "Must be a one-handed thrown weapon.";
                }
                //TODO: there's no bulk on weapons in DD
                return null;
            }));
        });

        ModManager.RegisterActionOnEachActionPossibility(action =>
        {
            if (action.ActionId == ActionId.DrawItem && (action.Item?.Runes.Any(rune => rune.ItemName == ikonRune) ?? false))
            {
                action.ActionCost = 0;
            }
        });

        yield return new Ikon(new Feat(
            ExemplarFeats.ShadowSheath,
            "With an infinite array of darts, throwing knives, or similar weapons, you never need to worry about being unarmed.",
            "{b}Usage{/b} a one-handed thrown weapon of light Bulk or less\n\n" +
            "{b}Immanence{/b} You can Interact to draw a weapon from the {i}shadow sheath{/i} as a free action. Your Strikes with a weapon produced from the {i}shadow sheath{/i} deal 2 additional spirit damage per weapon damage die, which increases to 3 per die if the target is off-guard. " +
            "In addition, after you make a thrown Strike, you automatically draw another weapon from the {i}shadow sheath{/i}\n\n" +
            $"{{b}}Transcendence — Liar's Hidden Blade {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (spirit, transcendence)\n" +
            "{b}Requirements{/b} Your previous action was an unsuccessful Strike with the weapon from the {i}shadow sheath{/i}; {b}Effect{/b} The shadow weapon you threw fades, the distraction covering " +
            "your true intention all along—a second strike hidden in the blind spot of the first! Interact to draw another weapon from the {i}shadow sheath{/i}, then Strike with it at the same multiple attack penalty as the unsuccessful attack. " +
            "The opponent is off-guard to this attack. This strike counts towards your multiple attack penalty as normal. After the Strike resolves, you automatically draw another weapon from the {i}shadow sheath{/i}.",
            [ExemplarTraits.Ikon, Trait.Extradimensional, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.ShadowSheath), q =>
        {
            q.AddExtraKindedDamageOnStrike = (action, target) =>
            {
                if (action.Item?.Runes.Any(rune => rune.ItemName == ikonRune) ?? false)
                {
                    int dice = action.Item.WeaponProperties?.DamageDieCount ?? 0;
                    int multiplier = target.IsFlatFootedTo(action.Owner, action) ? 3 : 2;
                    return new KindedDamage(DiceFormula.FromText($"{multiplier * dice}", "Shadow Sheath"), Ikon.GetBestDamageKindForSpark(action.Owner, target));
                }
                return null;
            };
        },
        q =>
        {
            var heldIkon = Ikon.GetIkonItem(q.Owner, ikonRune);
            var lastAction = q.Owner.Actions.ActionHistoryThisTurn.LastOrDefault();
            var lastIkon = lastAction?.Item?.Runes.Any(r => r.ItemName == ikonRune) ?? false ? lastAction.Item : null;
            bool thrown = lastAction?.HasTrait(Trait.Thrown) ?? false;
            var action = new CombatAction(
                q.Owner,
                ExemplarIllustrations.ShadowSheath,
                "Liar's Hidden Blade",
                [ExemplarTraits.Spirit, ExemplarTraits.Transcendence],
                "The shadow weapon you threw previously fades, the distraction covering " +
                "your true intention all along—a second strike hidden in the blind spot of the first! Interact to draw another weapon from the {i}shadow sheath{/i}, then Strike with it at the same multiple attack penalty as the unsuccessful attack. " +
                "The opponent is off-guard to this attack. This strike counts towards your multiple attack penalty as normal. After the Strike resolves, you can Interact to draw another weapon from the {i}shadow sheath{/i}.",
                heldIkon == null ? Target.Uncastable("You must be wielding a weapon produced from the {i}shadow sheath{/i}.") : heldIkon.DetermineStrikeTarget(thrown ? RangeKind.Ranged : RangeKind.Melee).WithAdditionalConditionOnTargetCreature((self, target) =>
                {
                    if (lastAction == null || !lastAction.HasTrait(Trait.Strike) ||
                        lastAction.CheckResult >= CheckResult.Success ||
                        (lastIkon == null))
                    {
                        return Usability.NotUsable("Your last action must be an unsuccessful Strike with the {i}shadow sheath{/i}.");
                    }
                    if (lastAction.ChosenTargets.ChosenCreature != target)
                    {
                        return Usability.NotUsableOnThisCreature("You must target the same creature as your previous Strike.");
                    }
                    return Usability.Usable;
                })
            )
            .WithActionCost(1)
            .WithEffectOnChosenTargets(async (action, self, targets) =>
            {
                var strike = StrikeRules.CreateStrike(self, lastIkon!, thrown ? RangeKind.Ranged : RangeKind.Melee, self.Actions.AttackedThisManyTimesThisTurn - 1, thrown).WithActionCost(0);
                if (thrown)
                {
                    strike.WithSoundEffect(lastIkon?.WeaponProperties?.Sfx ?? SfxName.Bow);
                }
                strike.ChosenTargets = ChosenTargets.CreateSingleTarget(targets.ChosenCreature!);
                var offguard = QEffect.FlatFooted("Liar's Hidden Blade");
                targets.ChosenCreature!.AddQEffect(offguard);
                await strike.AllExecute();
                targets.ChosenCreature.RemoveAllQEffects(q => q == offguard);
            });
            if (lastIkon != null)
            {
                var strike = StrikeRules.CreateStrike(q.Owner, lastIkon, thrown ? RangeKind.Ranged : RangeKind.Melee, q.Owner.Actions.AttackedThisManyTimesThisTurn - 1, thrown).WithActionCost(0);
                action.WithActiveRollSpecification(new ActiveRollSpecification(Utility.Attack(strike, lastIkon!, q.Owner.Actions.AttackedThisManyTimesThisTurn - 1), TaggedChecks.DefenseDC(Defense.AC)))
                .WithNoSaveFor((action, cr) => true);
            }
            if (thrown)
            {
                action.Traits.Add(Trait.Ranged);
            }
            return new ActionPossibility(action);
        })
        .WithRune(ikonRune)
        .IkonFeat;
    }
}
