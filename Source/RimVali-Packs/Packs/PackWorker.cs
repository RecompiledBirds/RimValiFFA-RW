using HarmonyLib;
using RimWorld;
using RVCRestructured;
using RVCRestructured.RVR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
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
        /// <param name="existingPawns">The <see cref="Pawn"/>s that make up the <see cref="Pack"/> that the <paramref name="pawn"/> is trying to join</param>
        /// <param name="pawn">The given <see cref="Pawn"/></param>
        /// <param name="quietError">If the function should stay quiet about errors</param>
        /// <param name="reason">The reason as to why a <paramref name="pawn"/> can't join</param>
        /// <returns>True if a <see cref="Pawn"/> can join a <see cref="Pack"/>, false otherwise</returns>
        public virtual bool PawnCanJoinPack(IEnumerable<Pawn> existingPawns, Pawn pawn, bool ignoreIsInPack, bool quietError, [NotNullWhen(false)] out string? reason)
        {
            double avgOpinionOfMember = EvaluateAverageOpinionForPawn(pawn, existingPawns);
            reason = null;

            if (pawn == null)
            {
                reason = "RVFFA_PackWorker_DoesNotExist".Translate();
                MessageOf(pawn, reason, quietError);
                return false;
            }
            if (pawn.Dead) reason = MakeCanNotJoinReasonStringForPawn(pawn, "RVFFA_PackWorker_SubjectIsDead");
            if (pawn.NonHumanlikeOrWildMan()) reason = MakeCanNotJoinReasonStringForPawn(pawn, "RVFFA_PackWorker_SubjectIsWild");
            if (pawn.IsInPack() && !ignoreIsInPack) reason = MakeCanNotJoinReasonStringForPawn(pawn, "RVFFA_PackWorker_SubjectIsInPackAlready");
            if (def.minGroupOpinionNeededCreation > avgOpinionOfMember) reason = "RVFFA_PackCreationWindow_PawnIsNotLikedEnoughByGoup".Translate(pawn.NameShortColored, avgOpinionOfMember.ToString("0.##"), def.minGroupOpinionNeededCreation.ToString("0.##"));
            if (!pawn.IsAvali()) reason = "RVFFA_PackWorker_SubjectIsNotAvali".Translate();

            if (reason != null)
            {
                MessageOf(pawn, reason, quietError);
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
                RVCLog.Log($"Tried to check if {member.NameShortColored} could be removed from pack with loadID: {pack.GetUniqueLoadID()}, but the pawn isn't even inside that pack");
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
        public virtual bool MemberShouldLeave(Pawn member, Pack pack,/* [NotNullWhen(true)] */out string? reason)
        {
            reason = null;
            if (member is null)
            {
                reason = "RVFFA_PackWorker_PawnWentMissing".Translate();
                return true;
            }

            double avgOpinionOfMember = EvaluateAverageOpinionForPawn(member, pack.Members);
            if (def.minGroupOpinionNeededSustain > avgOpinionOfMember)
            {
                reason = "RVFFA_PackWorker_AverageOpinionOfMemberTooLow".Translate(member.ProSubj(), avgOpinionOfMember);
                return true;
            }

            if (pack.Members.Count == 1)
            {
                reason = "RVFFA_PackWorker_LastInPack".Translate(member.ProSubj());
                return true;
            }

            if (pack.Members.Count < def.minSizeToSustain)
            {
                reason = "RVFFA_PackWorker_PackTooSmall".Translate(pack.NameColored);
                return true;
            }

            if (member.Dead)
            {
                reason = "RVFFA_PackWorker_SubjectIsDead".Translate(member.ProSubj());
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Checks if a group of <see cref="Pawn"/>s can be used to create a Pack
        /// </summary>
        /// <param name="pawns">The given <see cref="IEnumerable{T}"/> of <see cref="Pawn"/>s</param>
        /// <returns>True if the <paramref name="pawns"/> can make a new <see cref="Pack"/>, false otherwise.</returns>
        public virtual bool CanPawnsMakePack(IEnumerable<Pawn> pawns, PackDef def, bool quietError, [NotNullWhen(false)]  out string? reason)
        {
            reason = null;
            if (pawns.Count() < def.MinSizeToCreate)
            {
                reason = "RVFFA_PackWorker_CountLowerThanMin".Translate(pawns.Count(), def.MinSizeToCreate);
                MessageOf(new LookTargets(pawns), reason, quietError);
                return false;
            }

            if (EvaluateAverageOpinionForEveryPawn(pawns) < def.minGroupOpinionNeededCreation)
            {
                reason = "RVFFA_PackWorker_SubjectGroupOpinionTooLow".Translate(pawns.SkipLast(1).Join(pawn => pawn.NameShortColored), pawns.Last());
                MessageOf(new LookTargets(pawns), reason, quietError);
                return false;
            }

            if (pawns.Count() > def.MaxSize)
            {
                reason = "RVFFA_PackWorker_CountHigherThanMax".Translate(pawns.Count(), def.MaxSize);
                MessageOf(new LookTargets(pawns), reason, quietError);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Checks if a member had Hediffs added already
        /// </summary>
        /// <param name="tracker">The given <see cref="PackMemberHediffTracker"/></param>
        /// <returns>True if the <paramref name="tracker"/> had already added hediffs</returns>
        public virtual bool MemberHasHediffsAlready(PackMemberHediffTracker tracker) => def.memberHediffs.Any(def => tracker.HasHediffOfDef(def));

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
            List<Hediff> workingList = tracker.Hediffs.ToList();
            foreach (Hediff hediff in workingList)
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
        private static void MessageOf(LookTargets lookTarget, string message, bool doNot)
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

        /// <param name="member">the given <see cref="Pawn"/></param>
        /// <param name="pack">the given <see cref="Pack"/></param>
        /// <returns>The average opinion of a <paramref name="member"/> in a <paramref name="pack"/></returns>
        public virtual double EvaluateAverageOpinionForPawn(Pawn member, IEnumerable<Pawn> members)
        {
            if (!members.Where(pawn => pawn != member).Any()) return 0d;
            return members.Where(pawn => pawn != member).Average(otherMembers => otherMembers.relations.OpinionOf(member));
        }

        /// <param name="pack">the given <see cref="Pack"/></param>
        /// <returns>Calculates the Average of all Pawn opinion averages inside the given <paramref name="pack"/></returns>
        public virtual double EvaluateAverageOpinionForEveryPawn(Pack pack) => EvaluateAverageOpinionForEveryPawn(pack.Members);

        /// <param name="pawns">the given <see cref="IEnumerable{T}"/> of <see cref="Pawn"/>s</param>
        /// <returns>Calculates the Average of all Pawn opinion averages inside the given <paramref name="pawns"/> <see cref="IEnumerable{T}"/></returns>
        public virtual double EvaluateAverageOpinionForEveryPawn(IEnumerable<Pawn> pawns)
        {
            double total = 0;
            double count = 0;

            if (pawns.Count() < 2) return 100;
            if (pawns.Any(pawn => pawn == null)) return 100;

            foreach (Pawn member in pawns)
            {
                total += pawns.Where(otherMember => otherMember != member).Average(otherMember => otherMember.relations.OpinionOf(member));
                count++;
            }

            return total / count;
        }

        /// <summary>
        ///     Checks if a pack is still valid by making a bunch of comparisons
        /// </summary>
        /// <param name="pack">the given <see cref="Pack"/></param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/>s that contain reasons for why a <see cref="Pack"/> should be disbanded</returns>
        public virtual IEnumerable<string> IsPackStillValid(Pack pack)
        {
            bool testSingleMembers = true;
            if (pack.Members.Count < pack.Def.MinSizeToSustain)
            {
                testSingleMembers = false;
                yield return "RVFFA_PackWorker_PackTooSmall".Translate(pack.NameColored);
            }
            if (pack.Worker.EvaluateAverageOpinionForEveryPawn(pack) < pack.Def.minGroupOpinionNeededSustain)
            {
                testSingleMembers = false;
                yield return "RVFFA_PackWorker_PackAverageOpinionTooLow".Translate(pack.NameColored);
            }

            if (!testSingleMembers) yield break;
            foreach (Pawn member in pack.Members)
            {
                pack.Worker.MemberShouldLeave(member, pack, out string? reason);
                if (reason == null) continue;

                Messages.Message("RVFFA_PackWorker_MemberLeavesBecause".Translate(member?.NameShortColored ?? "???", pack.NameColored, reason), MessageTypeDefOf.NegativeEvent, true);
                CleanUpMember(member, pack);
                break;
            }
        }

        /// <summary>
        ///     Disbands a <see cref="Pack"/>, cleaning up every <see cref="Pawn"/> within
        /// </summary>
        /// <param name="pack">the given <see cref="Pack"/></param>
        public virtual void Disband(Pack pack)
        {
            Packmanager.GetLastActivePackmanager.RemovePack(pack);
            List<Pawn> membersToRemove = pack.Members.ToList();

            while (membersToRemove.Count > 0)
            {
                Pawn member = membersToRemove[0];
                CleanUpMember(member, pack);
                membersToRemove.RemoveAt(0);
            }
        }

        /// <summary>
        ///     Cleans up a <paramref name="member"/>, deleting every <see cref="Hediff"/> through <see cref="Pack.RemoveMember(Pawn)"/>
        /// </summary>
        /// <param name="member">The given <see cref="Pawn"/></param>
        /// <param name="pack">The given <see cref="Pack"/></param>
        public virtual void CleanUpMember(Pawn? member, Pack pack)
        {
            if(member == null) return;
            pack.RemoveMember(member);
            Packmanager.GetLastActivePackmanager.RemoveMemberRelation(member);
        }
    }
}
