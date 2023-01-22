using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RimValiFFARW.Packs
{
    public class PackInspectionWindow : ITab
    {
        public readonly Vector2 WinSize = new Vector2(700, 400);

        public override bool IsVisible => base.IsVisible;

        public PackInspectionWindow()
        {
            size = WinSize;
            labelKey = "RVFFA_PackInspectionWindow_LabelKey";
        }

        protected override void FillTab()
        {
            
        }
    }
}
