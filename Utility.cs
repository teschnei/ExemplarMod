using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Treasure;

namespace Dawnsbury.Mods.Classes.Exemplar;

public static class Utility
{
    public static CalculatedNumber.CalculatedNumberProducer Attack(CombatAction action, Item item, int mapCount = -1)
    {
        return delegate (CombatAction combatAction, Creature self, Creature? defender)
        {
            var producer = Checks.Attack(item, mapCount);
            return producer(action, self, defender);
        };
    }
}
