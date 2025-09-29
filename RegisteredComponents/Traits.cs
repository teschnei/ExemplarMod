using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;

public static class ExemplarTraits
{
    public static readonly Trait Exemplar = ModManager.RegisterTrait("Exemplar", new TraitProperties("Exemplar", true) { IsClassTrait = true });
    public static readonly Trait ShiftImmanence = ModManager.RegisterTrait("ShiftImmanence", new TraitProperties("Shift Immanence", false));
    public static readonly Trait EnergizedSpark = ModManager.RegisterTrait("EnergizedSpark", new TraitProperties("Energized Spark", false));
    public static readonly Trait Sanctified = ModManager.RegisterTrait("Sanctified", new TraitProperties("Sanctified", false));
    //Base Ikons
    public static readonly Trait Ikon = ModManager.RegisterTrait("Ikon", new TraitProperties("Ikon", true, "Divine power inhabits this item. When filled with an exemplar's divine spark, it will grant it's {i}immanence{/i} ability and {/i}transcendence{/i} action."));
    public static readonly Trait IkonWeapon = ModManager.RegisterTrait("IkonWeapon", new TraitProperties("IkonWeapon", false));
    public static readonly Trait IkonWorn = ModManager.RegisterTrait("IkonWorn", new TraitProperties("IkonWorn", false));
    public static readonly Trait IkonBody = ModManager.RegisterTrait("IkonBody", new TraitProperties("IkonBody", false));
    public static readonly Trait PeltOfTheBeastAttune = ModManager.RegisterTrait("PeltOfTheBeastAttune", new TraitProperties("PeltOfTheBeastAttune", false));
    public static readonly Trait SkinHardAsHornAttune = ModManager.RegisterTrait("SkinHardAsHornAttune", new TraitProperties("SkinHardAsHornAttune", false));

    //Rune Traits
    public static readonly Trait IkonBands = ModManager.RegisterTrait("Ikon1", new TraitProperties("Ikon", true, "Drag onto any worn anklets, bracers, or circlet."));
    public static readonly Trait IkonSlashingPiercing = ModManager.RegisterTrait("Ikon2", new TraitProperties("Ikon", true, "Drag onto a melee weapon that deals slashing or piercing damage."));
    public static readonly Trait IkonBracers = ModManager.RegisterTrait("Ikon3", new TraitProperties("Ikon", true, "Drag onto any worn bracers."));
    public static readonly Trait IkonSwordKnife = ModManager.RegisterTrait("Ikon4", new TraitProperties("Ikon", true, "Drag onto any weapon in the sword or knife group."));
    public static readonly Trait IkonFreeHand = ModManager.RegisterTrait("Ikon5", new TraitProperties("Ikon", true, "Drag onto any melee free-hand weapon."));
    public static readonly Trait IkonShield = ModManager.RegisterTrait("Ikon6", new TraitProperties("Ikon", true, "Drag onto any shield."));
    public static readonly Trait IkonSickleAxeFlailPolearm = ModManager.RegisterTrait("Ikon7", new TraitProperties("Ikon", true, "Drag onto any sickle or any weapon from the axe, flail, or polearm group."));
    public static readonly Trait IkonStaff = ModManager.RegisterTrait("Ikon8", new TraitProperties("Ikon", true, "Drag onto any staff, bo staff, fighting stick, khakkara, or any weapon in the spear or polearm weapon group."));
    public static readonly Trait IkonCloakBelt = ModManager.RegisterTrait("Ikon9", new TraitProperties("Ikon", true, "Drag onto any cloak or belt worn item."));
    public static readonly Trait IkonThrown = ModManager.RegisterTrait("Ikon10", new TraitProperties("Ikon", true, "Drag onto any one-handed thrown weapon of light Bulk or less."));
    public static readonly Trait IkonBelt = ModManager.RegisterTrait("Ikon11", new TraitProperties("Ikon", true, "Drag onto any belt worn item."));
    public static readonly Trait IkonRanged = ModManager.RegisterTrait("Ikon12", new TraitProperties("Ikon", true, "Drag onto any ranged weapon."));
    public static readonly Trait IkonShoes = ModManager.RegisterTrait("Ikon13", new TraitProperties("Ikon", true, "Drag onto any worn shoes."));
    public static readonly Trait IkonBludgeon = ModManager.RegisterTrait("Ikon14", new TraitProperties("Ikon", true, "Drag onto any melee weapon in the club, hammer, or axe group, or any your melee unarmed Strikes that deals bludgeoning damage."));
    public static readonly Trait IkonHeadBelt = ModManager.RegisterTrait("Ikon15", new TraitProperties("Ikon", true, "Drag onto any headwear or belt worn item."));
    public static readonly Trait IkonTwinStars = ModManager.RegisterTrait("Ikon16", new TraitProperties("Ikon", true, "Drag onto any one-handed melee weapon ikon."));
    //Feats that add effects to Ikons
    public static readonly Trait IkonExpansion = ModManager.RegisterTrait("IkonExpansion", new TraitProperties("Ikon", true));
    public static readonly Trait Transcendence = ModManager.RegisterTrait("Transcendence", new TraitProperties("Transcendence", true));
    public static readonly Trait HurlAtTheHorizon = ModManager.RegisterTrait("HurlAtTheHorizon", new TraitProperties("Hurl At The Horizon", false));
    public static readonly Trait Epithet = ModManager.RegisterTrait("Epithet", new TraitProperties("Epithet", false));
    public static readonly Trait RootEpithet = ModManager.RegisterTrait("RootEpithet", new TraitProperties("Root Epithet", false));
    public static readonly Trait DominionEpithet = ModManager.RegisterTrait("DominionEpithet", new TraitProperties("Dominion Epithet", false));

    //Misc traits that don't exist in DD
    public static readonly Trait Twin = ModManager.RegisterTrait("Twin", new TraitProperties("Twin", true, "When you attack with a twin weapon, you add a circumstance bonus to the damage roll equal to the weapon’s number of damage dice if you have previously attacked with a different weapon of the same type this turn."));
    static Trait GetTrait(string technicalName)
    {
        ModManager.TryParse<Trait>(technicalName, out var trait);
        return trait;
    }
    public static readonly Trait Spirit = GetTrait("ST_Spirit");
}

public static class ExemplarDamageKinds
{
    static DamageKind GetDamageKind(string technicalName)
    {
        ModManager.TryParse<DamageKind>(technicalName, out var trait);
        return trait;
    }

    public static readonly DamageKind Spirit = GetDamageKind("Spirit");
}
