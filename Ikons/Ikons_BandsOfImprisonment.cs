using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Dawnsbury.Campaign.Encounters.A_Crisis_in_Dawnsbury;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Exemplar;
/*
    DONE
*/
public class Ikons_BandsOfImprisonment
{
    [DawnsburyDaysModMainMethod]
    public static void Load()
    {
        Feat bandsOfImprisonment = new TrueFeat(
            ExemplarFeatNames.IkonBandsOfImprisonment,
            1,
            "These weighted bands don't enhance your power—rather, they keep your strength in check, honing your discipline.",
            "{b}Immanence{/b} The bands of imprisonment tighten, keeping your mind sharp. You gain a +1 status bonus to Will saving throws and resistance to mental damage equal to half your level.\n\n" +
            "{b}Transcendence — Break Free (two-actions){/b} You can attempt to Escape with a +2 status bonus on your check, then Stride up to twice your Speed in a straight line, and finally make a melee Strike. If you don't need to Escape or you can't move or choose not to, you still take the other actions listed.",
            [ModTraits.Ikon],
            null
        ).WithMultipleSelection()
        .WithPermanentQEffect(null, qf =>
        {
            qf.ProvideMainAction = qf =>
            {
                if (!qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredBandsOfImprisonment))
                    return null;

                var owner = qf.Owner;
                var action = new CombatAction(
                    owner,
                    IllustrationName.FreedomOfMovement,
                    "Break Free",
                    [Trait.Move, Trait.Attack, ModTraits.Ikon, ModTraits.Transcendence, ModTraits.BodyIkon],
                    "You attempt to Escape, then Stride up to twice your Speed in a straight line, and finally make a melee Strike.",
                    Target.Self()
                ).WithActionCost(2);

                _ = action.WithEffectOnSelf(async (action, self) =>
                {
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredBandsOfImprisonment))
                        return;

                    // Attempt Escape with +2 status bonus
                    var escapeCheck = TaggedChecks.SkillCheck(Skill.Athletics + 2);

                    var result = CommonSpellEffects.RollCheck("Escape", new ActiveRollSpecification(escapeCheck, Checks.FlatDC(20)), self, self);

                    // Log result
                    self.Occupies.Overhead($"Escape: {result}", Microsoft.Xna.Framework.Color.White);

                    if (!await self.StrideAsync(
                        "Choose where to Stride. (1/2)",
                        true))
                        action.RevertRequested = true;
                    else
                    {
                        _ = await self.StrideAsync("Choose where to Stride. And you should be in reach of an enemy. (2/2)", allowPass: true);
                        await CommonCombatActions.StrikeAdjacentCreature(self, null);
                    }
                    // After your Transcendence effect finishes:
                    _ = qf.Owner.RemoveAllQEffects(q => q.Id == ExemplarIkonQEffectIds.QEmpoweredBandsOfImprisonment); // or whichever ikon this is
                    _ = qf.Owner.AddQEffect(new QEffect("First Shift Free", "You can Shift Immanence without spending an action.")
                    {
                        Id = ExemplarIkonQEffectIds.FirstShiftFree
                    });
                    qf.Owner.AddQEffect(new QEffect("Spark exhaustion", "You cannot use another Transcendence this turn",
                     ExpirationCondition.ExpiresAtStartOfYourTurn, qf.Owner, IllustrationName.Chaos)
                    {
                        Id = ExemplarIkonQEffectIds.TranscendenceTracker
                    });
                });

                return new ActionPossibility(action);
            };
            qf.BonusToDefenses = (qfSelf, action, defense) =>
            {
                if (!qfSelf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredBandsOfImprisonment))
                    return null; // Only applies if empowered!

                var currentWill = qf.Owner.Defenses.GetBaseValue(Defense.Will);
                if (defense == Defense.Will)
                {
                    return new Bonus(1, BonusType.Status, "Bands of Imprisonment");
                }
                qf.Owner.WeaknessAndResistance.AddResistance(DamageKind.Mental, qf.Owner.Level / 2);

                return null;
            };
        });

        ModManager.AddFeat(bandsOfImprisonment);
    }
}
