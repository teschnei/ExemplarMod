using System.Linq;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Specific;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Mods.Exemplar.Utilities;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_FetchingBangles
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var bangles = new TrueFeat(
                ExemplarFeatNames.IkonFetchingBangles,
                1,
                "Fetching Bangles",
                "These lovely armbands sparkle and gleam, reflecting your own incredible magnetism.\n\n" +
                "{b}Immanence — Aura, Mental{/b} Others find it hard to leave your captivating presence. An aura surrounds you in a 10-foot emanation. " +
                "An enemy in the aura that attempts to move away from you must succeed at a Will save against your class DC or its move action is disrupted.\n\n" +
                "{b}Transcendence — Embrace of Destiny (one-action){/b} Mental, Spirit, Transcendence\n" +
                "Choose an enemy within 20 feet of you. It must succeed at a Will save against your class DC or be pulled directly toward you into a square adjacent to you.",
                new[] { ModTraits.Ikon , ModTraits.BodyIkon},
                null
            ).WithMultipleSelection()
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: aura that disrupts enemy movement
                qf.StateCheck = effect =>
                {
                    if (!qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredFetchingBangles))
                        return; // Only applies if empowered!
                    var owner = effect.Owner;
                    // Process foes within 10 ft
                    foreach (var foe in owner.Battle.AllCreatures.Where(c => c.OwningFaction != owner.OwningFaction))
                    {
                        bool inAura = foe.DistanceTo(owner) <= 2;
                        // Remove existing aura effect if moved out
                        if (!inAura)
                        {
                            foe.RemoveAllQEffects(q => q.Id == ExemplarIkonQEffectIds.QFetchingBanglesAura);
                        }
                        else if (inAura && !foe.HasEffect(ExemplarIkonQEffectIds.QFetchingBanglesAura))
                        {
                            // Add disruption tracker
                            foe.AddQEffect(new QEffect("Bangles Disruption Aura", "Your movement may be disrupted by Fetching Bangles.", ExpirationCondition.Ephemeral, qf.Owner, IllustrationName.FreedomOfMovement)
                            {
                                Id = ExemplarIkonQEffectIds.QFetchingBanglesAura,
                                AfterYouTakeAction = async (q, action) =>
                                {
                                    if (!action.HasTrait(Trait.Move))
                                        return;
                                    var save =  CommonSpellEffects.RollSavingThrow(foe,action,Defense.Will,q.Owner.ClassDC());
                                    if (save.ToString() == "critical failure" || save.ToString() == "failure")
                                    {
                                        action.RevertRequested = true;
                                        foe.Occupies.Overhead("Your movement is disrupted by Fetching Bangles!", Color.Orange);
                                    }
                                }
                            });
                        }
                    }
                };

                // Transcendence: pull a foe adjacent
                qf.ProvideMainAction = qf =>
                {
                    var owner = qf.Owner;
                    if (!owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredFetchingBangles))
                        return null;

                    var action = new CombatAction(
                        owner,
                        IllustrationName.Whip,
                        "Embrace of Destiny",
                        new[] { Trait.Mental, ModTraits.Transcendence, ModTraits.Ikon },
                        "Choose an enemy within 20 feet. It must succeed at a Will save or be pulled to a square adjacent to you.",
                        Target.Ranged(20)
                    ).WithActionCost(1)
                     .WithSavingThrow(new SavingThrow(Defense.Will, qf.Owner.ClassDC()))
                     .WithEffectOnEachTarget(async (act, caster, target, result) =>
                    {
                            
                        if (result.Equals(CheckResult.CriticalFailure) || result.Equals(CheckResult.Failure))
                        {
                            var map = caster.Battle.Map;
                            var adj = map.AllTiles
                                .Where(tile => tile.DistanceTo(caster.Occupies) == 1 && tile.PrimaryOccupant == null)
                                    .FirstOrDefault();

                            if (adj != null)
                                await CommonSpellEffects.Teleport(target, adj);
                            
                        };
                        // Cleanup empowerment
                        IkonEffectHelper.CleanupEmpoweredEffects(caster, ExemplarIkonQEffectIds.QEmpoweredFetchingBangles);
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(bangles);
        }
    }
}
