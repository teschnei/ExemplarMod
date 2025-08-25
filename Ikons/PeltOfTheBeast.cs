using System;
using System.Linq;
using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using static Dawnsbury.Mods.Classes.Exemplar.ExemplarClassLoader;

namespace Dawnsbury.Mods.Classes.Exemplar;

public class PeltOfTheBeast
{
    [FeatGenerator(0)]
    public static IEnumerable<Feat> GetFeat()
    {
        ItemName ikonRune = ModManager.RegisterNewItemIntoTheShop("PeltOfTheBeast", itemName =>
        {
            return new Item(itemName, IllustrationName.FearsomeRunestone, "Pelt of the Beast", 1, 0, Trait.DoNotAddToShop, ExemplarTraits.IkonCloakBelt)
            .WithRuneProperties(new RuneProperties("Ikon", IkonRuneKind.PeltOfTheBeast, "This animal hide, whether worn about the shoulders or waist, is all you need to survive in the harshest elements.",
            "", item =>
            {
                item.Traits.AddRange([ExemplarTraits.Ikon, Trait.Divine]);
            })
            .WithCanBeAppliedTo((Item rune, Item item) =>
            {
                if (!item.HasTrait(Trait.Worn) || (item.WornAt != Trait.Cloak && item.WornAt != Trait.Belt))
                {
                    return "Must be a worn cloak or belt.";
                }
                return null;
            }));
        });
        ItemName freeItem = ModManager.RegisterNewItemIntoTheShop("OrdinaryCloak", itemName =>
        {
            return new Item(itemName, IllustrationName.CloakOfEnergyResistance, "Cloak", 1, 0, Trait.DoNotAddToShop)
            .WithDescription("An ordinary cloak.")
            .WithWornAt(Trait.Cloak);
        });

        Dictionary<FeatName, (DamageKind, Trait)> peltFeatTypes = new()
        {
            [ExemplarFeats.PeltOfTheBeastCold] = (DamageKind.Cold, Trait.Cold),
            [ExemplarFeats.PeltOfTheBeastElectricity] = (DamageKind.Electricity, Trait.Electricity),
            [ExemplarFeats.PeltOfTheBeastFire] = (DamageKind.Fire, Trait.Fire),
            [ExemplarFeats.PeltOfTheBeastPoison] = (DamageKind.Poison, Trait.Poison),
            [ExemplarFeats.PeltOfTheBeastSonic] = (DamageKind.Sonic, Trait.Sonic)
        };

        var peltFeats = peltFeatTypes.Select(feats =>
        {
            return new Feat(feats.Key, null, $"Attune the Pelt of the Beast to {feats.Value.Item1.ToString()}", [ExemplarTraits.PeltOfTheBeastAttune], null)
                .WithOnCreature(creature =>
                {
                    var pelt = creature.FindQEffect(ExemplarQEffects.PeltOfTheBeastAttunement);
                    if (pelt != null)
                    {
                        pelt.Tag = feats.Value;
                    }
                });
        });

        foreach (var feat in peltFeats)
        {
            yield return feat;
        }

        yield return new Ikon(new Feat(
            ExemplarFeats.PeltOfTheBeast,
            "This animal hide, whether worn about the shoulders or waist, is all you need to survive in the harshest elements.",
            "When you make your daily preparations, choose cold, electricity, fire, poison, or sonic damage. The pelt attunes to that damage type.\n\n" +
            "{b}Immanence{/b} You gain resistance equal to half your level to the damage type the pelt is attuned to.\n\n" +
            $"{{b}}Transcendence — Survive the Wilds {RulesBlock.GetIconTextFromNumberOfActions(1)}{{/b}} (aura, manipulate, transcendence)\n" +
            "You wrap the pelt around yourself. You can choose to change the damage type the pelt is attuned to. The pelt shines gold, drawing the offending " +
            "energies into itself. Until the start of your next turn, this shine creates an aura in a 15-foot emanation. You and all allies in the emanation gain a " +
            "+2 circumstance bonus to AC and saving throws against effects with that trait.",
            [ExemplarTraits.Ikon],
            null
        ).WithIllustration(IllustrationName.MagicHide)
        .WithOnSheet(sheet =>
        {
            sheet.AddSelectionOption(new SingleFeatSelectionOption(
                key: "PeltOfBeast:Attunement",
                name: "Pelt of the Beast Attunement",
                level: SelectionOption.MORNING_PREPARATIONS_LEVEL,
                eligible: ft => ft.HasTrait(ExemplarTraits.PeltOfTheBeastAttune)
            ));
        })
        .WithPermanentQEffect(null, q =>
        {
            q.Id = ExemplarQEffects.PeltOfTheBeastAttunement;
        }), q =>
        {
            q.StateCheck = (qe) =>
            {
                var pelt = Ikon.GetIkonItemWorn(qe.Owner, ikonRune);
                var peltq = qe.Owner.FindQEffect(ExemplarQEffects.PeltOfTheBeastAttunement);
                if (pelt != null && peltq != null && peltq.Tag is ValueTuple<DamageKind, Trait> tag)
                {
                    qe.Owner.WeaknessAndResistance.AddResistance(tag.Item1, qe.Owner.Level / 2);
                }
            };
        },
        q =>
        {
            var peltActions = peltFeatTypes.Select(feats =>
            {
                return new ActionPossibility(new CombatAction(q.Owner, IllustrationName.MagicHide,
                    $"Survive the Wilds ({feats.Value.Item1.ToString()})", [Trait.Aura, Trait.Manipulate, ExemplarTraits.Transcendence],
                    "You wrap the pelt around yourself. You can choose to change the damage type the pelt is attuned to. The pelt shines gold, drawing the offending " +
                    "energies into itself. Until the start of your next turn, this shine creates an aura in a 15-foot emanation. You and all allies in the emanation gain a " +
                    "+2 circumstance bonus to AC and saving throws against effects with that trait.",
                    Target.Self().WithAdditionalRestriction(self =>
                    {
                        if (Ikon.GetIkonItemWorn(self, ikonRune) == null)
                        {
                            return "You must be wearing the pelt of the beast.";
                        }
                        return null;
                    }))
                    .WithActionCost(1)
                    .WithEffectOnChosenTargets(async (action, self, targets) =>
                    {
                        self.AddQEffect(new QEffect("Survive the Wilds", $"You and all allies in the emanation gain a +2 circumstance bonus to AC and saving throws against effects with the {feats.Value.Item1.ToString()} trait.", ExpirationCondition.ExpiresAtStartOfYourTurn, self, IllustrationName.MagicHide)
                        {
                            SpawnsAura = q => q.Owner.AnimationData.AddAuraAnimation(IllustrationName.MagicCircle150, 3)
                        }.AddGrantingOfTechnical(cr => cr.FriendOf(q.Owner), qe =>
                        {
                            qe.BonusToDefenses = (qe, action, defense) =>
                            {
                                if (qe.Owner.DistanceTo(q.Owner) <= 3 && (action?.HasTrait(feats.Value.Item2) ?? false) && defense is Defense.AC or Defense.Reflex or Defense.Will or Defense.Fortitude)
                                {
                                    return new Bonus(2, BonusType.Circumstance, "Survive the Wilds", true);
                                }
                                return null;
                            };
                        }));
                    })) as Possibility;
            });
            return new SubmenuPossibility(IllustrationName.MagicHide, "Survive the Wilds")
            {
                Subsections =
                [
                    new PossibilitySection("SurvivetheWilds")
                    {
                        Possibilities = peltActions?.ToList() ?? []
                    }
                ]
            };

        })
        .WithRune(ikonRune)
        .WithFreeWornItem(freeItem)
        .IkonFeat;
    }
}
