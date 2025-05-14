// file: Exemplar_EpithetTheProud.cs
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
/*
    I need to brainstorm this one, I think it works, but it is strange.
*/
namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_EpithetTheProud
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var theProud = new TrueFeat(
                ExemplarFeatNames.EpithetTheProud,
                3,
                "The Proud[WIP]",
                "Whether out of overconfidence, a desire to protect your comrades, or the unslakable thirst for glory, you invite challengers to strike you down. " +
                "You are trained in Intimidation. After you Spark Transcendence, you can boast to one enemy within 6 squares (30 ft) to draw its attention; " +
                "this effect has the auditory, emotion, mental, and linguistic traits. Until the start of your next turn, the target takes a –1 status penalty " +
                "to attack rolls, damage rolls, and skill checks against creatures other than you, and it gains a +1 status bonus to these rolls when targeting you.",
                new[] { ModTraits.RootEpithet },
                null
            )
            .WithOnSheet(sheet =>
            {
                // Train Intimidation
                sheet.GrantFeat(FeatName.Intimidation);
            })
            .WithPermanentQEffect(null, qf =>
            {
                qf.AfterYouTakeAction = async (selfQf, action) =>
                {
                    // Only once per turn, and only after Transcendence
                    if (!action.HasTrait(ModTraits.Transcendence)
                        || selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QTheProudUsedThisTurn))
                        return;

                    // Inject the free “Boast”
                    qf.ProvideMainAction = qf2 =>
                    {
                        if(selfQf.Owner.HasEffect(ExemplarIkonQEffectIds.QTheProudUsedThisTurn))
                            return null;

                        var boast = new CombatAction(
                            qf2.Owner,
                            IllustrationName.Rage,
                            "The Proud: Boast",
                            new[]
                            {
                                ModTraits.Epithet, ModTraits.Transcendence,
                                Trait.Auditory, Trait.Emotion,
                                Trait.Mental, Trait.Linguistic
                            },
                            "Boast to one enemy within 6 squares (30 ft) to draw its attention; until the start of your next turn, " +
                            "that target takes –1 status to attack rolls, damage rolls, and skill checks against creatures other than you, " +
                            "and gains +1 status to those rolls when targeting you.",
                            Target.Distance(6)   // 6 squares = 30 ft
                        ).WithActionCost(0);

                        // When you use it…
                        boast.WithEffectOnEachTarget(async (_, caster, target, __) =>
                        {
                            // 1) Apply the boast effect to the target
                            var effect = new QEffect(
                                "Proud Boast",
                                "You are so drawn in by the boast that your focus narrows.",
                                ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                                caster,
                                IllustrationName.Rage
                            )
                            {
                                Id = ExemplarIkonQEffectIds.QTheProudEffect,
                                // Here you’d wire in your numeric modifiers:
                                BonusToAttackRolls = (eff, act, defender) =>
                                    qf.Owner == caster
                                        ? new Bonus(1, BonusType.Status, "The Proud")
                                        : new Bonus(-1, BonusType.Status, "The Proud"),
                                BonusToDamage = (eff, act, defender) =>
                                    qf.Owner == caster
                                        ? new Bonus(1, BonusType.Status, "The Proud")
                                        : new Bonus(-1, BonusType.Status, "The Proud"),
                                BonusToAllChecksAndDCs = eff =>
                                    qf.Owner == caster
                                        ? new Bonus(1, BonusType.Status, "The Proud")
                                        : new Bonus(-1, BonusType.Status, "The Proud"),
                            };
                            target.AddQEffect(effect);

                            // 2) Mark your use so you can’t do it again until next turn
                            caster.AddQEffect(new QEffect(
                                    "The Proud Used",
                                    "You have used The Proud this turn.",
                                    ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                                    caster,
                                    IllustrationName.Chaos
                                )
                                {
                                    Id    = ExemplarIkonQEffectIds.QTheProudUsedThisTurn,
                                    Value = 1
                                }
                            );
                        });

                        return new ActionPossibility(boast);
                    };
                };
            });

            ModManager.AddFeat(theProud);
        }
    }
}
