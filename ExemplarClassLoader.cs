using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using HarmonyLib;

namespace Dawnsbury.Mods.Classes.Exemplar;

public static class ExemplarClassLoader
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FeatGeneratorAttribute : Attribute
    {
        public int Level
        {
            get; set;
        }
        public FeatGeneratorAttribute(int level)
        {
            Level = level;
        }
    }

    static IEnumerable<MethodInfo> GetFeatGenerators()
    {
        var a = typeof(ExemplarClassLoader).Assembly.GetTypes().Where(x => x.IsClass).SelectMany(x => x.GetMethods())
        .Where(x => x.GetCustomAttributes(typeof(FeatGeneratorAttribute), false).FirstOrDefault() != null)
        .OrderBy(x => (x.GetCustomAttributes(typeof(FeatGeneratorAttribute), false).First() as FeatGeneratorAttribute)!.Level);
        return a;
    }

    [DawnsburyDaysModMainMethod]
    public static void LoadMod()
    {
        ModManager.AssertV3();

        foreach (var featGenerator in GetFeatGenerators())
        {
            foreach (var feat in (featGenerator.Invoke(null, null) as IEnumerable<Feat>)!)
            {
                ModManager.AddFeat(feat);
            }
        }

        //Implementation of "Twin" trait
        ModManager.RegisterActionOnEachCreature(creature =>
        {
            creature.AddQEffect(new QEffect()
            {
                BonusToDamage = (q, action, target) =>
                {
                    if (action.Item?.HasTrait(ExemplarTraits.Twin) ?? false)
                    {
                        if (q.Owner.Actions.ActionHistoryThisTurn.Where(hist => hist.Item != action.Item && hist.Item?.BaseItemName == action.Item?.BaseItemName).Count() > 0)
                        {
                            return new Bonus(action.Item.WeaponProperties?.DamageDieCount ?? 0, BonusType.Circumstance, "Twin", true);
                        }
                    }
                    return null;
                }
            });
        });

        //Implementation of "Lightning Swap" feat
        ModManager.RegisterActionOnEachActionPossibility(action =>
        {
            if (action.Owner.HasEffect(ExemplarQEffects.LightningSwap) &&
                (action.ActionId == ActionId.DrawItem || action.ActionId == ActionId.ReplaceItemInHand) &&
                ((action.Item?.HasTrait(Trait.Weapon) ?? false) || (action.Item?.HasTrait(Trait.Shield) ?? false)))
            {
                action.ActionCost = 0;
            }
        });

        var harmony = new Harmony("junabell.dawnsburydays.exemplar");
        harmony.PatchAll();
    }
}
