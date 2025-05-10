using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_HornOfPlenty
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var horn = new TrueFeat(
                ExemplarFeatNames.IkonHornOfPlenty,
                1,
                "Horn Of Plenty",
                "Whether a bag, gourd, wallet, cornucopia, or similar food receptacle, this ikon recalls the harvest and hearth. " +
                "The receptacle can store up to 1 Bulk of potions and elixirs, but no other items. It can't be opened except by the ikon's immanence and transcendence abilities.\n\n" +
                "{b}Immanence{/b} You can Interact to draw a consumable from the horn and drink it in one action. Other creatures can't access the horn unless you empower it.\n\n" +
                "{b}Transcendence — Feed the Masses (one-action){/b} Mental, Spirit, Transcendence\n" +
                "You Interact to draw a consumable from the horn and then Interact to transfer its effects to a willing ally within 60 feet, as if they had consumed it. " +
                "If it would restore Hit Points, you may divide the healing between you and the ally after rolling.",
                new[] { ModTraits.Ikon },
                null
            )
            .WithPermanentQEffect(null, qf =>
            {
                qf.ProvideMainAction = qf =>
                {
                    var owner = qf.Owner;
                    if (!owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredHornOfPlenty))
                        return null;

                    return new SubmenuPossibility(IllustrationName.MinorHealingElixir, "Horn of Plenty")
                    {
                        Subsections = new[]
                        {
                            new PossibilitySection("Immanence: Draw & Drink")
                            {
                                Possibilities = new List<Possibility>
                                {
                                    (Possibility)new ActionPossibility(
                                        new CombatAction(owner, IllustrationName.MinorHealingElixir, "Draw & Drink",
                                            new[]{ Trait.Interact, ModTraits.Ikon },
                                            "Draw a consumable from the horn and consume it.",
                                            Target.Self())
                                        .WithActionCost(1)
                                        .WithEffectOnEachTarget(async (act, caster, _, _) =>
                                        {
                                            // 1) locate the horn item in your inventory
                                            var hornItem = caster.CarriedItems
                                                .FirstOrDefault(i => i.BaseItemName == ExemplarItemNames.IkonHornOfPlentyItem);
                                            if (hornItem == null)
                                            {
                                                caster.Occupies.Overhead("Horn of Plenty not equipped.", Color.Orange);
                                                return;
                                            }

                                            // 2) pick the first potion/elixir
                                            var consumable = hornItem.StoredItems
                                                .FirstOrDefault(it => it.HasTrait(Trait.Consumable));
                                            if (consumable == null)
                                            {
                                                caster.Occupies.Overhead("Horn is empty.", Color.Orange);
                                                return;
                                            }

                                            // 3) remove it from the horn (memory + modification history)
                                            hornItem.StoredItems.Remove(consumable);
                                            hornItem.ItemModifications.RemoveAll(mod =>
                                                mod.Kind == ItemModificationKind.StoredItem
                                                && mod.StoredItem.ItemName == consumable.ItemName
                                            );

                                            // 4) trigger its drink effect
                                            if (consumable.WhenYouDrink != null)
                                                await consumable.WhenYouDrink(act, caster);
                                        })
                                    )
                                }
                            },

                            new PossibilitySection("Transcendence: Feed the Masses")
                            {
                                Possibilities = new List<Possibility>
                                {
                                    (Possibility)new ActionPossibility(
                                        new CombatAction(owner, IllustrationName.HealCompanion, "Feed the Masses",
                                            new[]{ Trait.Mental, ModTraits.Transcendence, ModTraits.Ikon },
                                            "Draw a consumable and transfer its effects to a willing ally within 60 feet.",
                                            Target.Ranged(60))
                                        .WithActionCost(1)
                                        .WithEffectOnEachTarget(async (act, caster, target, result) =>
                                        {

                                            // same locate + remove logic
                                            var hornItem = caster.CarriedItems
                                                .FirstOrDefault(i => i.BaseItemName == ExemplarItemNames.IkonHornOfPlentyItem);
                                            var consumable = hornItem?.StoredItems
                                                .FirstOrDefault(it => it.HasTrait(Trait.Consumable));
                                            if (hornItem == null || consumable == null)
                                            {
                                                caster.Occupies.Overhead("Horn is empty.", Color.Orange);
                                                return;
                                            }
                                            hornItem.StoredItems.Remove(consumable);
                                            hornItem.ItemModifications.RemoveAll(mod =>
                                                mod.Kind == ItemModificationKind.StoredItem
                                                && mod.StoredItem.ItemName == consumable.ItemName
                                            );

                                            // invoke its drink effect on the ally
                                            if (consumable.WhenYouDrink != null)
                                                await consumable.WhenYouDrink(act, target);
                                        })
                                    )
                                }
                            }
                        }.ToList()
                    };
                };

            });

            ModManager.AddFeat(horn);
        }
    }
}
