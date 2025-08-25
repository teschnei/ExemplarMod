using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
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

namespace Dawnsbury.Mods.Classes.Exemplar;

public class VictorsWreath
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("VictorsWreath", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Victor's Wreath", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.Ikon)
            .WithRuneProperties(new RuneProperties("Ikon", IkonRuneKind.VictorsWreath, "This symbol of victory—whether a laurel worn around the head or a medal that hangs from your neck—reminds you and your allies that victory is the only acceptable outcome.",
            "", item =>
            {
                item.Traits.AddRange([ExemplarTraits.Ikon, Trait.Divine]);
            })
            .WithCanBeAppliedTo((Item rune, Item item) =>
            {
                if (!item.HasTrait(Trait.Worn) || (item.WornAt != Trait.Belt && item.WornAt != Trait.Headwear && item.WornAt != Trait.Headband))
                {
                    return "Must be a worn headwear or belt.";
                }
                return null;
            }));
        });
        ItemName freeItem = ModManager.RegisterNewItemIntoTheShop("OrdinaryWreath", itemName =>
        {
            return new Item(itemName, IllustrationName.TiaraOfOpenSkies, "Wreath", 1, 0, Trait.DoNotAddToShop)
            .WithDescription("An ordinary wreath.")
            .WithWornAt(Trait.Headband);
        });
        yield return new Ikon(new Feat(
            ExemplarFeats.VictorsWreath,
            "This symbol of victory—whether a laurel worn around the head or a medal that hangs from your neck—reminds you and your allies that victory is the only acceptable outcome.",
            "{b}Usage{/b} worn headwear or belt (typically a sash)\n\n" +
            "{b}Immanence{/b} (aura, emotion, mental) You inspire your allies to greater glory. You and all your allies in a 15-foot emanation gain a +1 status bonus to attack rolls.\n\n" +
            $"{{b}}Transcendence — One Moment till Glory {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (concentrate, emotion, mental, transcendence)\n" +
            "You rally your allies, carrying them from the brink of disaster to the verge of victory. Each ally in your aura can immediately attempt a new saving throw with a +2 status bonus against one ongoing negative effect or condition currently affecting them, " +
            "even if that effect would not normally allow a new saving throw.",
            [ExemplarTraits.Ikon],
            null
        ).WithIllustration(IllustrationName.TiaraOfOpenSkies), q =>
        {
            q.AddGrantingOfTechnical(cr => cr.FriendOf(q.Owner), qe =>
            {
                qe.BonusToAttackRolls = (qe, action, target) =>
                {
                    var wreath = Ikon.GetIkonItemWorn(q.Owner, ikonRune);
                    if (wreath != null && qe.Owner.DistanceTo(q.Owner) <= 3 && action.HasTrait(Trait.Attack))
                    {
                        return new Bonus(1, BonusType.Status, "Victor's Wreath", true);
                    }
                    return null;
                };
            });
            q.SpawnsAura = q => q.Owner.AnimationData.AddAuraAnimation(IllustrationName.BlessCircle, 3);
        }, q =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                IllustrationName.TiaraOfOpenSkies,
                "One Moment till Glory",
                [Trait.Concentrate, Trait.Emotion, Trait.Mental, ExemplarTraits.Transcendence],
                "You rally your allies, carrying them from the brink of disaster to the verge of victory. Each ally in your aura can immediately attempt a new saving throw with a +2 status bonus against one ongoing negative effect or condition currently affecting them, " +
                "even if that effect would not normally allow a new saving throw.",
                Target.Self().WithAdditionalRestriction(self =>
                {
                    if (Ikon.GetIkonItemWorn(self, ikonRune) == null)
                    {
                        return "You must be wearing the Victor's Wreath";
                    }
                    return null;
                })
            ).WithActionCost(1)
            .WithEffectOnChosenTargets(async (action, self, target) =>
            {
                var allies = self.Battle.AllCreatures.Where(a => a.FriendOf(self) && a != self && a.DistanceTo(self) <= 3);
                foreach (var ally in allies)
                {
                    //TODO: QEffects don't save the DC usually, so...
                }
            }));
        })
        .WithRune(ikonRune)
        .WithFreeWornItem(freeItem)
        .IkonFeat;
    }
}

