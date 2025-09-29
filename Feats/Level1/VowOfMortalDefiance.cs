using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Feats.Level1;

public class VowOfMortalDefiance
{
    [FeatGenerator(1)]
    public static IEnumerable<Feat> GetFeat()
    {
        yield return new TrueFeat(
            ExemplarFeats.VowOfMortalDefiance,
            1,
            "Having seen the wreckage left by gods and their servitors as they play in their great war of good and evil, you've come to the only reasonable conclusion: they all must be cut from their silken thrones.",
            "You swear a vow to defeat one creature within 60 feet that has the Good or Evil trait. The first time each round that you deal damage to that creature, you deal an additional 1d6 spirit damage. " +
            "You can't use Vow of Mortal Defiance again until you or the target is defeated, flees, or the encounter ends.",
            [Trait.Auditory, Trait.Concentrate, ExemplarTraits.Exemplar, Trait.Linguistic, Trait.Mental],
            null
        )
        .WithActionCost(1)
        .WithEquivalent(sheet => sheet.HasFeat(ExemplarFeats.SanctifiedSoul))
        .WithPermanentQEffect(null, qf =>
        {
            qf.ProvideContextualAction = q =>
                q.Owner.Battle.AllCreatures.Any(creature => creature.EnemyOf(q.Owner) && (creature.HasTrait(Trait.Good) || creature.HasTrait(Trait.Evil))) &&
                !q.Owner.Battle.AllCreatures.Any(creature => creature.HasEffect(ExemplarQEffects.VowOfMortalDefiance)) ?
                new ActionPossibility(new CombatAction(q.Owner, IllustrationName.VisionOfWeakness, "Vow of Mortal Defiance",
                        [Trait.Auditory, Trait.Concentrate, Trait.Linguistic, Trait.Mental],
                        "",
                        new CreatureTarget(RangeKind.Ranged, [new EnemyCreatureTargetingRequirement(), new LegacyCreatureTargetingRequirement((source, target) =>
                        {
                            if (!target.HasTrait(Trait.Good) && !target.HasTrait(Trait.Evil))
                            {
                                return Usability.NotUsableOnThisCreature("not good/evil");
                            }
                            return Usability.Usable;
                        })], (_, _, _) => float.MinValue))
                    .WithActionCost(1)
                    .WithEffectOnEachTarget(async (action, self, target, _) =>
                    {
                        target.AddQEffect(new QEffect("Vow of Mortal Defiance", $"{self.Name} has denounced you and will deal bonus damage to you once per round.", ExpirationCondition.Never, self, action.Illustration)
                        {
                            Id = ExemplarQEffects.VowOfMortalDefiance
                        }.AddGrantingOfTechnical(cr => cr == self, q =>
                        {
                            q.YouDealDamageEvent = async (q, damageEvent) =>
                            {
                                if (!damageEvent.Source.HasEffect(ExemplarQEffects.VowOfMortalDefianceUsed))
                                {
                                    var damageKind = Ikon.GetBestDamageKindForSpark(damageEvent.Source, damageEvent.TargetCreature);
                                    var diceFormula = DiceFormula.FromText("1d6", "Vow of Mortal Defiance");
                                    var existing = damageEvent.KindedDamages.Find(kd => kd.DamageKind == damageKind);
                                    if (existing != null)
                                    {
                                        existing.DiceFormula = existing.DiceFormula?.Add(diceFormula) ?? diceFormula;
                                    }
                                    else
                                    {
                                        damageEvent.KindedDamages.Add(new KindedDamage(diceFormula, damageKind));
                                    }
                                    damageEvent.Source.AddQEffect(new QEffect(ExpirationCondition.ExpiresAtStartOfYourTurn)
                                    {
                                        Id = ExemplarQEffects.VowOfMortalDefianceUsed
                                    });
                                }
                            };
                        }));
                    })
                )
                :
                null;
        });
    }
}
