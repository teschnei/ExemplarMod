using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Exemplar.Utilities;

namespace Dawnsbury.Mods.Exemplar
{
    /*
        TODO :
        Implement the wild strikes taking off guard to give a -2 to their saving throws.
    */
    public class Ikons_HandsOfTheWildling
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var handsOfTheWildling = new TrueFeat(
                ExemplarFeatNames.IkonHandsOfTheWildling,
                1,
                "Hands Of The Wildling",
                "Tattooed fists, savage claws, or even powerful gauntlets—you swing each with the fury of an animal from the woods.\n\n" +
                "{b}Immanence{/b} Strikes with your hands of the wildling deal an additional 1 spirit splash damage per weapon damage die. You are immune to this splash damage.\n\n" +
                "{b}Transcendence — Feral Swing (two-actions){/b} Spirit, Transcendence\n" +
                "You lash out with both arms, rending all before you. Each creature in a 15-foot cone must succeed at a basic Reflex save against your class DC or take spirit damage equal to your normal Strike damage with your hands of the wildling. " +
                "You can choose to swing with abandon, which imposes a –2 circumstance penalty to enemies' saving throws, but causes you to become off-guard until the start of your next turn.",
                new[] { ModTraits.Ikon , ModTraits.BodyIkon},
                null
            ).WithMultipleSelection()
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: extra spirit splash damage per die
                qf.BonusToDamage = (qSelf, action, defender) =>
                {
                    if (!qSelf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredHandsOfTheWildling))
                        return null;
                    int dice = action.Item.WeaponProperties.DamageDieCount;
                    return new Bonus(dice, BonusType.Status, "Wildling Splash Damage");
                };

                // Transcendence — Feral Swing
                qf.ProvideMainAction = qf =>
                {
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredHandsOfTheWildling))
                        return null;

                    var action = new CombatAction(
                        qf.Owner,
                        IllustrationName.DragonClaws,
                        "Feral Swing",
                        new[] { ModTraits.Transcendence, ModTraits.Ikon },
                        "15-foot cone Reflex save or take spirit damage equal to your wildling Strike damage.",
                        Target.Cone(3)
                    )
                    .WithActionCost(2)
                    .WithSavingThrow(new SavingThrow(Defense.Will, qf.Owner.ClassOrSpellDC()))
                    .WithEffectOnEachTarget(async (act, caster, target, result) =>
                    {
                        int diceCount = 1;
                        int diceSize = 6;
                        var formula = DiceFormula.FromText($"{diceCount}d{diceSize}", "Feral Swing Spirit Damage");

                        DamageKind damageKind = DamageKindHelper.GetDamageKindFromEffect(qf.Owner, ExemplarIkonQEffectIds.QEnergizedSpark);   
                        
                        await CommonSpellEffects.DealBasicDamage(
                            act, caster, target, result, formula, damageKind
                        );

                        // Remove the old tracker
                        IkonEffectHelper.CleanupEmpoweredEffects(caster, ExemplarIkonQEffectIds.QEmpoweredHandsOfTheWildling);

                    });

                    // TODO: implement “swing with abandon” option and off-guard QEffect

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(handsOfTheWildling);
        }
    }
}
