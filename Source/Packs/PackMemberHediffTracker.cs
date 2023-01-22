using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    /// <summary>
    ///     This helper class tracks the status of <see cref="Hediff"/>s as they are applied to, and removed from <see cref="Pawn"/>s
    /// </summary>
    public class PackMemberHediffTracker : IExposable
    {
        private Pawn member;
        private HashSet<Hediff> packHediffs = new HashSet<Hediff>();

        /// <summary>
        ///     Returns the tracked <see cref="Pawn"/>
        /// </summary>
        public Pawn Member => member;

        /// <summary>
        ///     Creates a new <see cref="PackMemberHediffTracker"/> for a given <paramref name="member"/>
        /// </summary>
        /// <param name="member">The given <see cref="Pawn"/></param>
        public PackMemberHediffTracker(Pawn member) => this.member = member;

        /// <summary>
        ///     Empty constructor for use in Loading
        /// </summary>
        public PackMemberHediffTracker() { }

        /// <summary>
        ///     The <see cref="IEnumerable{T}"/> of added <see cref="Hediff"/>s
        /// </summary>
        public IEnumerable<Hediff> Hediffs => packHediffs;

        public bool AddHediff(Hediff hediff) => packHediffs.Add(hediff); //TODO: Check that they are correctly applied

        public bool RemoveHediff(Hediff hediff) => packHediffs.Remove(hediff);

        public bool HasHediffOfDef(HediffDef def) => packHediffs.Any(hediff => hediff.def == def);

        public void ExposeData()
        {
            Scribe_References.Look(ref member, nameof(member));
            Scribe_Collections.Look(ref packHediffs, nameof(packHediffs), LookMode.Reference);
        }
    }
}
