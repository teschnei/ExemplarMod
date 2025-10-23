using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Roller;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level2;

public class RedGoldMortality
{
    [FeatGenerator(2)]
    public static IEnumerable<Feat> GetFeat()
    {
        var flavorText = "By channeling your divinity into a creature along with a strike, you can disrupt their ability to recover.";
        var rulesText = "{b}Usage{/b} imbued into a weapon ikon\n\n The imbued ikon gains the following ability.\n\n" +
            "{b}Immanence{/b} When you successfully damage an enemy with the ikon, a marking appears around the wound, painted in the red of mortal blood and the gold of divine ichor.\n\n" +
            "When the target would regain Hit Points, such as from a healing effect or an ability like fast healing or regeneration, it must attempt a Will save against your class DC to determine " +
            "the effects, and then the marking fades.\n" +
            "{b}Success{/b} The creature regains the full number of Hit Points that would be healed.\n" +
            "{b}Failure{/b} The creature regains only half the number of Hit Points as the contradictory energies swirl within it.\n" +
            "{b}Critical Failure{/b} The creature doesn't regain any Hit Points.";
        yield return new TrueFeat(
            ExemplarFeats.RedGoldMortality,
            2,
            flavorText,
            rulesText,
            [ExemplarTraits.Exemplar, ExemplarTraits.IkonExpansion, Trait.Negative],
            Ikon.AddExpansionFeat("RedGoldMortality", flavorText, rulesText, [], ikon => ikon.IkonFeat.HasTrait(ExemplarTraits.IkonWeapon), (ikon, feat) =>
            {
                feat.WithPermanentQEffect(null, q =>
                {
                    q.AfterYouDealDamage = async (self, action, target) =>
                    {
                        if (ikon.IsIkonItem(action.Item))
                        {
                            target.RemoveAllQEffects(q => q.Id == ExemplarQEffects.RedGoldMortality);
                            target.AddQEffect(new QEffect("Red-Gold Mortality", "The next time you recover Hit Points, your divine wound may absorb some of recovery.",
                                        ExpirationCondition.Never, action.Owner, IllustrationName.BloodVendetta)
                            {
                                Id = ExemplarQEffects.RedGoldMortality,
                                AdjustDiceFormulaForSelfHealing = (q, _, diceFormula) =>
                                {
                                    q.ExpiresAt = ExpirationCondition.Immediately;
                                    var checkResult = CommonSpellEffects.RollSavingThrow(q.Owner, CombatAction.CreateSimple(action.Owner, "Red-Gold Mortality"), Defense.Will, q.Owner.ClassDC());
                                    switch (checkResult)
                                    {
                                        case CheckResult.Failure:
                                            return new HalfDiceFormula(diceFormula, "Red-Gold Mortality");
                                        case CheckResult.CriticalFailure:
                                            return DiceFormula.FromText("0", "Red-Gold Mortality");
                                        default:
                                            return diceFormula;
                                    }
                                }
                            });
                        }
                    };
                });
            },
            item => null
            ).ToList()
        );
    }
}
