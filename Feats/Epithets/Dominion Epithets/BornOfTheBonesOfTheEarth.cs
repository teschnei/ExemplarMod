using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Dominion;

public class BornOfTheBonesOfTheEarth
{
    [FeatGenerator(7)]
    public static IEnumerable<Feat> GetFeat()
    {
        List<FeatName> sparkOptions = [ExemplarFeats.EnergizedSparkEarth, ExemplarFeats.EnergizedSparkFire];
        yield return new Epithet(
            ExemplarFeats.BornOfTheBonesOfTheEarth,
            "Your dominion is over stone and soil, the pillars holding up the stage of history's great legends.",
            "You gain the Energized Spark feat for your choice of earth or fire. When you critically succeed on a Strike, " +
            "the target is driven down and mired in the ground. The target is immobilized and must succeed at an Escape attempt against your class DC to end the immobilization. " +
            "The creature doesn't become stuck if it is incorporeal, is liquid (like a water elemental or some oozes), has a burrow speed, or could otherwise escape without effort.\n\n" +
            "When you Spark Transcendence, you can choose to shatter the surface you are standing on in a 10-foot emanation, making it difficult terrain. You are not affected by this difficult terrain.",
            [ExemplarTraits.DominionEpithet],
            sparkOptions.Select(spark => new Feat(spark, "", "", [], null)
                .WithEquivalent(sheet => sheet.HasFeat(spark))
                .WithOnSheet(sheet =>
                {
                    sheet.GrantFeat(ExemplarFeats.EnergizedSpark, spark);
                })).ToList()
        )
        .WithTranscendPossibility("When you Spark Transcendence, you can choose to shatter the surface you are standing on in a 10-foot emanation, making it difficult terrain. You are not affected by this difficult terrain.",
            (exemplar, action) =>
            new ActionPossibility(new CombatAction(exemplar, IllustrationName.ElementalBlastEarth, "Born of the Bones of the Earth", [],
                    "You shatter the surface you are standing on in a 10-foot emanation, making it difficult terrain. You are not affected by this difficult terrain.",
                    Target.Emanation(2))
                .WithActionCost(0)
                .WithEffectOnChosenTargets(async (action, self, targets) =>
                {
                    foreach (Tile tile in targets.ChosenTiles.Where((Tile tl) => !tl.AlwaysBlocksMovement))
                    {
                        tile.AddQEffect(new TileQEffect(tile)
                        {
                            Illustration = IllustrationName.Rock,
                            TileQEffectId = ExemplarTileQEffects.BornOfTheBonesOfTheEarthTerrain,
                            TransformsTileIntoDifficultTerrain = true
                        });
                    }
                })
            )
        )
        .WithPermanentQEffect(null, q =>
        {
            q.AfterYouTakeAction = async (selfQf, action) =>
            {
                if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess && action.ChosenTargets.ChosenCreature != null)
                {
                    if (action.ChosenTargets.ChosenCreature.HasTrait(Trait.Incorporeal) || action.ChosenTargets.ChosenCreature.HasTrait(Trait.Ooze))
                    {
                        action.ChosenTargets.ChosenCreature.Battle.Log(action.ChosenTargets.ChosenCreature?.ToString() + " is incorporeal or an ooze and so immune to Born of the Bones of the Earth.");
                        return;
                    }
                    QEffect immobilized = QEffect.Immobilized().WithExpirationNever();
                    immobilized.ProvideContextualAction = q => new ActionPossibility(Possibilities.CreateEscapeAgainstEffect(
                            q.Owner, q, "Born of the Bones of the Earth", selfQf.Owner.ClassDC())).WithPossibilityGroup("Remove debuff");
                    action.ChosenTargets.ChosenCreature.AddQEffect(immobilized);
                }
            };
        });
    }
}
