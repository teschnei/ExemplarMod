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

namespace Dawnsbury.Mods.Classes.Exemplar;

public class LightningSwap
{
    [FeatGenerator(2)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new TrueFeat(
            ExemplarFeats.LightningSwap,
            2,
            "You have practiced quickly switching between combat styles and the equipment needed for them, especially if you wield more than one weapon ikon.",
            "You can Interact to stow items or draw weapons (or a shield) as free actions until you do any other action.",
            [ExemplarTraits.Exemplar, Trait.Flourish],
            null
        )
        .WithActionCost(1)
        .WithPermanentQEffectAndSameRulesText(q =>
        {
            q.ProvideMainAction = q => new ActionPossibility(new CombatAction(
                    q.Owner,
                    IllustrationName.LightningBolt,
                    "Lightning Swap",
                    [Trait.Flourish, Trait.Basic],
                    "You can Interact to stow items or draw weapons (or a shield) as free actions until you do any other action.",
                    Target.Self()
                )
                .WithActionCost(1)
                .WithEffectOnChosenTargets(async (self, targets) =>
                {
                    self.AddQEffect(new QEffect()
                    {
                        Id = ExemplarQEffects.LightningSwap,
                        AfterYouTakeAction = async (q, action) =>
                        {
                            if (action.ActionId != ActionId.DrawItem && action.ActionId != ActionId.ReplaceItemInHand && action.Name != "Lightning Swap" ||
                                ((!action.Item?.HasTrait(Trait.Weapon) ?? false) && (!action.Item?.HasTrait(Trait.Shield) ?? false)))
                            {
                                q.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        }
                    });
                })
            );
        });
    }
}
