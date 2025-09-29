using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level1;

public class EnergizedSpark
{
    [FeatGenerator(1)]
    public static IEnumerable<Feat> GetFeat()
    {
        Dictionary<FeatName, (Trait, DamageKind)> sparkOptions = new()
        {
            [ExemplarFeats.EnergizedSparkAir] = (Trait.Air, DamageKind.Slashing),
            [ExemplarFeats.EnergizedSparkCold] = (Trait.Cold, DamageKind.Cold),
            [ExemplarFeats.EnergizedSparkEarth] = (Trait.Earth, DamageKind.Bludgeoning),
            [ExemplarFeats.EnergizedSparkElectricity] = (Trait.Electricity, DamageKind.Electricity),
            [ExemplarFeats.EnergizedSparkFire] = (Trait.Fire, DamageKind.Fire),
            [ExemplarFeats.EnergizedSparkMetal] = (Trait.Metal, DamageKind.Slashing),
            [ExemplarFeats.EnergizedSparkPoison] = (Trait.Poison, DamageKind.Poison),
            [ExemplarFeats.EnergizedSparkSonic] = (Trait.Sonic, DamageKind.Sonic),
            [ExemplarFeats.EnergizedSparkVitality] = (Trait.Positive, DamageKind.Positive),
            [ExemplarFeats.EnergizedSparkVoid] = (Trait.Negative, DamageKind.Negative),
            [ExemplarFeats.EnergizedSparkWater] = (Trait.Water, DamageKind.Bludgeoning),
            [ExemplarFeats.EnergizedSparkWood] = (Trait.Wood, DamageKind.Piercing)
        };

        yield return new TrueFeat(
            ExemplarFeats.EnergizedSpark,
            1,
            "The energy of your spirit manifests as crackling lightning, the chill of winter, or the power of an element.",
            "Choose one of the following traits: air (slashing), cold, earth (bludgeoning), electricity, fire, metal (slashing), poison, sonic, vitality, void, water (bludgeoning), or wood (piercing). " +
            "Your spirit damage dealt by your exemplar abilities will gain the trait and deal the corresponding damage type if it would deal more damage.",
            [ExemplarTraits.Exemplar],
            sparkOptions.Select(options => new Feat(options.Key, "", "", [options.Value.Item1], null)
                .WithOnCreature(creature =>
                {
                    var sparkList = creature.FindQEffect(ExemplarQEffects.EnergizedSpark)?.Tag as List<(Trait, DamageKind)>;
                    if (sparkList != null)
                    {
                        sparkList.Add(options.Value);
                    }
                })).ToList()
            )
        .WithMultipleSelection()
        .WithPermanentQEffect(null, qf =>
        {
            qf.Id = ExemplarQEffects.EnergizedSpark;
            qf.Tag = new List<(Trait, DamageKind)>();
        });
    }
}
