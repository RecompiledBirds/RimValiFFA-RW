using RimWorld;
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
    ///     This class implements all the active functionality that a <see cref="PackDef"/> can not
    /// </summary>
    public class PackWorker
    {
        private readonly PackDef def;

        public PackWorker(PackDef def) => this.def = def;

        /// <summary>
        ///     Executes code that should run when a <paramref name="member"/> is removed from a <see cref="Pack"/>
        /// </summary>
        /// <param name="member">The removed <see cref="Pawn"/></param>
        public virtual void NotifyMemberRemoved(Pawn member) { }

        /// <summary>
        ///     Executes code that should run when a <paramref name="member"/> is added to a <see cref="Pack"/>
        /// </summary>
        /// <param name="member">The added <see cref="Pawn"/></param>
        public virtual void NotifyMemberAdded(Pawn member) { }
        
        /// <summary>
        ///     Checks if a <paramref name="pawn"/> can join a <see cref="Pack"/>.
        ///     Throws errors if they can't, describing why.
        /// </summary>
        /// <param name="pawn">The given <see cref="Pawn"/></param>
        /// <param name="quietError">If the function should stay quiet about errors</param>
        /// <returns>True if a <see cref="Pawn"/> can join a <see cref="Pack"/>, false otherwise</returns>
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

        /// <summary>
        ///     Checks to see if a <paramref name="member"/> can leave a <paramref name="pack"/>
        /// </summary>
        /// <param name="member"></param>
        /// <param name="pack"></param>
        /// <returns>true if a member can leave, false otherwise.</returns>
        public virtual bool MemberCanLeave(Pawn member, Pack pack)
        {
            if (!pack.Members.Contains(member))
            {
                RVCLog.Log($"Tried to check if {member.Name.ToStringShort} could be removed from pack with loadID: {pack.GetUniqueLoadID()}, but the pawn isn't even inside that pack");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Checks if a <paramref name="member"/> should remain
        /// </summary>
        /// <param name="member">The given <see cref="Pawn"/></param>
        /// <param name="pack">The given <see cref="Pack"/></param>
        /// <returns></returns>
        public virtual bool MemberShouldLeave(Pawn member, Pack pack)
        {
            //TODO: If pack really hates member
            //TODO: If member is last member in pack
            //TODO: If number of members is lower than minimum, by definition
            return false;
        }

        /// <summary>
        ///     Checks if a group of <see cref="Pawn"/>s can be used to create a Pack
        /// </summary>
        /// <param name="pawns">The given <see cref="IEnumerable{T}"/> of <see cref="Pawn"/>s</param>
        /// <returns>True if the <paramref name="pawns"/> can make a new <see cref="Pack"/>, false otherwise.</returns>
        public virtual bool CanPawnsMakePack(IEnumerable<Pawn> pawns)
        {
            if (pawns.Any(pawn => pawn.IsInPack())) return false;
            return true;
        }

        /// <summary>
        ///     Checks if a member had Hediffs added already
        /// </summary>
        /// <param name="tracker">The given <see cref="PackMemberHediffTracker"/></param>
        /// <returns>True if the <paramref name="tracker"/> had already added hediffs</returns>
        public virtual bool MemberHasHediffsAlready(PackMemberHediffTracker tracker) => def.memberHediffs.Any(def => !tracker.HasHediffOfDef(def));

        /// <summary>
        ///     Runs through all <see cref="Hediff"/>s that were missing from the given <see cref="PackMemberHediffTracker"/> and adds them
        /// </summary>
        /// <param name="tracker">The given <see cref="PackMemberHediffTracker"/></param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="Hediff"/>s that were added during this operation</returns>
        public virtual IEnumerable<Hediff> ApplyMissingMemberHediffs(PackMemberHediffTracker tracker)
        {
            foreach (HediffDef hediffDef in def.memberHediffs.Where(def => !tracker.HasHediffOfDef(def)))
            {
                yield return tracker.Member.health.AddHediff(hediffDef);
            }
        }

        /// <summary>
        ///     Removes all <see cref="Hediff"/>s from a <paramref name="tracker"/>s <see cref="PackMemberHediffTracker.Member"/>
        /// </summary>
        /// <param name="tracker">The given <see cref="PackMemberHediffTracker"/></param>
        public virtual void RemoveMemberHediffs(PackMemberHediffTracker tracker)
        {
            foreach (Hediff hediff in tracker.Hediffs)
            {
                tracker.Member.health.RemoveHediff(hediff);
                tracker.RemoveHediff(hediff);
            }
        }

        /// <summary>
        ///     Notifies the player about a reason, why a <see cref="Pawn"/> couldn't be added to a <see cref="Pack"/>
        /// </summary>
        /// <param name="lookTarget">The given <see cref="Pawn"/></param>
        /// <param name="message">The message the player is notified with</param>
        /// <param name="doNot">If it shouldn't</param>
        private static void MessageOf(Pawn lookTarget, string message, bool doNot)
        {
            if (doNot) return;
            Messages.Message(message, lookTarget, MessageTypeDefOf.RejectInput, false);
        }

        /// <summary>
        ///     Makes a customized string that explains why a <see cref="Pawn"/> can't join a <see cref="Pack"/>
        /// </summary>
        /// <param name="pawn">The given <see cref="Pawn"/></param>
        /// <param name="reasonTranslationKey">The given explaination</param>
        /// <returns></returns>
        protected string MakeCanNotJoinReasonStringForPawn(Pawn pawn, string reasonTranslationKey) => $"RVFFA_PackWorker_CanNotJoin".Translate(pawn.NameShortColored, $"{reasonTranslationKey.Translate(pawn.ProSubj())}");
    }
}
