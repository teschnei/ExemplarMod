using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class FetchingBangles
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName freeItem = ModManager.RegisterNewItemIntoTheShop("OrdinaryBangles", itemName =>
        {
            return new Item(itemName, IllustrationName.DoublingRings, "Bangles", 1, 0, Trait.DoNotAddToShop)
            .WithDescription("An ordinary pair of bangles.")
            .WithWornAt(Trait.Bracers);
        });
        yield return new Ikon(new Feat(
            ExemplarFeats.FetchingBangles,
            "These lovely armbands sparkle and gleam, reflecting your own incredible magnetism.",
            "{b}Usage{/b} worn bracers\n\n" +
            "{b}Immanence{/b} (aura, mental) Others find it hard to leave your captivating presence. An aura surrounds you in a 10-foot emanation. " +
            "An enemy in the aura that attempts to move away from you must succeed at a Will save against your class DC or its move action is disrupted.\n\n" +
            $"{{b}}Transcendence — Embrace of Destiny {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (mental, spirit, transcendence)\n" +
            "Choose an enemy within 20 feet of you. It must succeed at a Will save against your class DC or be pulled directly toward you into a square adjacent to you.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWorn],
            null
        ).WithIllustration(ExemplarIllustrations.FetchingBangles), (ikon, q) =>
        {
            q.AddGrantingOfTechnical(cr => cr.EnemyOf(q.Owner) && cr.DistanceTo(q.Owner) <= 2, tq =>
            {
                tq.YouBeginAction = async (qe, action) =>
                {
                    if (action.HasTrait(Trait.Move) && action.ChosenTargets.ChosenTile?.DistanceTo(q.Owner) > qe.Owner.DistanceTo(q.Owner))
                    {
                        if (CommonSpellEffects.RollSavingThrow(action.Owner, CombatAction.CreateSimple(q.Owner, "Fetching Bangles"), Defense.Will, q.Owner.ClassDC()) < CheckResult.Success)
                        {
                            action.Disrupted = true;
                        }
                    }
                };
            });
        }, (ikon, q) =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                ExemplarIllustrations.FetchingBangles,
                "Embrace of Destiny",
                [Trait.Mental, ExemplarTraits.Spirit, ExemplarTraits.Transcendence],
                "Choose an enemy within 20 feet of you. It must succeed at a Will save against your class DC or be pulled directly toward you into a square adjacent to you.",
                Target.Ranged(4)
            ).WithActionCost(1)
            .WithSavingThrow(new SavingThrow(Defense.Will, q.Owner.ClassDC()))
            .WithEffectOnEachTarget(async (action, caster, target, result) =>
            {
                if (result < CheckResult.Success)
                {
                    await caster.PullCreature(target);
                }
            }));
        })
        .WithValidItem(item =>
        {
            if (!item.HasTrait(Trait.Worn) || item.WornAt != Trait.Bracers)
            {
                return "Must be worn bracers.";
            }
            return null;
        })
        .WithFreeWornItem(freeItem)
        .IkonFeat;
    }
}
