using System.Collections.Generic;
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
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;
using Dawnsbury.Display;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level4;

public class ThroughTheNeedlesEye
{
    [FeatGenerator(4)]
    public static IEnumerable<Feat> GetFeat()
    {
        var flavorText = "Your weapon strikes with the perfection your will demands.";
        var rulesText = "{b}Usage{/b} imbued into a weapon ikon\n\n The imbued ikon gains the following ability.\n\n" +
            $"{{b}}Transcendence — Blinding of the Needle {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (transcendence)\n" +
            "You aim your weapon in a superficial cut above your opponent's eye. Make a Strike with the imbued ikon. If that Strike is " +
            "successful, the target must succeed at a Fortitude save against your class DC or become blinded for 1 round or until it uses " +
            "an Interact action to clear the blood from its vision.";
        yield return new TrueFeat(
            ExemplarFeats.ThroughTheNeedlesEye,
            4,
            flavorText,
            rulesText,
            [ExemplarTraits.Exemplar, ExemplarTraits.IkonExpansion],
            Ikon.AddExpansionFeat("ThroughTheNeedlesEye", flavorText, rulesText, [], ikon => ikon.IkonFeat.HasTrait(ExemplarTraits.IkonWeapon), (ikon, feat) =>
            {
                feat.WithPermanentQEffect(null, q =>
                {
                    q.ProvideMainAction = q =>
                    {
                        var ikonItem = Ikon.GetIkonItem(q.Owner, (ItemName)ikon.Rune!);
                        return q.Owner.HasEffect(ikon.EmpoweredQEffectId) && ikonItem != null ?
                            Ikon.CreateTranscendence(q =>
                                new ActionPossibility(new CombatAction(q.Owner, IllustrationName.BloodVendetta,
                                    "Blinding of the Needle", [ExemplarTraits.Transcendence],
                                    "You aim your weapon in a superficial cut above your opponent's eye. Make a Strike with the imbued ikon. If that Strike is " +
                                    "successful, the target must succeed at a Fortitude save against your class DC or become blinded for 1 round or until it uses " +
                                    "an Interact action to clear the blood from its vision.",
                                    Target.Reach(ikonItem))
                                .WithActionCost(2)
                                .WithEffectOnChosenTargets(async (action, self, targets) =>
                                {
                                    if (targets.ChosenCreature != null)
                                    {
                                        if (await self.MakeStrike(targets.ChosenCreature, ikonItem) >= CheckResult.Success)
                                        {
                                            if (CommonSpellEffects.RollSavingThrow(targets.ChosenCreature, action, Defense.Fortitude, self.ClassDC()) <= CheckResult.Failure)
                                            {
                                                targets.ChosenCreature.AddQEffect(QEffect.QuenchableBlinded("Blinding of the Needle").WithExpirationAtEndOfOwnerTurn());
                                            }
                                        }
                                    }
                                })), q, ikon
                            )
                        : null;
                    };
                });
            }).ToList()
        );
    }
}
