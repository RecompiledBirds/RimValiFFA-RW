using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public class ScenPart_StarterPack : ScenPart
    {
        public override void PostGameStart()
        {
            Map currentMap = Find.AnyPlayerHomeMap;
            Packmanager packmanager = Packmanager.GetLastActivePackmanager;

            if (!currentMap.IsPlayerHome) return;

            List<Pawn> colonists = currentMap.mapPawns.FreeColonists.InRandomOrder().Where(pawn => !pawn.IsInPack() && pawn.IsAvali()).ToList();
            PackDef def = DefDatabase<PackDef>.GetRandom();
            colonists.TruncateToLength(def.MaxSize);

            if (!Pack.TryMakeNewPackFromPawns(DefDatabase<PackDef>.GetNamed("RVFFA_StarterPack"), colonists, false, false, out Pack? pack)) return;

            packmanager.AddPack(pack);
            base.PostGameStart();
        }
    }
}
