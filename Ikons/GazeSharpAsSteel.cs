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

public class GazeSharpAsSteel
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new Ikon(new Feat(
            ExemplarFeats.GazeSharpAsSteel,
            "Your eyes glint with an almost-tangible sharpness, letting you spot the tiniest swallow on the horizon or the swiftest arrow in flight.",
            "{b}Usage{/b} imbued in the eyes\n\n" +
            "{b}Immanence{/b} Your vision sharpens and allows you to sense an enemy's attack almost as soon as it begins, granting you a +1 status bonus to Perception checks and a +2 status bonus to your AC against ranged attacks.\n\n" +
            $"{{b}}Transcendence — A Moment Unending {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (concentrate, prediction, transcendence)\n" +
            "You take in every movement around you, affording you unparalleled accuracy. Your next successful Strike against an enemy before the end of your next turn deals an additional 1d6 precision damage (2d6 at 10th level, 3d6 at 18th).",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonBody],
            null
        ).WithIllustration(IllustrationName.Blinded), q =>
        {
            q.BonusToDefenses = (q, action, defense) => action?.HasTrait(Trait.Ranged) ?? false && defense == Defense.AC ? new Bonus(2, BonusType.Status, "Gaze Sharp as Steel") : null;
        }, q =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                IllustrationName.Blinded,
                "A Moment Unending",
                [Trait.Concentrate, Trait.Prediction, ExemplarTraits.Transcendence],
                "You take in every movement around you, affording you unparalleled accuracy. Your next successful Strike against an enemy before the end of your next turn deals an additional 1d6 precision damage (2d6 at 10th level, 3d6 at 18th).",
                Target.Self()
            ).WithActionCost(1)
            .WithEffectOnSelf(async (action, self) =>
            {
                string extraDamage = $"{(self.Level >= 18 ? 3 : self.Level >= 10 ? 2 : 1)}d6";
                self.AddQEffect(new QEffect("A Moment Unending", $"Your next successful Strike against an enemy deals an additional {extraDamage} precision damage.", ExpirationCondition.ExpiresAtEndOfYourTurn, self, IllustrationName.Blinded)
                {
                    YouDealDamageWithStrike = (q, action, diceFormula, target) =>
                    {
                        if (action.HasTrait(Trait.Strike))
                        {
                            q.ExpiresAt = ExpirationCondition.Immediately;
                            if (!target.IsImmuneTo(Trait.PrecisionDamage))
                            {
                                return diceFormula.Add(DiceFormula.FromText(extraDamage, "A Moment Unending"));
                            }
                        }
                        return diceFormula;
                    },
                    CannotExpireThisTurn = true
                });
            }));
        }).IkonFeat;
    }
}
