using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class SanctifiedSoul
{
    [FeatGenerator(1)]
    public static IEnumerable<Feat> GetFeat()
    {
        Dictionary<FeatName, (Trait, DamageKind)> sanctifiedOptions = new()
        {
            [ExemplarFeats.SanctifiedSoulHoly] = (Trait.Good, DamageKind.Good),
            [ExemplarFeats.SanctifiedSoulUnholy] = (Trait.Evil, DamageKind.Evil)
        };
        yield return new TrueFeat(
            ExemplarFeats.SanctifiedSoul,
            1,
            "You've drawn a line in the sand in the cosmic struggle between good and evil and chosen a side.",
            "You gain either the holy trait or the unholy trait. All your exemplar abilities that deal spirit damage gain the sanctified trait, " +
            "allowing you to apply your chosen trait to better affect your enemies.",
            [ExemplarTraits.Exemplar],
            sanctifiedOptions.Select(options => new Feat(options.Key, "", "", [options.Value.Item1], null)
                .WithOnCreature(creature =>
                {
                    var sanctified = creature.FindQEffect(ExemplarQEffects.SanctifiedSoul);
                    if (sanctified != null)
                    {
                        sanctified.Tag = options.Value;
                    }
                })).ToList()
        )
        .WithEquivalent(sheet => sheet.HasFeat(ExemplarFeats.VowOfMortalDefiance))
        .WithPermanentQEffect(null, qf =>
        {
            qf.Id = ExemplarQEffects.SanctifiedSoul;
        });
    }
}
