using System;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Exemplar.Utilities;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_NobleBranch
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            // 1) Register your feat name in ExemplarFeatNames.cs:
            //    public static readonly FeatName IkonNobleBranch = ModManager.RegisterFeatName("IkonNobleBranch", "Noble Branch");

            var nobleBranch = new TrueFeat(
                ExemplarFeatNames.IkonNobleBranch,
                1,
                "Noble Branch",
                "{b}Immanence{/b} The noble branch deals 2 additional spirit damage per weapon damage die to creatures it Strikes.\n\n" +
                "{b}Transcendence — Strike, Breathe, Rend (one-action){/b} [Requirements] Your last action this turn was a successful Strike with the noble branch. " +
                "You channel a rending pulse of energy down your weapon in the moment of contact. The target of the Strike takes spirit damage equal to the noble branch's weapon damage dice.",
                new[] { ModTraits.Ikon },
                null
            ).WithMultipleSelection()
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: +2 spirit per die
                qf.BonusToDamage = (selfQf, action, defender) =>
                {
                    if (!selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredNobleBranch))
                        return null;
                    if (!action.HasTrait(Trait.Strike) || action.Item == null || defender == null)
                        return null;
                    int dice = action.Item.WeaponProperties?.DamageDieCount ?? 1;
                    return new Bonus(2 * dice, BonusType.Circumstance, "Noble Branch bonus");
                };

                // **Damage tracker** – record exactly how much your Strike dealt
                qf.AfterYouDealDamage = async (attacker, action, target) =>
                {
                    if (!qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredNobleBranch))
                        return;
                    if (!action.HasTrait(Trait.Strike) || action.Item == null)
                        return;

                    var owner = qf.Owner;
                    // clear out any old tracker
                    owner.RemoveAllQEffects(q => q.Id == ExemplarIkonQEffectIds.QNobleBranchDamageTracker);
                    // stash the raw damage total into a never-expiring QEffect
                    owner.AddQEffect(new QEffect("Noble Branch Damage Tracker", "", ExpirationCondition.Never, attacker)
                    {
                        Id = ExemplarIkonQEffectIds.QNobleBranchDamageTracker,
                        Source = attacker,
                        Value = target.Damage
                    });
                }; // this mirrors Barrow's Edge's tracker logic

                // Transcendence: pull that stored damage and re-deal it as spirit
                qf.ProvideMainAction = qf =>
                {
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredNobleBranch))
                        return null;

                    var action = new CombatAction(
                        qf.Owner,
                        IllustrationName.Quarterstaff,
                        "Strike, Breathe, Rend",
                        new[] { ModTraits.Transcendence, ModTraits.Ikon },
                        "Channel a rending pulse down your weapon. Deals spirit damage equal to that Strike's damage dice.",
                        Target.Self()
                    ).WithActionCost(1);

                    action.WithEffectOnSelf(async (act, self) =>
                    {
                        // 1) Was your last action a Strike?
                        var prev = self.Actions.ActionHistoryThisEncounter.LastOrDefault();
                        if (prev == null || !prev.HasTrait(Trait.Strike) || prev.Item == null)
                        {
                            self.Overhead("You must Strike first to use Strike, Breathe, Rend.", Color.Orange);
                            self.Actions.RevertExpendingOfResources(1, act);
                            return;
                        }

                        // 2) Grab the damagetracker QEffect
                        var tracker = self.QEffects.FirstOrDefault(q => q.Id == ExemplarIkonQEffectIds.QNobleBranchDamageTracker);
                        if (tracker == null)
                        {
                            self.Overhead("No damage recorded. You must hit with your Strike first.", Color.Orange);
                            self.Actions.RevertExpendingOfResources(1, act);
                            return;
                        }

                        // 3) Build a dice formula matching your weapon's dice (including any Striking rune dice)
                        var wp = prev.Item.WeaponProperties!;

                        var trackerValue = tracker?.Value ?? 0;
                        DiceFormula formula = DiceFormula.FromText(trackerValue.ToString(), "Damage");

                        // 4) Re-deal that as spirit damage against your original target
                        var target = prev.ChosenTargets.ChosenCreature!;
                        
                        DamageKind damageKind = DamageKindHelper.GetDamageKindFromEffect(qf.Owner, ExemplarIkonQEffectIds.QEnergizedSpark);   
                        
                        await CommonSpellEffects.DealDirectDamage(act, formula, target, prev.CheckResult, damageKind);

                        // 5) Clean up empowerment and grant free Shift (and exhaustion-tracker)
                        self.RemoveAllQEffects(q => q.Id == ExemplarIkonQEffectIds.QNobleBranchDamageTracker);
                        IkonEffectHelper.CleanupEmpoweredEffects(self, ExemplarIkonQEffectIds.QEmpoweredNobleBranch);

                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(nobleBranch);
        }
    }
}
