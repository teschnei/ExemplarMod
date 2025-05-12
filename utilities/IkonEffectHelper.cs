using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Display.Illustrations;

namespace Dawnsbury.Mods.Exemplar.Utilities
{
    public static class IkonEffectHelper
    {
        /// <summary>
        /// Cleans up the empowered effects, adds the "First Shift Free" and "Spark Exhaustion" effects.
        /// </summary>
        /// <param name="owner">The creature owning the effects.</param>
        /// <param name="empoweredEffectIds">The list of empowered effect IDs to remove.</param>
        public static void CleanupEmpoweredEffects(Creature owner, QEffectId empoweredEffectIds)
        {
            // Remove all empowered effects
            owner.RemoveAllQEffects(q =>q.Id == empoweredEffectIds);

            // Add "First Shift Free" effect
            owner.AddQEffect(new QEffect("First Shift Free", "You can Shift Immanence without spending an action.")
            {
                Id = ExemplarIkonQEffectIds.FirstShiftFree
            });

            // Add "Spark Exhaustion" effect
            owner.AddQEffect(new QEffect(
                "Spark Exhaustion",
                "You cannot use another Transcendence this turn.",
                ExpirationCondition.ExpiresAtStartOfYourTurn,
                owner,
                IllustrationName.Chaos
            )
            {
                Id = ExemplarIkonQEffectIds.TranscendenceTracker
            });
        }
    }
}