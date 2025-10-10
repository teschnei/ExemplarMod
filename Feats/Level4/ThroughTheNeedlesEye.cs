using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Audio;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level4;

public class ThroughTheNeedlesEye
{
    static Possibility? CreateBlinding(Creature owner, Item ikonItem, RangeKind range, bool thrown)
    {
        return
            new ActionPossibility(new CombatAction(owner, IllustrationName.BloodVendetta,
                "Blinding of the Needle" + (thrown ? " (throw)" : ""), [ExemplarTraits.Transcendence],
                "You aim your weapon in a superficial cut above your opponent's eye. Make a Strike with the imbued ikon. If that Strike is " +
                "successful, the target must succeed at a Fortitude save against your class DC or become blinded for 1 round or until it uses " +
                "an Interact action to clear the blood from its vision.",
                ikonItem.DetermineStrikeTarget(range))
            .WithActionCost(2)
            .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(ikonItem, -1), TaggedChecks.DefenseDC(Defense.AC)))
            .WithNoSaveFor((action, cr) => true)
            .WithEffectOnChosenTargets(async (action, self, targets) =>
            {
                if (targets.ChosenCreature != null)
                {
                    CombatAction strike = StrikeRules.CreateStrike(self, ikonItem, range, -1, thrown).WithActionCost(0);
                    if (range == RangeKind.Ranged)
                    {
                        strike.WithSoundEffect(ikonItem.WeaponProperties?.Sfx ?? SfxName.Bow);
                    }
                    strike.ChosenTargets = ChosenTargets.CreateSingleTarget(targets.ChosenCreature);
                    if (await strike.AllExecute() && strike.CheckResult >= CheckResult.Success)
                    {
                        if (CommonSpellEffects.RollSavingThrow(targets.ChosenCreature, action, Defense.Fortitude, self.ClassDC()) <= CheckResult.Failure)
                        {
                            targets.ChosenCreature.AddQEffect(QEffect.QuenchableBlinded("Blinding of the Needle").WithExpirationAtEndOfOwnerTurn());
                        }
                    }
                }
            })
        );
    }
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
                        var ikonItem = Ikon.GetHeldIkon(q.Owner, ikon);
                        if (q.Owner.HasEffect(ikon.EmpoweredQEffectId) && ikonItem != null)
                        {
                            var ranged = ikonItem.HasTrait(Trait.Ranged);
                            var throwable = ikonItem.WeaponProperties?.Throwable ?? false;

                            if (!ranged && !throwable)
                            {
                                return Ikon.CreateTranscendence((ikon, q) => CreateBlinding(q.Owner, ikonItem, RangeKind.Melee, false), q, ikon);
                            }
                            else if (ranged)
                            {
                                return Ikon.CreateTranscendence((ikon, q) => CreateBlinding(q.Owner, ikonItem, RangeKind.Ranged, false), q, ikon);
                            }
                            else
                            {
                                return Ikon.CreateTranscendence((ikon, q) =>
                                    new SubmenuPossibility(IllustrationName.BloodVendetta, "Blinding of the Needle")
                                    {
                                        Subsections = [
                                            new PossibilitySection("Blinding of the Needle")
                                            {
                                                Possibilities = [CreateBlinding(q.Owner, ikonItem, RangeKind.Melee, false),
                                                                CreateBlinding(q.Owner, ikonItem, RangeKind.Ranged, true)]
                                            }
                                        ]
                                    }, q, ikon
                                );
                            }
                        }
                        return null;
                    };
                });
            },
        item =>
            {
                if (item.WeaponProperties == null)
                {
                    return "Through the Needle's Eye: must be a weapon ikon";
                }
                return null;
            }).ToList()
        );
    }
}
