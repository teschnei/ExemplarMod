using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets.Dominion;

public class WhoseCryIsThunder
{
    [FeatGenerator(7)]
    public static IEnumerable<Feat> GetFeat()
    {
        List<FeatName> sparkOptions = [ExemplarFeats.EnergizedSparkElectricity, ExemplarFeats.EnergizedSparkSonic];
        yield return new Epithet(
            ExemplarFeats.WhoseCryIsThunder,
                "The sky overhead is yours to command as lightning strikes your soul.",
                "You gain the Energized Spark feat for your choice of electricity or sonic. " +
                "When you critically succeed on a Strike, a thunderclap booms! The target must make a Fortitude saving throw against your class DC. " +
                "On a failure, they are knocked prone and deafened. This is a sonic effect.\n\n" +
                "When you Spark Transcendence, you become electrically charged until the start of your next turn. " +
                "Enemies that damage you with an unarmed attack or non-reach melee weapon while you're charged take 1d6 electricity damage as lightning courses back to them.",
            [ExemplarTraits.DominionEpithet],
            sparkOptions.Select(spark => new Feat(spark, "", "", [], null)
                .WithEquivalent(sheet => sheet.HasFeat(spark))
                .WithOnSheet(sheet =>
                {
                    sheet.GrantFeat(ExemplarFeats.EnergizedSpark, spark);
                })).ToList()
        )
        .WithTranscendPossibility("When you Spark Transcendence, you become electrically charged until the start of your next turn. " +
                "Enemies that damage you with an unarmed attack or non-reach melee weapon while you're charged take 1d6 electricity damage as lightning courses back to them.", (exemplar, action) =>
                new ActionPossibility(new CombatAction(exemplar, IllustrationName.ElectricArc, "Whose Cry is Thunder", [],
                        "You become electrically charged until the start of your next turn. " +
                        "Enemies that damage you with an unarmed attack or non-reach melee weapon while you're charged take 1d6 electricity damage as lightning courses back to them.",
                        Target.Self())
                    .WithActionCost(0)
                    .WithEffectOnChosenTargets(async (action, self, targets) =>
                    {
                        self.AddQEffect(new QEffect(
                            "Electrified",
                            "You are electrically charged until the start of your next turn. Enemies that damage you with an unarmed or non-reach melee attack take 1d6 electricity damage.",
                            ExpirationCondition.ExpiresAtStartOfYourTurn,
                            self,
                            IllustrationName.ElectricArc)
                        {
                            AfterYouTakeDamage = async (effect, amount, kind, action, critical) =>
                            {
                                // Apply electricity damage to the attacker
                                var attacker = action?.Owner;
                                if (attacker != null && attacker.DistanceTo(effect.Owner) <= 1)
                                {
                                    var damageFormula = DiceFormula.FromText("1d6", "Electricity Damage");
                                    await CommonSpellEffects.DealDirectDamage(CombatAction.CreateSimple(effect.Owner, "Whose Cry is Thunder"), damageFormula, attacker, CheckResult.Failure, DamageKind.Electricity);
                                }
                            }
                        });
                    })
                )
        )
        .WithPermanentQEffect(null, q =>
        {
            q.AfterYouTakeAction = async (selfQf, action) =>
            {
                if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess && action.ChosenTargets.ChosenCreature != null)
                {
                    if (CommonSpellEffects.RollSavingThrow(action.ChosenTargets.ChosenCreature, CombatAction.CreateSimple(selfQf.Owner, "Whose Cry is Thunder", [Trait.Sonic]), Defense.Fortitude, selfQf.Owner.ClassDC()) <= CheckResult.Failure)
                    {
                        action.ChosenTargets.ChosenCreature.AddQEffect(QEffect.Prone());
                        action.ChosenTargets.ChosenCreature.AddQEffect(QEffect.Deafened());
                    }
                }
            };
        });
    }
}
