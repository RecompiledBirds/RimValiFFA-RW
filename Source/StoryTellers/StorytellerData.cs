using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.StoryTellers
{
    public class StorytellerData : WorldComponent
    {
        public static StorytellerData GetStorytellerData
        {
            get
            {
                return Find.World.GetComponent<StorytellerData>();
            }
        }

        private StorytellerState state;
        private int dayLastUpdated;

        public bool HasUpdatedInXDays(int x)
        {
            return dayLastUpdated >= x+GenDate.DaysPassed;
        }

        public void UpdateDays()
        {
            dayLastUpdated = GenDate.DaysPassed;
        }

        public StorytellerState StorytellerState
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }
        public StorytellerData(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref state, nameof(state));
            Scribe_Values.Look(ref dayLastUpdated,nameof(dayLastUpdated));
        }
    }
}
