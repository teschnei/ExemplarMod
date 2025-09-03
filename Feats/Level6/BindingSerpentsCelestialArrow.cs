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
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Audio;

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
            "Escape attempt against your class DC.\n\n" +
            "{i}Note: there is no warning for using this on an incompatible item, it will just do nothing{/i}.";
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
                        var ikonItem = Ikon.GetIkonItem(q.Owner, (ItemName)ikon.Rune!);
                        return q.Owner.HasEffect(ikon.EmpoweredQEffectId) && ikonItem != null && ((ikonItem.WeaponProperties?.Throwable ?? false) || ikonItem.HasTrait(Trait.Ranged)) ?
                            Ikon.CreateTranscendence(q =>
                                new ActionPossibility(new CombatAction(q.Owner, IllustrationName.BloodVendetta,
                                    "Coiling Serpents", [ExemplarTraits.Transcendence],
                                    "Make a ranged Strike with your ikon. If the Strike hits, the target must succeed at a Reflex save against your class DC or " +
                                    "the arrow transforms into a multitude of ethereal snakes that coil around the target, immobilizing it until it succeeds at an " +
                                    "Escape attempt against your class DC.",
                                    Target.Reach(ikonItem))
                                .WithActionCost(2)
                                .WithEffectOnChosenTargets(async (action, self, targets) =>
                                {
                                    if (targets.ChosenCreature != null)
                                    {
                                        bool flag = ikonItem.HasTrait(Trait.Ranged);
                                        CombatAction combatAction = StrikeRules.CreateStrike(self, ikonItem, flag ? RangeKind.Ranged : RangeKind.Melee, -1, !flag, null).WithActionCost(0);
                                        if (flag)
                                        {
                                            combatAction.WithSoundEffect(ikonItem.WeaponProperties?.Sfx ?? SfxName.Bow);
                                        }
                                        if (await self.MakeStrike(combatAction, targets.ChosenCreature) >= CheckResult.Success)
                                        {
                                            if (CommonSpellEffects.RollSavingThrow(targets.ChosenCreature, action, Defense.Reflex, self.ClassDC()) <= CheckResult.Failure)
                                            {
                                                QEffect immobilized = QEffect.Immobilized().WithExpirationNever();
                                                immobilized.ProvideContextualAction = q => new ActionPossibility(Possibilities.CreateEscapeAgainstEffect(
                                                        q.Owner, q, "Coiling Serpents", self.ClassDC())).WithPossibilityGroup("Remove debuff");
                                            }
                                        }
                                    }
                                })), q, feat
                            )
                        : null;
                    };
                });
            }).ToList()
        );
    }
}
