using System;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Exemplar.Utilities;

namespace Dawnsbury.Mods.Exemplar;

/*
    TODO: 
    1. Currently first strike is dealing damage, my understanding only one has to deal damage [NEEDS TESTING]
    2. Need to properly implement the agile check
*/

public class Ikons_GleamingBlade
{
    [DawnsburyDaysModMainMethod]
    public static void Load()
    {
        Feat gleamingBlade = new TrueFeat(
            ExemplarFeatNames.IkonGleamingBlade,
            1,
            "This blade glitters with such sharpness it seems to cut the very air in front of it.",
            "{b}Immanence{/b} Strikes with the gleaming blade deal 2 additional spirit damage per weapon damage die.\n\n" +
            "{b}Transcendence — Flowing Spirit Strike (two-actions){/b} Make two Strikes with the gleaming blade, each against the same target and using your current multiple attack penalty. If the gleaming blade doesn't have the agile trait, the second Strike takes a –2 penalty. If both attacks hit, you combine their damage, which is all dealt as spirit damage. You add any precision damage only once. Combine the damage from both Strikes and apply resistances and weaknesses only once. This counts as two attacks when calculating your multiple attack penalty.",
            [ModTraits.Ikon],
            null
        ).WithMultipleSelection()
        .WithPermanentQEffect(null, qf =>
        {
            // Provide the 2-action Transcendence
            qf.ProvideMainAction = qf =>
            {
                if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredGleamingBlade))
                    return null;
                CombatAction flowSpiritStrike = new CombatAction(
                    qf.Owner,
                    IllustrationName.Greatsword,
                    "Flowing Spirit Strike",
                    [Trait.Aasimar, ModTraits.Ikon, ModTraits.Transcendence],
                    "Make two Strikes with the gleaming blade against the same target. If both hit, combine their damage as spirit damage.",
                    Target.ReachWithAnyWeapon() //since it checks to make sure your trans weapon is equpped, and all reach weapons as 2-handed should be okay maybe.
                ).WithActionCost(2);    

                flowSpiritStrike.WithEffectOnEachTarget(async (mainAction, caster, target, _) =>
                {
                    Item? weapon = caster.HeldItems.FirstOrDefault(item =>
                        item.WeaponProperties != null &&
                        item.HasTrait(Trait.Weapon) &&
                        item.HasTrait(Trait.Melee));

                    if (weapon == null)
                    {
                        caster.Overhead("Missing Weapon", Microsoft.Xna.Framework.Color.Orange);
                        caster.Actions.RevertExpendingOfResources(2, mainAction);
                        return;
                    }

                    bool isAgile = weapon.HasTrait(Trait.Agile);

                    // First Strike
                    CombatAction strike1 = caster.CreateStrike(weapon);
                    strike1.ActionCost = 0;
                    await caster.Battle.GameLoop.FullCast(strike1, ChosenTargets.CreateSingleTarget(target));
                    CheckResult result1 = strike1.CheckResult;

                    // Second Strike
                    CombatAction strike2 = caster.CreateStrike(weapon);
                    strike2.ActionCost = 0;
                    //have a way to add a -2 if not agile.
                    await caster.Battle.GameLoop.FullCast(strike2, ChosenTargets.CreateSingleTarget(target));
                    CheckResult result2 = strike2.CheckResult;

                    if (result2 < CheckResult.Success || result1 < CheckResult.Success)
                        return;
                        //else they both are hits.

                    // Combine spirit damage from both attacks
                    int dice = weapon.WeaponProperties?.DamageDieCount ?? 1;
                    int diceSize = weapon.WeaponProperties?.DamageDieSize ?? 6;

                    DiceFormula combined = DiceFormula.FromText($"{dice * 2}d{diceSize}", "Flowing Spirit Strike");

                    DamageKind damageKind = DamageKindHelper.GetDamageKindFromEffect(caster, ExemplarIkonQEffectIds.QEnergizedSpark);
                    
                    await CommonSpellEffects.DealDirectDamage(flowSpiritStrike, combined, target, result2, damageKind);
                    // After your Transcendence effect finishes:
                    IkonEffectHelper.CleanupEmpoweredEffects(caster, ExemplarIkonQEffectIds.QEmpoweredGleamingBlade);

                });

                return new ActionPossibility(flowSpiritStrike);
            };

            // Immanence: Bonus to strike damage
            //todo later, bonus damage QF lambda instead of a new damage event via DealDirectDamage.
            qf.AfterYouDealDamage = async (qfSelf, action, target) =>
            {
                if (!qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredGleamingBlade))
                    return; // Only applies if empowered!
                if (!action.HasTrait(Trait.Strike) || action.Item == null || action.ChosenTargets?.ChosenCreature == null)
                    return;
                if (!(action.CheckResult < CheckResult.Success))
                    return;

                int dice = action.Item.WeaponProperties?.DamageDieCount ?? 1;
                DiceFormula bonus = DiceFormula.FromText($"{dice * 2}", "Gleaming Blade");

                DamageKind damageKind = DamageKindHelper.GetDamageKindFromEffect(qf.Owner, ExemplarIkonQEffectIds.QEnergizedSpark);   
                
                await CommonSpellEffects.DealDirectDamage(
                    action,
                    bonus,
                    action.ChosenTargets.ChosenCreature,
                    action.CheckResult,
                    damageKind
                );
            };

        });

        ModManager.AddFeat(gleamingBlade);
    }
}