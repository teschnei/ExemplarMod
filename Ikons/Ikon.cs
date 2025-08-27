using System;
using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class Ikon
{
    public static readonly string IkonKey = "IkonEmpowered";
    public static Dictionary<FeatName, Ikon> IkonLUT = new();
    public static Dictionary<FeatName, ItemName> ExtraRunes = new();

    public Feat IkonFeat { get; }
    public ItemName? Rune { get; private set; }
    public ItemName? FreeWornItem { get; private set; }
    public QEffectId EmpoweredQEffectId { get; private set; }

    private Action<QEffect> Immanence { get; }
    private Func<QEffect, Possibility?> Transcendence { get; }

    public Ikon(Feat ikon, Action<QEffect> immanence, Func<QEffect, Possibility?> transcendence)
    {
        IkonFeat = ikon;
        Immanence = immanence;
        Transcendence = transcendence;
        EmpoweredQEffectId = ModManager.RegisterEnumMember<QEffectId>($"Empowered{ikon.FeatName.ToString()}");

        IkonLUT.Add(ikon.FeatName, this);
    }

    public Ikon WithRune(ItemName rune)
    {
        Rune = rune;
        return this;
    }

    public Ikon WithFreeWornItem(ItemName item)
    {
        FreeWornItem = item;
        return this;
    }

    public QEffect GetEmpoweredQEffect(Creature exemplar)
    {
        var q = new QEffect($"Empowered {IkonFeat.Name}", $"Your {IkonFeat.Name} is housing your divine spark, granting you its Immanence and Transcendence abilities.",
                ExpirationCondition.Never, exemplar, IllustrationName.SpiritualWeapon)
        {
            Id = EmpoweredQEffectId,
            Key = IkonKey,
            ProvideMainAction = q =>
            {
                var poss = Transcendence?.Invoke(q);
                if (poss is ActionPossibility action)
                {
                    action.CombatAction.WithEffectOnChosenTargets(async (self, targets) =>
                    {
                        q.ExpiresAt = ExpirationCondition.Immediately;
                        q.Owner.FindQEffect(ExemplarQEffects.ShiftImmanence)!.Tag = IkonFeat;
                    });
                }
                return poss;
            }
        };
        Immanence(q);
        return q;
    }

    public CombatAction ShiftImmanence(Creature exemplar)
    {
        return new CombatAction(exemplar, IllustrationName.SpiritualWeapon, $"Empower {IkonFeat.Name}",
            [ExemplarTraits.Exemplar, Trait.Divine, Trait.Basic],
            $"{IkonFeat.FullTextDescription}", Target.Self())
            .WithActionCost(exemplar.QEffects.Any(q => q.Key == IkonKey) ? 1 : 0)
            .WithEffectOnEachTarget(async (spell, caster, target, result) =>
            {
                caster.AddQEffect(GetEmpoweredQEffect(exemplar));
            }
        );
    }

    public static Item? GetIkonItem(Creature exemplar, ItemName ikonRune) => exemplar.HeldItems.Where(item => item.Runes.Any(rune => rune.ItemName == ikonRune)).FirstOrDefault();
    public static Item? GetIkonItemWorn(Creature exemplar, ItemName ikonRune) => exemplar.CarriedItems.Where(item => item.HasTrait(Trait.Worn) && item.Runes.Any(rune => rune.ItemName == ikonRune)).FirstOrDefault();

    public static DamageKind GetBestDamageKindForSpark(Creature exemplar, Creature target)
    {
        List<Trait> damageTraits = new List<Trait>();
        List<DamageKind> damageKinds = [DamageKind.Force];
        var energizedSpark = exemplar.FindQEffect(ExemplarQEffects.EnergizedSpark);
        if (energizedSpark != null && energizedSpark.Tag is List<(Trait, DamageKind)> energizedSparkList)
        {
            damageTraits.AddRange(energizedSparkList.Select(spark => spark.Item1));
            damageKinds.AddRange(energizedSparkList.Select(spark => spark.Item2));
        }
        var sanctifiedSoul = exemplar.FindQEffect(ExemplarQEffects.SanctifiedSoul);
        if (sanctifiedSoul != null && sanctifiedSoul.Tag is ValueTuple<Trait, DamageKind> sanctification)
        {
            damageTraits.Add(sanctification.Item1);
            damageKinds.Add(sanctification.Item2);
        }
        //TODO: also check special resistances with the trait list
        return target.WeaknessAndResistance.WhatDamageKindIsBestAgainstMe([DamageKind.Force]);
    }
}

public class IkonWieldedTargetingRequirement : CreatureTargetingRequirement
{
    public ItemName IkonRune { get; }

    public IkonWieldedTargetingRequirement(ItemName ikonRune)
    {
        IkonRune = ikonRune;
    }

    public override Usability Satisfied(Creature source, Creature target)
    {
        if (Ikon.GetIkonItem(source, IkonRune) == null)
        {
            return Usability.NotUsable($"You must be wielding the {{i}}{IkonRune.ToString().ToLower()}{{/i}}.");
        }
        return Usability.Usable;
    }
}
