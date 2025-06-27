using RimValiFFARW.Packs;
using RimWorld;
using RVCRestructured;
using RVCRestructured.RVR.Harmony;
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
    public class PackInfoContainer : IExposable
    {
        private int packLossProgression;
        private int lastSawPackmateOn;
        private int lastUpdateDay;
        public void TriggerUpdate()
        {
            packLossProgression = 0;
            lastSawPackmateOn = -1;
            lastUpdateDay = -1;
        }
        public int PackLossProgression
        {
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


        public void CompTickInterval(int delta, Pawn pawn)
        {
            if (!pawn.Spawned) return;
            //check every X ticks
            if (!pawn.IsHashIntervalTick(1000, delta)) return;
            if (pawn.health.hediffSet.HasHediff(RVFFA_Defs.RVFFA_PackReplacement))
            {
                packLossProgression = 0;
                return;
            }
            int day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(pawn.Map.Tile).x);
            if (lastSawPackmateOn == day) return;

            if (pawn.story.traits.HasTrait(RVFFA_Defs.RVFFA_PackBroken) || (pawn.IsInPack(out Pack? pack) && pack.Members.Any(otherPawn => otherPawn.Spawned && otherPawn != pawn && ((otherPawn.GetRoom() == pawn.GetRoom()) || pawn.Map == otherPawn.Map))))
            {
                PackLossProgression--;
                //If we ticked pack loss today, roll it back 1 extra point.
                if (lastUpdateDay == day) PackLossProgression--;
                lastSawPackmateOn = day;
                return;
            }
            if (lastUpdateDay == day) return;
            DoPackBreakingChance(pawn, () => pack?.RemoveMember(pawn));
            packLossProgression++;
            lastUpdateDay = day;
            return;
        }

        /// <summary>
        /// Rolls <see cref="Rand.Chance(this.PackLossProgression/30)"/>, and if the pawn is not packbroken + roll passes, pack-breaks pawn and executes a given action.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="onBroken"></param>
        private void DoPackBreakingChance(Pawn pawn, Action? onBroken = null)
        {
            if (!pawn.CanJoinAPack()) return;
            if (!Rand.Chance(PackLossProgression / 30))
            {
                return;
            }
            onBroken?.Invoke();
            pawn.story.traits.GainTrait(new Trait(RVFFA_Defs.RVFFA_PackBroken, forced: true), true);
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref lastUpdateDay, nameof(lastUpdateDay));
            Scribe_Values.Look(ref packLossProgression, nameof(packLossProgression));
            Scribe_Values.Look(ref lastSawPackmateOn, nameof(lastSawPackmateOn));
        }
    }
    public class PackInfoComp : ThingComp
    {

        private PackInfoContainer packContainer = new();
        public PackInfoContainer PackInfoContainer
        {
            get
            {
                return packContainer;
            }
        }
        public override void CompTickInterval(int delta)
        {
            if (!parent.Spawned || parent is not Pawn pawn) return;
            if (ModsConfig.BiotechActive && !pawn.genes.GenesListForReading.Any(x=>x is PackGene)) return;
            packContainer.CompTickInterval(delta, pawn);
        }
     

        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref packContainer, nameof(packContainer));
            base.PostExposeData();
        }
    }
}
