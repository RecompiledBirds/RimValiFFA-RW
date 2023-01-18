using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW
{
    public class RVFFA_ModSettings : ModSettings
    {
        private bool packsActive;
        private bool nexusActive;

        public RVFFA_ModSettings()
        {
            this.packsActive = true;
            this.nexusActive = true;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref packsActive, nameof(packsActive));
            Scribe_Values.Look(ref nexusActive, nameof(nexusActive));
        }
    }
}
