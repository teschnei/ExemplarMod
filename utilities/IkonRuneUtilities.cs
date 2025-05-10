// IkonRuneUtilities.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Mods.Exemplar;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using static Dawnsbury.Mods.Classes.Exemplar.Ikon;
using Dawnsbury.Core.Mechanics.Core;

namespace Dawnsbury.Mods.Classes.Exemplar
{
    public class Ikon : Item
    {
        public enum IkonRuneIDs
        {
            BarrowsEdge,
            GleamingBlade,
            MirroredAegis,
            MortalHarvest,
            HornOfPlenty,
            Starshot,
            NobleBranch,
            TitansBreaker

        }
        public IkonRuneIDs IkonRuneID {get; set;}

        public Ikon(IkonRuneIDs ikonRuneIDs, ItemName itemName, Illustration illustration, string name, string description) : base(itemName, illustration, name, 0, 0, [Trait.DoNotAddToShop, ModTraits.Ikon])
        {
            this.IkonRuneID = ikonRuneIDs;
            this.Description = description;
            this.WithStoresItem((Item implement, Item storedItem) =>
            {
                if (implement.StoredItems.Count >= 1)
                {
                    return "Already holding a scroll";
                }
                else if (storedItem.ScrollProperties == null)
                {
                    return "Only Scrolls";
                }

                return null;
            });
        }
    }




    public static class IkonRuneUtilities
    {

        private static ItemName[] itemList = [
            ExemplarItemNames.IkonBarrowsEdgeRune, ExemplarItemNames.IkonGleamingBladeRune,
             ExemplarItemNames.IkonMortalHarvestRune, ExemplarItemNames.IkonMirroredAegis, 
             ExemplarItemNames.IkonHornOfPlentyItem, ExemplarItemNames.IkonStarshotRune, 
             ExemplarItemNames.IkonNobleBranch, ExemplarItemNames.IkonTitansBreaker, ];
        private static IkonRuneIDs[] allRunes = [IkonRuneIDs.BarrowsEdge, IkonRuneIDs.GleamingBlade, 
        IkonRuneIDs.MortalHarvest, IkonRuneIDs.MirroredAegis, IkonRuneIDs.HornOfPlenty, IkonRuneIDs.Starshot,
        IkonRuneIDs.NobleBranch, IkonRuneIDs.TitansBreaker,
        ];
        // **1**: map each rune to the exact same FeatName you registered

        //only needed for weapons
        public static FeatName LookupIkonFeat(IkonRuneIDs rune) => rune switch
        {
            IkonRuneIDs.BarrowsEdge => ExemplarFeatNames.IkonBarrowsEdge,
            IkonRuneIDs.GleamingBlade => ExemplarFeatNames.IkonGleamingBlade,
            IkonRuneIDs.MirroredAegis => ExemplarFeatNames.IkonMirroredAegis,
            IkonRuneIDs.MortalHarvest => ExemplarFeatNames.IkonMortalHarvest,
            IkonRuneIDs.HornOfPlenty => ExemplarFeatNames.IkonHornOfPlenty,
            IkonRuneIDs.Starshot => ExemplarFeatNames.IkonStarshot,
            IkonRuneIDs.NobleBranch => ExemplarFeatNames.IkonNobleBranch,
            IkonRuneIDs.TitansBreaker => ExemplarFeatNames.IkonTitansBreaker,
            _ => throw new NotImplementedException()
        };

        // **2**: mirror the Thaumaturge EnsureCorrectImplements
        public static void EnsureCorrectRunes(CalculatedCharacterSheetValues sheet)
        {

            var toAdd    = new List<IkonRuneIDs>();
            var toRemove = new List<IkonRuneIDs>();

            foreach (var rune in allRunes)
            {
                var feat = LookupIkonFeat(rune);
                if (sheet.HasFeat(feat))
                    toAdd.Add(rune);
                else
                    toRemove.Add(rune);
            }

            // add
            foreach (var rune in toAdd)
                AddRune(sheet, rune);
        }

        private static void AddRune(CalculatedCharacterSheetValues character, IkonRuneIDs rune)
        {
            Inventory campaignInventory = character.Sheet.CampaignInventory;
            Item item;
            switch(rune)
            {
                case IkonRuneIDs.BarrowsEdge:
                    item = Items.CreateNew(ExemplarItemNames.IkonBarrowsEdgeRune);
                    break;
                case IkonRuneIDs.GleamingBlade:
                    item = Items.CreateNew(ExemplarItemNames.IkonGleamingBladeRune);
                    break;
                case IkonRuneIDs.MortalHarvest:
                    item = Items.CreateNew(ExemplarItemNames.IkonMortalHarvestRune);
                    break;
                case IkonRuneIDs.MirroredAegis:
                    item = Items.CreateNew(ExemplarItemNames.IkonMirroredAegis);
                    break;
                case IkonRuneIDs.HornOfPlenty:
                    item = Items.CreateNew(ExemplarItemNames.IkonHornOfPlentyItem);
                    break;
                case IkonRuneIDs.Starshot:
                    item = Items.CreateNew(ExemplarItemNames.IkonStarshotRune);
                    break;
                case IkonRuneIDs.NobleBranch:
                    item = Items.CreateNew(ExemplarItemNames.IkonNobleBranch);
                    break;
                case IkonRuneIDs.TitansBreaker:
                    item = Items.CreateNew(ExemplarItemNames.IkonTitansBreaker);
                    break;
                default:
                    throw new InvalidOperationException("Unknown Ikon");
            }

            AddRuneIntoInventory(campaignInventory, item);

            int[] levels = character.Sheet.InventoriesByLevel.Keys.ToArray();

            foreach(int level in levels)
            {
                Inventory inventory = character.Sheet.InventoriesByLevel[level];
                if (level == 1 || inventory.LeftHand != null || inventory.RightHand != null || inventory.Backpack.Count != 0)
                {
                    
                        AddRuneIntoInventory(inventory, item);
                }
            }

        }

        private static void AddRuneIntoInventory(Inventory inventory, Item implement)
        {

            if (itemList.Contains(implement.BaseItemName) && 
                ((inventory.LeftHand?.Runes.Any(rune => rune.BaseItemName == implement.BaseItemName) ?? false) || 
                (inventory.RightHand?.Runes.Any(rune => rune.BaseItemName == implement.BaseItemName) ?? false) || 
                inventory.Backpack.Any(item => item?.Runes.Any(rune => rune.BaseItemName == implement.BaseItemName) ?? false)))
            {
            }
            else if (itemList.Contains(implement.BaseItemName) && 
                (inventory.LeftHand?.StoredItems.Any(item => item.Runes.Any(rune => rune.BaseItemName == implement.BaseItemName)) ?? false) || 
                (inventory.RightHand?.StoredItems.Any(item => item.Runes.Any(rune => rune.BaseItemName == implement.BaseItemName)) ?? false) ||
                inventory.Backpack.Any(item => item?.StoredItems.Any(item => item.Runes.Any(rune => rune.BaseItemName == implement.BaseItemName)) ?? false))
            {

            }
            else if ((inventory.LeftHand?.StoredItems.Any(item => item.BaseItemName == implement.BaseItemName) ?? false)||
                (inventory.RightHand?.StoredItems.Any(item => item.BaseItemName == implement.BaseItemName) ?? false) ||
                (inventory.Backpack.Any(item => item?.StoredItems.Any(item => item.BaseItemName == implement.BaseItemName) ?? false)))
            {

            }
            else if ((inventory.LeftHand == null || inventory.LeftHand.BaseItemName != implement.BaseItemName) && (inventory.RightHand == null || inventory.RightHand.BaseItemName != implement.BaseItemName) && !inventory.Backpack.Any(item => item != null && item.BaseItemName == implement.BaseItemName))
            {
                if (inventory.CanBackpackFit(implement, 0))
                {
                        inventory.AddAtEndOfBackpack(implement);
                }
            }  
        }
    }
}
