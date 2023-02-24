using RimValiFFARW.Packs.Activities.LordJobs.LordToils;
using RimWorld;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimValiFFARW.Packs.Activities.LordJobs
{
    public class LordJob_PillowPileLordJob : LordJob
    {
        private Building_Bed bed;
        private Pack pack;

        public override bool AllowStartNewGatherings => true;
        public override bool AllowStartNewRituals => true;
        public override bool AlwaysShowWeapon => false;
        public override bool CanBlockHostileVisitors => false;
        public override bool DontInterruptLayingPawnsOnCleanup => false;
        public override bool GuiltyOnDowned => false;
        public override bool IsCaravanSendable => true;

        public LordJob_PillowPileLordJob() { }

        public LordJob_PillowPileLordJob(Pack pack, Building_Bed bed)
        {
            this.pack = pack;
            this.bed = bed;
        }

        public override StateGraph CreateGraph()
        {
            LordToil wanderCloseToBedAndWait = new LordToil_WanderClose(bed.Position);
            LordToil cuddle = new LordToil_PileInPillow(bed);

            Transition transition = new Transition(wanderCloseToBedAndWait, cuddle)
            {
                triggers =
                {
                    new Trigger_Custom((TriggerSignal _) => lord.ownedPawns.Average(pawn => bed.Position.DistanceTo(pawn.Position)) < 10f)
                },

                postActions =
                {
                    new TransitionAction_Custom(() => Log.Message("jhksadhasb"))
                }
            };

            StateGraph graph = new StateGraph();
            graph.StartingToil = wanderCloseToBedAndWait;
            graph.AddToil(cuddle);
            graph.AddTransition(transition);

            return graph;
        }


        public override void ExposeData()
        {
            Scribe_References.Look(ref pack, nameof(pack));
            Scribe_References.Look(ref bed, nameof(bed));
            base.ExposeData();
        }
    }
}
