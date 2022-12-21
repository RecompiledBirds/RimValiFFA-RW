using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Utils;
using RimWorld;
using Verse;

namespace RimValiFFARW.Packs
{
    public class Pack : IExposable, ILoadReferenceable
    {
        private readonly PackWorker worker;

        private PackDef def;
        private HashSet<Pawn> members = new HashSet<Pawn>();
        private Dictionary<Pawn, PackMemberHediffTracker> memberHediffDic = new Dictionary<Pawn, PackMemberHediffTracker>();

        private string loadID;

        public PackDef Def => def;
        public int MaxSize => def.MaxSize;
        public HashSet<Pawn> Members => members;
        public PackWorker Worker => worker;

        private Pack(PackDef def, IEnumerable<Pawn> pawns)
        {
            this.def = def;

            members.AddRange(pawns);
            worker = def.GetNewPackWorker();

            foreach(Pawn pawn in pawns)
            {
                memberHediffDic.Add(pawn, new PackMemberHediffTracker(pawn));
            }
        }

        public Pack()
        {
            worker = def.GetNewPackWorker();
            loadID = loadID ?? "RimValiPack_" + Packmanager.GetLastActivePackmanager.NextPackLoadID;
        }

        public static bool CanPawnsMakePack(PackDef def, IEnumerable<Pawn> pawns) => def.GetNewPackWorker().CanPawnsMakePack(pawns);

        public static bool TryMakeNewPackFromPawns(PackDef def, IEnumerable<Pawn> pawns, bool quietError, out Pack pack)
        {
            pack = null;

            PackWorker packWorker = def?.GetNewPackWorker();
            if (pawns.Any(pawn => !packWorker.PawnCanJoinPack(pawn, quietError))) return false;

            pack = new Pack(def, pawns);
            pack.ApplyHediffsToPackMembers();

            foreach (Pawn pawn in pawns)
            {
                pack.Worker.NotifyMemberAdded(pawn);
            }
            
            return true;
        }

        public void RemoveMember(Pawn member)
        {
            if (!worker.MemberCanLeave(member, this)) return;
            if (!members.Remove(member)) return;

            worker.NotifyMemberRemoved(member);
        }

        public void AddMember(Pawn member, bool quietError = true)
        {
            if (!worker.PawnCanJoinPack(member, quietError)) return;
            if (!members.Add(member)) return;

            worker.NotifyMemberAdded(member); 
            ApplyHeddifsToMember(member);
        }

        public void ApplyHediffsToPackMembers()
        {
            foreach(Pawn member in Members.Where(member => !worker.MemberHasHediffsAlready(memberHediffDic[member])))
            {
                ApplyHeddifsToMember(member);
            }
        }

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
            Scribe_Collections.Look(ref memberHediffDic, nameof(memberHediffDic), LookMode.Reference, LookMode.Deep);
        }

        public string GetUniqueLoadID() => loadID;
    }
}
