using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Root;

public class TheDeft
{
    [FeatGenerator(3)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Epithet(
            ExemplarFeats.TheDeft,
            "Speed, subtlety, and precision. Your feet rush as fast as a gale, but your fingers touch as lightly as a breeze.",
            "You are trained in Thievery. After you Spark Transcendence, you can Interact as a free action to reload or draw a weapon ikon. " +
            "The weapon ikon must be a ranged weapon with the reload trait or a one-handed melee weapon with the thrown trait.",
            [ExemplarTraits.RootEpithet],
            null
        )
        .WithTranscendPossibility("After you Spark Transcendence, you can Interact as a free action to reload or draw a weapon ikon. " +
            "The weapon ikon must be a ranged weapon with the reload trait or a one-handed melee weapon with the thrown trait.", (exemplar, action) =>
        {
            var actions = Possibilities.Create(exemplar).Filter(poss =>
                (poss.CombatAction.ActionId == ActionId.DrawItem || poss.CombatAction.ActionId == ActionId.ReplaceItemInHand || poss.CombatAction.ActionId == ActionId.Reload) &&
                (poss.CombatAction.Item != null && poss.CombatAction.Item.HasTrait(ExemplarTraits.Ikon) &&
                (poss.CombatAction.Item.HasTrait(Trait.Reload1) || poss.CombatAction.Item.HasTrait(Trait.Reload2) || (!poss.CombatAction.Item.HasTrait(Trait.Ranged) && (poss.CombatAction.Item.WeaponProperties?.Throwable ?? false)))));
            return new SubmenuPossibility(IllustrationName.Feint, "The Deft")
            {
                Subsections = actions.Sections
            };
        })
        .WithOnSheet(sheet =>
        {
            sheet.TrainInThisOrSubstitute(Skill.Thievery);
        });
    }
}
