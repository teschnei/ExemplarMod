using System;
using System.Linq;
using System.Collections.Generic;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core.Animations;
using System.Reflection.Metadata.Ecma335;
using Dawnsbury.Display;

namespace Dawnsbury.Mods.Exemplar
{
    public class Ikons_PeltOfTheBeast
    {
        // 1) Dummy FeatNames for each attunement
        public static readonly FeatName IkonPeltBeastCold =
            ModManager.RegisterFeatName("IkonPeltBeastCold", "Pelt of the Beast: Cold Attunement");
        public static readonly FeatName IkonPeltBeastElectricity =
            ModManager.RegisterFeatName("IkonPeltBeastElectricity", "Pelt of the Beast: Electricity Attunement");
        public static readonly FeatName IkonPeltBeastFire =
            ModManager.RegisterFeatName("IkonPeltBeastFire", "Pelt of the Beast: Fire Attunement");
        public static readonly FeatName IkonPeltBeastPoison =
            ModManager.RegisterFeatName("IkonPeltBeastPoison", "Pelt of the Beast: Poison Attunement");
        public static readonly FeatName IkonPeltBeastSonic =
            ModManager.RegisterFeatName("IkonPeltBeastSonic", "Pelt of the Beast: Sonic Attunement");

        private static readonly Dictionary<FeatName,string> PeltAttunementDescriptions = new()
        {
            { IkonPeltBeastCold,        "Your hide shivers against chilling assaults, granting resistance to cold." },
            { IkonPeltBeastElectricity, "Your hide crackles with static wards against lightning strikes." },
            { IkonPeltBeastFire,        "Your hide smolders with embers, softening the sting of fire." },
            { IkonPeltBeastPoison,      "Your hide seeps toxins back, reducing the venom's venomous bite." },
            { IkonPeltBeastSonic,       "Your hide vibrates with harmonics, dulling the roar of sonic blasts." }
        };

        [DawnsburyDaysModMainMethod]
        public static void Load()
        {
            var peltAttunements = new (FeatName Name, string Title)[]
            {
                (IkonPeltBeastCold,        "Pelt of the Beast: Cold Attunement"),
                (IkonPeltBeastElectricity, "Pelt of the Beast: Electricity Attunement"),
                (IkonPeltBeastFire,        "Pelt of the Beast: Fire Attunement"),
                (IkonPeltBeastPoison,      "Pelt of the Beast: Poison Attunement"),
                (IkonPeltBeastSonic,       "Pelt of the Beast: Sonic Attunement")
            };

            foreach (var (fn, title) in peltAttunements)
            {
                // pull the long description, or empty if missing
                var desc = PeltAttunementDescriptions.TryGetValue(fn, out var d) ? d : "";

                ModManager.AddFeat(new TrueFeat(
                    fn,
                    1,
                    title,    // display name
                    desc,     // ← now your new flavor text
                    Array.Empty<Trait>(), 
                    null
                ));
            }

            // 3) The real “Pelt of the Beast” ikon
            var pelt = new TrueFeat(
                ExemplarFeatNames.IkonPeltOfTheBeast,
                1,
                "Pelt Of The Beast",
                "This animal hide…\n\n" +
                "{b}Immanence{/b} At daily preparations, choose one attunement: Cold, Electricity, Fire, Poison, or Sonic. " +
                "You gain resistance equal to half your level to that damage type.\n\n" +
                "{b}Transcendence — Survive the Wilds (one-action){/b} Wrapping the pelt, you may re-attune. " +
                "You and allies in 15-ft. emanation gain +2 circumstance bonus to AC & saves vs. that damage type until the start of your next turn.",
                new[] { ModTraits.Ikon, ModTraits.BodyIkon }, null
            ).WithMultipleSelection()
            // 4) Morning-prep dropdown
            .WithOnSheet(sheet =>
            {
                sheet.AddSelectionOption(
                    new SingleFeatSelectionOption(
                        key: "PeltOfBeast:Attunement",
                        name: "Pelt of the Beast Attunement",
                        level: SelectionOption.MORNING_PREPARATIONS_LEVEL,
                        eligible: ft =>
                            ft.FeatName == IkonPeltBeastCold
                         || ft.FeatName == IkonPeltBeastElectricity
                         || ft.FeatName == IkonPeltBeastFire
                         || ft.FeatName == IkonPeltBeastPoison
                         || ft.FeatName == IkonPeltBeastSonic
                    ).WithIsOptional()
                );
            })
            // 5) Passive “resistance” + the one-action Transcendence
            .WithPermanentQEffect(null, qf =>
            {
                // Immanence: grant you resistance as a reactive BonusToDefenses
                qf.BonusToDefenses = (eff, action, def) =>
                {
                    if (action == null)
                        return null;
                    if (!eff.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredPeltOfTheBeast))
                        return null;

                    // figure out which dummy feat you have
                    var owner = eff.Owner;
                    DamageKind kind =
                        owner.HasFeat(IkonPeltBeastCold) ? DamageKind.Cold :
                        owner.HasFeat(IkonPeltBeastElectricity) ? DamageKind.Electricity :
                        owner.HasFeat(IkonPeltBeastFire) ? DamageKind.Fire :
                        owner.HasFeat(IkonPeltBeastPoison) ? DamageKind.Poison 
                        : DamageKind.Sonic ;

                    // if the incoming action deals that kind, treat it as resistance half-your-level
                    if (true)
                    {
                        eff.Owner.WeaknessAndResistance.AddResistance(kind, owner.Level / 2);
                    }


                    return null;
                };

                // Transcendence — recast that one-action effect
                qf.ProvideMainAction = qf =>
                {
                    var owner = qf.Owner;
                    if (qf.Owner.HasEffect(ExemplarIkonQEffectIds.TranscendenceTracker) || !qf.Owner.HasEffect(ExemplarIkonQEffectIds.QEmpoweredPeltOfTheBeast))
                        return null;

                    var act = new CombatAction(
                        owner,
                        IllustrationName.MagicHide,
                        "Survive the Wilds",
                        new[] { Trait.Aura, Trait.Manipulate, ModTraits.Transcendence, ModTraits.Ikon },
                        "Re-wrap the pelt (you may re-attune) and grant +2 circumstance bonus to AC & saves against the attuned type in 15-ft. emanation.",
                        Target.Self()
                    ).WithActionCost(1);

                    act.WithEffectOnSelf(async (action, self) =>
                    {
                        // 1) Build an array of display-strings for each dummy-feat
                        var options = new[] {
                            (IkonPeltBeastCold, Label: "Cold"),
                            (IkonPeltBeastElectricity, Label: "Electricity"),
                            (IkonPeltBeastFire, Label : "Fire"),
                            (IkonPeltBeastPoison,Label: "Poison"),
                            (IkonPeltBeastSonic, Label: "Sonic")
                        };
                        var labels = options.Select(fn => fn.Label).ToArray();  // use ToString(), not .Name

                        // 2) Prompt the player with buttons
                        var choice = await self.AskForChoiceAmongButtons(
                            IllustrationName.MagicHide,
                            "Re-choose your Pelt of the Beast attunement",
                            labels  // this is now a string[], exactly what AskForChoiceAmongButtons wants
                        );

                        // 3) Figure out which feat they picked
                        var pick = options.First(fn => fn.Label == choice.Text);

                        // 4) Announce it
                        self.Occupies.Overhead($"Pelt attuned to {pick.Label}.", Microsoft.Xna.Framework.Color.Gold);
                        AuraAnimation auraAnimation = qf.Owner.AnimationData.AddAuraAnimation(IllustrationName.MagicCircle150, 3F);

                        // 5) Blast out the +2 aura vs. the new damage kind
                        var kind = pick.Item1 == IkonPeltBeastCold ? Trait.Cold
                                : pick.Item1 == IkonPeltBeastElectricity ? Trait.Electricity
                                : pick.Item1 == IkonPeltBeastFire ? Trait.Fire
                                : pick.Item1 == IkonPeltBeastPoison ? Trait.Poison
                                : Trait.Sonic;

                        foreach (var ally in self.Battle.AllCreatures)
                        {
                            if (ally.DistanceTo(self) > 3
                                || ally.HasEffect(ExemplarIkonQEffectIds.QMirroredAegisAura))
                                continue;
                            ally.AddQEffect(new QEffect(
                                    "Pelt Aura",
                                    $"+2 circumstance bonus to AC & saves vs. {kind}",
                                    ExpirationCondition.ExpiresAtStartOfYourTurn,
                                    self, IllustrationName.MagicHide
                                )
                            {
                                WhenExpires = delegate { auraAnimation.MoveTo(0f); },
                                Id = ExemplarIkonQEffectIds.QPeltAura,
                                BonusToDefenses = (qfSelf, act2, def2) =>
                                {
                                    if (act2 == null)
                                        return null;
                                    if (def2 == Defense.AC)
                                    {
                                        return new Bonus(2, BonusType.Circumstance, "Survive the Wilds");
                                    }
                                    else if (act2.HasTrait(kind))
                                    {
                                        return new Bonus(2, BonusType.Circumstance, "Survive the Wilds");
                                    }
                                    return null;
                                },

                            });
                        }



                        // 3) Clean up empowerment + grant your free shift / exhaustion tracker
                        self.RemoveAllQEffects(q => ExemplarIkonQEffectIds.EmpoweredIkonIds.Contains(q.Id));
                        self.AddQEffect(new QEffect("First Shift Free", "Your next Immanence is free.")
                        { Id = ExemplarIkonQEffectIds.FirstShiftFree });
                        self.AddQEffect(new QEffect(
                                "Spark Exhaustion",
                                "You cannot Transcendence again this turn.",
                                ExpirationCondition.ExpiresAtStartOfYourTurn, self
                            )
                        { Id = ExemplarIkonQEffectIds.TranscendenceTracker });
                    });

                    return new ActionPossibility(act);
                };
            });

            ModManager.AddFeat(pelt);
        }
    }
}
