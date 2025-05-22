using System;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
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
using Dawnsbury.Mods.Exemplar.Utilities;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Exemplar;

public class Ikons_BarrowsEdge
{
    [DawnsburyDaysModMainMethod]
    public static void Load()
    {
        Feat barrowsEdge = new TrueFeat(
            ExemplarFeatNames.IkonBarrowsEdge,
            1,
            "This blade subtly rattles in its scabbard, as if it wants to be unsheathed so it can consume violence.",
            "{b}Immanence{/b} The barrow's edge deals 1 additional spirit damage per weapon damage die to a creature it Strikes. If the creature is below half its maximum Hit Points, the weapon deals 3 additional spirit damage per weapon damage die instead.\n\n" +
            "{b}Transcendence — Drink of my Foes (one-action){/b} [Requirements] Your last action was a successful Strike with the barrow's edge. Your blade glows as it absorbs your foe's vitality. You regain Hit Points equal to half the damage dealt.",
            [ModTraits.Ikon],
            null
        ).WithMultipleSelection()
        .WithPermanentQEffect(null, qf =>
        {

            // Immanence effect (simplified with conditional bonus)
            qf.BonusToDamage = (qfSelf, action, defender) =>
            {
                if (!qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredBarrowsEdge))
                    return null;

                if (!action.HasTrait(Trait.Strike) || action.Item == null || defender == null)
                    return null;

                bool isWeakened = defender.HP <= defender.MaxHP / 2;
                int bonusPerDie = isWeakened ? 3 : 1;
                int dice = action.Item.WeaponProperties?.DamageDieCount ?? 1;
                return new Bonus(bonusPerDie * dice, BonusType.Circumstance, "Barrow's Edge bonus damage");
            };

            qf.AfterYouDealDamage = async (attacker, action, target) =>
            {
                var owner = qf.Owner;

                if (!action.HasTrait(Trait.Strike)
                    )
                    return;

                // Clear any old tracker
                owner.RemoveAllQEffects(q => q.Id == ExemplarIkonQEffectIds.QBarrowsEdgeDamageTracker);

                // Add a new one with the exact damage
                owner.AddQEffect(new QEffect("Barrow's Edge Damage Tracker", "", ExpirationCondition.Never, attacker, IllustrationName.BloodVendetta)
                {
                    Id = ExemplarIkonQEffectIds.QBarrowsEdgeDamageTracker,
                    Source = attacker,
                    Value = target.Damage,
                });
            };

            // Transcendence
            qf.ProvideMainAction = qf =>
            {
                if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredBarrowsEdge))
                    return null;
                    
                CombatAction action = new CombatAction(
                    qf.Owner,
                    IllustrationName.Scythe,
                    "Drink of my Foes",
                    [Trait.Healing, ModTraits.Ikon, ModTraits.Transcendence],
                    "Your blade glows as it absorbs your foe's vitality.",
                    Target.Self()
                ).WithActionCost(1);

                action.WithEffectOnSelf(async (act, caster) =>
                {
                    var previous = caster.Actions.ActionHistoryThisEncounter.LastOrDefault();

                    var tracker = caster.QEffects.Where(q => q.Id == ExemplarIkonQEffectIds.QBarrowsEdgeDamageTracker).FirstOrDefault();

                    if (previous == null || !previous.HasTrait(Trait.Strike) || tracker == null)
                    {
                        caster.Occupies.Overhead("You must Strike before using Drink of My Foes.", Microsoft.Xna.Framework.Color.Orange);
                        caster.Actions.RevertExpendingOfResources(1, act);
                        return;
                    }



                    // After your Transcendence effect finishes:
                    int healed = tracker?.Value ?? 0;

                    // Remove the tracker
                    IkonEffectHelper.CleanupEmpoweredEffects(caster, ExemplarIkonQEffectIds.QEmpoweredBarrowsEdge);
                    qf.Owner.RemoveAllQEffects(q => q.Id == ExemplarIkonQEffectIds.QBarrowsEdgeDamageTracker);
                    await caster.HealAsync((healed / 2).ToString(), act);
                });



                return new ActionPossibility(action);
            };
        });

        ModManager.AddFeat(barrowsEdge);
    }
}
