using HarmonyLib;
using Mono.Math;
using RimWorld;
using RimWorld.Planet;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private const int packCheckTickDelay = 2000;

        [AllowNull]
        private static Packmanager packmanager;

        private Dictionary<Pawn, Pack> memberPackTable = new Dictionary<Pawn, Pack>();
        private HashSet<Pack> packs = new HashSet<Pack>();
        private List<Pack> packsList = new List<Pack>();

        private int lastWorkedOnPackListIndex = 0;
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
        public IEnumerable<Pack> PacksReadOnly => memberPackTable.Values.Distinct();

        /// <summary>
        ///     Creates a new <see cref="Packmanager"/>. Called during world creation.
        /// </summary>
        /// <param name="world">The given <see cref="World"/></param>
        public Packmanager(World world) : base(world) => packmanager = this;
        
        public override void FinalizeInit(bool fromLoad)
        {
            packsList = packs.ToList();
            base.FinalizeInit(fromLoad);
        }

        public override void WorldComponentTick()
        {
            if (Find.TickManager.TicksGame % packCheckTickDelay != 0) return;
            if (packsList.Count == 0) return;

            Pack pack = packsList[lastWorkedOnPackListIndex];
            if (pack == null)
            {
                packsList.RemoveAt(lastWorkedOnPackListIndex);
                return;
            }
            lastWorkedOnPackListIndex++;

            if (lastWorkedOnPackListIndex >= packsList.Count) lastWorkedOnPackListIndex = 0;

            bool anyReasonsExist = false;
            foreach(string reason in pack.Worker.IsPackStillValid(pack))
            {
                anyReasonsExist = true;
                Messages.Message(reason, MessageTypeDefOf.NegativeEvent);
            }

            if (!anyReasonsExist) return;
            if (PackInspectionWindow.GetCurrentPackInspectionWindow?.CurrentPack == pack) 
                PackInspectionWindow.GetCurrentPackInspectionWindow.CurrentPack = null;
            pack.Worker.Disband(pack);

            base.WorldComponentTick();
        }

        /// <summary>
        ///     Adds a new <see cref="Pack"/> to the manager, and also saves the <see cref="Pawn"/> to <see cref="Pack"/> relation in <see cref="memberPackTable"/>
        /// </summary>
        /// <param name="pack"></param>
        /// <returns>if the <paramref name="pack"/> could be added successfully</returns>
        public bool AddPack(Pack pack)
        {
            if (!packs.Add(pack)) return false;
            foreach(Pawn member in pack.Members) memberPackTable.Add(member, pack);
            packsList.Add(pack);
            return true;
        }

        /// <summary>
        ///     Removes a <see cref="Pack"/> from the manager, and also removes the <see cref="Pawn"/> to <see cref="Pack"/> relation in <see cref="memberPackTable"/>
        /// </summary>
        /// <param name="pack"></param>
        /// <returns>if the <paramref name="pack"/> could be removed successfully</returns>
        public bool RemovePack(Pack pack)
        {
            if (!packs.Remove(pack)) return false;
            memberPackTable.RemoveAll(kvp => kvp.Key == null || pack.Members.Contains(kvp.Key));
            packsList.Remove(pack);
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
            //Log.Message($"Dictionary: {memberPackTable.Join(kvp => $"{kvp.Value.GetUniqueLoadID()}", ", ")}");
            return memberPackTable.TryGetValue(pawn, out pack);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextPackLoadID, nameof(nextPackLoadID));
            Scribe_Values.Look(ref lastWorkedOnPackListIndex, nameof(lastWorkedOnPackListIndex));

            Scribe_Collections.Look(ref packs, nameof(packs), LookMode.Deep);
            Scribe_Collections.Look(ref memberPackTable, nameof(memberPackTable), LookMode.Reference, LookMode.Reference);
        }

        public void AddMemberRelation(Pawn Member, Pack pack)
        {
            memberPackTable.Add(Member, pack);
        }

        public void RemoveMemberRelation(Pawn member)
        {
            if (member != null && memberPackTable.ContainsKey(member))
            {
                memberPackTable.Remove(member);
            }
        }
    }
}
