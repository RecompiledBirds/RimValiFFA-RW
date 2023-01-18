using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;

namespace RimValiFFARW.Nexus
{
    public class NexusSiteWorker :SitePartWorker
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
            List<Thing> things = new List<Thing>();
            things.Add(NexusUtils.GenNexus());
            slate.Set("nexusReward",things, false);
            outExtraDescriptionRules.Add(new Rule_String("nexusConnection",GenLabel.ThingLabel(NexusDefOf.RVFFA_AvaliNexus,ThingDefOf.Steel)));
        }
    }
}
