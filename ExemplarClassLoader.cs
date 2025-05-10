// -------- ExemplarClassLoader.cs --------
using Dawnsbury.Modding;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;

namespace Dawnsbury.Mods.Classes.Exemplar;

public static class ExemplarClassLoader
{
    public static Trait? IkonTrait;

    [DawnsburyDaysModMainMethod]
    public static void LoadMod()
    {
        ModManager.AssertV3();

        // Register custom traits
        IkonTrait = ModManager.RegisterTrait("Ikon");

        // Register feats
        ModManager.AddFeat(ExemplarBaseClass.CreateExemplarClassFeat());
    }
}