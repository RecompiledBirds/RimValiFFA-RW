using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Nexus
{
    public static class NexusUtils
    {
        public static Thing GenNexus()
        {
            return ThingMaker.MakeThing(NexusDefOf.RVFFA_AvaliNexus);
        }

        public static Thing SpawnNexus(Map map, IntVec3 pos)
        {
            Thing nexus = GenNexus();

            return GenSpawn.Spawn(nexus,pos,map,WipeMode.FullRefund);
        }

        public static Thing DoAirdrop(Map map)
        {
            List<Thing> things = new List<Thing>()
            {
                GenNexus()
            };
            DropCellFinder.FindSafeLandingSpot(out IntVec3 pos, Faction.OfPlayer, map);
            DropPodUtility.DropThingsNear(pos, map, things);
            return things[0];
        }
    }
}
