using System;
using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core.CharacterBuilder;
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

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class Immanence
{
    public string ShortDesc { get; }
    public Action<Ikon, QEffect> Modifier { get; }

    Immanence(string shortDesc, Action<Ikon, QEffect> modifier)
    {
        ShortDesc = shortDesc;
        Modifier = modifier;
    }
}

public class Transcendence
{
    public string ShortDesc { get; }
    public Func<Ikon, QEffect, Possibility?> Creator { get; }

    Transcendence(string shortDesc, Func<Ikon, QEffect, Possibility?> creator)
    {
        ShortDesc = shortDesc;
        Creator = creator;
    }
}

public class Ikon
{
    public static readonly string IkonKey = "IkonEmpowered";
    public static readonly FeatGroup WeaponIkonGroup = new FeatGroup("Weapon Ikon", 1);
    public static readonly FeatGroup WornIkonGroup = new FeatGroup("Worn Ikon", 2);
    public static readonly FeatGroup BodyIkonGroup = new FeatGroup("Body Ikon", 3);
    public static Dictionary<FeatName, Ikon> IkonLUT = new();
    public static Dictionary<FeatName, Dictionary<FeatName, Func<Item, string?>>> IkonExpansionReqLUT = new();

    public Feat IkonFeat { get; }
    public Func<Item, string?>? ValidItem { get; private set; }
    public ItemName? FreeWornItem { get; private set; }
    public QEffectId EmpoweredQEffectId { get; private set; }
    public bool Equippable(CharacterSheet sheet) => ValidItem != null && (WeaponFeat != null ? sheet.Calculated.HasFeat((FeatName)WeaponFeat) : true);
    public Action<Item> ModifyItem { get; private set; }
    public FeatName? WeaponFeat { get; private set; }
    public FeatName? UnarmedFeat { get; private set; }

    public string ModString => $"ikon_{IkonFeat.FeatName.ToStringOrTechnical()}";
    public ItemModification IkonModification => new ItemModification(ItemModificationKind.CustomPermanent)
    {
        Tag = ModString,
        ModifyItem = this.ModifyItem
    };

    private Action<Ikon, QEffect> Immanence { get; }
    private Func<Ikon, QEffect, Possibility?> Transcendence { get; }

    public Ikon(Feat ikon, Action<Ikon, QEffect> immanence, Func<Ikon, QEffect, Possibility?> transcendence)
    {
        IkonFeat = ikon;
        Immanence = immanence;
        Transcendence = transcendence;
        ModifyItem = item => item.Traits.AddRange(Trait.Divine, ExemplarTraits.Ikon);
        EmpoweredQEffectId = ModManager.RegisterEnumMember<QEffectId>($"Empowered{ikon.FeatName.ToString()}");

        IkonFeat.FeatGroup = IkonFeat.Traits switch
        {
            var a when a.Contains(ExemplarTraits.IkonWeapon) => WeaponIkonGroup,
            var a when a.Contains(ExemplarTraits.IkonWorn) => WornIkonGroup,
            var a when a.Contains(ExemplarTraits.IkonBody) => BodyIkonGroup,
            _ => null
        };

        IkonLUT.Add(ikon.FeatName, this);
    }

    public Ikon WithValidItem(Func<Item, string?> validItem)
    {
        ValidItem = validItem;
        return this;
    }

    public Ikon WithFreeWornItem(ItemName item)
    {
        FreeWornItem = item;
        return this;
    }

    public Ikon WithModifyItem(Action<Item> action)
    {
        ModifyItem = item =>
        {
            ModifyItem(item);
            action(item);
        };
        return this;
    }

    public Ikon WithWeaponUnarmedSubFeats(FeatName weaponFeatName, FeatName unarmedFeatName)
    {
        var weaponFeat = new Feat(weaponFeatName, "", "This ikon can be assigned to an item in the Inventory.", [], null);
        var unarmedFeat = new Feat(unarmedFeatName, "", "This ikon will be assigned to matching unarmed weapons automatically.", [], null);

        WeaponFeat = weaponFeatName;
        UnarmedFeat = unarmedFeatName;

        IkonFeat.Subfeats = [weaponFeat, unarmedFeat];

        return this;
    }

    public QEffect GetEmpoweredQEffect(Creature exemplar)
    {
        var q = new QEffect($"Empowered {IkonFeat.Name}", $"Your {IkonFeat.Name} is housing your divine spark, granting you its Immanence and Transcendence abilities.",
                ExpirationCondition.Never, exemplar, IkonFeat.Illustration)
        {
            Id = EmpoweredQEffectId,
            Key = IkonKey,
            ProvideMainAction = q => CreateTranscendence(Transcendence, q, this),
            WhenExpires = q => q.Owner.RemoveAllQEffects(q => q.Id == ExemplarQEffects.IkonExpansion)
        };
        q.Owner = exemplar;
        Immanence(this, q);
        return q;
    }

    public CombatAction ShiftImmanence(Creature exemplar)
    {
        return new CombatAction(exemplar, IkonFeat.Illustration!, $"Empower {IkonFeat.Name}",
            [ExemplarTraits.Exemplar, Trait.Divine, Trait.Basic, ExemplarTraits.ShiftImmanence],
            $"{IkonFeat.RulesText}", Target.Self())
            .WithActionCost(1)
            .WithEffectOnEachTarget(async (spell, caster, target, result) =>
            {
                caster.RemoveAllQEffects(q => q.Key == IkonKey);
                caster.AddQEffect(GetEmpoweredQEffect(exemplar));
                caster.FindQEffect(ExemplarQEffects.ShiftImmanence)!.Tag = null;
            }
        );
    }

    public bool IsIkonItem(Item? item) => item?.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.CustomPermanent && ((mod.Tag as string) == ModString)) ?? false;
    public static Item? GetHeldIkon(Creature exemplar, Ikon ikon) => exemplar.HeldItems.Where(item => ikon.IsIkonItem(item)).FirstOrDefault() ?? (ikon.IsIkonItem(exemplar.UnarmedStrike) ? exemplar.UnarmedStrike : null);
    public static Item? GetWornIkon(Creature exemplar, Ikon ikon) => exemplar.CarriedItems.Where(item => item.HasTrait(Trait.Worn) && ikon.IsIkonItem(item)).FirstOrDefault();

    public static DamageKind GetBestDamageKindForSpark(Creature exemplar, Creature target)
    {
        List<Trait> damageTraits = new List<Trait>();
        List<DamageKind> damageKinds = [ExemplarDamageKinds.Spirit];
        var energizedSpark = exemplar.FindQEffect(ExemplarQEffects.EnergizedSpark);
        if (energizedSpark != null && energizedSpark.Tag is List<(Trait, DamageKind)> energizedSparkList)
        {
            damageTraits.AddRange(energizedSparkList.Select(spark => spark.Item1));
            damageKinds.AddRange(energizedSparkList.Select(spark => spark.Item2));
        }
        var sanctifiedSoul = exemplar.FindQEffect(ExemplarQEffects.SanctifiedSoul);
        if (sanctifiedSoul != null && sanctifiedSoul.Tag is ValueTuple<Trait, DamageKind> sanctification && Alignments.IsCreatureVulnerableToAlignmentDamage(target, sanctification.Item2))
        {
            damageTraits.Add(sanctification.Item1);
            damageKinds.Add(sanctification.Item2);
        }
        //TODO: also check special resistances with the trait list
        return target.WeaknessAndResistance.WhatDamageKindIsBestAgainstMe(damageKinds);
    }

    public static IEnumerable<Feat> AddExpansionFeat(string technicalName, string flavorText, string rulesText, List<Trait> traits, Func<Ikon, bool> predicate, Action<Ikon, Feat> modifyFeat, Func<Item, string?> requirement)
    {
        return IkonLUT.Values.Where(predicate).Select(ikon =>
        {
            var feat = new Feat(ModManager.RegisterFeatName(ikon.IkonFeat.FeatName.ToStringOrTechnical() + technicalName, ikon.IkonFeat.Name), flavorText, rulesText, [.. traits, ExemplarTraits.IkonExpansion], null)
                .WithPrerequisite(sheet => sheet.HasFeat(ikon.IkonFeat), $"You must have the {ikon.IkonFeat.Name} ikon.")
                .WithIllustration(ikon.IkonFeat.Illustration);
            modifyFeat(ikon, feat);
            if (!IkonExpansionReqLUT.ContainsKey(ikon.IkonFeat.FeatName))
            {
                IkonExpansionReqLUT.Add(ikon.IkonFeat.FeatName, new());
            }
            IkonExpansionReqLUT[ikon.IkonFeat.FeatName].Add(feat.FeatName, requirement);
            return feat;
        });
    }

    public static Possibility? CreateTranscendence(Func<Ikon, QEffect, Possibility?> transcendence, QEffect q, Ikon ikon)
    {
        var poss = transcendence.Invoke(ikon, q);
        if (poss is ActionPossibility action)
        {
            action.CombatAction.WithEffectOnChosenTargets(async (self, targets) =>
            {
                q.ExpiresAt = ExpirationCondition.Immediately;
                q.Owner.FindQEffect(ExemplarQEffects.ShiftImmanence)!.Tag = ikon.IkonFeat;
            });
        }
        else if (poss is SubmenuPossibility submenu)
        {
            foreach (var section in submenu.Subsections)
            {
                foreach (var subposs in section.Possibilities)
                {
                    if (subposs is ActionPossibility subpossAction)
                    {
                        subpossAction.CombatAction.WithEffectOnChosenTargets(async (self, targets) =>
                        {
                            q.ExpiresAt = ExpirationCondition.Immediately;
                            q.Owner.FindQEffect(ExemplarQEffects.ShiftImmanence)!.Tag = ikon.IkonFeat;
                        });
                    }
                }
            }
        }
        return poss?.WithPossibilityGroup("Spark Transcendence");
    }

    public static string GetImmanenceText(string rulesText)
    {
        string imm = "{b}Immanence{/b} ";
        var startIndex = rulesText.IndexOf(imm) + imm.Count();
        if (startIndex >= imm.Count())
        {
            var endIndex = rulesText.IndexOf("\n\n", startIndex);
            return endIndex < 0 ? rulesText.Substring(startIndex) : rulesText.Substring(startIndex, endIndex - startIndex);
        }
        return "";
    }

    public static string GetTranscendenceText(string rulesText)
    {
        string imm = "Transcendence — ";
        var startIndex = rulesText.IndexOf(imm) + imm.Count();
        if (startIndex >= imm.Count())
        {
            var endIndex = rulesText.IndexOf("\n\n", startIndex);
            return "{b}" + (endIndex < 0 ? rulesText.Substring(startIndex) : rulesText.Substring(startIndex, endIndex - startIndex));
        }
        return "";
    }
}

public class IkonWieldedTargetingRequirement : CreatureTargetingRequirement
{
    public Ikon Ikon { get; }
    public string IkonName { get; }

    public IkonWieldedTargetingRequirement(Ikon ikon, string ikonName)
    {
        Ikon = ikon;
        IkonName = ikonName;
    }

    public override Usability Satisfied(Creature source, Creature target)
    {
        if (Ikon.GetHeldIkon(source, Ikon) == null)
        {
            return Usability.NotUsable($"You must be wielding the {{i}}{IkonName}{{/i}}.");
        }
        return Usability.Usable;
    }
}
