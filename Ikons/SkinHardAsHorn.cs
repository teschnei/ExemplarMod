using System.Linq;
using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class SkinHardAsHorn
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        Dictionary<FeatName, DamageKind> skinTypes = new()
        {
            [ExemplarFeats.SkinHardAsHornBludgeoning] = DamageKind.Bludgeoning,
            [ExemplarFeats.SkinHardAsHornPiercing] = DamageKind.Piercing,
            [ExemplarFeats.SkinHardAsHornSlashing] = DamageKind.Slashing
        };

        var skinFeats = skinTypes.Select(feats =>
        {
            return new Feat(feats.Key, null, $"Attune the Skin Hard as Horn to {feats.Value.ToString()}.", [ExemplarTraits.SkinHardAsHornAttune], null)
                .WithOnCreature(creature =>
                {
                    var skin = creature.FindQEffect(ExemplarQEffects.SkinHardAsHornAttunement);
                    if (skin != null)
                    {
                        skin.Tag = feats.Value;
                    }
                });
        });

        foreach (var feat in skinFeats)
        {
            yield return feat;
        }

        yield return new Ikon(new Feat(
            ExemplarFeats.SkinHardAsHorn,
            "Tempered in your spirit, your very skin is as a suit of armor, though a single location on your body remains unprotected, a curse and a challenge within your legend.",
            "During your daily preparations, you can strike your skin lightly with an object that deals bludgeoning, slashing, or piercing damage to habituate your skin against this type of injury, attuning the ikon to that damage type.\n\n" +
            "{b}Immanence{/b} When your skin houses your divine spark, you gain resistance to the attuned damage type equal to half your level. This resistance doesn't apply against critical hits, which successfully find your unprotected spot.\n\n" +
            $"{{b}}Transcendence — Crash Against Me {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (transcendence)\n" +
            "Your skin becomes virtually unbreakable. Until the start of your next turn, you have resistance equal to your level to the chosen damage type. During this time, if a creature attacking you " +
            "using a weapon dealing the same damage type as your resistance misses you or hits you but deals no damage due to your resistance, the weapon clangs wildly off your skin. This painful reverberation makes " +
            "the attacking enemy off-guard and gives it a -2 circumstance penalty to attacks with that weapon until the start of the enemy's next turn.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonBody],
            null
        ).WithIllustration(IllustrationName.FullPlate)
        .WithOnSheet(sheet =>
        {
            sheet.AddSelectionOption(new SingleFeatSelectionOption(
                key: "SkinHardAsHorn:Attunement",
                name: "Skin Hard As Horn Attunement",
                level: SelectionOption.MORNING_PREPARATIONS_LEVEL,
                eligible: ft => ft.HasTrait(ExemplarTraits.SkinHardAsHornAttune)
            ));
        })
        .WithPermanentQEffect(null, q =>
        {
            q.Id = ExemplarQEffects.SkinHardAsHornAttunement;
        }), q =>
        {
            q.StateCheck = (qe) =>
            {
                var skin = qe.Owner.FindQEffect(ExemplarQEffects.SkinHardAsHornAttunement);
                if (skin != null && skin.Tag is DamageKind tag)
                {
                    //TODO: ask if there's a better way
                    qe.Owner.WeaknessAndResistance.AddSpecialResistance(tag.HumanizeLowerCase2(), (action, damageKind) => damageKind == tag && action?.CheckResult != CheckResult.CriticalSuccess, qe.Owner.Level / 2, "critical hits");
                }
            };
        },
        q =>
        {
            return new ActionPossibility(new CombatAction(q.Owner, IllustrationName.FullPlate,
                "Crash Against Me", [ExemplarTraits.Transcendence],
                "Your skin becomes virtually unbreakable. Until the start of your next turn, you have resistance equal to your level to the chosen damage type. During this time, if a creature attacking you " +
                "using a weapon dealing the same damage type as your resistance misses you or hits you but deals no damage due to your resistance, the weapon clangs wildly off your skin. This painful reverberation makes " +
                "the attacking enemy off-guard and gives it a -2 circumstance penalty to attacks with that weapon until the start of the enemy's next turn.",
                Target.Self())
                .WithActionCost(1)
                .WithEffectOnChosenTargets(async (action, self, targets) =>
                {
                    self.AddQEffect(new QEffect("Crash Against Me", "Your skin is virtually unbreakable. You have resistance to your chosen damage type and will retaliate against foes who deal no damage to you with their weapon.",
                        ExpirationCondition.ExpiresAtStartOfYourTurn, self, IllustrationName.FullPlate)
                    {
                        AfterYouTakeIncomingDamageEventEvenZero = async (q, damageEvent) =>
                        {
                            var skin = q.Owner.FindQEffect(ExemplarQEffects.SkinHardAsHornAttunement);
                            if (skin != null && skin.Tag is DamageKind kind)
                            {
                                if (damageEvent.KindedDamages.Sum((KindedDamage part) => part.ResolvedDamage) <= 0 && damageEvent.KindedDamages.Any(kinded => kinded.DamageKind == kind))
                                {
                                    if (damageEvent.CombatAction != null)
                                    {
                                        ApplyPenalty(q.Owner, damageEvent.CombatAction.Owner, damageEvent.CombatAction.Item);
                                    }
                                }
                            }
                        },
                        AfterYouAreTargeted = async (q, action) =>
                        {
                            var skin = q.Owner.FindQEffect(ExemplarQEffects.SkinHardAsHornAttunement);
                            if (skin != null && skin.Tag is DamageKind kind)
                            {
                                if (action.CheckResult <= CheckResult.Failure && (action.Item?.DetermineDamageKinds().Contains(kind) ?? false))
                                {
                                    ApplyPenalty(q.Owner, action.Owner, action.Item);
                                }
                            }

                        }
                    });
                    void ApplyPenalty(Creature source, Creature target, Item? item)
                    {
                        target.AddQEffect(new QEffect("Crashed Against", "The painful reverberation of Skin Hard As Horn makes you off-guard and have a -2 circumstance penalty to attacks with the associated weapon.",
                                    ExpirationCondition.ExpiresAtStartOfYourTurn, source, IllustrationName.Flatfooted)
                        {
                            IsFlatFootedTo = (_, _, _) => "Crash Against Me",
                            BonusToAttackRolls = (q, action, target) => action.Item == item && action.HasTrait(Trait.Strike) ? new Bonus(-2, BonusType.Circumstance, "Crash Against Me") : null
                        });
                    }
                }));
        }).IkonFeat;
    }
}
