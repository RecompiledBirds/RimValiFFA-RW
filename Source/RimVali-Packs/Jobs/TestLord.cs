using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace RimVali_Packs.Jobs
{
    public class TestLordJob : LordJob
    {
        public override StateGraph CreateGraph()
        {
            Pawn pawn = lord.ownedPawns[0];
            StateGraph graph = new();

            LordToil_EscortPawn escortPawn = new LordToil_EscortPawn(pawn);
            graph.AddToil(escortPawn);

            return graph;

        }
    }
}
