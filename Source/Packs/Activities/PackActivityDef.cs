using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs.Activities
{
    public class PackActivityDef : Def
    {
        public List<PackDef> packDefBlackList = new List<PackDef>();
        public List<PackDef> packDefWhiteList = new List<PackDef>();
        public GatheringDef gatheringDef;
        public Type worker;
        public int minDaysInBetween = 30;
        public int minPawns = 4;

        public PackActivityWorker GetNewPackActivityWorker(Pack pack) => worker.GetConstructor(new Type[] { typeof(PackActivityDef), typeof(Pack) }).Invoke(new object[] { this, pack }) as PackActivityWorker;

        public override IEnumerable<string> ConfigErrors()
        {
            if(worker == null) 
            {
                yield return $"The PackActivityWorker for {defName ?? "missing defName"} is missing!";
                yield break;
            }

            if (!worker.IsSubclassOf(typeof(PackActivityWorker)))
            {
                yield return $"The PackActivityWorker {worker.FullName} for {defName ?? "missing defName"} needs to inherit from {typeof(PackActivityWorker)}!";
            }
        }
    }
}
