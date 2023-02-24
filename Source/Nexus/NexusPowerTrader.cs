using RimWorld;
using System.Linq;
using Verse;

namespace RimValiFFARW.Nexus
{
    public class NexusPowerTraderProperties : CompProperties
    {
        public NexusPowerTraderProperties()
        {
            this.compClass=typeof(NexusPowerTrader);
        }
    }
    public class NexusPowerTrader : CompPowerTrader
    {
        public new bool PowerOn
        {
            get
            {
                if (PowerNet.powerComps.Any(x => x.parent.def == RVFFA_ThingDefOf.RVFFA_Nexus && x.PowerOn))
                    return base.PowerOn;
                return false;
            }
            set
            {
                if (PowerNet.powerComps.Any(x => x.parent.def == RVFFA_ThingDefOf.RVFFA_Nexus && x.PowerOn))
                {
                    base.PowerOn = value;
                }
                base.PowerOn = false;
            }
        }
        
    }
}
