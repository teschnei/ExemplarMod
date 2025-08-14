using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Roller;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_EpithetWhoseCryIsThunder
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var thunder = new TrueFeat(
                ExemplarFeatNames.EpithetWhoseCryIsThunder,
                7,
                "Whose Cry is Thunder",
                "The sky overhead is yours to command as lightning strikes your soul. You gain the Energized Spark feat for your choice of electricity or sonic. " +
                "When you critically succeed on a Strike, a thunderclap booms! The target must make a Fortitude saving throw against your class DC. " +
                "On a failure, they are knocked prone and deafened for 1 minute. This is a sonic effect.\n\n" +
                "When you Spark Transcendence, you can choose to become electrically charged until the start of your next turn. " +
                "Any creature that touches you or damages you with an unarmed attack or non-reach melee weapon while you're charged takes 1d6 electricity damage as lightning courses back to them.",
                new[] { ModTraits.DominionEpithet },
                null
            )
            .WithOnSheet(sheet =>
            {
                // Grant the base Spark feat and restrict to electricity or sonic
                sheet.GrantFeat(ExemplarFeatNames.FeatEnergizedSpark);
                var allowed = new HashSet<FeatName>
                {
                    ExemplarFeatNames.AttunementFeats[3], // Electricity
                    ExemplarFeatNames.AttunementFeats[7]  // Sonic
                };
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "ThunderSparkType",
                        name: "Choose your Thunder Spark damage type",
                        level: 7,
                        eligible: ft => ft.HasTrait(ModTraits.TEnergizedSpark) && allowed.Contains(ft.FeatName)
                    )
                );
            })
            .WithPermanentQEffect(null, qf =>
            {
                qf.AfterYouTakeAction = async (selfQf, action) =>
                {
                    // Thunderclap on critical Strike
                    if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess)
                    {
                        var target = action.ChosenTargets?.ChosenCreature;
                        if (target != null)
                        {
                            var clapAction = new CombatAction(
                                selfQf.Owner,
                                IllustrationName.Thunderburst,
                                "Thunderclap",
                                new[] { Trait.Sonic, Trait.Electricity },
                                "A thunderclap booms! The target must make a Fortitude saving throw against your class DC. " +
                                "On a failure, they are knocked prone and deafened for 1 minute. This is a sonic effect.",
                                Target.Distance(0)
                            ).WithActionCost(0)
                             .WithSavingThrow(new SavingThrow(Defense.Fortitude, selfQf.Owner.ClassOrSpellDC()))
                             .WithEffectOnEachTarget(async (act, caster, target, result) =>
                             {
                                 // Only apply prone and deafened if the save failed
                                 if (result == CheckResult.Failure || result == CheckResult.CriticalFailure)
                                 {
                                     target.AddQEffect(QEffect.Prone());
                                     target.AddQEffect(QEffect.Deafened().WithExpirationAtStartOfSourcesTurn(caster, 10));
                                 }
                             });
                        }
                    }

                    // Electrified charge on Transcendence
                    if (action.HasTrait(ModTraits.Transcendence))
                    {
                        selfQf.Owner.AddQEffect(new QEffect(
                            "Electrified",
                            "You are electrically charged until the start of your next turn. Any creature that touches you or damages you with an unarmed or non-reach melee attack takes 1d6 electricity damage.",
                            ExpirationCondition.ExpiresAtStartOfYourTurn,
                            selfQf.Owner
                        )
                        {
                            AfterYouTakeDamage = async (effect, amount, kind, action, critical) =>
                            {
                                // Apply electricity damage to the attacker
                                var attacker = effect.Owner;
                                if (attacker != null)
                                {
                                    var damageFormula = DiceFormula.FromText("1d6", "Electricity Damage");
                                    await CommonSpellEffects.DealDirectDamage(null, damageFormula, attacker, CheckResult.Success, DamageKind.Electricity);
                                }
                            }
                        });
                    }
                };
            });

            ModManager.AddFeat(thunder);
        }
    }
}
