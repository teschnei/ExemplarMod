using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Feats.Features;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Mods.Classes.Exemplar.Epithets;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public static class ExemplarBaseClass
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> CreateExemplarClassFeat()
    {
        yield return new ClassSelectionFeat(
            ExemplarFeats.ExemplarClass,
            "You carry a divine spark forged from your own soul. By empowering sacred ikons, you unleash transcendent power.",
            ExemplarTraits.Exemplar,
            new LimitedAbilityBoost(Ability.Strength, Ability.Dexterity),
            10,
            [
                Trait.Reflex, Trait.Perception,
                Trait.Religion,
                Trait.Simple, Trait.Martial, Trait.Unarmed,
                Trait.LightArmor, Trait.MediumArmor, Trait.UnarmoredDefense
            ],
            [Trait.Fortitude, Trait.Will],
            3,
            "{b}Divine Spark and Ikons{/b}\n" +
            "You can focus your divine power through special items known as ikons. You can select 3 ikons. You can empower one ikon with your divine spark. Each ikon has both a passive immanence effect and an active transcendence effect. You can place your divine spark into an item using the Shift Immanence action.\n\n" +
            "{b}Spark Transcendence{/b}\n" +
            "When your spark dwells within an ikon, you get that ikon's immanence effect continually. However, you can also Spark Transcendence in a mighty deed, channeling your divinity through the ikon - though when you Spark Transcendence, the force of the act temporarily casts your divine spark out of the ikon.",
            null
        )
        .WithClassFeatures(features =>
        {
            features.AddFeature(3, "Root Epithet", "Your epithet is born. You learn a Root Epithet feat of your choice.")
                .AddFeature(5, "Weapon Expertise", "Your proficiency ranks for unarmed attacks, simple weapons, and martial weapons increase to expert.")
                .AddFeature(7, "Dominion Epithet", "Your epithet grows. You learn a Dominion Epithet feat of your choice.")
                .AddFeature(7, "Spirit Striking", "You deal 2 additional spirit damage with weapons and unarmed attacks in which you are an expert. This damage increases to 3 if you're a master, and 4 if you're legendary.")
                .AddFeature(7, "Unassailable Soul", "Your proficiency rank for Will saves increases to master. When you roll a success at a Will save, you get a critical success instead.")
                .AddFeature(9, WellKnownClassFeature.ExpertInReflex)
                .AddFeature(9, WellKnownClassFeature.ExpertInClassDC)
                .AddFeature(9, WellKnownClassFeature.ExpertInPerception)
                .AddFeature(13, "Godly Expertise", "Your proficiency ranks for unarmed attacks, simple weapons, and martial weapons increase to master.")
                .AddFeature(13, "Greater Unassailable Soul", "Your proficiency rank for Will saves increases to legendary. When you roll a success at a Will save, you get a critical success instead. When you roll a critical failure at a Will save, you get a failure instead. When you fail a Will save against a damaging effect, you take half damage.")
                .AddFeature(13, "Burnished Armor Expertise", "Your proficiency ranks for light armor, medium armor, and unarmored defense increases to expert.");
        })
        .WithOnSheet(sheet =>
        {
            sheet.GrantFeat(FeatName.ShieldBlock);
            sheet.GrantFeat(FeatName.DeadlySimplicity);
            sheet.AddSelectionOption(new MultipleFeatSelectionOption("Ikon", "Ikon", 1, ft => ft.HasTrait(ExemplarTraits.Ikon), 3));
            sheet.AddSelectionOption(new SingleFeatSelectionOption("ExemplarFeat1", "Exemplar Feat", 1, ft => ft.HasTrait(ExemplarTraits.Exemplar)));
            sheet.AddSelectionOption(new SingleFeatSelectionOption("RootEpithet", "Root Epithet", 3, ft => ft.HasTrait(ExemplarTraits.RootEpithet)));
            sheet.AddSelectionOption(new SingleFeatSelectionOption("DominionEpithet", "Dominion Epithet", 7, ft => ft.HasTrait(ExemplarTraits.DominionEpithet)));

            sheet.AddAtLevel(5, values =>
            {
                values.SetProficiency(Trait.Simple, Proficiency.Expert);
                values.SetProficiency(Trait.Martial, Proficiency.Expert);
                values.SetProficiency(Trait.Unarmed, Proficiency.Expert);
            });

            sheet.AddAtLevel(7, values =>
            {
                values.SetProficiency(Trait.Will, Proficiency.Master);
            });
            sheet.AtEndOfRecalculation += (sheet) =>
            {
                EnsureCorrectRunes(sheet);
            };
        })
        .WithPermanentQEffect(null, q =>
        {
            q.Id = ExemplarQEffects.ShiftImmanence;
            q.ProvideMainAction = qf =>
            {
                var ikons = (qf.Owner.PersistentCharacterSheet?.Calculated?.AllFeats ?? Enumerable.Empty<Feat>())
                    .Where(f => f.HasTrait(ExemplarTraits.Ikon) && f != qf.Tag).Select(f => new ActionPossibility(Ikon.IkonLUT[f.FeatName].ShiftImmanence(qf.Owner)) as Possibility);

                return new SubmenuPossibility(IllustrationName.SpiritualWeapon, "Shift Immanence")
                {
                    Subsections =
                    [
                        new PossibilitySection("Select Ikon to Empower")
                        {
                            Possibilities = ikons.ToList()
                        }
                    ]
                };
            };

            q.StartOfCombat = async qf =>
            {
                await EmpowerIkon(qf.Owner, qf);
            };

            q.AfterYouTakeAction = async (qf, action) =>
            {
                if (action.HasTrait(ExemplarTraits.Transcendence))
                {
                    await EmpowerIkon(qf.Owner, qf);
                }
            };

            async Task EmpowerIkon(Creature exemplar, QEffect shiftImmanence)
            {
                var shifts = Possibilities.Create(exemplar).Filter(ap => false);
                shifts.Sections.Add(new PossibilitySection("Shift Immanence")
                {
                    Possibilities = (exemplar.PersistentCharacterSheet?.Calculated?.AllFeats ?? Enumerable.Empty<Feat>())
                        .Where(f => f.HasTrait(ExemplarTraits.Ikon) && f != shiftImmanence.Tag).Select(f =>
                        {
                            var ap = new ActionPossibility(Ikon.IkonLUT[f.FeatName].ShiftImmanence(exemplar).WithActionCost(0));
                            ap.RecalculateUsability();
                            return ap as Possibility;
                        }).ToList()
                });
                shifts.Sections.Add(new PossibilitySection("Pass")
                {
                    Possibilities = [new ActionPossibility(new CombatAction(exemplar, IllustrationName.EndTurn, "Pass", [
                        Trait.Basic,
                        Trait.UsableEvenWhenUnconsciousOrParalyzed,
                        Trait.DoesNotPreventDelay
                    ], "Do nothing.", Target.Self()).WithActionCost(0))]
                });
                var active = exemplar.Battle.ActiveCreature;
                exemplar.Battle.ActiveCreature = exemplar;
                typeof(Creature).InvokeMember("Possibilities", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, exemplar, [shifts]);
                var options = await exemplar.Battle.GameLoop.CreateActions(exemplar, exemplar.Possibilities, null);
                exemplar.Battle.GameLoopCallback.AfterActiveCreaturePossibilitiesRegenerated();
                await exemplar.Battle.GameLoop.OfferOptions(exemplar, options, true);
                exemplar.Battle.ActiveCreature = active;
            }
        })
        .WithOnCreature((sheet, cr) =>
        {
            if (cr.Level >= 7)
            {
                cr.AddQEffect(new QEffect("Spirit Striking", "+2 spirit damage on expert weapons and unarmed attacks.")
                {
                    AddExtraStrikeDamage = (action, defender) =>
                    {
                        var item = action.Item;
                        if (item != null && item.HasTrait(Trait.Simple) && (int)cr.GetProficiency(item) >= (int)Proficiency.Expert)
                        {
                            return (DiceFormula.FromText("2", "Spirit Striking"), DamageKind.Untyped);
                        }
                        return null;
                    }
                });

                cr.AddQEffect(CreateUnassailableSoul());
            }
        });
    }

    public static QEffect CreateUnassailableSoul()
    {
        return new QEffect("Unassailable Soul", "You gain master proficiency in Will saves. Successes on Will saves are treated as critical successes.")
        {
            BonusToDefenses = (effect, action, defense) =>
            {
                return defense == Defense.Will
                    ? new Bonus(0, BonusType.Untyped, "Unassailable Soul")
                    : null;
            }
        };
    }

    private static async Task EpithetActions(Creature exemplar, CombatAction transcendence)
    {
        var epithetFeats = (exemplar.PersistentCharacterSheet?.Calculated.AllFeats ?? Enumerable.Empty<Feat>()).Where(feat => feat is Epithet).Cast<Epithet>().Select(epithet => epithet.TranscendAction);
        var epithets = Possibilities.Create(exemplar).Filter(ap => false);
        epithets.Sections.Add(new PossibilitySection("Epithet Actions")
        {
            Possibilities = epithetFeats.Select(generator => generator?.Possibility?.Invoke(exemplar, transcendence)).Where(possibility => possibility != null).ToList()!
        });
        epithets.Sections.Add(new PossibilitySection("Pass")
        {
            Possibilities = [new ActionPossibility(new CombatAction(exemplar, IllustrationName.EndTurn, "Pass", [
                Trait.Basic,
                Trait.UsableEvenWhenUnconsciousOrParalyzed,
                Trait.DoesNotPreventDelay
            ], "Do nothing.", Target.Self()).WithActionCost(0))]
        });
        typeof(Creature).InvokeMember("Possibilities", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, exemplar, [epithets]);
        var options = await exemplar.Battle.GameLoop.CreateActions(exemplar, exemplar.Possibilities, null);
        exemplar.Battle.GameLoopCallback.AfterActiveCreaturePossibilitiesRegenerated();
        await exemplar.Battle.GameLoop.OfferOptions(exemplar, options, true);
    }

    private static void EnsureCorrectRunes(CalculatedCharacterSheetValues sheet)
    {
        var allFeats = Ikon.IkonLUT.Values.Where(kvp => kvp.Rune != null);
        List<Inventory> inventories = [sheet.Sheet.CampaignInventory, .. sheet.Sheet.InventoriesByLevel.Values];
        foreach (var inventory in inventories)
        {
            var toAdd = new List<Item>();
            var toRemove = new List<Item>();
            List<Item?> allItems = [inventory.LeftHand, inventory.RightHand, inventory.Armor, .. inventory.Backpack];
            foreach (var ikon in allFeats)
            {
                ItemName ikonRune = (ItemName)ikon.Rune!;
                List<Item> items = [.. allItems.Where(item => item != null && item.ItemName == ikonRune),
                                    .. allItems.Where(item => item != null && item.Runes.Any(rune => rune.ItemName == ikonRune))
                                               .Select(item => item?.Runes.Where(rune => rune.ItemName == ikonRune).FirstOrDefault())];
                var amount = sheet.AllFeats.Contains(ikon.IkonFeat) ? 1 : 0;
                if (items.Count() < amount)
                {
                    if (ikon.FreeWornItem != null)
                    {
                        Item freeItem = Items.CreateNew((ItemName)ikon.FreeWornItem).WithModificationRune(ikonRune);
                        toAdd.Add(freeItem);
                    }
                    else
                    {
                        Item newItem = Items.CreateNew(ikonRune);
                        toAdd.Add(newItem);
                    }
                }
                else if (items.Count() > amount)
                {
                    toRemove.AddRange(items.Skip(amount)!);
                }
            }
            foreach (var (featName, ikonRune) in Ikon.ExtraRunes)
            {
                List<Item> items = [.. allItems.Where(item => item != null && item.ItemName == ikonRune),
                                    .. allItems.Where(item => item != null && item.Runes.Any(rune => rune.ItemName == ikonRune))
                                               .Select(item => item?.Runes.Where(rune => rune.ItemName == ikonRune).FirstOrDefault())];
                var amount = sheet.AllFeatNames.Contains(featName) ? 1 : 0;
                if (items.Count() < amount)
                {
                    Item newItem = Items.CreateNew(ikonRune);
                    toAdd.Add(newItem);
                }
                else if (items.Count() > amount)
                {
                    toRemove.AddRange(items.Skip(amount)!);
                }
            }
            foreach (var item in toAdd)
            {
                AddItem(inventory, item);
            }
            foreach (var item in toRemove)
            {
                RemoveItem(inventory, item);
            }
        }
    }

    private static void RemoveItem(Inventory inventory, Item item)
    {
        if (inventory.Backpack.Remove(item))
        {

        }
        else if (inventory.LeftHand?.Runes.Contains(item) ?? false)
        {
            inventory.LeftHand = RunestoneRules.RecreateWithUnattachedSubitem(inventory.LeftHand, item, true);
        }
        else if (inventory.RightHand?.Runes.Contains(item) ?? false)
        {
            inventory.RightHand = RunestoneRules.RecreateWithUnattachedSubitem(inventory.RightHand, item, true);
        }
        else if (inventory.Armor?.Runes.Contains(item) ?? false)
        {
            inventory.Armor = RunestoneRules.RecreateWithUnattachedSubitem(inventory.Armor, item, true);
        }
        else
        {
            var index = inventory.Backpack.FindIndex(item => item?.Runes.Contains(item) ?? false);
            if (index >= 0)
            {
                inventory.Backpack[index] = RunestoneRules.RecreateWithUnattachedSubitem(inventory.Backpack[index]!, item, true);
            }
        }
    }

    private static void AddItem(Inventory inventory, Item newItem)
    {
        if (inventory.CanBackpackFit(newItem, 0))
        {
            inventory.AddAtEndOfBackpack(newItem);
        }
    }
}
