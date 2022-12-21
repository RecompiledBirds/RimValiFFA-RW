using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public class PackMemberHediffTracker : IExposable
    {
        private Pawn member;
        private HashSet<Hediff> packHediffs = new HashSet<Hediff>();

        public Pawn Member => member;

        public PackMemberHediffTracker(Pawn member)
        {
            this.member = member;
        }

        public IEnumerable<Hediff> Hediffs => packHediffs;

        public bool AddHediff(Hediff hediff) => packHediffs.Add(hediff);

        public bool RemoveHediff(Hediff hediff) => packHediffs.Remove(hediff);

        public bool HasHediffOfDef(HediffDef def) => packHediffs.Any(hediff => hediff.def == def);

        public void ExposeData()
        {
            Scribe_References.Look(ref member, nameof(member));
            Scribe_Collections.Look(ref packHediffs, nameof(packHediffs), LookMode.Reference);
        }
    }
}
