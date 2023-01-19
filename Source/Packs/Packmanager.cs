using HarmonyLib;
using Mono.Math;
using RimWorld.Planet;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    /// <summary>
    ///     <see cref="WorldComponent"/> which tracks and manages every <see cref="Pack"/>
    /// </summary>
    public class Packmanager : WorldComponent
    {
        private static Packmanager packmanager;

        private HashSet<Pack> packs = new HashSet<Pack>();
        private Dictionary<Pawn, Pack> memberPackTable = new Dictionary<Pawn, Pack>();

        private int nextPackLoadID = -1;

        public static Packmanager GetLastActivePackmanager => packmanager;

        /// <summary>
        ///     Returns and manages PackLoadIDs
        /// </summary>
        public int NextPackLoadID
        {
            get
            {
                nextPackLoadID++;
                return nextPackLoadID;
            }
        }

        /// <summary>
        ///     Returns a readonly list of <see cref="Pack"/>s
        /// </summary>
        public IEnumerable<Pack> PacksReadOnly => memberPackTable.Values;

        /// <summary>
        ///     Creates a new <see cref="Packmanager"/>. Called during world creation.
        /// </summary>
        /// <param name="world">The given <see cref="World"/></param>
        public Packmanager(World world) : base(world) => packmanager = this;

        /// <summary>
        ///     Adds a new <see cref="Pack"/> to the manager, and also save
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public bool AddPack(Pack pack)
        {
            if (!packs.Add(pack)) return false;
            
            foreach(Pawn member in pack.Members) memberPackTable.Add(member, pack);

            return true;
        }

        /// <summary>
        ///     Checks and returns the <see cref="Pack"/> a <paramref name="pawn"/> is in, if it is in one.
        /// </summary>
        /// <param name="pawn">The given <see cref="Pawn"/></param>
        /// <param name="pack">The found <see cref="Pack"/></param>
        /// <returns>True if a <see cref="Pack"/> was found, False otherwise</returns>
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
