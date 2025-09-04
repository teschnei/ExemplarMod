using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Treasure;
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
            "do not increase your penalty until you have made all your attacks.\n\n" +
            "{i}Note: there is no warning for using this on an incompatible item, it will just do nothing{/i}.";
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
                        var ikonItem = Ikon.GetIkonItem(q.Owner, (ItemName)ikon.Rune!);
                        return q.Owner.HasEffect(ikon.EmpoweredQEffectId) && ikonItem != null && (!ikonItem.HasTrait(Trait.Ranged) && ikonItem.DetermineDamageKinds().Contains(DamageKind.Slashing)) ?
                            Ikon.CreateTranscendence(q =>
                                new ActionPossibility(new CombatAction(q.Owner, ExemplarIllustrations.SeverFourDragonflyWings,
                                    "Sever Four Dragonfly Wings", [ExemplarTraits.Transcendence],
                                    "Make a Strike that deals slashing damage with your weapon ikon. If that Strike is successful, you can immediately make another Strike " +
                                    "against a different target within your reach. You can continue making Strikes in this manner, each against a different target, until you have " +
                                    "made a total of four Strikes or you miss with a Strike, whichever comes first. Each attack counts towards your multiple attack penalty, but you " +
                                    "do not increase your penalty until you have made all your attacks.",
                                    Target.Reach(ikonItem))
                                .WithActionCost(3)
                                .WithEffectOnChosenTargets(async (action, self, targets) =>
                                {
                                    if (targets.ChosenCreature != null)
                                    {
                                        List<Creature> alreadyStruck = new();
                                        var strike = self.CreateStrike(ikonItem, self.Actions.AttackedThisManyTimesThisTurn, new StrikeModifiers { ReplacementDamageKind = DamageKind.Slashing }).WithActionCost(0);
                                        ((CreatureTarget)strike.Target).CreatureTargetingRequirements.Add(
                                            new LegacyCreatureTargetingRequirement((a, d) => (alreadyStruck.Contains(d)) ? Usability.NotUsableOnThisCreature("already struck") : Usability.Usable)
                                        );
                                        int strikes = 0;
                                        var target = targets.ChosenCreature;
                                        while (strikes < 4 && target != null && await self.MakeStrike(strike, target) >= CheckResult.Success)
                                        {
                                            alreadyStruck.Add(target);
                                            strikes++;
                                            target = null;
                                            List<Option> options = new();
                                            foreach (Creature targetCreature in self.Battle.AllCreatures)
                                            {
                                                if (((CreatureTarget)strike.Target).IsLegalTarget(strike.Owner, targetCreature))
                                                {
                                                    Option option = Option.ChooseCreature(strike.Name, targetCreature, async delegate
                                                    {
                                                        target = targetCreature;
                                                    }, -2.1474836E+09f).WithIllustration(strike.Illustration);
                                                    option.ContextMenuText = strike.ContextMenuName ?? strike.Name;
                                                    option.SuppressFromContextMenu = strike.HasTrait(Trait.DoNotShowInContextMenu);
                                                    string? text = strike.TooltipCreator?.Invoke(strike, targetCreature, 0);
                                                    if (text != null)
                                                    {
                                                        option.WithTooltip(text);
                                                    }
                                                    else if (strike.ActiveRollSpecification != null)
                                                    {
                                                        option.WithTooltip(CombatActionExecution.BreakdownAttackForTooltip(strike, targetCreature).TooltipDescription);
                                                    }
                                                    else if (strike.SavingThrow != null && (strike.ExcludeTargetFromSavingThrow == null || !strike.ExcludeTargetFromSavingThrow(strike, targetCreature)))
                                                    {
                                                        option.WithTooltip(CombatActionExecution.BreakdownSavingThrowForTooltip(strike, targetCreature, strike.SavingThrow).TooltipDescription);
                                                    }
                                                    else
                                                    {
                                                        option.WithTooltip(strike.Description);
                                                    }
                                                    option.PossibilityChain = strike.PossibilityChain;
                                                    options.Add(option);
                                                }
                                            }
                                            options.Add(new CancelOption(midspell: true));
                                            RequestResult result = await self.Battle.SendRequest(new AdvancedRequest(self, "Choose a creature to Strike or right-click to cancel.", options)
                                            {
                                                TopBarText = "Choose a creature to Strike.",
                                                TopBarIcon = strike.Illustration
                                            });
                                            await result.ChosenOption.Action();
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
