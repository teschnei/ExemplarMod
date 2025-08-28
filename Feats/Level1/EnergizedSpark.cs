using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class EnergizedSpark
{
    /*
    public static readonly Dictionary<FeatName, string> AttunementDescriptions = new()
    {
        { AttunementFeats[0], "Makes your spirit damage slashing, as though carried on razor-sharp wind." },
        { AttunementFeats[1], "Makes your spirit damage cold, chilling and numbing your foe." },
        { AttunementFeats[2], "Makes your spirit damage bludgeoning, like a crushing earth strike." },
        { AttunementFeats[3], "Makes your spirit damage electricity, crackling through armor." },
        { AttunementFeats[4], "Makes your spirit damage fire, scorching whatever it hits." },
        { AttunementFeats[5], "Makes your spirit damage slashing, like a forged metal blade." },
        { AttunementFeats[6], "Makes your spirit damage poison, inflicting potent toxins." },
        { AttunementFeats[7], "Makes your spirit damage sonic, tearing with deafening force." },
        { AttunementFeats[8], "Makes your spirit damage bludgeoning, like a tidal water surge." },
        { AttunementFeats[9], "Makes your spirit damage piercing, like a sharpened wooden spear." }
    };
    */
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
