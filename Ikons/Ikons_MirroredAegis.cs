using System.Linq;
using System.Runtime.Serialization;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Exemplar.Utilities;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_MirroredAegis
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            Feat mirroredAegis = new TrueFeat(
                ExemplarFeatNames.IkonMirroredAegis,
                1,
                "This shield is polished so brightly it can reflect even spiritual and ethereal attacks.",
                "{b}Immanence{/b} While empowered, allies within 3 squares of you gain +1 status bonus to AC and Reflex, and +1 status bonus to saves against force, spirit, vitality, and void.\n\n" +
                "{b}Transcendence — Raise the Walls (one action){/b} You and one ally within 15 feet gain +1 status bonus to AC, Reflex, and saves against force, spirit, vitality, and void for 1 minute.",
                [ModTraits.Ikon],
                null
            ).WithMultipleSelection()
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence aura
                qf.StateCheck = effect =>
                {
                    if (!effect.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredMirroredAegis))
                        return;

                    AuraAnimation auraAnimation = qf.Owner.AnimationData.AddAuraAnimation(IllustrationName.BlessCircle, 3f);

                    foreach (var ally in effect.Owner.Battle.AllCreatures)
                    {
                        if (ally.DistanceTo(effect.Owner) > 3
                            || ally.HasEffect(ExemplarIkonQEffectIds.QMirroredAegisAura))
                            continue;


                        // MagicCircleAuraAnimation 
                        ally.AddQEffect(new QEffect("Mirrored Aura", "+1 to AC, Reflex, and key saves", ExpirationCondition.ExpiresAtEndOfYourTurn, qf.Owner, IllustrationName.ShieldSpell)
                        {
                            WhenExpires = delegate
                            {
                                auraAnimation.MoveTo(0f);
                            },
                            Id = ExemplarIkonQEffectIds.QMirroredAegisAura,
                            BonusToDefenses = (eff, act, def) =>
                            {
                                if (act == null || eff?.Owner == null)
                                    return null;
                                    
                                if (def == Defense.AC || def == Defense.Reflex || !act.HasTrait(Trait.Force) || !act.HasTrait(Trait.Positive) || !act.HasTrait(Trait.Negative))
                                {
                                    return new Bonus(1, BonusType.Status, "Mirrored Aegis");
                                }
                                return null;
                            },
                        });
                    }
                };

                // Transcendence action
                qf.ProvideMainAction = qf =>
                {
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredMirroredAegis))
                        return null;

                    var action = new CombatAction(
                        qf.Owner,
                        IllustrationName.SteelShield,
                        "Raise the Walls",
                        [ModTraits.Transcendence, ModTraits.Ikon],
                        "You and an ally within 15 feet gain +1 status bonus to AC, Reflex, and key saves for 1 minute.",
                        Target.Self()
                    ).WithActionCost(1);

                    action.WithEffectOnEachTarget(async (act, caster, target, _) =>
                    {
                        // Choose an ally (or self if no other)
                        var candidates = caster.Battle.AllCreatures
                            .Where(a => a.DistanceTo(caster) <= 3)
                            .ToList();
                        var ally = candidates.Count == 1
                            ? candidates[0]
                            : await caster.Battle.AskToChooseACreature(
                                caster, candidates,
                                IllustrationName.SteelShield,
                                "Choose an ally to shield",
                                "Shield", "Cancel"
                              );

                        void ApplyShield(Creature cr)
                        {

                            cr.AddQEffect(new QEffect("Ethereal Shield", "+1 to AC, Reflex, and key saves", ExpirationCondition.CountsDownAtEndOfYourTurn, qf.Owner, IllustrationName.Shield)
                            {
                                Value = 10,
                                BonusToDefenses = (eff, act2, def) =>
                                    def == Defense.AC || def == Defense.Reflex || !act2.HasTrait(Trait.Force) || !act2.HasTrait(Trait.Positive) || !act2.HasTrait(Trait.Negative)
                                        ? new Bonus(1, BonusType.Status, "Raise the Walls")
                                        : null,
                            });
                        }

                        ApplyShield(caster);
                        if (ally != null)
                            ApplyShield(ally);

                        // Remove empowerment and grant free shift
                        IkonEffectHelper.CleanupEmpoweredEffects(caster, ExemplarIkonQEffectIds.QEmpoweredMirroredAegis);
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(mirroredAegis);
        }
    }
}
