using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Auxiliary;
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
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class VictorsWreath
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
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
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWorn],
            null
        ).WithIllustration(ExemplarIllustrations.VictorsWreath), (ikon, q) =>
        {
            q.AddGrantingOfTechnical(cr => cr.FriendOf(q.Owner), qe =>
            {
                qe.BonusToAttackRolls = (qe, action, target) =>
                {
                    var wreath = ikon.GetWornIkon(q.Owner);
                    if (wreath != null && qe.Owner.DistanceTo(q.Owner) <= 3 && action.HasTrait(Trait.Attack))
                    {
                        return new Bonus(1, BonusType.Status, "Victor's Wreath", true);
                    }
                    return null;
                };
            });
            q.SpawnsAura = q => q.Owner.AnimationData.AddAuraAnimation(IllustrationName.BlessCircle, 3);
        }, (ikon, q) =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                ExemplarIllustrations.VictorsWreath,
                "One Moment till Glory",
                [Trait.Concentrate, Trait.Emotion, Trait.Mental, ExemplarTraits.Transcendence],
                "You rally your allies, carrying them from the brink of disaster to the verge of victory. Each ally in your aura can immediately attempt a new saving throw with a +2 status bonus against one ongoing negative effect or condition currently affecting them, " +
                "even if that effect would not normally allow a new saving throw.",
                Target.Self().WithAdditionalRestriction(self =>
                {
                    if (ikon.GetWornIkon(self) == null)
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
                    var effect = ally.QEffects.Where(q => q.CountsAsADebuff && q.SourceAction?.SavingThrow != null && q.SourceAction?.Owner != null).FirstOrDefault();
                    if (effect != null)
                    {
                        var bonus = new QEffect()
                        {
                            BonusToDefenses = (_, _, _) => new Bonus(2, BonusType.Status, "One Moment till Glory", true)
                        };
                        ally.AddQEffect(bonus);
                        if (CommonSpellEffects.RollSavingThrow(ally, effect.SourceAction!, effect.SourceAction!.SavingThrow!.Defense, effect.SourceAction.SavingThrow.DC(effect.SourceAction?.Owner)) >= CheckResult.Success)
                        {
                            //This ability doesn't actually say what it does, so I'm just going to assume it removes the effect on a success
                            effect.ExpiresAt = ExpirationCondition.Immediately;
                        }
                        bonus.ExpiresAt = ExpirationCondition.Immediately;
                    }
                    else
                    {
                        var persistentEffect = ally.QEffects.Where(q => q.Key?.StartsWith("PersistentDamage") ?? false).FirstOrDefault();
                        if (persistentEffect != null)
                        {
                            persistentEffect.RollPersistentDamageRecoveryCheck(false);
                        }
                    }
                }
            }));
        })
        .WithValidItem(item =>
        {
            if (!item.HasTrait(Trait.Worn) || (item.WornAt != Trait.Belt && item.WornAt != Trait.Headwear && item.WornAt != Trait.Headband))
            {
                return "Must be a worn headwear or belt.";
            }
            return null;
        })
        .WithFreeWornItem(freeItem)
        .IkonFeat;
    }
}

