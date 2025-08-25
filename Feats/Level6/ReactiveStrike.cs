using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;

namespace Dawnsbury.Mods.Classes.Exemplar
{
    public class Exemplar_ReactiveStrike
    {

        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var ReactiveStrike = new TrueFeat(
                ExemplarFeats.ReactiveStrike,
                6,
                "Reactive Strike",
                "You lash out at a foe that leaves an opening. Make a melee Strike against the triggering creature. If your attack is a critical hit and the trigger was a manipulate action, you disrupt that action. This Strike doesn't count toward your multiple attack penalty, and your multiple attack penalty doesn't apply to this Strike.",
                [ExemplarTraits.Exemplar],
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
