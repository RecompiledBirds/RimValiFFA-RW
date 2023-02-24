using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimValiFFARW.Packs.Activities.LordJobs.LordToils.JobGivers
{
    public class JobGiver_PillowPileAndCuddle : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!(pawn.mindState.duty.focus.Thing is Building_Bed bed)) return null;
            if (!pawn.CanReach(new LocalTargetInfo(bed), PathEndMode.ClosestTouch, Danger.None)) return null;

            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("RVFFA_PillowAndCuddle"));
            job.targetA = bed;

            return job;
        }

    }
}
