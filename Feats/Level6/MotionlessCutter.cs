using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level6;

public class MotionlessCutter
{
    [FeatGenerator(6)]
    public static IEnumerable<Feat> GetFeat()
    {
        var flavorText = "Your weapon is so sharp even an insect alighting upon its still blade would be severed.";
        var rulesText = "{b}Usage{/b} imbued into a melee weapon ikon that deals slashing damage\n\n The imbued ikon gains the following ability.\n\n" +
            $"{{b}}Transcendence — Sever Four Dragonfly Wings {RulesBlock.GetIconTextFromNumberOfActions(3)}{{/b}} (transcendence)\n" +
            "Make a Strike that deals slashing damage with your weapon ikon. If that Strike is successful, you can immediately make another Strike " +
            "against a different target within your reach. You can continue making Strikes in this manner, each against a different target, until you have " +
            "made a total of four Strikes or you miss with a Strike, whichever comes first. Each attack counts towards your multiple attack penalty, but you " +
            "do not increase your penalty until you have made all your attacks.";
        yield return new TrueFeat(
            ExemplarFeats.MotionlessCutter,
            6,
            flavorText,
            rulesText,
            [ExemplarTraits.Exemplar, ExemplarTraits.IkonExpansion],
            Ikon.AddExpansionFeat("MotionlessCutter", flavorText, rulesText, [], ikon => ikon.IkonFeat.HasTrait(ExemplarTraits.IkonWeapon), (ikon, feat) =>
            {
                feat.WithPermanentQEffect(null, q =>
                {
                    q.ProvideMainAction = q =>
                    {
                        var ikonItem = Ikon.GetHeldIkon(q.Owner, ikon);
                        return q.Owner.HasEffect(ikon.EmpoweredQEffectId) && ikonItem != null ?
                            Ikon.CreateTranscendence((ikon, q) =>
                                new ActionPossibility(new CombatAction(q.Owner, ExemplarIllustrations.SeverFourDragonflyWings,
                                    "Sever Four Dragonfly Wings", [ExemplarTraits.Transcendence],
                                    "Make a Strike that deals slashing damage with your weapon ikon. If that Strike is successful, you can immediately make another Strike " +
                                    "against a different target within your reach. You can continue making Strikes in this manner, each against a different target, until you have " +
                                    "made a total of four Strikes or you miss with a Strike, whichever comes first. Each attack counts towards your multiple attack penalty, but you " +
                                    "do not increase your penalty until you have made all your attacks.",
                                    Target.MultipleCreatureTargets(Enumerable.Repeat(ikonItem.DetermineStrikeTarget(RangeKind.Melee), 4).ToArray()).WithMinimumTargets(1).WithMustBeDistinct())
                                .WithActionCost(3)
                                .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(ikonItem, -1), Checks.DefenseDC(Defense.AC)))
                                .WithNoSaveFor((action, cr) => true)
                                .WithEffectOnChosenTargets(async (action, self, targets) =>
                                {
                                    var strike = self.CreateStrike(ikonItem, self.Actions.AttackedThisManyTimesThisTurn, new StrikeModifiers { ReplacementDamageKind = DamageKind.Slashing }).WithActionCost(0);
                                    foreach (var target in targets.GetAllTargetCreatures())
                                    {
                                        if (await self.MakeStrike(strike, target) < CheckResult.Success)
                                        {
                                            break;
                                        }
                                    }
                                })), q, ikon
                            )
                        : null;
                    };
                });
            },
            item =>
            {
                if (item.HasTrait(Trait.Ranged) || item.WeaponProperties == null || !item.DetermineDamageKinds().Contains(DamageKind.Slashing))
                {
                    return "Motionless Cutter: must be a melee weapon ikon that deals slashing damage.";
                }
                return null;
            }).ToList()
        );
    }
}
