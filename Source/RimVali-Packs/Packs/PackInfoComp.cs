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
        public bool canGetPackBroken = true;

        public bool CanGetPackBroken
        {
            get
            {
                return canGetPackBroken;
            }
        }
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
        public int PackLossProgression {
            get
            {
                return packLossProgression;
            }
            set
            {
                if (value < 0) value = 0;
                packLossProgression = value;
            }
        }

       
        public override void CompTickInterval(int delta)
        {

            if (!parent.Spawned || parent is not Pawn pawn) return;
            //check every 150 ticks
            if (!pawn.IsHashIntervalTick(150, delta)) return;
            int day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(parent.Map.Tile).x);
            bool isInPack = pawn.IsInPack(out Pack? pack);


            bool activelySeeingPackMate = isInPack
                                        && pack!=null 
                                        && pack.Members.Any(other => other != pawn 
                                                                    && (other.GetRoom() == pawn.GetRoom()
                                                                        || (other.Spawned
                                                                        && other.GetRoom() == null
                                                                        && other.Map == pawn.Map 
                                                                        && pawn.GetRoom() != null)));
            if (activelySeeingPackMate && lastSawPackmateOn != day)
            {
                lastSawPackmateOn = day;
            }

            if (lastUpdateDay == day)
            {
                return;
            }
            // if pawn saw a packmate or is not in a pack, fade over time
            if (lastSawPackmateOn == day || !isInPack)
            {
                packLossProgression--;
            }
            else
            {
                packLossProgression++;
            }
            DoPackBreakingChance(pawn, () => pack?.RemoveMember(pawn));
            


            lastUpdateDay = day;
            return;



        }

        private void DoPackBreakingChance(Pawn pawn, Action? onBroken = null)
        {
            if (Rand.Chance(PackLossProgression / 30) && !pawn.story.traits.HasTrait(RVFFA_Defs.RVFFA_PackBroken))
            {
                onBroken?.Invoke();
                pawn.story.traits.GainTrait(new Trait(RVFFA_Defs.RVFFA_PackBroken, forced: true), true);
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
