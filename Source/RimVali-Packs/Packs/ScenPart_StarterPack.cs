using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimValiFFARW.Packs
{
    public class ScenPart_StarterPack : ScenPart
    {
        [AllowNull]
        public PackDef packDef;
        public override void PostGameStart()
        {
            Map currentMap = Find.AnyPlayerHomeMap;
            Packmanager packmanager = Packmanager.GetLastActivePackmanager;

            if (!currentMap.IsPlayerHome) return;

            List<Pawn> colonists = [.. currentMap.mapPawns.FreeColonists.InRandomOrder().Where(pawn => !pawn.IsInPack() && pawn.IsPackable(packDef))];
            colonists.TruncateToLength(packDef.MaxSize);

            if (!Pack.TryMakeNewPackFromPawns(packDef, colonists, false, false, out Pack? pack)) return;

            packmanager.AddPack(pack);
            base.PostGameStart();
        }
        public override void ExposeData()
        {
            Scribe_Defs.Look(ref packDef, nameof(packDef));
            base.ExposeData();
        }
        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, RowHeight);
            if (!Widgets.ButtonText(scenPartRect, packDef.LabelCap)) return;
            List<FloatMenuOption> options = [];
            foreach (PackDef pDef in DefDatabase<PackDef>.AllDefsListForReading)
            {
                options.Add(new FloatMenuOption(pDef.LabelCap, delegate ()
                {
                    packDef = pDef;
                }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }
    }

}
