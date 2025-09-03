using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class ScarOfTheSurvivor
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Ikon(new Feat(
            ExemplarFeats.ScarOfTheSurvivor,
            "A scar on your body commemorates a time someone tried to end your story and failed—a testament to your resilience and fortitude.",
            "{b}Usage{/b} imbued in the skin\n\n" +
            "{b}Immanence{/b} Divine energy spreads outward from your scar, reinforcing your flesh. You gain the benefits of the Diehard feat and a +1 status bonus to Fortitude saving throws.\n\n" +
            $"{{b}}Transcendence — No Scar but This {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (concentrate, healing, transcendence, vitality)\n" +
            "Your wounds knit shut with hardly a scratch. You regain 1d8 Hit Points. At 3rd level and every 2 levels thereafter, the healing increases by 1d8.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonBody],
            null
        ).WithIllustration(IllustrationName.BloodVendetta), q =>
        {
            var diehard = new QEffect()
            {
                Innate = true,
                Id = QEffectId.Diehard
            };
            q.Owner.AddQEffect(diehard);
            q.BonusToDefenses = (qe, action, defense) => defense == Defense.Fortitude ? new Bonus(1, BonusType.Status, "Scar of the Survivor", true) : null;
            q.WhenExpires = q => q.Owner.RemoveAllQEffects(q => q == diehard);
        }, q =>
        {
            var healing = (q.Owner.Level + 1) / 2;
            return new ActionPossibility(new CombatAction(
                q.Owner,
                IllustrationName.Heal,
                "No Scar but This",
                [Trait.Concentrate, Trait.Healing, ExemplarTraits.Transcendence, Trait.Positive],
                $"Your wounds knit shut with hardly a scratch. You regain {healing}d8 Hit Points.",
                Target.Self()
            ).WithActionCost(1)
            .WithSavingThrow(new SavingThrow(Defense.Will, q.Owner.ClassDC()))
            .WithEffectOnEachTarget(async (action, caster, target, result) =>
            {
                await caster.HealAsync(DiceFormula.FromText($"{healing}d8", "No Scar but This"), action);
            }));
        }).IkonFeat;
    }
}
