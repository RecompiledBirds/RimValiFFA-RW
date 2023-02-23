using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs.Activities
{
    public class PackActivityManager : IExposable
    {
        private List<PackActivityRecordCollection> recordCollections = new List<PackActivityRecordCollection>();
        private List<PackActivityWorker> activeWorkers = new List<PackActivityWorker>();

        public void RegisterWorker(PackActivityWorker worker) => activeWorkers.Add(worker);

        public void UnregisterWorker(PackActivityWorker worker) => activeWorkers.Remove(worker);

        public void ExposeData()
        {
            Scribe_Collections.Look(ref activeWorkers, nameof(activeWorkers), LookMode.Deep);
            Scribe_Collections.Look(ref recordCollections, nameof(recordCollections), LookMode.Deep);
        }
    }
}
