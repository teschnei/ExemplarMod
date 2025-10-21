using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class MortalHarvest
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Ikon(new Feat(
            ExemplarFeats.MortalHarvest,
            "This weapon, once used for felling trees or crops, now harvests lives instead.",
            "{b}Usage{/b} a sickle or any weapon from the axe, flail, or polearm group\n\n" +
            "{b}Immanence{/b} The {i}mortal harvest{/i} deals 1 persistent force damage per weapon damage die to creatures it Strikes.\n\n" +
            $"{{b}}Transcendence — Reap the Field {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (transcendence)\n{{b}}Requirements{{/b}} Your previous action was a successful Strike with the mortal harvest.\n " +
            "{b}Effect{/b} Time seems to lag as you blur across the battlefield, deciding the fate of many in a moment. " +
            "Stride up to half your Speed and make another melee Strike with the {i}mortal harvest{/i} against a different creature. " +
            "This Strike uses the same multiple attack penalty as your previous Strike, but counts toward your multiple attack penalty as normal.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWeapon],
            null
        ).WithIllustration(ExemplarIllustrations.MortalHarvest), (ikon, q) =>
        {
            q.YouDealDamageWithStrike = (qe, action, diceFormula, target) =>
            {
                if (ikon.IsIkonItem(action.Item))
                {
                    target.AddQEffect(QEffect.PersistentDamage($"{action.Item?.WeaponProperties?.DamageDieCount}".ToString(), Ikon.GetBestDamageKindForSpark(action.Owner, target)));
                }
                return diceFormula;
            };
        },
        (ikon, q) =>
        {
            var heldIkon = Ikon.GetHeldIkon(q.Owner, ikon);
            var lastAction = q.Owner.Actions.ActionHistoryThisTurn.LastOrDefault();
            var lastIkon = ikon.IsIkonItem(lastAction?.Item) ? lastAction?.Item : null;
            return new ActionPossibility(new CombatAction(
                q.Owner,
                ExemplarIllustrations.MortalHarvest,
                "Reap the Field",
                [ExemplarTraits.Transcendence],
                "Stride up to half your Speed and make another Strike with the {i}mortal harvest{/i} against a different creature. This Strike uses the same multiple attack penalty as your previous Strike, but counts toward your multiple attack penalty as normal.",
                Target.Self().WithAdditionalRestriction(self =>
                {
                    if (heldIkon == null)
                    {
                        return "You must be wielding the Mortal Harvest.";
                    }
                    if (lastAction == null || !lastAction.HasTrait(Trait.Strike) ||
                        lastAction.CheckResult < CheckResult.Success ||
                        (lastIkon == null))
                    {
                        return "Your last action must be a successful Strike with the {i}mortal harvest{/i}.";
                    }
                    return null;
                })
            )
            .WithActionCost(1)
            .WithEffectOnSelf(async (act, self) =>
            {
                // Stride up to half Speed
                bool moved = await self.StrideAsync("Choose where to stride (half Speed).", allowPass: true, allowCancel: true, maximumHalfSpeed: true);

                List<Option> list = new List<Option>();
                CombatAction combatAction = self.CreateStrike(lastIkon!, self.Actions.AttackedThisManyTimesThisTurn - 1);
                combatAction.WithActionCost(0);
                ((CreatureTarget)combatAction.Target).CreatureTargetingRequirements.Add(
                    new LegacyCreatureTargetingRequirement((Creature a, Creature d) =>
                        (d == self.Actions.ActionHistoryThisEncounter.LastOrDefault()!.ChosenTargets.ChosenCreature) ? Usability.NotUsableOnThisCreature("excluded") : Usability.Usable
                    )
                );
                GameLoop.AddDirectUsageOnCreatureOptions(combatAction, list);
                list.Add(new PassViaButtonOption(moved ? "Don't Strike" : "Cancel"));
                if (list.Count > 0)
                {
                    if (list.Count == 1)
                    {
                        await list[0].Action();
                    }
                    RequestResult result = await self.Battle.SendRequest(new AdvancedRequest(self, "Choose a creature to Strike.", list)
                    {
                        TopBarText = "Choose a creature to Strike.",
                        TopBarIcon = act.Illustration
                    });
                    if (!await result.ChosenOption.Action())
                    {
                        if (!moved)
                        {
                            act.RevertRequested = true;
                        }
                    }
                }
            }));
        })
        .WithValidItem(item =>
        {
            if (item.WeaponProperties == null)
            {
                return "Must be a weapon.";
            }
            if ((!item.HasTrait(Trait.Axe)) && (!item.HasTrait(Trait.Flail)) && (!item.HasTrait(Trait.Polearm)) && (!item.HasTrait(Trait.Sickle)))
            {
                return "Must be a Sickle, Axe, Flail, or Polearm group.";
            }
            return null;
        })
        .IkonFeat;
    }
}
