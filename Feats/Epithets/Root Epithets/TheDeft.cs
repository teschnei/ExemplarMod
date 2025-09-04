using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Root;

public class TheDeft
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        ModManager.RegisterActionOnEachActionPossibility(action =>
        {
            if (action.Owner.HasEffect(ExemplarQEffects.TheDeft) &&
                (action.ActionId == ActionId.DrawItem || action.ActionId == ActionId.ReplaceItemInHand || action.ActionId == ActionId.Reload) &&
                (action.Item != null && action.Item.Runes.Any(rune => rune.RuneProperties?.RuneKind == IkonRuneKind.Ikon) &&
                (action.Item.HasTrait(Trait.Reload1) || action.Item.HasTrait(Trait.Reload2) || (!action.Item.HasTrait(Trait.Ranged) && (action.Item.WeaponProperties?.Throwable ?? false)))))
            {
                action.ActionCost = 0;
            }
        });
        yield return new Feat(
            ExemplarFeats.TheDeft,
            "Speed, subtlety, and precision. Your feet rush as fast as a gale, but your fingers touch as lightly as a breeze.",
            "You are trained in Thievery. After you Spark Transcendence, you can Interact as a free action to reload or draw a weapon ikon. " +
            "The weapon ikon must be a ranged weapon with the reload trait or a one-handed melee weapon with the thrown trait.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Thievery);
        })
        .WithPermanentQEffect("After you Spark Transcendence, you can Interact as a free action to reload or draw a weapon ikon. " +
            "The weapon ikon must be a ranged weapon with the reload trait or a one-handed melee weapon with the thrown trait.", q =>
        {
            q.AfterYouTakeAction = async (q, action) =>
            {
                if (action.HasTrait(ExemplarTraits.Transcendence))
                {
                    q.Owner.AddQEffect(new QEffect()
                    {
                        Id = ExemplarQEffects.TheDeft,
                        AfterYouTakeAction = async (q, action) =>
                        {
                            q.ExpiresAt = ExpirationCondition.Immediately;
                        },
                    });
                }
            };
        });
    }
}
