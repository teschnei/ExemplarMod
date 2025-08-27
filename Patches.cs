using Dawnsbury.Core.CombatActions;
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
