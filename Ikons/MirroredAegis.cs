using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using Microsoft.Xna.Framework;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class MirroredAegis
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Ikon(new Feat(
            ExemplarFeats.MirroredAegis,
            "This shield is polished so brightly it can reflect even spiritual and ethereal attacks.",
            "{b}Usage{/b} any shield\n\n" +
            "{b}Immanence{/b} The {i}mirrored aegis{/i} emits an aura in a 15-foot emanation that protects you and all allies in the aura from harm, granting a +1 status bonus to AC.\n\n" +
            $"{{b}}Transcendence — Raise the Walls {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (force, transcendence)\n" +
            "You Raise the {i}mirrored aegis{/i}, which summons ethereal shields that surround you and one ally of your choice within 15 feet in a tortoise shield formation. You and the ally gain a +1 status bonus to AC, Reflex saves, and any save against a force, spirit, vitality, or void effect for 1 minute.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWorn],
            null
        ).WithIllustration(ExemplarIllustrations.MirroredAegis), (ikon, q) =>
        {
            q.AddGrantingOfTechnical(cr => cr.FriendOf(q.Owner), qe =>
            {
                qe.BonusToDefenses = (qe, action, defense) =>
                {
                    var aegis = ikon.GetHeldIkon(q.Owner);
                    if (aegis != null && qe.Owner.DistanceTo(q.Owner) <= 3 && defense == Defense.AC)
                    {
                        return new Bonus(1, BonusType.Status, "Mirrored Aegis", true);
                    }
                    return null;
                };
            });
            q.SpawnsAura = q => q.Owner.AnimationData.AddAuraAnimation(IllustrationName.BlessCircle, 3, Color.White);
        }, (ikon, q) =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                ExemplarIllustrations.MirroredAegis,
                "Raise the Walls",
                [ExemplarTraits.Transcendence],
                "You Raise the {i}mirrored aegis{/i}, which summons ethereal shields that surround you and one ally of your choice within 15 feet in a tortoise shield formation. You and the ally gain a +1 status bonus to AC, Reflex saves, and any save against a force, spirit, vitality, or void effect for 1 minute.",
                Target.RangedFriend(3).WithAdditionalConditionOnTargetCreature((self, target) =>
                {
                    var aegis = ikon.GetHeldIkon(q.Owner);
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
                    cr.RemoveAllQEffects(q => q.Id == ExemplarQEffects.RaiseTheWalls && q.Source == self);
                    cr.AddQEffect(new QEffect("Raise the Walls", "Ethereal shields from a mirrored aegis protect you, granting you a +1 status bonus to AC, Reflex saves, and any save against a force, spirit, vitality, or void effect.", ExpirationCondition.Never, q.Owner, ExemplarIllustrations.MirroredAegis)
                    {
                        Id = ExemplarQEffects.RaiseTheWalls,
                        BonusToDefenses = (_, offensiveAction, defense) =>
                            (defense == Defense.AC || defense == Defense.Reflex ||
                            (offensiveAction != null && (offensiveAction.HasTrait(Trait.Force) || offensiveAction.HasTrait(ExemplarTraits.Spirit) || offensiveAction.HasTrait(Trait.Positive) || offensiveAction.HasTrait(Trait.Negative))))
                                ? new Bonus(1, BonusType.Status, "Raise the Walls")
                                : null,
                    });
                }

                ApplyShield(self);
                if (targets.ChosenCreature != null && targets.ChosenCreature != self)
                {
                    ApplyShield(targets.ChosenCreature);
                }
                var raiseShield = ((ActionPossibility)Fighter.CreateRaiseShield(self, ikon.GetHeldIkon(q.Owner)!)).CombatAction.WithActionCost(0);
                await raiseShield.AllExecute();
            }));
        })
        .WithValidItem(item =>
        {
            if (!item.HasTrait(Trait.Shield))
            {
                return "Must be a Shield.";
            }
            return null;
        })
        .IkonFeat;
    }
}
