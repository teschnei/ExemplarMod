using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;

public class IkonRuneKind
{
    /// <summary>
    /// thanks to Sudo!
    /// The technical Weapon Implement persistent Rune Kind ID 
    /// </summary>
    public static readonly RuneKind Ikon = ModManager.RegisterEnumMember<RuneKind>("Ikon");
    public static readonly RuneKind IkonTwinStars = ModManager.RegisterEnumMember<RuneKind>("IkonTwinStars");
}
