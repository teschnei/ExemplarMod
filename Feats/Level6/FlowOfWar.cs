using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level6;

public class FlowOfWar
{
    [FeatGenerator(6)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new TrueFeat(
            ExemplarFeats.FlowOfWar,
            6,
            "Divine battle instincts take over your body, letting you move and lash out with instrinctive speed.",
            "{b}Frequency{/b} once per combat\n{b}Trigger{/b}Your turn begins.\n\nYou become quickened until the end of your turn and can use the extra action only to Strike or Stride.",
            [ExemplarTraits.Exemplar],
            null
        )
        .WithActionCost(0)
        .WithPermanentQEffect("You become quickened until the end of your turn and can use the extra action only to Strike or Stride.", q =>
        {
            q.ProvideMainAction = q.Owner.Actions.ActionHistoryThisTurn.Count() == 0 && !q.Owner.HasEffect(ExemplarQEffects.FlowOfWarUsed) ? q => new ActionPossibility(new CombatAction(
                    q.Owner,
                    IllustrationName.Haste,
                    "Flow of War",
                    [],
                    "You become quickened until the end of your turn and can use the extra action only to Strike or Stride.",
                    Target.Self()
                )
                .WithActionCost(0)
                .WithEffectOnChosenTargets(async (self, targets) =>
                {
                    self.AddQEffect(QEffect.Quickened(action => action.HasTrait(Trait.Strike) || action.HasTrait(Trait.Move)).WithExpirationAtEndOfOwnerTurn());
                    self.AddQEffect(new QEffect()
                    {
                        Id = ExemplarQEffects.FlowOfWarUsed,
                        ExpiresAt = ExpirationCondition.Never
                    });
                })
            ) : null;
        });
    }
}
