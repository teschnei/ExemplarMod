using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Audio;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level6;

public class BindingSerpentsCelestialArrow
{
    [FeatGenerator(6)]
    public static IEnumerable<Feat> GetFeat()
    {
        var flavorText = "As you prepare to shoot your weapon, you invoke divine serpents that bind your enemies.";
        var rulesText = "{b}Usage{/b} imbued into a ranged weapon ikon, or a melee weapon ikon with the thrown trait\n\n The imbued ikon gains the following ability.\n\n" +
            $"{{b}}Transcendence — Coiling Serpents {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (transcendence)\n" +
            "Make a ranged Strike with your ikon. If the Strike hits, the target must succeed at a Reflex save against your class DC or " +
            "the arrow transforms into a multitude of ethereal snakes that coil around the target, immobilizing it until it succeeds at an " +
            "Escape attempt against your class DC.";
        yield return new TrueFeat(
            ExemplarFeats.BindingSerpentsCelestialArrow,
            6,
            flavorText,
            rulesText,
            [ExemplarTraits.Exemplar, ExemplarTraits.IkonExpansion],
            Ikon.AddExpansionFeat("BindingSerpentsCelestialArrow", flavorText, rulesText, [], ikon => ikon.IkonFeat.HasTrait(ExemplarTraits.IkonWeapon), (ikon, feat) =>
            {
                feat.WithPermanentQEffect(null, q =>
                {
                    q.ProvideMainAction = q =>
                    {
                        var ikonItem = ikon.GetHeldIkon(q.Owner);
                        if (ikonItem != null)
                        {
                            bool flag = ikonItem.HasTrait(Trait.Ranged);
                            CombatAction combatAction = StrikeRules.CreateStrike(q.Owner, ikonItem, RangeKind.Ranged, -1, !flag, null).WithActionCost(0);
                            if (flag)
                            {
                                combatAction.WithSoundEffect(ikonItem.WeaponProperties?.Sfx ?? SfxName.Bow);
                            }
                            return q.Owner.HasEffect(ikon.EmpoweredQEffectId) && ikonItem != null && ((ikonItem.WeaponProperties?.Throwable ?? false) || ikonItem.HasTrait(Trait.Ranged)) ?
                                Ikon.CreateTranscendence((ikon, q) =>
                                    new ActionPossibility(new CombatAction(q.Owner, IllustrationName.AnimalFormSnake,
                                        "Coiling Serpents", [ExemplarTraits.Transcendence, Trait.AlwaysHits, Trait.IsHostile],
                                        "Make a ranged Strike with your ikon. If the Strike hits, the target must succeed at a Reflex save against your class DC or " +
                                        "the arrow transforms into a multitude of ethereal snakes that coil around the target, immobilizing it until it succeeds at an " +
                                        "Escape attempt against your class DC.",
                                        ikonItem.DetermineStrikeTarget(RangeKind.Ranged))
                                    .WithActionCost(2)
                                    .WithTargetingTooltip((action, target, _) => CombatActionExecution.BreakdownAttackForTooltip(combatAction, target).TooltipDescription)
                                    .WithEffectOnChosenTargets(async (action, self, targets) =>
                                    {
                                        if (targets.ChosenCreature != null)
                                        {
                                            if (await self.MakeStrike(combatAction, targets.ChosenCreature) >= CheckResult.Success)
                                            {
                                                if (CommonSpellEffects.RollSavingThrow(targets.ChosenCreature, action, Defense.Reflex, self.ClassDC()) <= CheckResult.Failure)
                                                {
                                                    QEffect immobilized = QEffect.Immobilized().WithExpirationNever();
                                                    immobilized.ProvideContextualAction = q => new ActionPossibility(Possibilities.CreateEscapeAgainstEffect(
                                                            q.Owner, q, "Coiling Serpents", self.ClassDC())).WithPossibilityGroup("Remove debuff");
                                                    targets.ChosenCreature.AddQEffect(immobilized);
                                                }
                                            }
                                        }
                                    })), q, ikon
                                )
                            : null;
                        }
                        return null;
                    };
                });
            },
            item =>
            {
                if (!item.HasTrait(Trait.Ranged) && !(item.WeaponProperties?.Throwable ?? false))
                {
                    return "Binding Serpents Celestial Arrow: must be a ranged weapon ikon or melee weapon ikon with the thrown trait.";
                }
                return null;
            }).ToList()
        );
    }
}
