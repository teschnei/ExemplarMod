using System.Linq;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using HarmonyLib;

namespace Dawnsbury.Mods.Classes.Exemplar;

[HarmonyPatch(typeof(CombatActionExecution), nameof(CombatActionExecution.DetermineGuaranteedRollNumber))]
public static class UnfailingBowPatch
{
    static void Postfix(CombatAction combatAction, ref int __result)
    {
        var unfailing = combatAction.Owner.FindQEffect(Ikon.IkonLUT[ExemplarFeats.UnfailingBow].EmpoweredQEffectId);
        if (unfailing != null && unfailing.Tag != null)
        {
            __result = (int)unfailing.Tag;
        }
    }
}

[HarmonyPatch(typeof(Tile), nameof(Tile.CountsAsNonignoredDifficultTerrainFor))]
public static class BornOfTheBonesOfTheEarthPatch
{
    static void Postfix(Tile __instance, Creature who, ref bool __result)
    {
        var born = __instance.TileQEffects.Where(q => q.TileQEffectId == ExemplarTileQEffects.BornOfTheBonesOfTheEarthTerrain).FirstOrDefault();
        if (__result == true && (who.PersistentCharacterSheet?.Calculated.HasFeat(ExemplarFeats.BornOfTheBonesOfTheEarth) ?? false))
        {
            __result = false;
        }
    }
}
