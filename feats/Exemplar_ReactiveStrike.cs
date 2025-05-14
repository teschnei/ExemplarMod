using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;

namespace Dawnsbury.Mods.Exemplar
{
    public class Exemplar_ReactiveStrike
    {

        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var ReactiveStrike = new TrueFeat(
                ExemplarFeatNames.FeatReactiveStrike,
                6,
                "Reactive Strike",
                "You lash out at a foe that leaves an opening. Make a melee Strike against the triggering creature. If your attack is a critical hit and the trigger was a manipulate action, you disrupt that action. This Strike doesn't count toward your multiple attack penalty, and your multiple attack penalty doesn't apply to this Strike.",
                new[] { ExemplarBaseClass.TExemplar },
                null
            )
            .WithOnSheet(sheet =>
            {
                sheet.GrantFeat(FeatName.AttackOfOpportunity);
            });
            ModManager.AddFeat(ReactiveStrike);
        }
    }
}
