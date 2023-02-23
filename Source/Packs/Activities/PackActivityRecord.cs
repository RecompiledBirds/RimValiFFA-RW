using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs.Activities
{
    public class PackActivityRecord : IExposable
    {
        public int tick;
        public PackActivityDef def;

        public PackActivityRecord() { }

        public PackActivityRecord(PackActivityDef def, int tick) 
        {
            this.def = def;
            this.tick = tick;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, nameof(def));
            Scribe_Values.Look(ref tick, nameof(tick));
        }
    }

    public class PackActivityRecordCollection : IExposable
    {
        public Pack pack;

        public List<PackActivityRecord> records = new List<PackActivityRecord>();
        
        public PackActivityRecordCollection() { }

        public PackActivityRecordCollection(Pack pack) => this.pack = pack;

        public void AddNewRecord(PackActivityDef def, int tick)
        {
            records.Add(new PackActivityRecord(def, tick));
        }

        public int LastTickForDef(PackActivityDef def) 
        {
            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (def == records[i].def) return records[i].tick;
            }

            return 0;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref pack, nameof(pack));
            Scribe_Collections.Look(ref records, nameof(records), LookMode.Deep);
        }
    }
}
