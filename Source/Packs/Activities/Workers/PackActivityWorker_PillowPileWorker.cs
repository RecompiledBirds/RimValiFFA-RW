using HarmonyLib;
using RimValiFFARW;
using RimValiFFARW.Packs.Activities.LordJobs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace RimValiFFARW.Packs.Activities.Workers
{
    public class PackActivityWorker_PillowPileWorker : PackActivityWorker
    {
        private Building_Bed bed;

        public PackActivityWorker_PillowPileWorker() { }

        protected override bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot)
        {
            spot = new IntVec3();
            IEnumerable<Pawn> membersWithBeds = pack.Members.Where(pawn => def.gatherSpotDefs.Contains(pawn.ownership.OwnedBed?.def));
            if (membersWithBeds.EnumerableNullOrEmpty()) return false;

            bed = membersWithBeds.RandomElement().ownership.OwnedBed;
            spot = bed.Position;
            return true;
        }

        protected override LordJob CreateLordJob(IntVec3 spot, Pawn organizer) => new LordJob_PillowPileLordJob(pack, bed);

        public override void ExposeData()
        {
            Scribe_References.Look(ref bed, nameof(bed));
            base.ExposeData();
        }

        public override PackActivityDef GetPackActivityDef() => PackActivityDefOf.RVFFA_PillowPile;
    }
}
