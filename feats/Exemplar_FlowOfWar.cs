// using System.Linq;
// using Dawnsbury.Core;
// using Dawnsbury.Core.CharacterBuilder.Feats;
// using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
// using Dawnsbury.Core.CharacterBuilder.Selections.Options;
// using Dawnsbury.Core.CombatActions;
// using Dawnsbury.Core.Mechanics.Enumerations;
// using Dawnsbury.Core.Mechanics.Targeting;
// using Dawnsbury.Core.Possibilities;
// using Dawnsbury.Modding;
// using Dawnsbury.Mods.Classes.Exemplar;

// namespace Dawnsbury.Mods.Exemplar
// {
//     public class Exemplar_FlowOfWar
//     {
//         [DawnsburyDaysModMainMethod]
//         public static void Load()
//         {
//             var feat = new TrueFeat(
//                 FeatName.CustomFeat, // Replace with actual feat name
//                 6, // Replace with actual level
//                 "Flow Of War", // Replace with actual feat display name
//                 "Frequency once per hour Trigger Your turn begins. Divine battle instincts take over your body, letting you move and lash out with instinctive speed. You become quickened until the end of your turn and can use the extra action only to Strike or Stride.", // Replace with actual description
//                 new[] { ModTraits.Ikon }, // Replace with actual traits
//                 null
//             )
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