using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Nexus
{
    public class NexusManager : WorldComponent
    {
        private Map map;
        public NexusManager(World world) : base(world)
        {
        }

        private void DoNexusDropOnMap()
        {
            if (map == null)
                return;
            NexusUtils.DoAirdrop(map);
        }

      
    }
}
