using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public class PackWorker
    {
        private readonly PackDef def;

        public PackWorker(PackDef def)
        {
            this.def = def;
        }

        public virtual void NotifyMemberRemoved(Pawn member) { }
        public virtual void NotifyMemberAdded(Pawn member) { }
        
        public virtual bool PawnCanJoinPack(Pawn pawn, bool quietError)
        {

            if (pawn == null)
            {
                MessageOf(pawn, "RVFFA_PackWorker_DoesNotExist".Translate(), quietError);
                return false;
            }

            if (pawn.Dead)
            {
                MessageOf(pawn, MakeCanNotJoinReasonStringForPawn(pawn, "RVFFA_PackWorker_SubjectIsDead"), quietError);
                return false;
            }

            if (pawn.NonHumanlikeOrWildMan())
            {
                MessageOf(pawn, MakeCanNotJoinReasonStringForPawn(pawn, "RVFFA_PackWorker_SubjectIsWild"), quietError);
                return false;
            }

            if(pawn.IsInPack())
            {
                MessageOf(pawn, MakeCanNotJoinReasonStringForPawn(pawn, "RVFFA_PackWorker_SubjectIsInPackAlready"), quietError);
                return false;
            }

            return true;
        }

        public virtual bool MemberCanLeave(Pawn member, Pack pack) => true;

        public virtual bool MemberShouldLeave(Pawn member, Pack pack)
        {
            //TODO: If pack really hates member
            //TODO: If member is last member in pack
            return false;
        }

        public virtual bool CanPawnsMakePack(IEnumerable<Pawn> pawns) => true;

        public virtual bool MemberHasHediffsAlready(PackMemberHediffTracker tracker) => def.memberHediffs.Any(def => !tracker.HasHediffOfDef(def));

        public virtual IEnumerable<Hediff> ApplyMissingMemberHediffs(PackMemberHediffTracker tracker)
        {
            foreach (HediffDef hediffDef in def.memberHediffs.Where(def => !tracker.HasHediffOfDef(def)))
            {
                yield return tracker.Member.health.AddHediff(hediffDef);
            }
        }

        public virtual void RemoveMemberHediffs(Pawn member, PackMemberHediffTracker tracker)
        {
            foreach (Hediff hediff in tracker.Hediffs)
            {
                member.health.RemoveHediff(hediff);
                tracker.RemoveHediff(hediff);
            }
        }

        private static void MessageOf(Pawn lookTarget, string message, bool doNot)
        {
            if (doNot) return;
            Messages.Message(message, lookTarget, MessageTypeDefOf.RejectInput, false);
        }

        protected string MakeCanNotJoinReasonStringForPawn(Pawn pawn, string reasonTranslationKey) => $"RVFFA_PackWorker_CanNotJoin".Translate(pawn.NameShortColored, $"{reasonTranslationKey.Translate(pawn.ProSubj())}");
    }
}
