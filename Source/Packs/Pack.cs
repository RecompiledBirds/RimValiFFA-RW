using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Utils;
using RimWorld;
using RVCRestructured;
using UnityEngine;
using Verse;

namespace RimValiFFARW.Packs
{
    /// <summary>
    ///     Class that attempts to reflect the structure of a Pack of Avali.
    ///     Stores information regarding a pack, as well as a <see cref="PackWorker"/>, who handles the calculations and actions.
    /// </summary>
    public class Pack : IExposable, ILoadReferenceable
    {
        private PackWorker worker;

        private PackDef def;
        private HashSet<Pawn> members = new HashSet<Pawn>();
        private Dictionary<Pawn, PackMemberHediffTracker> memberHediffDic = new Dictionary<Pawn, PackMemberHediffTracker>();

        private List<Pawn> workingListPawn = new List<Pawn>();
        private List<PackMemberHediffTracker> workingListTracker = new List<PackMemberHediffTracker>();

        private string loadID;

        /// <summary>
        ///     The internal <see cref="PackDef"/>
        /// </summary>
        public PackDef Def => def;

        /// <summary>
        ///     The maximum size of a pack
        /// </summary>
        public int MaxSize => def.MaxSize;

        /// <summary>
        ///     The <see cref="Pawn"/>s that make up a pack
        /// </summary>
        public HashSet<Pawn> Members => members;

        /// <summary>
        ///     The internal <see cref="PackWorker"/> of a <see cref="Pack"/>
        /// </summary>
        public PackWorker Worker => worker ?? (worker = def.GetNewPackWorker);

        /// <summary>
        ///     A packs name
        ///     TODO: Fully implement the Pack names
        /// </summary>
        public string Name => def.LabelCap;

        /// <summary>
        ///     A packs name Color
        ///     TODO: Fully implement the Pack name colors
        /// </summary>
        public Color NameColor => Color.blue + Color.red;

        /// <summary>
        ///     A <see cref="Pack"/>s <see cref="Name"/> colored using <see cref="NameColor"/>
        /// </summary>
        public TaggedString NameColored
        {
            get 
            { 
                if (Name != null)
                {
                    return Name.Colorize(NameColor);
                }
                return def.LabelCap;
            }
        }

        /// <summary>
        ///     Private Pack creation function. To be called by <see cref="TryMakeNewPackFromPawns(PackDef, IEnumerable{Pawn}, bool, out Pack)"/>
        /// </summary>
        /// <param name="def">The <see cref="PackDef"/> used to make a Pack.</param>
        /// <param name="pawns">The <see cref="Pawn"/>s with which a <see cref="Pack"/> is initialized</param>
        private Pack(PackDef def, IEnumerable<Pawn> pawns) 
        {
            this.def = def;
            worker = def.GetNewPackWorker;
            loadID = "RimValiPack_" + Packmanager.GetLastActivePackmanager.NextPackLoadID;

            members.AddRange(pawns);

            foreach(Pawn pawn in pawns)
            {
                memberHediffDic.Add(pawn, new PackMemberHediffTracker(pawn));
            }
        }

        /// <summary>
        ///     ONLY TO BE CALLED WHILE LOADING
        ///     Creates an empty class to be populated during loading
        /// </summary>
        public Pack() { }

        /// <summary>
        ///     Creates and calls a <see cref="PackWorker"/> of a given <see cref="PackDef"/> to see if the given <paramref name="pawns"/>
        ///     can create a <see cref="Pack"/> or not
        /// </summary>
        /// <param name="def">The given <see cref="PackDef"/></param>
        /// <param name="pawns">The given <see cref="IEnumerable{T}"/> of <see cref="Pawn"/>s</param>
        /// <returns>If the <paramref name="pawns"/> can create a <see cref="Pack"/></returns>
        public static bool CanPawnsMakePack(PackDef def, IEnumerable<Pawn> pawns, bool quietError, out string reason)
        {
            reason = null;
            return def?.GetNewPackWorker.CanPawnsMakePack(pawns, def, quietError, out reason) ?? false;
        }

        /// <summary>
        ///     Attempts to make a <see cref="Pack"/>, checking if the given parameters are impossible to make a <see cref="Pack"/> with
        /// </summary>
        /// <param name="def">The given <see cref="PackDef"/></param>
        /// <param name="pawns">The given <see cref="IEnumerable{T}"/> of <see cref="Pawn"/>s</param>
        /// <param name="quietError">If the function should throw errors</param>
        /// <param name="pack">The created <see cref="Pack"/> or Null if the process failed</param>
        /// <returns>True if a pack was formed, false otherwise</returns>
        public static bool TryMakeNewPackFromPawns(PackDef def, IEnumerable<Pawn> pawns, bool quietError, out Pack pack)
        {
            pack = null;

            PackWorker packWorker = def?.GetNewPackWorker;
            if (!def.GetNewPackWorker.CanPawnsMakePack(pawns, def, quietError, out string _)) return false;
            if (pawns.Any(pawn => !packWorker.PawnCanJoinPack(pawns, pawn, quietError, out string _))) return false;

            pack = new Pack(def, pawns);
            pack.ApplyHediffsToPackMembers();

            foreach (Pawn pawn in pawns)
            {
                pack.Worker.NotifyMemberAdded(pawn);
            }
            
            return true;
        }

        /// <summary>
        ///     Removes a given <paramref name="member"/> from the <see cref="Pack"/>
        /// </summary>
        /// <param name="member">The member to be removed</param>
        /// <returns>True if successful, false if a member couldn't be removed for any reason.</returns>
        public bool RemoveMember(Pawn member)
        {
            if (!worker.MemberCanLeave(member, this)) return false;
            if (!members.Remove(member)) return false;

            worker.RemoveMemberHediffs(memberHediffDic[member]);
            worker.NotifyMemberRemoved(member);
            return true;
        }

        /// <summary>
        ///     Attempts to add a given <paramref name="member"/> to the <see cref="Pack"/>
        /// </summary>
        /// <param name="member">The given member</param>
        /// <param name="quietError">If the function should throw errors</param>
        /// <returns>True if a member could be added, false otherwise.</returns>
        public bool AddMember(Pawn member, bool quietError = true)
        {
            if (!worker.PawnCanJoinPack(members, member, quietError, out string _)) return false;
            if (!members.Add(member)) return false;

            worker.NotifyMemberAdded(member); 
            ApplyHeddifsToMember(member);
            return true;
        }

        /// <summary>
        ///     Adds all available <see cref="Hediff"/>s to every member
        /// </summary>
        public void ApplyHediffsToPackMembers()
        {
            foreach(Pawn member in Members.Where(member => !worker.MemberHasHediffsAlready(memberHediffDic[member])))
            {
                ApplyHeddifsToMember(member);
            }
        }

        /// <summary>
        ///     Adds all available <see cref="Hediff"/>s to a singular member
        /// </summary>
        /// <param name="member">The given member</param>
        private void ApplyHeddifsToMember(Pawn member)
        {
            foreach (Hediff hediff in worker.ApplyMissingMemberHediffs(memberHediffDic[member]))
            {
                memberHediffDic[member].AddHediff(hediff);
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, nameof(def));
            Scribe_Values.Look(ref loadID, nameof(loadID));
            Scribe_Collections.Look(ref members, nameof(members), LookMode.Reference);
            Scribe_Collections.Look(ref memberHediffDic, nameof(memberHediffDic), LookMode.Reference, LookMode.Deep, ref workingListPawn, ref workingListTracker);
        }

        /// <returns>Hopefully a working loadID</returns>
        public string GetUniqueLoadID() => loadID;
    }
}
