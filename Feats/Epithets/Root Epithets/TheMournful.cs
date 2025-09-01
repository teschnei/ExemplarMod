using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class TheMournful
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Feat(
            ExemplarFeats.TheMournful,
                "To be a hero is to endure countless hardships and stand where others have fallen, shouldering dreams and destinies in their stead. " +
                "Though this weight may reach your eyes, you bear this burden so that those under you can live smiling.",
                "You are trained in Diplomacy. After you Spark Transcendence, your act resonates with bittersweet poignancy, making one enemy of your choice within 30 feet who witnessed the act dazzled as tears or memories dance in their eyes. " +
                "The enemy remains dazzled until the start of your next turn, and then they are immune to this effect for 10 minutes.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Diplomacy);
        })
        .WithPermanentQEffect("After you Spark Transcendence, your act resonates with bittersweet poignancy, making one enemy of your choice within 30 feet who witnessed the act dazzled as tears or memories dance in their eyes. " +
            "The enemy remains dazzled until the start of your next turn, and then they are immune to this effect for 10 minutes.", q =>
        {
            q.AfterYouTakeAction = async (q, action) =>
            {
                if (action.ActionId == ExemplarActions.SparkTranscendence)
                {
                    var target = await q.Owner.Battle.AskToChooseACreature(q.Owner,
                        q.Owner.Battle.AllCreatures.Where(cr => cr.EnemyOf(q.Owner) && cr.DistanceTo(q.Owner) <= 6 && !cr.HasEffect(ExemplarQEffects.TheMournfulUsedOnTarget)),
                        IllustrationName.QuestionMark, "The Mournful: Choose a creature to dazzle with your act.", "Choose this creature to inflict dazzle.", "Don't dazzle anyone");
                    if (target != null)
                    {
                        target.AddQEffect(QEffect.Dazzled().WithExpirationAtStartOfSourcesTurn(q.Owner, 1));
                        target.AddQEffect(new QEffect()
                        {
                            Id = ExemplarQEffects.TheMournfulUsedOnTarget,
                            CountsAsADebuff = true
                        });
                    }
                }
            };
        });
    }
}
