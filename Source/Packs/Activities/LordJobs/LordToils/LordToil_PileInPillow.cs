using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimValiFFARW.Packs.Activities.LordJobs.LordToils
{
    public class LordToil_PileInPillow : LordToil
    {
        private readonly PawnDuty duty;

        public LordToil_PileInPillow(Building_Bed bed)
        {
            duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("RVFFA_PillowPileCuddle")) { focus = bed, maxDanger = Danger.Some};
            data = new LordToilData_PileInPillow(bed);
        }

        public override bool AllowRestingInBed => true;
        public override bool AllowSatisfyLongNeeds => true;
        public override float? CustomWakeThreshold => 0.3f;

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                lord.ownedPawns[i].mindState.duty = duty;
            }
        }
    }

    public class LordToilData_PileInPillow : LordToilData
    {
        public Building_Bed bed;

        public LordToilData_PileInPillow() { }

        public LordToilData_PileInPillow(Building_Bed bed)
        {
            this.bed = bed;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref bed, nameof(bed));
        }
    }
}
