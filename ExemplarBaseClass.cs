using Dawnsbury.Modding;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Display.Text;
using Dawnsbury.Display.Illustrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Microsoft.Xna.Framework.Graphics;
using Dawnsbury.Mods.Exemplar;
using Dawnsbury.Core.Mechanics.Treasure;
using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Classes.Exemplar
{
    public static class ExemplarBaseClass
    {
        public static Trait TExemplar;

        public static void Load()
        {
            TExemplar = ModManager.RegisterTrait("Exemplar", new TraitProperties("Exemplar", true)
            {
                IsClassTrait = true
            });
            ModManager.AddFeat(CreateExemplarClassFeat());
        }

        public static ClassSelectionFeat CreateExemplarClassFeat()
        {
            return (ClassSelectionFeat)new ClassSelectionFeat(
                ModManager.RegisterFeatName("FeatExemplar", "Exemplar"),
                "You carry a divine spark forged from your own soul. By empowering sacred ikons, you unleash transcendent power.",
                TExemplar,
                new LimitedAbilityBoost(Ability.Strength, Ability.Dexterity),
                10,
                [
                    Trait.Reflex, Trait.Perception,
                    Trait.Religion,
                    Trait.Simple, Trait.Martial, Trait.Unarmed,
                    Trait.LightArmor, Trait.MediumArmor, Trait.UnarmoredDefense
                ],
                [Trait.Fortitude, Trait.Will],
                3,
                @"{b}Ikons, Spark, and Transcendence.
                {/b} You select 3 ikons. You can empower one at a time with your divine spark using {i}Shift Immanence{/i}, gaining its passive benefits. Once per round, you can use that ikon's active {i}Transcendence{/i} ability. After you do, your spark returns to your soul.
                {b}At higher levels:{/b}  
                {b}Level 2:{/b} Exemplar feat, skill feat  
                {b}Level 3:{/b} General feat, root epithet, skill increase  
                {b}Level 4:{/b} Exemplar feat, skill feat  
                {b}Level 5:{/b} Ability boosts, ancestry feat, skill increase, weapon expertise {i}(You gain expert proficiency in all weapons you are trained in from your class features.){/i}  
                {b}Level 6:{/b} Exemplar feat, skill feat  
                {b}Level 7:{/b} Dominion epithet, general feat, skill increase, spirit striking, unassailable soul  
                {b}Level 8:{/b} Exemplar feat, skill feat
                ",
                null
            )
            .WithOnSheet(sheet =>
            {
                sheet.GrantFeat(FeatName.ShieldBlock);
                sheet.GrantFeat(FeatName.DeadlySimplicity);
                sheet.AddSelectionOption(new MultipleFeatSelectionOption("Ikon", "Ikon", 1, ft => ft.HasTrait(ModTraits.Ikon), 3));
                sheet.AddSelectionOption(new SingleFeatSelectionOption("ExemplarFeat1", "Exemplar Feat", 1, ft => ft.HasTrait(TExemplar)).WithIsOptional());
                sheet.AddSelectionOption(new SingleFeatSelectionOption("RootEpithet", "Root Epithet", 3, ft => ft.HasTrait(ModTraits.RootEpithet)).WithIsOptional());
                sheet.AddSelectionOption(new SingleFeatSelectionOption("DominionEpithet", "Dominion Epithet", 7, ft => ft.HasTrait(ModTraits.DominionEpithet)).WithIsOptional());

                sheet.AddAtLevel(5, values =>
                {
                    values.SetProficiency(Trait.Simple, Proficiency.Expert);
                    values.SetProficiency(Trait.Martial, Proficiency.Expert);
                    values.SetProficiency(Trait.Unarmed, Proficiency.Expert);
                });

                sheet.AddAtLevel(7, values =>
                {
                    values.SetProficiency(Trait.Will, Proficiency.Master);
                });
                IkonRuneUtilities.EnsureCorrectRunes(sheet);
            })
            .WithOnCreature((sheet, cr) =>
            {
                IkonRuneUtilities.EnsureCorrectRunes(sheet);
                cr.AddQEffect(new QEffect("Shift Immanence", "Empower one of your chosen Ikons.")
                {
                    ProvideMainAction = qf =>
                    {
                        // 1. Gather all your Ikon feats
                        var allIkonFeats = (qf.Owner.PersistentCharacterSheet?.Calculated?.AllFeats ?? Enumerable.Empty<Feat>())
                            .Where(f => f.HasTrait(ModTraits.Ikon));

                        // 2. Build a set of rune ItemNames currently on your held weapons
                        var runesOnWeapons = qf.Owner.HeldItems
                            .SelectMany(item => item.Runes)
                            .Select(rune => rune.BaseItemName)
                            .ToHashSet();

                        // 3. Map each weapon Ikon feat to its corresponding rune item
                        var weaponIkonMap = new Dictionary<FeatName, ItemName>
                        {
                            { ExemplarFeatNames.IkonBarrowsEdge,         ExemplarItemNames.IkonBarrowsEdgeRune },
                            { ExemplarFeatNames.IkonGleamingBlade,       ExemplarItemNames.IkonGleamingBladeRune },
                            { ExemplarFeatNames.IkonMortalHarvest,       ExemplarItemNames.IkonMortalHarvestRune},
                            { ExemplarFeatNames.IkonMirroredAegis ,      ExemplarItemNames.IkonMirroredAegis},
                            { ExemplarFeatNames.IkonStarshot,            ExemplarItemNames.IkonStarshotRune},
                            { ExemplarFeatNames.IkonNobleBranch,         ExemplarItemNames.IkonNobleBranch},
                            { ExemplarFeatNames.IkonTitansBreaker,       ExemplarItemNames.IkonTitansBreaker},
                        };

                        var bodyIkonNames = new List<String> {
                            "Bands of Imprisonment", "Eye Catching Spot", "Thousand League Sandals" , "Gaze Sharp As Steel",
                            "Hands Of The Wildling", "Scar Of The Survivor", "Fetching Bangles", "Pelt Of The Beast", "Skin Hard As Horn",
                        };

                        var exhaustedIkonId = qf.Owner.FindQEffect(ExemplarIkonQEffectIds.TranscendenceTracker)?.Tag;

                        var empowerableIkons = allIkonFeats.Where(f =>
                            //make sure the weapon has the feat
                            ((weaponIkonMap.TryGetValue(f.FeatName, out var runeName) && runesOnWeapons.Contains(runeName))
                            //or maybe it's just a body feat!
                            || bodyIkonNames.Contains(f.Name.ToString()) ) 
                            //regardless, if it's exhausted, don't show it.
                            && !Equals(exhaustedIkonId, ExemplarIkonQEffectIds.GetEmpowermentIdForIkon(f.Name.ToString()))
                             ).ToList();

                        return new SubmenuPossibility(IllustrationName.SpiritualWeapon, "Shift Immanence")
                        {
                            Subsections = [
                                new PossibilitySection("Select Ikon to Empower")
                                {
                                    Possibilities = empowerableIkons.Select(q =>
                                    {                                            

                                        int actionCost = qf.Owner.HasEffect(ExemplarIkonQEffectIds.FirstShiftFree) ? 0 : 1;

                                        return (Possibility) new ActionPossibility(
                                            new CombatAction(qf.Owner, IllustrationName.SpiritualWeapon, $"Empower {q.Name}",
                                                [TExemplar, Trait.Divine, Trait.Basic],
                                                $"{q.FullTextDescription}", Target.Self())
                                                .WithActionCost(actionCost)
                                                .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                                                {
                                                    // Remove any previously empowered Ikons
                                                    caster.RemoveAllQEffects(qfx => ExemplarIkonQEffectIds.EmpoweredIkonIds.Contains(qfx.Id));
                                                    // Remove First Shift Free if active
                                                    caster.RemoveAllQEffects(qfx => qfx.Id == ExemplarIkonQEffectIds.FirstShiftFree);

                                                    // Add the new empowerment manually
                                                    caster.AddQEffect(new QEffect($"Divine Spark: {q.Name}", "You have empowered this Ikon.")
                                                    {
                                                        Id = ExemplarIkonQEffectIds.GetEmpowermentIdForIkon(q.Name)
                                                    });
                                                })
                                        );
                                    })
                                    .ToList()
                                }
                            ]
                        };
                    }
                });
                // Give "First Shift Free" at start of game (optional - if your system sets buffs at load)
                cr.AddQEffect(new QEffect("First Shift Free", "Your first Shift Immanence is free.")
                {
                    Id = ExemplarIkonQEffectIds.FirstShiftFree,
                    ExpiresAt = ExpirationCondition.Never
                });

                if (cr.Level >= 7)
                {
                    cr.AddQEffect(new QEffect("Spirit Striking", "+2 spirit damage on expert weapons and unarmed attacks.")
                    {
                        AddExtraStrikeDamage = (action, defender) =>
                        {
                            var item = action.Item;
                            if (item != null && item.HasTrait(Trait.Simple) && (int)cr.GetProficiency(item) >= (int)Proficiency.Expert)
                            {
                                return (DiceFormula.FromText("2", "Spirit Striking"), DamageKind.Untyped);
                            }
                            return null;
                        }
                    });

                    cr.AddQEffect(CreateUnassailableSoul());
                }
            });
        }

        public static QEffect CreateUnassailableSoul()
        {
            return new QEffect("Unassailable Soul", "You gain master proficiency in Will saves. Successes on Will saves are treated as critical successes.")
            {
                BonusToDefenses = (effect, action, defense) =>
                {
                    return defense == Defense.Will
                        ? new Bonus(0, BonusType.Untyped, "Unassailable Soul")
                        : null;
                }
            };
        }
    }
}
