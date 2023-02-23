using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace RimValiFFARW.Packs.Activities
{
    public abstract class PackActivityWorker : IExposable
    {
        private PackActivityDef def;
        private Pack pack;

        public PackActivityWorker() { }

        public PackActivityWorker(PackActivityDef def, Pack pack)
        {
            this.def = def;
            this.pack = pack;
        }

        protected abstract LordJob GetJob();

        public void StartLordJob(Pack pack)
        {
            if (!(pack.Members.FirstOrDefault() is Pawn pawn)) return;

            LordMaker.MakeNewLord(pawn.Faction, GetJob(), pawn.Map, pack.Members);
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, nameof(def));
            Scribe_References.Look(ref pack, nameof(pack));
        }
    }
}
