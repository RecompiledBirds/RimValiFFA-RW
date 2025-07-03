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
        public PackDef packDef=RVFFA_PackDefs.RVFFA_StarterPack;
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
            Rect scenPartRect = listing.GetScenPartRect(this, RowHeight*3);
         
            Widgets.Label(new Rect(scenPartRect.x, scenPartRect.y + RowHeight, scenPartRect.width, RowHeight), "RVFFA_Scen_MinimumPawnsToCreate".Translate(packDef.MinSizeToCreate.Named("MinSizeToCreate")));
            Widgets.Label(new Rect(scenPartRect.x, scenPartRect.y + (RowHeight * 2), scenPartRect.width, RowHeight), "RVFFA_Scen_MinimumPawnsToSustain".Translate(packDef.MinSizeToSustain.Named("MinSizeToSustain")));
            if (!Widgets.ButtonText(new Rect(scenPartRect.x,scenPartRect.y,scenPartRect.width, RowHeight), packDef.LabelCap))
            {

                return;
            }
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
