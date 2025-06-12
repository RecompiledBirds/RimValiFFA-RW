using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimValiFFARW
{
    public class RVFFA_Mod : Mod
    {
        public RVFFA_ModSettings Settings
        {
            get
            {
                return GetSettings<RVFFA_ModSettings>();
            }
        }
        public RVFFA_Mod(ModContentPack content) : base(content)
        {
           
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
        }
    }
}
