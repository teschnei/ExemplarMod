using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level4;

public class SteelOnSteel
{
    [FeatGenerator(4)]
    public static IEnumerable<Feat> GetFeat()
    {
        var flavorText = "The ring of divinely empowered steel assails your enemies.";
        var rulesText = "{b}Usage{/b} imbued into a melee weapon ikon or shield ikon\n\n The imbued ikon gains the following ability.\n\n" +
            $"{{b}}Transcendence — Ringing Challenge {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (sonic, spirit, transcendence)\n" +
            "You clang your ikon against a weapon, shield, or the ground, emitting a shockwave that deals 1d4 spirit damage and 1d4 sonic damage to all creatures in a 30-foot " +
            "cone or 15-foot emanation (basic Fortitude save). A creature that critically fails its saving throw is deafened for 1 minute.\n" +
            "At 6th level and every 2 levels thereafter, the damage increases by 1d4 spirit damage and 1d4 sonic damage.";
        yield return new TrueFeat(
            ExemplarFeats.SteelOnSteel,
            4,
            flavorText,
            rulesText,
            [ExemplarTraits.Exemplar, ExemplarTraits.IkonExpansion],
            Ikon.AddExpansionFeat("SteelOnSteel", flavorText, rulesText, [], ikon => ikon.IkonFeat.HasTrait(ExemplarTraits.IkonWeapon) || ikon.IkonFeat.FeatName == ExemplarFeats.MirroredAegis, (ikon, feat) =>
            {
                feat.WithPermanentQEffect(null, q =>
                {
                    q.ProvideMainAction = q =>
                    {
                        var ikonItem = ikon.GetHeldIkon(q.Owner);
                        return q.Owner.HasEffect(ikon.EmpoweredQEffectId) && (ikonItem != null && ((ikonItem.WeaponProperties != null && !ikonItem.HasTrait(Trait.Ranged)) || (ikonItem.HasTrait(Trait.Shield)))) ?
                            Ikon.CreateTranscendence((ikon, q) =>
                                new SubmenuPossibility(IllustrationName.SteelShield, "Ringing Challenge")
                                {
                                    Subsections = [
                                        new PossibilitySection("Steel on Steel")
                                        {
                                            Possibilities = [steelOnSteelAction(q, Target.Emanation(6)),
                                                            steelOnSteelAction(q, Target.Cone(3))]
                                        }
                                    ]
                                }, q, ikon)
                            : null;
                    };

                });
            },
            item =>
            {
                if (!item.HasTrait(Trait.Shield) && (item.WeaponProperties == null || item.HasTrait(Trait.Ranged)))
                {
                    return "Steel on Steel: must be a melee weapon ikon or shield ikon.";
                }
                return null;
            }).ToList()
        );
        Possibility? steelOnSteelAction(QEffect q, Target target)
        {
            return new ActionPossibility(new CombatAction(q.Owner, IllustrationName.SteelShield, $"Ringing Challenge ({(target is EmanationTarget ? "emanation" : "cone")})", [Trait.Sonic, ExemplarTraits.Spirit, ExemplarTraits.Transcendence],
                $"You clang your ikon against a weapon, shield, or the ground, emitting a shockwave that deals {S.HeightenedVariable((q.Owner.Level - 2) / 2, 1)}d4 spirit damage " +
                $"and {S.HeightenedVariable((q.Owner.Level - 2) / 2, 1)}d4 sonic damage to all creatures in a " +
                $"{(target is EmanationTarget ? "30-foot emanation" : "15-foot cone")} (basic Fortitude save). A creature that critically fails its saving throw is deafened for 1 minute.", target)
                .WithHeighteningNumerical(q.Owner.Level, 4, true, 2, "1d4 spirit damage and 1d4 sonic damage.")
                .WithActionCost(2)
                .WithSavingThrow(new SavingThrow(Defense.Fortitude, q.Owner.ClassDC()))
                .WithEffectOnEachTarget(async (action, self, target, result) =>
                {
                    var level = q.Owner.Level - 2 / 2;
                    await CommonSpellEffects.DealBasicDamage(action, q.Owner, target, result,
                            new KindedDamage(DiceFormula.FromText($"{level}d4"), Ikon.GetBestDamageKindForSpark(q.Owner, target)),
                            new KindedDamage(DiceFormula.FromText($"{level}d4"), DamageKind.Sonic));
                    if (result == CheckResult.CriticalFailure)
                    {
                        target.AddQEffect(QEffect.Deafened());
                    }
                })
            );
        }
    }
}
