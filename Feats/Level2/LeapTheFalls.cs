using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class LeapTheFalls
{
    [FeatGenerator(2)]
    public static IEnumerable<Feat> GetFeat()
    {
        var flavorText = "Reinforcing your legs with divine energy, you can leap the battlefield as powerfully as a salmon clearing a waterfall.";
        var rulesText = "{b}Usage{/b} imbued into a body ikon\n\n The imbued ikon gains the following ability.\n\n" +
            "{b}Immanence{/b} You gain the Powerful Leap general feat, allowing you to jump further, even if you do not meet the prerequisite for it.";
        yield return new TrueFeat(
            ExemplarFeats.LeapTheFalls,
            2,
            flavorText,
            rulesText,
            [ExemplarTraits.Exemplar, ExemplarTraits.IkonExpansion],
            Ikon.AddExpansionFeat("LeapTheFalls", flavorText, rulesText, [], ikon => ikon.IkonFeat.HasTrait(ExemplarTraits.IkonBody), (ikon, feat) =>
            {
                feat.WithPermanentQEffect(null, q =>
                {
                    q.AfterYouAcquireEffect = async (q, newQ) =>
                    {
                        if (newQ.Id == ikon.EmpoweredQEffectId)
                        {
                            var leap = new QEffect()
                            {
                                Id = QEffectId.PowerfulLeap
                            };
                            q.Owner.AddQEffect(leap);
                            q.Owner.AddQEffect(new QEffect()
                            {
                                Id = ExemplarQEffects.IkonExpansion,
                                WhenExpires = q =>
                                {
                                    q.Owner.RemoveAllQEffects(qe => qe == leap);
                                }
                            });
                        }
                    };
                });
            }).ToList()
        );
    }
}
