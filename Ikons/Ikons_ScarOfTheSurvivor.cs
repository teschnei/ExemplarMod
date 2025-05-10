using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_ScarOfTheSurvivor
    {
        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var scar = new TrueFeat(
                ExemplarFeatNames.IkonScarOfTheSurvivor,
                1,
                "Scar Of The Survivor",
                "A scar on your body commemorates a time someone tried to end your story and failed—a testament to your resilience and fortitude.\n\n" +
                "{b}Immanence{/b} Divine energy spreads outward from your scar, reinforcing your flesh. You gain the benefits of the Diehard feat and a +1 status bonus to Fortitude saving throws.\n\n" +
                "{b}Transcendence — No Scar but This (one-action){/b} Concentrate, Healing, Transcendence, Vitality\n" +
                "Your wounds knit shut with hardly a scratch. You regain 1d8 Hit Points. At 3rd level and every 2 levels thereafter, the healing increases by 1d8.",
                new[] { ModTraits.Ikon },
                null
            )
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: +1 to Fortitude saves, but only when not yet empowered
                qf.BonusToDefenses = (eff, action, defense) =>
                {
                    // Don't apply once they've used Transcendence
                    if (eff.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredScarOfTheSurvivor))
                        return null;

                    // Only on Fortitude saves
                    if (defense == Defense.Fortitude)
                        return new Bonus(1, BonusType.Status, "Scar Of The Survivor");

                    return null;
                };


                // TODO: apply full Diehard benefits (stabilize at 0 HP, etc.)

                // Transcendence — No Scar but This
                qf.ProvideMainAction = qf =>
                {
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredScarOfTheSurvivor))
                        return null;

                    var action = new CombatAction(
                        qf.Owner,
                        IllustrationName.Heal,
                        "No Scar but This",
                        new[] { Trait.Concentrate, Trait.Healing, ModTraits.Transcendence, ModTraits.Ikon },
                        "You regain 1d8 Hit Points (increases by 1d8 at 3rd level and every 2 levels thereafter).",
                        Target.Self()
                    ).WithActionCost(1);

                    action.WithEffectOnEachTarget(async (act, caster, _, _) =>
                    {
                        int diceCount = 1 + ((caster.Level - 1) / 2);
                        var formula = DiceFormula.FromText($"{diceCount}d8", "No Scar but This healing");
                        await caster.HealAsync(formula.ToString(), act);

                        // Cleanup: remove empowerment & grant First Shift Free + exhaustion
                        caster.RemoveAllQEffects(q => ExemplarIkonQEffectIds.EmpoweredIkonIds.Contains(q.Id));
                        caster.AddQEffect(new QEffect("First Shift Free", "You can Shift Immanence without spending an action.")
                        {
                            Id = ExemplarIkonQEffectIds.FirstShiftFree
                        });
                        caster.AddQEffect(new QEffect("Spark Exhaustion", "You cannot use another Transcendence this turn.",
                            ExpirationCondition.ExpiresAtStartOfYourTurn, caster, IllustrationName.Chaos)
                        {
                            Id = ExemplarIkonQEffectIds.TranscendenceTracker
                        });
                    });

                    return new ActionPossibility(action);
                };
            });

            ModManager.AddFeat(scar);
        }
    }
}
