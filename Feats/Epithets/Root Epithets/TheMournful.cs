using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Root;

public class TheMournful
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Epithet(
            ExemplarFeats.TheMournful,
                "To be a hero is to endure countless hardships and stand where others have fallen, shouldering dreams and destinies in their stead. " +
                "Though this weight may reach your eyes, you bear this burden so that those under you can live smiling.",
                "You are trained in Diplomacy. After you Spark Transcendence, your act resonates with bittersweet poignancy, making one enemy of your choice within 30 feet who witnessed the act dazzled as tears or memories dance in their eyes. " +
                "The enemy remains dazzled until the start of your next turn, and then they are immune to this effect for 10 minutes.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithTranscendPossibility("After you Spark Transcendence, your act resonates with bittersweet poignancy, making one enemy of your choice within 30 feet who witnessed the act dazzled as tears or memories dance in their eyes. " +
                "The enemy remains dazzled until the start of your next turn, and then they are immune to this effect for 10 minutes.", (exemplar, action) =>
            new ActionPossibility(new CombatAction(exemplar, IllustrationName.DazzlingFlash, "The Mournful", [],
                    "Your act resonates with bittersweet poignancy, making one enemy of your choice within 30 feet who witnessed the act dazzled as tears or memories dance in their eyes. " +
                    "The enemy remains dazzled until the start of your next turn, and then they are immune to this effect for 10 minutes.",
                    Target.Ranged(6).WithAdditionalConditionOnTargetCreature((user, cr) => cr.HasEffect(ExemplarQEffects.TheMournfulUsedOnTarget) ? Usability.NotUsableOnThisCreature("already targeted once") : Usability.Usable))
                .WithActionCost(0)
                .WithEffectOnEachTarget(async (action, self, target, _) =>
                {
                    target.AddQEffect(QEffect.Dazzled().WithExpirationAtStartOfSourcesTurn(self, 1));
                    target.AddQEffect(new QEffect()
                    {
                        Id = ExemplarQEffects.TheMournfulUsedOnTarget,
                        CountsAsADebuff = true
                    });
                })
            )
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Diplomacy);
        });
    }
}
