using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using Microsoft.Xna.Framework;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class MirroredAegis
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("MirroredAegis", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Mirrored Aegis", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonShield)
            .WithRuneProperties(new RuneProperties("Ikon", IkonRuneKind.MirroredAegis, "This shield is polished so brightly it can reflect even spiritual and ethereal attacks.",
            "", item =>
            {
                item.Traits.AddRange([ExemplarTraits.Ikon, Trait.Divine]);
            })
            .WithCanBeAppliedTo((Item rune, Item shield) =>
            {
                if (!shield.HasTrait(Trait.Shield))
                {
                    return "Must be a Shield.";
                }
                return null;
            }));
        });

        yield return new Ikon(new Feat(
            ExemplarFeats.MirroredAegis,
            "This shield is polished so brightly it can reflect even spiritual and ethereal attacks.",
            "{b}Usage{/b} any shield\n\n" +
            "{b}Immanence{/b} The {i}mirrored aegis{/i} emits an aura in a 15-foot emanation that protects you and all allies in the aura from harm, granting a +1 status bonus to AC.\n\n" +
            $"{{b}}Transcendence — Raise the Walls {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (force, transcendence)\n" +
            "You raise the {i}mirrored aegis{/i}, which summons ethereal shields that surround you and one ally of your choice within 15 feet in a tortoise shield formation. You and the ally gain a +1 status bonus to AC, Reflex saves, and any save against a force, spirit, vitality, or void effect for 1 minute.",
            [ExemplarTraits.Ikon],
            null
        ).WithIllustration(IllustrationName.DragonClaws), q =>
        {
            q.AddGrantingOfTechnical(cr => cr.FriendOf(q.Owner), qe =>
            {
                qe.BonusToDefenses = (qe, action, defense) =>
                {
                    var aegis = Ikon.GetIkonItem(q.Owner, ikonRune);
                    if (aegis != null && qe.Owner.DistanceTo(q.Owner) <= 3 && defense == Defense.AC)
                    {
                        return new Bonus(1, BonusType.Status, "Mirrored Aegis", true);
                    }
                    return null;
                };
            });
            q.SpawnsAura = q => q.Owner.AnimationData.AddAuraAnimation(IllustrationName.BlessCircle, 3, Color.White);
        }, q =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                IllustrationName.SteelShield,
                "Raise the Walls",
                [ExemplarTraits.Transcendence],
                "You raise the {i}mirrored aegis{/i}, which summons ethereal shields that surround you and one ally of your choice within 15 feet in a tortoise shield formation. You and the ally gain a +1 status bonus to AC, Reflex saves, and any save against a force, spirit, vitality, or void effect for 1 minute.",
                Target.RangedFriend(3).WithAdditionalConditionOnTargetCreature((self, target) =>
                {
                    var aegis = Ikon.GetIkonItem(q.Owner, ikonRune);
                    if (aegis == null)
                    {
                        return Usability.NotUsable("You must be wielding the mirrored aegis.");
                    }
                    return Usability.Usable;
                })
            ).WithActionCost(1)
            .WithEffectOnChosenTargets(async (action, self, targets) =>
            {
                void ApplyShield(Creature cr)
                {
                    cr.AddQEffect(new QEffect("Raise the Walls", "Ethereal shields from a mirrored aegis protect you, granting you a +1 status bonus to AC, Reflex saves, and any save against a force, spirit, vitality, or void effect.", ExpirationCondition.Never, q.Owner, IllustrationName.Shield)
                    {
                        Id = ExemplarQEffects.RaiseTheWalls,
                        BonusToDefenses = (_, offensiveAction, defense) =>
                            (defense == Defense.AC || defense == Defense.Reflex ||
                            (offensiveAction != null && (offensiveAction.HasTrait(Trait.Force) || offensiveAction.HasTrait(Trait.Positive) || offensiveAction.HasTrait(Trait.Negative))))
                                ? new Bonus(1, BonusType.Status, "Raise the Walls")
                                : null,
                    });
                }

                foreach (var creature in self.Battle.AllCreatures)
                {
                    creature.RemoveAllQEffects(q => q.Id == ExemplarQEffects.RaiseTheWalls && q.Source == self);
                }
                ApplyShield(self);
                if (targets.ChosenCreature != null && targets.ChosenCreature != self)
                {
                    ApplyShield(targets.ChosenCreature);
                }
            }));
        })
        .WithRune(ikonRune)
        .IkonFeat;
    }
}
