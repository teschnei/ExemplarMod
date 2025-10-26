using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Display.Controls.Statblocks;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using HarmonyLib;

namespace Dawnsbury.Mods.Classes.Exemplar;

public static class ExemplarClassLoader
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FeatGeneratorAttribute : Attribute
    {
        public int Level
        {
            get; set;
        }
        public FeatGeneratorAttribute(int level)
        {
            Level = level;
        }
    }

    static IEnumerable<MethodInfo> GetFeatGenerators()
    {
        var a = typeof(ExemplarClassLoader).Assembly.GetTypes().Where(x => x.IsClass).SelectMany(x => x.GetMethods())
        .Where(x => x.GetCustomAttributes(typeof(FeatGeneratorAttribute), false).FirstOrDefault() != null)
        .OrderBy(x => (x.GetCustomAttributes(typeof(FeatGeneratorAttribute), false).First() as FeatGeneratorAttribute)!.Level);
        return a;
    }

    [DawnsburyDaysModMainMethod]
    public static void LoadMod()
    {
        ModManager.AssertV3();

        foreach (var featGenerator in GetFeatGenerators())
        {
            foreach (var feat in (featGenerator.Invoke(null, null) as IEnumerable<Feat>)!)
            {
                ModManager.AddFeat(feat);
            }
        }

        LongTermEffects.EasyRegister(ExemplarLongTermEffects.HornOfPlentyDailyElixir, LongTermEffectDuration.UntilLongRest, () => new QEffect()
        {
            Id = ExemplarQEffects.HornOfPlentyDailyElixir
        });

        ModManager.RegisterActionOnEachItem(item =>
        {
            item.WithAfterModifiedWithModification((item, mod) =>
            {
                if (mod.Kind == ItemModificationKind.CustomPermanent && ((mod.Tag as string)?.StartsWith("ikon") ?? false))
                {
                    var ikon = Ikon.IkonLUT.Values.Where(f => (mod.Tag as string)?.Contains(f.IkonFeat.FeatName.ToStringOrTechnical()) ?? false).FirstOrDefault();
                    mod.ModifyItem = ikon?.ModifyItem;
                    ikon?.ModifyItem?.Invoke(item);
                }
            });
            return item;
        });

        //Implementation of "Twin" trait
        ModManager.RegisterActionOnEachCreature(creature =>
        {
            creature.AddQEffect(new QEffect()
            {
                BonusToDamage = (q, action, target) =>
                {
                    if (action.Item?.HasTrait(ExemplarTraits.Twin) ?? false)
                    {
                        if (q.Owner.Actions.ActionHistoryThisTurn.Where(hist => hist.Item != action.Item && hist.Item?.BaseItemName == action.Item?.BaseItemName).Count() > 0)
                        {
                            return new Bonus(action.Item.WeaponProperties?.DamageDieCount ?? 0, BonusType.Circumstance, "Twin", true);
                        }
                    }
                    return null;
                }
            });
        });

        var index = CreatureStatblock.CreatureStatblockSectionGenerators.FindIndex(i => i.Name == "Impulses");

        CreatureStatblock.CreatureStatblockSectionGenerators.Insert(index,
            new("Epithets", cr => String.Join("\n",
                String.Join("\n",
                    from f in cr.PersistentCharacterSheet?.Calculated.AllFeats ?? []
                    where f.HasTrait(ExemplarTraits.RootEpithet)
                    select $"{{b}}{f.DisplayName(cr.PersistentCharacterSheet!)}{{/b}}\n" + f.RulesText
                ),
                String.Join("\n",
                    from f in cr.PersistentCharacterSheet?.Calculated.AllFeats ?? []
                    where f.HasTrait(ExemplarTraits.DominionEpithet)
                    select $"{{b}}{f.DisplayName(cr.PersistentCharacterSheet!)}{{/b}}\n" + f.RulesText
                )
            ))
        );

        CreatureStatblock.CreatureStatblockSectionGenerators.Insert(index,
            new("Ikons", cr =>
                String.Join("\n",
                    from f in cr.PersistentCharacterSheet?.Calculated.AllFeats ?? []
                    where f.HasTrait(ExemplarTraits.Ikon) && cr.Battle.Encounter is Pseudoencounter
                    select $"{{b}}{f.DisplayName(cr.PersistentCharacterSheet!)}{{/b}}\n" +
                        "{b}Immanence{/b}\n" +
                        Ikon.GetImmanenceText(f.RulesText) + "\n" +
                        String.Join("\n",
                            (from f2 in cr.PersistentCharacterSheet?.Calculated.AllFeats ?? []
                             where f2.HasTrait(ExemplarTraits.IkonExpansion) && f2.FeatName.ToStringOrTechnical().StartsWith(f.FeatName.ToStringOrTechnical())
                             select Ikon.GetImmanenceText(f2.RulesText)).Where(s => !String.IsNullOrEmpty(s))
                        ) + "\n" +
                        "{b}Transcendence{/b}\n" +
                        Ikon.GetTranscendenceText(f.RulesText) + "\n" +
                        String.Join("\n",
                            (from f2 in cr.PersistentCharacterSheet?.Calculated.AllFeats ?? []
                             where f2.HasTrait(ExemplarTraits.IkonExpansion) && f2.FeatName.ToStringOrTechnical().StartsWith(f.FeatName.ToStringOrTechnical())
                             select Ikon.GetTranscendenceText(f2.RulesText)).Where(s => !String.IsNullOrEmpty(s))
                        ) + "\n"
                )
            )
        );

        var harmony = new Harmony("junabell.dawnsburydays.exemplar");
        harmony.PatchAll();
    }
}
