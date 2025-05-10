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

namespace Dawnsbury.Mods.Exemplar
{
    /*
        PFS Standard
        Mortal Harvest
        Weapon Ikon
        Source War of Immortals pg. 45
        Usage: a sickle or any weapon from the axe, flail, or polearm group

        Immanence
        The mortal harvest deals 1 persistent spirit damage per weapon damage die to creatures it Strikes.

        Transcendence — Reap the Field (one-action)
        [Requirements] Your previous action was a successful Strike with the mortal harvest.
        Effect: Stride up to half your Speed and make another melee Strike with the mortal harvest
        against a different creature. This Strike uses the same multiple attack penalty as your
        previous Strike, but counts toward your multiple attack penalty as normal.
    */
    /*
        TODO: Currently we need to make sure the MAP penalty is not being affected
    */
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
            )
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: apply persistent spirit damage per die
                qf.AfterYouTakeAction = async (selfQf, action) =>
                {
                    if (!selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredMortalHarvest))
                        return;

                    // Only on successful Strikes
                    if (!action.HasTrait(Trait.Strike) || action.Item?.WeaponProperties == null || action.ChosenTargets?.ChosenCreature == null)
                        return;

                    var target = action.ChosenTargets.ChosenCreature;
                    int dice = action.Item.WeaponProperties.DamageDieCount;
                    // var formula = DiceFormula.FromText($"{dice}", "Mortal Harvest persistent spirit");
                    var formula = ($"{dice}");

                    // TODO: replace with the actual persistent‐damage helper once available
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
                            self.Occupies.Overhead("Your last action must be a successful strike with the mortal harvest.", Microsoft.Xna.Framework.Color.Orange);
                            self.Actions.RevertExpendingOfResources(1,act);
                            return;
                        }

                        // Stride up to half Speed
                        await self.StrideAsync("Choose where to stride (half Speed).", allowPass: false, maximumHalfSpeed: true);

                        // Now strike a different creature
                        await CommonCombatActions.StrikeAdjacentCreature(self, null);

                        // Clean up empowerment
                        self.RemoveAllQEffects(q => q.Id == ExemplarIkonQEffectIds.QEmpoweredMortalHarvest);
                        self.AddQEffect(new QEffect("First Shift Free", "You can Shift Immanence without spending an action.")
                        {
                            Id = ExemplarIkonQEffectIds.FirstShiftFree
                        });
                        
                        self.AddQEffect(new QEffect("Spark exhaustion", "You cannot use another Transcendence this turn", ExpirationCondition.ExpiresAtStartOfYourTurn, self
                        , IllustrationName.Chaos){
                            Id = ExemplarIkonQEffectIds.TranscendenceTracker
                        });
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(mortalHarvest);
        }
    }
}
