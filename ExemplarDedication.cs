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
                "You become trained in martial weapons.\n\nYou gain one ikon, " +
                "the ability to use the ikon’s immanence and transcendence actions and effects, and the Shift Immanence action.\n" +
                "Because you have only a single ikon, when you Spark Transcendence, " +
                "your divine spark recedes back to the depths of your soul and must be recalled with Shift Immanence to re-empower your ikon.\n\n" +
                "You become trained in exemplar class DC.\n\n" +
                "You can place your ikons into items in the Inventory screens, with the 'Manage Ikons' button that appears on mousing over an item.  For unarmed ikons, they may be assigned via Handwraps of Mighty Blows, or simply left unassigned to automatically apply to unarmed Strikes.")
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
