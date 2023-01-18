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
                if (PowerNet.powerComps.Any(x => x.parent.def == NexusDefOf.RVFFA_AvaliNexus && x.PowerOn))
                    return base.PowerOn;
                return false;
            }
            set
            {
                if (PowerNet.powerComps.Any(x => x.parent.def == NexusDefOf.RVFFA_AvaliNexus && x.PowerOn))
                {
                    base.PowerOn = value;
                }
                base.PowerOn = false;
            }
        }
        
    }
}
