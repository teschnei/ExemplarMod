using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Dominion;

public class OfVerseUnbroken
{
    [FeatGenerator(7)]
    public static IEnumerable<Feat> GetFeat()
    {
        List<FeatName> sparkOptions = [ExemplarFeats.EnergizedSparkSonic, ExemplarFeats.EnergizedSparkVitality];
        yield return new Epithet(
            ExemplarFeats.OfVerseUnbroken,
            "Though you are a warrior, you respect the power of song, oratory, and other arts, knowing it is these forces that make fights worth fighting.",
            "You gain the Energized Spark feat for your choice of sonic or vitality. " +
            "When you critically succeed on a Strike, haunting melodies play around the target, making them stupefied 1 unless they succeed on a Will save against your class DC.\n\n" +
            "When you Spark Transcendence, your divine spark releases a sublime song that harmonizes with your allies. " +
            "Until the start of your next turn, any of your allies that starts their turn within 30 feet of you can hum to Sustain one of their effects that can be sustained. " +
            "This is a free action triggered by the ally's turn beginning. Your song then ends, and that ally can't benefit from this ability again for 10 minutes. This is an auditory and mental effect.",
            [ExemplarTraits.DominionEpithet],
            sparkOptions.Select(spark => new Feat(spark, "", "", [], null)
                .WithEquivalent(sheet => sheet.HasFeat(spark))
                .WithOnSheet(sheet =>
                {
                    sheet.GrantFeat(ExemplarFeats.EnergizedSpark, spark);
                })).ToList()
        )
        .WithTranscendPossibility("When you Spark Transcendence, your divine spark releases a sublime song that harmonizes with your allies. " +
            "Until the start of your next turn, any of your allies that starts their turn within 30 feet of you can hum to Sustain one of their effects that can be sustained. " +
            "This is a free action triggered by the ally's turn beginning. Your song then ends, and that ally can't benefit from this ability again for 10 minutes. This is an auditory and mental effect.",
            (exemplar, action) => new ActionPossibility(new CombatAction(exemplar, IllustrationName.ResistEnergy, "Of Verse Unbroken", [Trait.Auditory, Trait.Mental],
                    "Your divine spark releases a sublime song that harmonizes with your allies. " +
                    "Until the start of your next turn, any of your allies that starts their turn within 30 feet of you can hum to Sustain one of their effects that can be sustained. " +
                    "This is a free action triggered by the ally's turn beginning. Your song then ends, and that ally can't benefit from this ability again for 10 minutes.",
                    Target.Self())
                .WithActionCost(0)
                .WithEffectOnChosenTargets(async (action, self, targets) =>
                {
                    QEffect qe = new QEffect("Of Verse Unbroken", "Any ally that starts their turn within 30 feet of you can Sustain an effect as a free action.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, self, IllustrationName.SongOfStrength);
                    Zone zone = Zone.Spawn(qe, ZoneAttachment.Aura(6));
                    zone.AfterCreatureBeginsItsTurnHere = async (cr) =>
                    {
                        if (cr.FriendOf(qe.Owner) && !cr.HasEffect(ExemplarQEffects.OfVerseUnbrokenUsedOnTarget))
                        {
                            bool used = true;
                            var actions = Possibilities.Create(cr).Filter(ap => ap.CombatAction.HasTrait(Trait.SustainASpell));
                            actions.Sections.Add(new PossibilitySection("Pass")
                            {
                                Possibilities = [new ActionPossibility(new CombatAction(cr, IllustrationName.EndTurn, "Pass", [
                                    Trait.Basic,
                                    Trait.UsableEvenWhenUnconsciousOrParalyzed,
                                    Trait.DoesNotPreventDelay
                                ], "Do nothing.", Target.Self()).WithActionCost(0).WithEffectOnChosenTargets(async (_, _, _) => used = false))]
                            });
                            typeof(Creature).InvokeMember("Possibilities", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, cr, [actions]);
                            var options = await cr.Battle.GameLoop.CreateActions(cr, cr.Possibilities, null);
                            cr.Battle.GameLoopCallback.AfterActiveCreaturePossibilitiesRegenerated();
                            await cr.Battle.GameLoop.OfferOptions(cr, options, true);
                            if (used)
                            {
                                qe.ExpiresAt = ExpirationCondition.Immediately;
                                cr.AddQEffect(new QEffect()
                                {
                                    Id = ExemplarQEffects.OfVerseUnbrokenUsedOnTarget
                                });
                            }
                        }
                    };
                    self.AddQEffect(qe);
                })
            )
        )
        .WithPermanentQEffect(null, q =>
        {
            q.AfterYouTakeAction = async (selfQf, action) =>
            {
                if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess && action.ChosenTargets.ChosenCreature != null)
                {
                    if (CommonSpellEffects.RollSavingThrow(action.ChosenTargets.ChosenCreature, CombatAction.CreateSimple(selfQf.Owner, "Of Verse Unbroken", []), Defense.Will, selfQf.Owner.ClassDC()) <= CheckResult.Failure)
                    {
                        action.ChosenTargets.ChosenCreature.AddQEffect(QEffect.Stupefied(1));
                    }
                }
            };
        });
    }
}
