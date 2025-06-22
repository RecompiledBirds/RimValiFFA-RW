using RimValiFFARW.Packs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public class PackInfoCompProps : CompProperties
    {
        public PackInfoCompProps()
        {
            this.compClass = typeof(PackInfoComp);
        }
    }
    public class PackInfoComp : ThingComp
    {
        private int packLossProgression;
        private int lastSawPackmateOn;
        private int lastUpdateDay;
        public int PackLossProgression => packLossProgression;
        public override void CompTickInterval(int delta)
        {
            //Wait for 150 ticks before doing any checks
            if (delta == 150) return;
            if (!parent.Spawned || parent is not Pawn pawn) return;
            
            int day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(parent.Map.Tile).x);
            if (lastSawPackmateOn == day)
            {
                if (lastUpdateDay==day || PackLossProgression == 0) return;
                packLossProgression--;

                lastUpdateDay = day;
                return;
            }
            if (!Packmanager.GetLastActivePackmanager.TryGetPackForPawn(pawn, out Pack pack))
            {
                //allows pack loss to fade over time if a pack is disbanded, without being too easy to cheese.
                lastSawPackmateOn = day;
                return;
            };


            
            bool activelySeeingPackMate = pack.Members.Any(other => other != pawn && other.GetRoom() == pawn.GetRoom());
            if (activelySeeingPackMate && lastSawPackmateOn!=day)
            {
                lastSawPackmateOn = day;
            }
            if (lastSawPackmateOn != day && lastUpdateDay!=day)
            {
                lastUpdateDay = day;
                packLossProgression++;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref lastUpdateDay, nameof(lastUpdateDay));
            Scribe_Values.Look(ref packLossProgression, nameof(packLossProgression));
            Scribe_Values.Look(ref lastSawPackmateOn, nameof(lastSawPackmateOn));
            base.PostExposeData();
        }
    }
}
