using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public class Packmanager : WorldComponent
    {
        private static Packmanager packmanager;

        private HashSet<Pack> packs = new HashSet<Pack>();
        private Dictionary<Pawn, Pack> memberPackTable = new Dictionary<Pawn, Pack>();

        private int nextPackLoadID = -1;

        public static Packmanager GetLastActivePackmanager => packmanager;

        public int NextPackLoadID
        {
            get
            {
                nextPackLoadID++;
                return nextPackLoadID;
            }
        }

        public IEnumerable<Pack> PacksReadOnly => memberPackTable.Values;

        public Packmanager(World world) : base(world) => packmanager = this;

        public bool AddPack(Pack pack)
        {
            if (!packs.Add(pack)) return false;
            
            foreach(Pawn member in pack.Members) memberPackTable.Add(member, pack);

            return true;
        }

        public bool TryGetPackForPawn(Pawn pawn, out Pack pack)
        {
            Log.Message($"Dictionary: {memberPackTable.Join(kvp => $"{kvp.Value.GetUniqueLoadID()}", ", ")}");
            return memberPackTable.TryGetValue(pawn, out pack);
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextPackLoadID, nameof(nextPackLoadID));
            Scribe_Collections.Look(ref packs, nameof(packs), LookMode.Deep);
            Scribe_Collections.Look(ref memberPackTable, nameof(memberPackTable), LookMode.Reference, LookMode.Reference);
        }
    }
}
