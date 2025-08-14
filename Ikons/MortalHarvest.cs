using System.Collections;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
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
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Exemplar.Utilities;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_MortalHarvest
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var mortalHarvest = new TrueFeat(
                // Make sure you add this in ExemplarFeatNames.cs
                ExemplarFeatNames.IkonMortalHarvest,
                1,
                "This weapon, once used for felling trees or crops, now harvests lives instead.",
                "{b}Immanence{/b} The mortal harvest deals 1 persistent spirit damage per weapon damage die to creatures it Strikes.\n\n" +
                "{b}Transcendence — Reap the Field (one-action){/b} [Requirements] Your previous action was a successful Strike with the mortal harvest. " +
                "Time seems to lag as you blur across the battlefield, deciding the fate of many in a moment. " +
                "Stride up to half your Speed and make another melee Strike with the mortal harvest against a different creature. " +
                "This Strike uses the same multiple attack penalty as your previous Strike, but counts toward your multiple attack penalty as normal.",
                new[] { ModTraits.Ikon },
                null
            ).WithMultipleSelection()
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: apply persistent spirit damage per die
                qf.AfterYouDealDamage = async (selfQf, action, target) =>
                {
                    if (!qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredMortalHarvest))
                        return;

                    // Only on successful Strikes
                    if (!action.HasTrait(Trait.Strike) || action.Item?.WeaponProperties == null || action.ChosenTargets?.ChosenCreature == null)
                        return;

                    int dice = action.Item.WeaponProperties.DamageDieCount;
                    // var formula = DiceFormula.FromText($"{dice}", "Mortal Harvest persistent spirit");
                    var formula = ($"{dice}");

                    // await CommonSpellEffects.ApplyPersistentDamage(action, formula, target, action.CheckResult, DamageKind.Spirit);
                    await CommonSpellEffects.DealBasicPersistentDamage(target,action.CheckResult, formula, DamageKind.Negative );
                };

                // Transcendence: Reap the Field
                qf.ProvideMainAction = qf =>
                {
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredMortalHarvest) )
                        return null;

                    var action = new CombatAction(
                        qf.Owner,
                        IllustrationName.Scythe,
                        "Reap the Field",
                        new[] { ModTraits.Transcendence, ModTraits.Ikon },
                        "Stride up to half your Speed and make another Strike with the mortal harvest against a different creature.",
                        Target.Self()
                    )
                    .WithActionCost(1);

                    action.WithEffectOnSelf(async (act, self) =>
                    {
                        // Verify last action was a successful Strike with this weapon
                        var prev = self.Actions.ActionHistoryThisEncounter.LastOrDefault();
                        if (prev == null || !prev.HasTrait(Trait.Strike))
                        {
                            self.Overhead("Your last action must be a successful strike with the mortal harvest.", Microsoft.Xna.Framework.Color.Orange);
                            self.Actions.RevertExpendingOfResources(1,act);
                            return;
                        }
                        //map pen
                        var pen = 5;
                        if (prev.HasTrait(Trait.Agile))
                        {
                            pen = 4;
                        }

                        // Stride up to half Speed
                        await self.StrideAsync("Choose where to stride (half Speed).", allowPass: false, maximumHalfSpeed: true);

                        //Cleanup: currently this is to offset the MAP penalty.
                        qf.BonusToAttackRolls = (eff, act, defender) => 
                            qf.Owner == self ? new Bonus(pen, BonusType.Untyped, "Mortal Harvest") : null;

                        // Now strike a different creature
                        await CommonCombatActions.StrikeAdjacentCreature(self, null);

                        // Clean up empowerment
                        IkonEffectHelper.CleanupEmpoweredEffects(self, ExemplarIkonQEffectIds.QEmpoweredMortalHarvest);
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(mortalHarvest);
        }
    }
}
