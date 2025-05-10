using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Classes.Exemplar
{
    public class IkonRuneKind
    {
        /// <summary>
        /// thanks to Sudo!
        /// The technical Weapon Implement persistent Rune Kind ID 
        /// </summary>
        public static readonly RuneKind BarrowsEdge = ModManager.RegisterEnumMember<RuneKind>("BarrowsEdge IkonRuneKind Kind ID");
        public static readonly RuneKind BandOFImprisonment = ModManager.RegisterEnumMember<RuneKind>("BandOFImprisonment IkonRuneKind Kind ID");
        public static readonly RuneKind GleamingBlade = ModManager.RegisterEnumMember<RuneKind>("GleamingBlade IkonRuneKind Kind ID");
        public static readonly RuneKind MirroredAegis = ModManager.RegisterEnumMember<RuneKind>("MirroredAegis IkonRuneKind Kind ID");
        public static readonly RuneKind MortalHarvest = ModManager.RegisterEnumMember<RuneKind>("MortalHarvest IkonRuneKind");
        public static readonly RuneKind Starshot = ModManager.RegisterEnumMember<RuneKind>("Starshot IkonRuneKind");
        public static readonly RuneKind NobleBranch = ModManager.RegisterEnumMember<RuneKind>("NobleBranch IkonRuneKind");
        public static readonly RuneKind TitansBreaker = ModManager.RegisterEnumMember<RuneKind>("TitansBreaker IkonRuneKind");

    }
}
