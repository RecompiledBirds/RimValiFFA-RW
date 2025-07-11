using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.StoryTellers
{
    public class RVFFA_Storyteller : StorytellerCompProperties
    {
        public bool dislikesSlaves;

        public bool dislikesNonAvali;

        public int ratioDislikedPawns;

        [AllowNull]
        public SimpleCurve acceptFractionByDaysPassedCurve;

        [AllowNull]
        public SimpleCurve acceptPercentFactorPerProgressScoreCurve;

        [AllowNull]
        public SimpleCurve acceptPercentFactorPerThreatPointsCurve;
        public IntRange incidentsRange = new(1,2);

        public float minSpacingDays;

        public float offDays;
        public float onDays;

        public float minAcceptablePawnThreshold;
        
        public RVFFA_Storyteller()
        {
            compClass = typeof(RVFFA_StoryTellerComp);
        }
        public RVFFA_Storyteller(Type compClass)
        {
            this.compClass = typeof(RVFFA_StoryTellerComp);
        }
    }
    public class RVFFA_StoryTellerComp : StorytellerComp
    {
        private int index;
        private int dislikedPawns;
        private int likedPawns;

        public bool HasDislikedPawns
        {
            get
            {
                return dislikedPawns > 0;
            }
        }

        public bool HasDislikedPawnsWithThres
        {
            get
            {
                return HasDislikedPawns && Props.minAcceptablePawnThreshold<RatioDislikedLiked;
            }
        }


        public int RatioLikedDisliked
        {
            get
            {
                return likedPawns / dislikedPawns;
            }
        }

        public int RatioDislikedLiked
        {
            get
            {
                return dislikedPawns / likedPawns;
            }
        }


        private Random? random;
        private Random Random
        {
            get
            {
                if(random==null)random= new Random(Find.World.ConstantRandSeed);
                return random;
            }
            
        }
        public override void Initialize()
        {
            base.Initialize();
        }

        private RVFFA_Storyteller Props
        {
            get
            {

                return (RVFFA_Storyteller)props;
            }
        }

        bool started = false;

        //public RVFFA_StoryTellerComp()
        //{
        //    random = new Random(Find.World.ConstantRandSeed);
        //}

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            if (!started)
            {
               
                index = Find.Storyteller.storytellerComps.IndexOf(this);
                started=true;
            }
            Map map = Find.Maps.Find(x => x.Tile == target.Tile);
            UpdatePawnCount(map);

            float num = 1;

            StorytellerData storytellerData = StorytellerData.GetStorytellerData;

            if(Props.acceptFractionByDaysPassedCurve != null)
            {
                num *= Props.acceptFractionByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat);
            }

            if (Props.acceptPercentFactorPerThreatPointsCurve != null)
            {
                num *= Props.acceptPercentFactorPerThreatPointsCurve.Evaluate(
                    StorytellerUtility.DefaultThreatPointsNow(target));
            }

            if (Props.acceptPercentFactorPerProgressScoreCurve != null)
            {
                num *= Props.acceptPercentFactorPerProgressScoreCurve.Evaluate(
                    StorytellerUtility.GetProgressScore(target));
            }

            int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, index, Props.minDaysPassed, Props.onDays, Props.offDays, Props.minSpacingDays, Props.incidentsRange.min, Props.incidentsRange.max, num);

            for(int i =0; i < incCount; i++)
            {
                switch (storytellerData.StorytellerState)
                {
                    case StorytellerState.Angered:

                        FiringIncident? incidentAngry = GenAngry(target);
                        if (incidentAngry != null) yield return incidentAngry;
                        break;

                    case StorytellerState.Neutral:
                        FiringIncident? incidentNeutral = GenNeutral(target);
                        if (incidentNeutral != null) yield return incidentNeutral;
                        break;

                    case StorytellerState.Friendly:
                        FiringIncident? incidentFriend = GenFriend(target);
                        if (incidentFriend != null) yield return incidentFriend;
                        break;
                }
            }

            UpdateState(storytellerData);
        }

        public int GetDislikedPawns(Map map)
        {
            IEnumerable<Pawn> pawns = map.mapPawns.FreeColonistsAndPrisonersSpawned;

            int result = 0;

            if (Props.dislikesNonAvali)
            {
                int count = pawns.Where(x => x.def != RVFFA_Defs.RVFFA_Avali).Count() ;
                result += count;
            }
            if (Props.dislikesSlaves)
            {
                int count = map.mapPawns.PrisonersOfColonyCount;
                result += count;
            }
            return result;
        }
        public int GetLikedPawns(Map map)
        {
            IEnumerable<Pawn> pawns = map.mapPawns.FreeColonistsAndPrisonersSpawned;

            int result = 0;

            if (Props.dislikesNonAvali)
            {
                int count = pawns.Where(x => x.def == RVFFA_Defs.RVFFA_Avali).Count();
                result += count;
            }
            if (Props.dislikesSlaves)
            {
                int count = map.mapPawns.FreeColonistsCount;
                result += count;
            }
            return result;
        }
        public void UpdatePawnCount(Map map)
        {
            if (map == null)
                return;
            dislikedPawns = GetDislikedPawns(map);
            likedPawns = GetLikedPawns(map);
        }

        private FiringIncident? GenAngry(IIncidentTarget targ)
        {
            IncidentParms parms = this.GenerateParms(IncidentCategoryDefOf.ThreatBig, targ);
            List<IncidentDef> incidents = base.UsableIncidentsInCategory(IncidentCategoryDefOf.ThreatBig,parms).ToList();
            incidents.AddRange(UsableIncidentsInCategory(IncidentCategoryDefOf.DiseaseHuman, parms));
       //     incidents.AddRange(UsableIncidentsInCategory(IncidentCategoryDefOf.DiseaseAnimal, parms));
            incidents.AddRange(UsableIncidentsInCategory(IncidentCategoryDefOf.DeepDrillInfestation, parms));
            IncidentDef incident = incidents.RandomElement();
            if (incidents.Count == 0) return null;
            if (Rand.Chance(0.5f))
            {
                parms.points *= parms.points * RatioLikedDisliked;
            }

            return new FiringIncident(incident, this, parms);
        }

        private FiringIncident? GenNeutral(IIncidentTarget targ)
        {
            IncidentParms parms = this.GenerateParms(IncidentCategoryDefOf.ThreatBig, targ);
            List<IncidentDef> incidents = base.UsableIncidentsInCategory(IncidentCategoryDefOf.ThreatSmall, parms).ToList();
            incidents.AddRange(UsableIncidentsInCategory(IncidentCategoryDefOf.Misc, parms));

            if (incidents.Count == 0) return null;
            IncidentDef incident = incidents.RandomElement();
            if (Rand.Chance(0.2f))
            {
                parms.points *= parms.points;
            }

            return new FiringIncident(incident, this, parms);
        }

        private FiringIncident? GenFriend(IIncidentTarget targ)
        {
            IncidentParms parms = this.GenerateParms(IncidentCategoryDefOf.ThreatBig, targ);
            List<IncidentDef> incidents = base.UsableIncidentsInCategory(IncidentCategoryDefOf.ThreatSmall, parms).ToList();
      //      incidents.AddRange(UsableIncidentsInCategory(IncidentCategoryDefOf.FactionArrival, parms));
      //      incidents.AddRange(UsableIncidentsInCategory(IncidentCategoryDefOf.OrbitalVisitor, parms));
            incidents.AddRange(UsableIncidentsInCategory(IncidentCategoryDefOf.GiveQuest, parms));
            incidents.AddRange(UsableIncidentsInCategory(IncidentCategoryDefOf.Misc, parms));
            incidents.Add(IncidentDefOf.ShipChunkDrop);

            IncidentDef incident = incidents.RandomElement();
            if (Rand.Chance(0.2f))
            {
                parms.points *= parms.points * RatioDislikedLiked;
            }

            return new FiringIncident(incident, this, parms);
        }

        private void UpdateState(StorytellerData data)
        {
            if (!data.HasUpdatedInXDays(5))
                return;

            if(HasDislikedPawnsWithThres && data.StorytellerState>=0 && Rand.Chance(0.1f))
            {
                data.UpdateDays();
                data.StorytellerState--;
                return;
            }

            if (data.StorytellerState == 0 && Rand.Chance(0.2f))
            {
                data.StorytellerState += Random.Next(-1, HasDislikedPawnsWithThres ? 1 : 0);
                data.UpdateDays();
                return;
            }

            if(data.StorytellerState==StorytellerState.Angered && Rand.Chance(0.2f))
            {
                data.StorytellerState += Random.Next(0, HasDislikedPawnsWithThres ? 1 : 0);
                data.UpdateDays();
                return;
            }

            if (data.StorytellerState == StorytellerState.Friendly && Rand.Chance(0.5f))
            {
                data.StorytellerState -= Random.Next(HasDislikedPawnsWithThres ? 0:-1, 1);
                data.UpdateDays();
                return;
            }
        }
    }
}
