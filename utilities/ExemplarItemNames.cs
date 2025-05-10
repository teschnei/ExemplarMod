// ExemplarItemNames.cs
using Dawnsbury.Modding;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core;
using Dawnsbury.Mods.Exemplar;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Mechanics.Enumerations;
using System.Collections.Generic;
using System.Linq;

namespace Dawnsbury.Mods.Classes.Exemplar
{
    

    public static class ExemplarItemNames
    {
        public static readonly ItemName IkonBarrowsEdgeRune =
            ModManager.RegisterNewItemIntoTheShop("Barrow's Edge", itemName =>
            {
                return new Item(itemName,IllustrationName.FearsomeRunestone, "Barrow's Edge", 1, 0, Trait.DoNotAddToShop)
                .WithRuneProperties( new RuneProperties("Ikon", IkonRuneKind.BarrowsEdge, "This blade subtly rattles in its scabbard, as if it wants to be unsheathed so it can consume violence.",
                "Drag onto a melee weapon that deals slashing or piercing damage", item =>
                {
                    item.Traits.Add(ModTraits.Ikon);
                })
                .WithCanBeAppliedTo((Item rune, Item weapon) =>
                {   
                    if(weapon.WeaponProperties == null){
                        return "Must be a weapon.";
                    }
                    else if (weapon.WeaponProperties.DamageKind.ToString() != "Slashing" &&  weapon.WeaponProperties.DamageKind.ToString() != "Piercing" ) {
                        return "Must be a Slashing or a Piercing Weapon. " + weapon.WeaponProperties.DamageKind;
                    }
                    return null;
                }));
            });
        
        public static readonly ItemName IkonGleamingBladeRune =
            ModManager.RegisterNewItemIntoTheShop("Gleaming Blade", itemName =>
            {
                return new Item(itemName,IllustrationName.FearsomeRunestone, "Gleaming Blade", 1, 0, Trait.DoNotAddToShop)
                .WithRuneProperties( new RuneProperties("Ikon", IkonRuneKind.GleamingBlade, "This blade glitters with such sharpness it seems to cut the very air in front of it.",
                "Drag onto a weapon in the sword or knife group", item =>
                {
                    item.Traits.Add(ModTraits.Ikon);
                })
                .WithCanBeAppliedTo((Item rune, Item weapon) =>
                {   
                    if(weapon.WeaponProperties == null){
                        return "Must be a weapon.";
                    }
                    else if ((!weapon.HasTrait(Trait.Sword)) && (!weapon.HasTrait(Trait.Knife))) {
                        return "Must be a Sword or a Knife " + weapon.Traits;
                    }
                    return null;
                }));
            });

        public static readonly ItemName IkonMirroredAegis =
            ModManager.RegisterNewItemIntoTheShop("Mirrored Aegis", itemName =>
            {
                return new Item(itemName,IllustrationName.FearsomeRunestone, "Mirrored Aegis", 1, 0, Trait.DoNotAddToShop)
                .WithRuneProperties( new RuneProperties("Ikon", IkonRuneKind.MirroredAegis, "This shield is polished so brightly it can reflect even spiritual and ethereal attacks.",
                "Drag onto any shield", item =>
                {
                    item.Traits.Add(ModTraits.Ikon);
                })
                .WithCanBeAppliedTo((Item rune, Item shield) =>
                {   
                    if(!shield.HasTrait(Trait.Shield)){
                        return "Must be a Shield.";
                    }
                    return null;
                }));
            });    

        public static readonly ItemName IkonMortalHarvestRune =
            ModManager.RegisterNewItemIntoTheShop("Mortal Harvest", itemName =>
            {
                return new Item(itemName,IllustrationName.FearsomeRunestone,"Mortal Harvest", 1, 0, Trait.DoNotAddToShop)
                .WithRuneProperties( new RuneProperties("Ikon", IkonRuneKind.MortalHarvest,"This weapon, once used for felling trees or crops, now harvests lives instead.",
                "Drag onto a sickle or any weapon from the axe, flail, or polearm group", item =>
                {
                    item.Traits.Add(ModTraits.Ikon);
                })
                .WithCanBeAppliedTo((Item rune, Item weapon) =>
                {
                    if(weapon.WeaponProperties == null){
                        return "Must be a weapon.";
                    }
                    if((!weapon.HasTrait(Trait.Axe)) && (!weapon.HasTrait(Trait.Flail)) && (!weapon.HasTrait(Trait.Polearm)) && (!weapon.HasTrait(Trait.Sickle)) ){
                        return "Must be a Sickle, Axe, Flail, or Polearm group.";
                    }
                    return null;
                })
                );
            });

        public static ItemName IkonHornOfPlentyItem =
            ModManager.RegisterNewItemIntoTheShop("Horn Of Plenty", itemName => {
                return new Item(itemName,IllustrationName.BagOfHolding2,"Horn Of Plenty",1,0,Trait.DoNotAddToShop)
                .WithStoresItem( (container,subitem) =>{
                    if(!subitem.HasTrait(Trait.Consumable))
                        return "Horn of plenty only works with Consumables.";
                    //make sure it is not too bulky
                    // var currentBulk = container.StoredItems.Sum(i => i.w);
                    // if (currentBulk + subitem.Bulk > 1)
                    //     return "Horn of Plenty can hold up to 1 Bulk only.";
                    return null; // OK to store
                });
            });

        public static readonly ItemName IkonStarshotRune =
            ModManager.RegisterNewItemIntoTheShop("Starshot", itemName =>
            {
                return new Item(itemName,IllustrationName.FearsomeRunestone,"Starshot", 1, 0, Trait.DoNotAddToShop)
                .WithRuneProperties( new RuneProperties("Ikon", IkonRuneKind.Starshot,"You might be the only one capable of stringing this bow or pulling this trigger; either way, the ikon's shots are packed with explosive power, striking like falling stars.",
                "Drag onto a ranged weapon", item =>
                {
                    item.Traits.Add(ModTraits.Ikon);
                })
                .WithCanBeAppliedTo((Item rune, Item weapon) =>
                {
                    if(weapon.WeaponProperties == null){
                        return "Must be a weapon.";
                    }
                    if(!weapon.HasTrait(Trait.Ranged) ){
                        return "Must be a ranged weapon.";
                    }
                    return null;
                })
                );
            });

        public static readonly ItemName IkonNobleBranch =
            ModManager.RegisterNewItemIntoTheShop("Noble Branch", itemName =>
            {
                return new Item(itemName,IllustrationName.FearsomeRunestone, "Noble Branch", 1, 0, Trait.DoNotAddToShop)
                .WithRuneProperties( new RuneProperties("Ikon", IkonRuneKind.NobleBranch, "This humble stick-like weapon has an elegant simplicity to it, affording you reliable strikes over flashy maneuvers.",
                "Drag onto a staff, bo staff, fighting stick, khakkara, or any weapon in the spear or polearm weapon group", item =>
                {
                    item.Traits.Add(ModTraits.Ikon);
                })
                .WithCanBeAppliedTo((Item rune, Item weapon) =>
                {   
                    if(weapon.WeaponProperties == null){
                        return "Must be a weapon.";
                    }
                    if((!weapon.HasTrait(Trait.Staff)) && (!weapon.HasTrait(Trait.Spear)) && (!weapon.HasTrait(Trait.Polearm)) ){
                        return "Must be a staff, bo staff, fighting stick, khakkara, or any weapon in the spear or polearm weapon group";
                    }
                    return null;
                }));
            });
        public static readonly ItemName IkonTitansBreaker =
            ModManager.RegisterNewItemIntoTheShop("Titans Breaker", itemName =>
            {
                return new Item(itemName,IllustrationName.FearsomeRunestone, "Titans Breaker", 1, 0, Trait.DoNotAddToShop)
                .WithRuneProperties( new RuneProperties("Ikon", IkonRuneKind.TitansBreaker, "You wield a weapon whose blows shatter mountains with ease.",
                "any melee weapon in the club, hammer, or axe group, or any your melee unarmed Strikes that deals bludgeoning damage", item =>
                {
                    item.Traits.Add(ModTraits.Ikon);
                })
                .WithCanBeAppliedTo((Item rune, Item weapon) =>
                {   
                    if(weapon.WeaponProperties == null){
                        return "Must be a weapon.";
                    }
                    else if((!weapon.HasTrait(Trait.Club)) && (!weapon.HasTrait(Trait.Hammer)) && (!weapon.HasTrait(Trait.Axe)) && (!weapon.HasTrait(Trait.Unarmed)) ){
                        return "Must be a Slashing or a Piercing Weapon. " + weapon.WeaponProperties.DamageKind;
                    }
                    return null;
                }));
            });

        //…and any future Ikon runes
    }
}
