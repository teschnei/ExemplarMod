using System;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
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
using Dawnsbury.Phases.Modals;

namespace Dawnsbury.Mods.Classes.Exemplar
{
    /*
    public class Exemplar_SteelOnSteel
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {

            var steel = new TrueFeat(
                ExemplarFeatNames.FeatSteelOnSteel,
                4,
                "Steel On Steel",
                "{b}Transcendence — Ringing Challenge (two-actions){/b} You clang your ikon against a weapon, shield, or the ground, emitting a shock wave that deals 1d4 spirit damage and 1d4 sonic damage to all creatures in a 30-foot cone (basic Fortitude save; a critical failure deafens the target for 1 minute).  At 6th level and every 2 levels thereafter, the spirit and sonic damage each increase by 1d4.",
                new[] { ExemplarBaseClass.TExemplar },
                null
            )
            .WithOnSheet(sheet =>
            {
                //list of all feats that have the Ikon trait
                var ikonFeats = sheet.AllFeats
                    .Where(f => f.Traits.Contains(ModTraits.Ikon))
                    .ToList();
                
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "SteelOnSteel",
                        name: "Steel On Steel",
                        level: 4,
                        eligible: ft => ikonFeats.Contains(ft) && ft.Traits.Contains(ModTraits.Ikon) &&
                            ft.FeatName != ExemplarFeatNames.FeatSteelOnSteel &&
                            !ft.HasTrait(ModTraits.BodyIkon)
                    )
                );
            })
            .WithPermanentQEffect(null, qf =>
            {
                qf.ProvideMainAction = qf =>
                {
                    var owner = qf.Owner;
                    // Only once per turn, and only if this ikon is empowered
                    if (owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker))
                        return null;

                    // Build the two-action Transcendence
                    var action = new CombatAction(
                        owner,
                        IllustrationName.SteelShield,
                        "Ringing Challenge",
                        new[] { ModTraits.Ikon, Trait.Sonic, ModTraits.Transcendence },
                        "You clang your ikon, creating a shock wave in a 30-foot cone. Each creature must make a Fortitude save or take spirit and sonic damage; a critical failure deafens them for 1 minute.",
                        Target.Cone(6)
                    ).WithActionCost(2);

                    // Build the two-action Transcendence
                    var emanAct = new CombatAction(
                        owner,
                        IllustrationName.SteelShield,
                        "Ringing Challenge",
                        new[] { ModTraits.Ikon, Trait.Sonic, ModTraits.Transcendence },
                        "You clang your ikon, creating a shock wave in a 15-foot emanation. Each creature must make a Fortitude save or take spirit and sonic damage; a critical failure deafens them for 1 minute.",
                        Target.Emanation(3)
                    ).WithActionCost(2);
                    // Fortitude save against your class DC
                    emanAct.WithSavingThrow(new SavingThrow(Defense.Fortitude, owner.ClassOrSpellDC()));

                    // Damage and deafening effect
                    emanAct.WithEffectOnEachTarget(async (act, caster, target, result) =>
                    {

                        // Scale dice: 1d4 at 4th, +1d4 at 6th,8th,…  (floor((Level–4)/2)+1)
                        int diceCount = 1 + Math.Max(0, (caster.Level - 4) / 2);

                        // Spirit damage
                        var spiritFormula = DiceFormula.FromText(
                            $"{diceCount}d4",
                            "Ringing Challenge Spirit"
                        );

                        DamageKind damageKind = DamageKindHelper.GetDamageKindFromEffect(caster, ExemplarIkonQEffectIds.QEnergizedSpark);

                        await CommonSpellEffects.DealBasicDamage(
                            act, caster, target, result, spiritFormula, damageKind
                        );

                        // Sonic damage
                        var sonicFormula = DiceFormula.FromText(
                            $"{diceCount}d4",
                            "Ringing Challenge Sonic"
                        );
                        await CommonSpellEffects.DealBasicDamage(
                            act, caster, target, result, sonicFormula, damageKind
                        );

                        // Deafened on critical failure
                        if (result == CheckResult.CriticalFailure)
                        {
                            target.AddQEffect(QEffect.Deafened());
                        }
                    });

                    // Fortitude save against your class DC
                    action.WithSavingThrow(new SavingThrow(Defense.Fortitude, owner.ClassOrSpellDC()));

                    // Damage and deafening effect
                    action.WithEffectOnEachTarget(async (act, caster, target, result) =>
                    {

                        // Scale dice: 1d4 at 4th, +1d4 at 6th,8th,…  (floor((Level–4)/2)+1)
                        int diceCount = 1 + Math.Max(0, (caster.Level - 4) / 2);

                        // Spirit damage
                        var spiritFormula = DiceFormula.FromText(
                            $"{diceCount}d4",
                            "Ringing Challenge Spirit"
                        );

                        DamageKind damageKind = DamageKindHelper.GetDamageKindFromEffect(caster, ExemplarIkonQEffectIds.QEnergizedSpark);

                        await CommonSpellEffects.DealBasicDamage(
                            act, caster, target, result, spiritFormula, damageKind
                        );

                        // Sonic damage
                        var sonicFormula = DiceFormula.FromText(
                            $"{diceCount}d4",
                            "Ringing Challenge Sonic"
                        );
                        await CommonSpellEffects.DealBasicDamage(
                            act, caster, target, result, sonicFormula, damageKind
                        );

                        // Deafened on critical failure
                        if (result == CheckResult.CriticalFailure)
                        {
                            target.AddQEffect(QEffect.Deafened());
                        }
                    });

                    // Cleanup empowerment & exhaustion on self
                    action.WithEffectOnSelf(async (act, self) =>
                    {
                        IkonEffectHelper.CleanupEmpoweredEffects(self, ExemplarIkonQEffectIds.QSteelOnSteel);
                    });

                    return new SubmenuPossibility(IllustrationName.SteelShield, "Steel on Steel: Choose an action")
                    {
                        Subsections = [
                            new PossibilitySection("Select an action")
                            {
                                Possibilities = [
                                    new ActionPossibility(action),
                                    new ActionPossibility(emanAct)
                                ],
                            }
                        ]
                    };
                };
            });

            ModManager.AddFeat(steel);
        }
    }
*/
}
