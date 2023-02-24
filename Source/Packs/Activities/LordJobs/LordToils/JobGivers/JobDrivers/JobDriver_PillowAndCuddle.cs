using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;

namespace RimValiFFARW.Packs.Activities.LordJobs.LordToils.JobGivers.JobDrivers
{
    public class JobDriver_PillowAndCuddle : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true; 

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil toil = new Toil()
            {
                activeSkill = () => SkillDefOf.Social,
                socialMode = RandomSocialMode.SuperActive,
                tickAction = () => { }
            };

            toil.AddFailCondition(() => 
            {
                if (!pawn.IsInPack()) return true;

                return false; 
            });

            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }
    }
}
