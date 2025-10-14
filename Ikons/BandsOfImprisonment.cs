using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar.Ikons;

public class BandsOfImprisonment
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName freeItem = ModManager.RegisterNewItemIntoTheShop("OrdinaryBracers", itemName =>
        {
            return new Item(itemName, IllustrationName.BracersOfMissileDeflection, "Bracers", 1, 0, Trait.DoNotAddToShop)
            .WithDescription("An ordinary pair of bracers.")
            .WithWornAt(Trait.Bracers);
        });
        yield return new Ikon(new Feat(
            ExemplarFeats.BandsOfImprisonment,
            "These weighted bands don't enhance your power—rather, they keep your strength in check, honing your discipline.",
            "{b}Usage{/b} worn anklets, bracers, or circlet (often a headband)\n\n" +
            "{b}Immanence{/b} The {i}bands of imprisonment{i} tighten, keeping your mind sharp. You gain a +1 status bonus to Will saving throws and resistance to mental damage equal to half your level.\n\n" +
            $"{{b}}Transcendence — Break Free {RulesBlock.GetIconTextFromNumberOfActions(2)}{{/b}} (transcendence)\nYou can attempt to Escape with a +2 status bonus on your check, then Stride up to twice your Speed in a straight line, and finally make a melee Strike. If you don't need to Escape or you can't move or choose not to, you still take the other actions listed.",
            [ExemplarTraits.Ikon, ExemplarTraits.IkonWorn],
            null
        ).WithIllustration(ExemplarIllustrations.BandsOfImprisonment), (ikon, q) =>
        {
            q.StateCheck = q => q.Owner.WeaknessAndResistance.AddResistance(DamageKind.Mental, q.Owner.Level / 2);
            q.BonusToDefenses = (qfSelf, action, defense) => defense == Defense.Will ? new Bonus(1, BonusType.Status, "Bands of Imprisonment") : null;
        }, (ikon, q) =>
        {
            return new ActionPossibility(new CombatAction(
                q.Owner,
                ExemplarIllustrations.BandsOfImprisonment,
                "Break Free",
                [ExemplarTraits.Transcendence],
                "You can attempt to Escape with a +2 status bonus on your check, then Stride up to twice your Speed in a straight line, and finally make a melee Strike. If you don't need to Escape or you can't move or choose not to, you still take the other actions listed.",
                Target.Self()
            ).WithActionCost(2)
            .WithEffectOnSelf(async (action, self) =>
            {
                self.AddQEffect(new QEffect(ExpirationCondition.Ephemeral)
                {
                    BonusToAttackRolls = (q, action, target) => action.ActionId == ActionId.Escape ? new Bonus(2, BonusType.Status, "Break Free", true) : null,
                    BonusToSkillChecks = (skill, action, creature) => action.ActionId == ActionId.Escape ? new Bonus(2, BonusType.Status, "Break Free", true) : null,
                    BonusToAllSpeeds = q => new Bonus(q.Owner.Speed, BonusType.Untyped, "", false)
                });
                var grappled = self.QEffects.Where(q => q.Id == QEffectId.Grappled).FirstOrDefault();
                bool canCancel = true;
                bool escaped = true;
                if (grappled != null)
                {
                    var escape = Possibilities.CreateEscape(self, grappled);
                    await escape.AllExecute();
                    if (escape.CheckResult < CheckResult.Success)
                    {
                        escaped = false;
                    }
                }
                if (escaped)
                {
                    //TODO: test if the BonusToAllSpeeds works, then remove the second Stride
                    //TODO: straight line only!
                    if (!await self.StrideAsync("Choose where to Stride. (1/2)", allowCancel: canCancel, allowPass: !canCancel) && canCancel)
                    {
                        action.RevertRequested = true;
                    }
                    else
                    {
                        await self.StrideAsync("Choose where to Stride. And you should be in reach of an enemy. (2/2)", allowPass: true);
                        await CommonCombatActions.StrikeAdjacentCreature(self, null);
                    }
                }
            }));
        })
        .WithValidItem(item =>
        {
            if (!item.HasTrait(Trait.Worn) || (item.WornAt != Trait.Shoes && item.WornAt != Trait.Bracers && item.WornAt != Trait.Headband))
            {
                return "Must be worn anklets, bracers, or circlet.";
            }
            return null;
        })
        .WithFreeWornItem(freeItem)
        .IkonFeat;
    }
}
