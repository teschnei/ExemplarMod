using System.Collections.Generic;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes.Multiclass;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public static class ExemplarArchetype
{
    [FeatGenerator(1)]
    public static IEnumerable<Feat> CreateFeats()
    {
        yield return ArchetypeFeats.CreateMulticlassDedication(ExemplarTraits.Exemplar,
                "A spark of inherent divine power has lit itself within you.",
                "You become trained in martial weapons. You gain one ikon, " +
                "the ability to use the ikon’s immanence and transcendence actions and effects, and the Shift Immanence action. " +
                "Because you have only a single ikon, when you Spark Transcendence, " +
                "your divine spark recedes back to the depths of your soul and must be recalled with Shift Immanence to re-empower your ikon. " +
                "You become trained in exemplar class DC.")
            .WithDemandsAbilityOrAbility14(Ability.Strength, Ability.Dexterity)
            .WithOnSheet(sheet =>
            {
                sheet.SetProficiency(Trait.Martial, Proficiency.Trained);
                sheet.AddSelectionOptionRightNow(new SingleFeatSelectionOption("Ikon", "Ikon", -1, ft => ft.HasTrait(ExemplarTraits.Ikon)));

                sheet.AtEndOfRecalculation += (sheet) =>
                {
                    ExemplarBaseClass.EnsureCorrectRunes(sheet);
                };
            })
            .WithPermanentQEffect(null, q => ExemplarBaseClass.ShiftImmanenceQEffect(q));
        yield return MulticlassArchetypeFeats.CreateResiliencyFeat(ExemplarTraits.Exemplar, 8);
        foreach (var f in ArchetypeFeats.CreateBasicAndAdvancedMulticlassFeatGrantingArchetypeFeats(ExemplarTraits.Exemplar, "Glory"))
        {
            yield return f;
        }
    }
}
