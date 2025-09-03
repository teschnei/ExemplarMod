using System.Collections.Generic;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level6;

public class ReactiveStrike
{
    [FeatGenerator(6)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new TrueFeat(
            ExemplarFeats.FlowOfWar,
            6,
            "You swat at a foe that leaves an opening.",
            "{b}Trigger{/b} A creature within your reach uses a manipulate action or a move action, makes a ranged attack, or leaves a square during a move action it's using.\n\n" +
            "Make a melee Strike against the triggering creature. If your attack is a critical hit and the trigger was a manipulate action, you disrupt that action. This Strike doesn't count towards your multiple attack penalty, and your multiple attack penalty doesn't apply to this Strike.",
            [ExemplarTraits.Exemplar],
            null
        )
        .WithOnCreature(delegate (Creature cr)
        {
            cr.AddQEffect(QEffect.AttackOfOpportunity());
        });
    }
}
