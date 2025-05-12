using System;
using System.Linq;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;

namespace Dawnsbury.Mods.Exemplar.Utilities
{
    public static class DamageKindHelper
    {
        /// <summary>
        /// Retrieves the DamageKind based on the specified conditions.
        /// </summary>
        /// <param name="creature">The creature whose QEffects are being checked.</param>
        /// <param name="effectId">The QEffect ID to look for.</param>
        /// <returns>The determined DamageKind, or DamageKind.Untyped if no match is found.</returns>
        public static DamageKind GetDamageKindFromEffect(Creature creature, QEffectId qEffect)
        {

            if (creature == null)
                return DamageKind.Untyped;

            var effect = creature.QEffects.FirstOrDefault(e => e.Id == qEffect);
            if (effect != null && Enum.TryParse<DamageKind>(effect.Key, out var kind))
            {
                return kind;
            }

            return DamageKind.Untyped;
        }
    }
}