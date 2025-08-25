// file: Exemplar_ThroughTheNeedlesEye.cs
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
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Display.Illustrations;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Classes.Exemplar
{
    /*
    public class Exemplar_ThroughTheNeedlesEye
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var feat = new TrueFeat(
                ExemplarFeatNames.FeatThroughTheNeedlesEye,
                4,
                "Through The Needle's Eye",
                "{b}Usage: imbued into a weapon ikon{/b}\n" +
                "Your weapon strikes with the perfection your will demands.\n\n" +
                "{b}Transcendence — Blinding of the Needle (two-actions){/b} You aim your weapon in a superficial cut above your opponent's eye. " +
                "Make a Strike with the imbued ikon. If that Strike is successful, the target must succeed at a Fortitude save against your class DC or become blinded for 1 round or until it uses an Interact action to clear the blood from its vision.",
                new[] { ModTraits.Ikon },
                null
            )
            .WithMultipleSelection()
            .WithOnSheet(sheet =>
            {
                // allow selecting which weapon-Ikon to empower
                var ikonFeats = sheet.AllFeats
                    .Where(f => f.Traits.Contains(ModTraits.Ikon))
                    .ToList();
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "ThroughTheNeedlesEye",
                        name: "Through The Needle's Eye",
                        level: 4,
                        eligible: ft =>
                            ikonFeats.Contains(ft)
                            && ft.Traits.Contains(ModTraits.Ikon)
                            && ft.FeatName != ExemplarFeatNames.
                            FeatThroughTheNeedlesEye
                            && !ft.HasTrait(ModTraits.BodyIkon)
                    )
                );
            })
            .WithPermanentQEffect(null, qf =>
            {
                qf.ProvideMainAction = qf =>
                {
                    var owner = qf.Owner;
                    // Only if empowered and not used this round
                    if (owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker)
                        || !owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredThroughTheNeedlesEye))
                        return null;

                    // Build the two-action Transcendence
                    var action = new CombatAction(
                        owner,
                        IllustrationName.Shortsword,  // substitute a blade‐style icon if you have one
                        "Blinding of the Needle",
                        new[] { ModTraits.Ikon, ModTraits.Transcendence },
                        "Make a Strike with your imbued ikon; on success, the target must make a Fortitude save or be blinded for 1 round (or until it spends an Interact action).",
                        Target.Distance(6)  // 6 squares = 30 ft
                    ).WithActionCost(2);

                    // Fortitude save against your class DC
                    action.WithSavingThrow(new SavingThrow(Defense.Fortitude, owner.ClassOrSpellDC()));

                    // On a successful Strike
                    action.WithEffectOnEachTarget(async (act, caster, target, result) =>
                    {
                        // Only apply blindness if the Strike hit
                        if (result < CheckResult.Success)
                            return;

                        // Save failure → blinded
                        target.AddQEffect(QEffect.Blinded().WithExpirationAtStartOfSourcesTurn(caster,1));

                        // NOTE: TODO :
                        // if you need “until Interact”, you’d swap in a custom QEffect:
                        // new QEffect("Cut Above the Eye", "...", ExpirationCondition.ExpiresAtStartOfYourNextTurn, caster)
                        //    {  Id, description, etc.  };

                    });

                    // Cleanup empowerment & track exhaustion
                    action.WithEffectOnSelf(async (_, self) =>
                    {
                        IkonEffectHelper.CleanupEmpoweredEffects(self, ExemplarIkonQEffectIds.QEmpoweredThroughTheNeedlesEye);
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(feat);
        }
    }
*/
}
