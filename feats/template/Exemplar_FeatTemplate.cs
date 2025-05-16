// using System.Linq;
// using Dawnsbury.Core;
// using Dawnsbury.Core.CharacterBuilder.Feats;
// using Dawnsbury.Core.CharacterBuilder.Selections.Options;
// using Dawnsbury.Core.CombatActions;
// using Dawnsbury.Core.Mechanics.Enumerations;
// using Dawnsbury.Core.Mechanics.Targeting;
// using Dawnsbury.Core.Possibilities;
// using Dawnsbury.Modding;
// using Dawnsbury.Mods.Classes.Exemplar;

// namespace Dawnsbury.Mods.Exemplar
// {
//     public class Exemplar_FeatTemplate
//     {
//         [DawnsburyDaysModMainMethod]
//         public static void Load()
//         {
//             var feat = new TrueFeat(
//                 FeatName.CustomFeat, // Replace with actual feat name
//                 1, // Replace with actual level
//                 "Template Feat", // Replace with actual feat display name
//                 "{b}Immanence{/b} Description of the feat goes here.", // Replace with actual description
//                 new[] { ModTraits.Ikon }, // Replace with actual traits
//                 null
//             )
//             .WithOnSheet(sheet =>
//             {
//                 // Add the feat to the character sheet
//                 sheet.AddSelectionOption(
//                     new SingleFeatSelectionOption(
//                         key: "TemplateFeat", // Replace with actual key
//                         name: "Template Feat", // Replace with actual name
//                         level: 1, // Replace with actual level
//                         eligible: ft => true // Define eligibility criteria
//                     )
//                 );
//             })
//             .WithPermanentQEffect(null, qf =>
//             {
//                 qf.ProvideMainAction = qf =>
//                 {
//                     var action = new CombatAction(
//                         qf.Owner,
//                         IllustrationName.Acorn, // Replace with actual illustration
//                         "Template Action", // Replace with actual action name
//                         new[] { Trait.Aasimar    }, // Replace with actual traits
//                         "Description of the action goes here.", // Replace with actual action description
//                         Target.Self()
//                     ).WithActionCost(1);

//                     action.WithEffectOnSelf(async (act, self) =>
//                     {
//                         // Define the effects of the action here
//                     });

//                     return new ActionPossibility(action);
//                 };
//             });

//             ModManager.AddFeat(feat);
//         }
//     }
// }