using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
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
        var ikon = new Ikon(new Feat(
            ExemplarFeats.ShadowSheath,
            "With an infinite array of darts, throwing knives, or similar weapons, you never need to worry about being unarmed.",
            "{b}Usage{/b} a one-handed thrown weapon of light Bulk or less\n\n" +
            "Your weapon becomes a worn item. You can Interact to draw an exact copy of your weapon from thin air. These copies retain the runes and abilities of the hidden weapon. A copy disappears shortly after leaving your hand (or being used for a thrown Strike).\n" +
            "{b}Immanence{/b} You can Interact to draw a weapon from the {i}shadow sheath{/i} as a free action. Your Strikes with a weapon produced from the {i}shadow sheath{/i} deal 2 additional spirit damage per weapon damage die, which increases to 3 per die if the target is off-guard.\n\n" +
            $"{{b}}Transcendence — Liar's Hidden Blade {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (spirit, transcendence)\n" +
            "{b}Requirements{/b} Your previous action was an unsuccessful Strike with the weapon from the {i}shadow sheath{/i}; {b}Effect{/b} The shadow weapon you threw fades, the distraction covering " +
            "your true intention all along—a second strike hidden in the blind spot of the first! Interact to draw another weapon from the {i}shadow sheath{/i}, then Strike with it at the same multiple attack penalty as the unsuccessful attack. " +
            "The opponent is off-guard to this attack. This strike counts towards your multiple attack penalty as normal. After the Strike resolves, you automatically draw another weapon from the {i}shadow sheath{/i}.",
            [ExemplarTraits.Ikon, Trait.Extradimensional, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.ShadowSheath), (ikon, q) =>
        {
            q.AddExtraKindedDamageOnStrike = (action, target) =>
            {
                if (ikon.IsIkonItem(action.Item))
                {
                    int dice = action.Item?.WeaponProperties?.DamageDieCount ?? 0;
                    int multiplier = target.IsFlatFootedTo(action.Owner, action) ? 3 : 2;
                    return new KindedDamage(DiceFormula.FromText($"{multiplier * dice}", "Shadow Sheath"), Ikon.GetBestDamageKindForSpark(action.Owner, target));
                }
                return null;
            };
        },
        (ikon, q) =>
        {
            var ikonItem = (Item?)q.Owner.QEffects.Where(q => q.Id == ExemplarQEffects.ShadowSheathItemStorage).FirstOrDefault()?.Tag;
            var lastAction = q.Owner.Actions.ActionHistoryThisTurn.LastOrDefault();
            var lastIkon = ikon.IsIkonItem(lastAction?.Item) ? lastAction?.Item : null;
            bool thrown = lastAction?.HasTrait(Trait.Thrown) ?? false;
            var action = new CombatAction(
                q.Owner,
                ExemplarIllustrations.ShadowSheath,
                "Liar's Hidden Blade",
                [ExemplarTraits.Spirit, ExemplarTraits.Transcendence, Trait.AlwaysHits, Trait.IsHostile],
                "The shadow weapon you threw previously fades, the distraction covering " +
                "your true intention all along—a second strike hidden in the blind spot of the first! Interact to draw another weapon from the {i}shadow sheath{/i}, then Strike with it at the same multiple attack penalty as the unsuccessful attack. " +
                "The opponent is off-guard to this attack. This strike counts towards your multiple attack penalty as normal. After the Strike resolves, you can Interact to draw another weapon from the {i}shadow sheath{/i}.",
                ikonItem == null ? Target.Uncastable("You must have a weapon produced from the {i}shadow sheath{/i}.") : ikonItem.DetermineStrikeTarget(thrown ? RangeKind.Ranged : RangeKind.Melee).WithAdditionalConditionOnTargetCreature((self, target) =>
                {
                    if (lastAction == null || !lastAction.HasTrait(Trait.Strike) ||
                        lastAction.CheckResult >= CheckResult.Success ||
                        (lastIkon == null))
                    {
                        return Usability.NotUsable("Your last action must be an unsuccessful Strike with a weapon produced from the {i}shadow sheath{/i}.");
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
                var item = ikonItem!.Duplicate();
                item.Traits.Add(Trait.HandEphemeral);
                var strike = StrikeRules.CreateStrike(self, item, thrown ? RangeKind.Ranged : RangeKind.Melee, self.Actions.AttackedThisManyTimesThisTurn - 1, thrown).WithActionCost(0);
                if (thrown)
                {
                    strike.WithSoundEffect(item?.WeaponProperties?.Sfx ?? SfxName.Bow);
                }
                strike.ChosenTargets = ChosenTargets.CreateSingleTarget(targets.ChosenCreature!);
                var offguard = QEffect.FlatFooted("Liar's Hidden Blade");
                targets.ChosenCreature!.AddQEffect(offguard);
                await strike.AllExecute();
                targets.ChosenCreature.RemoveAllQEffects(q => q == offguard);
            });
            if (ikonItem != null)
            {
                var strike = StrikeRules.CreateStrike(q.Owner, ikonItem, thrown ? RangeKind.Ranged : RangeKind.Melee, q.Owner.Actions.AttackedThisManyTimesThisTurn - 1, thrown).WithActionCost(0);
                action.WithTargetingTooltip((action, target, _) => CombatActionExecution.BreakdownAttackForTooltip(strike, target).TooltipDescription);
            }
            if (thrown)
            {
                action.Traits.Add(Trait.Ranged);
            }
            return new ActionPossibility(action);
        })
        .WithValidItem(item =>
        {
            if (item.WeaponProperties == null)
            {
                return "Must be a weapon.";
            }
            if (!item.WeaponProperties.Throwable || item.HasTrait(Trait.TwoHanded))
            {
                return "Must be a one-handed thrown weapon.";
            }
            //TODO: there's no bulk on weapons in DD
            return null;
        });
        ikon.IkonFeat.WithPermanentQEffect(null, q =>
        {
            q.Id = ExemplarQEffects.ShadowSheathItemStorage;
            q.StartOfCombat = async q =>
            {
                var ikonItem = Ikon.GetHeldIkon(q.Owner, ikon) ?? Ikon.GetWornIkon(q.Owner, ikon);
                if (ikonItem != null)
                {
                    q.Tag = ikonItem;
                    var item = ikonItem.Duplicate();
                    item.Traits.Add(Trait.HandEphemeral);
                    if (q.Owner.CarriedItems.Remove(ikonItem))
                    {
                        q.Owner.CarriedItems.Add(item);
                    }
                    else if (q.Owner.HeldItems.Remove(ikonItem))
                    {
                        q.Owner.HeldItems.Add(item);
                    }
                }
            };
            q.ProvideSectionIntoSubmenu = (q, submenu) =>
            {
                if ((submenu.Caption == "Left hand" || submenu.Caption == "Right hand") && q.Tag is Item ikonItem)
                {
                    return new PossibilitySection("Shadow Sheath")
                    {
                        Possibilities = [
                            new ActionPossibility(new CombatAction(q.Owner, ikonItem.Illustration, $"Draw {ikonItem.Name} from Shadow Sheath", [Trait.Manipulate, Trait.Basic], $"Draw a {ikonItem.Name} from the Shadow Sheath.\n-----\n{ikonItem.GetItemDescription()}", Target.Self())
                                .WithActionCost(q.Owner.HasEffect(ikon.EmpoweredQEffectId) ? 0 : 1)
                                .WithEffectOnSelf(async (action, self) =>
                                {
                                    var item = ikonItem.Duplicate();
                                    item.Traits.Add(Trait.HandEphemeral);
                                    self.HeldItems.Add(item);
                                }))
                        ]
                    };
                }
                return null;
            };
            /*
            q.ProvideActionIntoPossibilitySection = (q, section) =>
            {
                if (section.PossibilitySectionId == PossibilitySectionId.MainActions && q.Tag is Item ikonItem && q.Owner.HeldItems.Count < 2)
                {
                    var item = ikonItem.Duplicate();
                    item.Traits.Add(Trait.HandEphemeral);
                    q.Owner.HeldItems.Add(item);
                    var bonusActionCost = q.Owner.HasEffect(ikon.EmpoweredQEffectId) ? 0 : 1;
                    List<Possibility> list =
                        (from strike in q.Owner.QEffects.Select(qf =>
                        {
                            CombatAction? combatAction2 = qf.ProvideStrikeModifier?.Invoke(item);
                            if (combatAction2 != null)
                            {
                                combatAction2.ContextMenuName = combatAction2.Name + " (" + item.Name + ")";
                            }
                            return combatAction2;
                        }).WhereNotNull()
                         select (Possibility)(new ActionPossibility(strike))
                    ).ToList();
                    list.AddRange(q.Owner.QEffects.Select((QEffect qf) => qf.ProvideStrikeModifierAsPossibility?.Invoke(item)).WhereNotNull());
                    foreach (QEffect qEffect in q.Owner.QEffects)
                    {
                        if (qEffect.ProvideStrikeModifierAsPossibilities != null)
                        {
                            list.AddRange(qEffect.ProvideStrikeModifierAsPossibilities(qEffect, item));
                        }
                    }
                    WeaponProperties? weaponProperties = item.WeaponProperties;
                    if (weaponProperties != null && weaponProperties.ForcedMelee && weaponProperties.Throwable)
                    {
                        list.Add((ActionPossibility)StrikeRules.CreateStrike(q.Owner, item, RangeKind.Ranged, -1, thrown: true));
                    }
                    CombatAction combatAction = q.Owner.CreateStrike(item);
                    if (list.Count > 0)
                    {
                        foreach (var poss in list)
                        {
                            if (poss is ActionPossibility actionPoss)
                            {
                                actionPoss.CombatAction.ActionCost = actionPoss.CombatAction.ActionCost + bonusActionCost;
                            }
                        }
                        SubmenuPossibility submenuPossibility = new SubmenuPossibility(combatAction.Illustration, combatAction.Name);
                        submenuPossibility.SpellIfAny = combatAction;
                        submenuPossibility.Subsections.Add(new PossibilitySection(combatAction.Name)
                        {
                            Possibilities = new ActionPossibility[1] { combatAction.WithActionCost(combatAction.ActionCost + bonusActionCost) }.Concat(list).ToList()
                        });
                        return submenuPossibility.WithPossibilityGroup("Strike");
                    }
                    return new ActionPossibility(combatAction.WithActionCost(combatAction.ActionCost + bonusActionCost)).WithPossibilityGroup("Strike");
                }
                return null;
            };
            */
            q.EndOfCombat = async (q, _) =>
            {
                if (q.Tag is Item ikonItem)
                {
                    if (q.Owner.HeldItems.Count < 2)
                    {
                        q.Owner.HeldItems.Add(ikonItem);
                    }
                    else
                    {
                        q.Owner.CarriedItems.Add(ikonItem);
                    }
                }
            };
        });

        ModManager.RegisterActionOnEachActionPossibility(action =>
        {
            if (action.ActionId == ActionId.DrawItem && ikon.IsIkonItem(action.Item) && action.Owner.HasEffect(ikon.EmpoweredQEffectId))
            {
                action.ActionCost = 0;
            }
        });
        yield return ikon.IkonFeat;
    }
}
