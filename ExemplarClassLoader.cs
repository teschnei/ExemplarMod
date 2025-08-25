using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Modding;
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

        var harmony = new Harmony("junabell.dawnsburydays.exemplar");
        harmony.PatchAll();
    }
}
