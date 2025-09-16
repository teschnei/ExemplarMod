
using System;
using System.Collections.Generic;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;

namespace Dawnsbury.Mods.Classes.Exemplar.Epithets;

public class TranscendAction
{
    public string Description;
    public Func<Creature, CombatAction, Possibility> Possibility;

    public TranscendAction(string description, Func<Creature, CombatAction, Possibility> possibility)
    {
        Description = description;
        Possibility = possibility;
    }
}

public class Epithet : Feat
{
    public TranscendAction? TranscendAction { get; private set; }

    public Epithet(FeatName featName, string flavorText, string rulesText, List<Trait> traits, List<Feat>? subfeats) : base(featName, flavorText, rulesText, traits, subfeats)
    {
    }

    public Epithet WithTranscendPossibility(string description, Func<Creature, CombatAction, Possibility> possibility)
    {
        TranscendAction = new TranscendAction(description, possibility);
        return this;
    }
}
