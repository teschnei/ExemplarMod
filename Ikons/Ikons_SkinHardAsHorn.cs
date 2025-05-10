using System;
using System.Linq;
using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Targeting;
using System.Reflection.Metadata.Ecma335;
using Dawnsbury.Display;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_SkinHardAsHorn
    {
        // 1) Dummy FeatNames for each attunement
        public static readonly FeatName IkonSkinHornBludgeoning =
            ModManager.RegisterFeatName("IkonSkinHornBludgeoning", "Skin Hard as Horn: Bludgeoning");
        public static readonly FeatName IkonSkinHornSlashing =
            ModManager.RegisterFeatName("IkonSkinHornSlashing", "Skin Hard as Horn: Slashing");
        public static readonly FeatName IkonSkinHornPiercing =
            ModManager.RegisterFeatName("IkonSkinHornPiercing", "Skin Hard as Horn: Piercing");
        private static readonly Dictionary<FeatName,string> SkinHornAttunementDescriptions = new()
        {
            { IkonSkinHornBludgeoning,
              "During Immanence, you gain a status bonus to AC equal to half your level against bludgeoning attacks." },
            { IkonSkinHornSlashing,
              "During Immanence, you gain a status bonus to AC equal to half your level against slashing attacks." },
            { IkonSkinHornPiercing,
              "During Immanence, you gain a status bonus to AC equal to half your level against piercing attacks." }
        };
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            // 2) Register the dummy feats
            foreach (var fn in new[] {
                IkonSkinHornBludgeoning,
                IkonSkinHornSlashing,
                IkonSkinHornPiercing
            })
            {
                // Title: the human‐readable name you registered above
                var title = fn.HumanizeLowerCase2();
                // Desc: pull from our new dictionary (fallback to empty if missing)
                var desc  = SkinHornAttunementDescriptions.TryGetValue(fn, out var d) ? d : "";

                ModManager.AddFeat(new TrueFeat(
                    fn, 
                    1, 
                    "",
                    desc,                   // <-- your new long description
                    Array.Empty<Trait>(),
                    null
                ));
            }

            // 3) The real Skin Hard as Horn ikon
            var horn = new TrueFeat(
                ExemplarFeatNames.IkonSkinHardAsHorn,
                1,
                "Skin Hard as Horn",
                "During daily preparations you lightly strike your skin with a bludgeoning, slashing, or piercing object, " +
                "attuning the ikon to that damage type.\n\n" +
                "{b}Immanence{/b} You gain a resistance-stand-in: a status bonus to AC equal to half your level against the attuned type. " +
                "This bonus does not apply to critical hits.\n\n" +
                "{b}Transcendence — Crash against Me (one-action){/b} Your skin becomes nearly unbreakable. " +
                "Until the start of your next turn, you gain a status bonus to AC equal to your level against the same type. " +
                "Additionally, if an enemy attacks you with that type and either misses or deals zero damage because of this bonus, " +
                "[Not Implemented] that creature takes a –2 circumstance penalty to further attacks with that weapon until its next turn.",
                new[] { ModTraits.Ikon },
                null
            )
            // 4) Morning-prep dropdown
            .WithOnSheet(sheet =>
            {
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "SkinHorn:Attunement",
                        name: "Skin Hard as Horn Attunement",
                        level: SelectionOption.MORNING_PREPARATIONS_LEVEL,
                        eligible: ft =>
                            ft.FeatName == IkonSkinHornBludgeoning
                         || ft.FeatName == IkonSkinHornSlashing
                         || ft.FeatName == IkonSkinHornPiercing
                    ).WithIsOptional()
                );
            })
            // 5) Permanent QEffect: half-level AC bonus vs. attuned type
            .WithPermanentQEffect(null, qf =>
            {
                qf.Id = ExemplarIkonQEffectIds.QEmpoweredSkinHardAsHorn;
                qf.Key = "SkinHardAsHorn";

                // Immanence: +½ level AC bonus vs. that weapon type
                qf.BonusToDefenses = (eff, action, def) =>
                {
                    if (action == null)
                        return null;
                    if (!eff.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredSkinHardAsHorn))
                        return null;

                    var owner = eff.Owner;
                    DamageKind? kind =
                        owner.HasFeat(IkonSkinHornBludgeoning) ? DamageKind.Bludgeoning :
                        owner.HasFeat(IkonSkinHornSlashing) ? DamageKind.Slashing :
                        owner.HasFeat(IkonSkinHornPiercing) ? DamageKind.Piercing :
                        (DamageKind?)null;

                    // only apply to non-critical hits of that type
                    if (kind.HasValue
                     && action.HasTrait((Trait)kind.Value)
                     && action.CheckResult != CheckResult.CriticalSuccess)
                    {
                        // half-level
                        return new Bonus(owner.Level / 2, BonusType.Status, "Skin Hard as Horn");
                    }

                    return null;
                };

                // 6) Transcendence — full-level AC bonus + reactive off-guard effect
                qf.ProvideMainAction = qf =>
                {
                    var owner = qf.Owner;
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredSkinHardAsHorn))
                        return null;

                    var act = new CombatAction(
                        owner,
                        IllustrationName.FullPlate,
                        "Crash against Me",
                        new[] { Trait.Basic, ModTraits.Transcendence, ModTraits.Ikon },
                        "Your skin hardens. You gain AC bonus equal to your level vs. the attuned type until your next turn; " +
                        "attacks of that type that miss or deal no damage clank off, inflicting –2 circumstance penalty on further attacks.",
                        Target.Self()
                    ).WithActionCost(1);

                    act.WithEffectOnSelf(async (_, self) =>
                    {
                        // Re-prompt for attunement
                        var options = new[]{
                            (Feat: IkonSkinHornBludgeoning, Label: "Bludgeoning"),
                            (Feat: IkonSkinHornSlashing,    Label: "Slashing"),
                            (Feat: IkonSkinHornPiercing,    Label: "Piercing")
                        };
                        var labels = options.Select(o => o.Label).ToArray();
                        var choice = await self.AskForChoiceAmongButtons(
                            IllustrationName.FullPlate,
                            "Re-choose your Skin Hard as Horn attunement",
                            labels
                        );
                        var picked = options.First(o => o.Label == choice.Text);

                        self.Occupies.Overhead($"Skin attuned to {picked.Label}.", Microsoft.Xna.Framework.Color.Gold);

                        // Full-level bonus QEffect
                        var kind = picked.Feat == IkonSkinHornBludgeoning ? DamageKind.Bludgeoning
                                 : picked.Feat == IkonSkinHornSlashing ? DamageKind.Slashing
                                 : DamageKind.Piercing;

                        self.AddQEffect(new QEffect(
                                "Crash against Me",
                                $"+{self.Level} AC vs. {picked.Label}",
                                ExpirationCondition.ExpiresAtStartOfYourTurn,
                                self, IllustrationName.FullPlate
                            )
                        {
                            Id = ExemplarIkonQEffectIds.QSkinHornAura,
                            BonusToDefenses = (eff2, act2, def2) =>
                            {
                                if (true)
                                {
                                    eff2.Owner.WeaknessAndResistance.AddResistance(kind, owner.Level);
                                }

                                return null;
                            },
                            // TODO: hook here to detect miss or zero damage and apply –2 penalty QEffect onto attacker.
                        });

                        // Cleanup & exhaustion tracker
                        self.RemoveAllQEffects(q => ExemplarIkonQEffectIds.EmpoweredIkonIds.Contains(q.Id));
                        self.AddQEffect(new QEffect("First Shift Free", "Your next Immanence is free.")
                        { Id = ExemplarIkonQEffectIds.FirstShiftFree });
                        self.AddQEffect(new QEffect("Crash Exhaustion", "No further Transcendence this turn.",
                            ExpirationCondition.ExpiresAtStartOfYourTurn, self, IllustrationName.Chaos)
                        { Id = ExemplarIkonQEffectIds.TranscendenceTracker });
                    });

                    return new ActionPossibility(act);
                };
            });

            ModManager.AddFeat(horn);
        }
    }
}
