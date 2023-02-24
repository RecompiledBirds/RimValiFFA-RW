using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using Verse;
using Verse.AI.Group;
using Verse.Noise;
using static System.Collections.Specialized.BitVector32;

namespace RimValiFFARW.Packs.Activities
{
    public abstract class PackActivityWorker : GatheringWorker, IExposable
    {
        protected PackActivityDef packActivitydef;
        protected Pack pack;

        public PackActivityWorker() 
        {
            packActivitydef = GetPackActivityDef();
        }

        public override bool CanExecute(Map map, Pawn organizer = null)
        {
            organizer = organizer ?? FindOrganizer(map);

            if (organizer == null) return false;
            if (packActivitydef is null) return false; 
            if (!GatheringsUtility.AcceptableGameConditionsToContinueGathering(map)) return false;
            if (GenLocalDate.HourInteger(map) < 4 || GenLocalDate.HourInteger(map) > 21) return false;
            if (map.dangerWatcher.DangerRating != StoryDanger.None) return false;
            if (pack.Members.Count < packActivitydef.minPawns) return false;
            if (pack.Members.Any(p => p.health.hediffSet.BleedRateTotal > 0f || p.Drafted)) return false;

            return true;
        }

        public abstract PackActivityDef GetPackActivityDef();

        public override bool TryExecute(Map map, Pawn organizer = null)
        {
            organizer = organizer ?? FindOrganizer(map);
            if (organizer == null) return false;
            if (!TryFindGatherSpot(organizer, out IntVec3 spot)) return false;

            LordMaker.MakeNewLord(organizer.Faction, CreateLordJob(spot, organizer), map, pack.Members);
            SendLetter(spot, organizer);
            return true;
        }

        protected override Pawn FindOrganizer(Map map)
        {
            if ((from pawn in map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer)
                 where IsValidForGathering(pawn)
                 select pawn).TryRandomElement(out Pawn result))
            {
                return result;
            }

            return null;
        }

        private bool IsValidForGathering(Pawn pawn)
        {
            Packmanager packmanager = Packmanager.GetLastActivePackmanager;
            if (!packmanager.TryGetPackForPawn(pawn, out Pack pack) || packActivitydef.packDefBlackList.Contains(pack.Def) || !(packActivitydef.packDefWhiteList.Contains(pack.Def) || packActivitydef.packDefWhiteList.Count == 0) || !pawn.RaceProps.Humanlike || pawn.InBed() || pawn.InMentalState || pawn.GetLord() != null || !GatheringsUtility.ShouldPawnKeepGathering(pawn, def) || pawn.Drafted) return false; 
            
            this.pack = pack;
            
            if (def.requiredTitleAny == null || def.requiredTitleAny.Count == 0) return true; 
            if (pawn.royalty != null)
            {
                return pawn.royalty.AllTitlesInEffectForReading.Any(title => def.requiredTitleAny.Contains(title.def));
            }

            return false;
        }

        protected override void SendLetter(IntVec3 spot, Pawn organizer)
        {
            Find.LetterStack.ReceiveLetter(def.letterTitle, def.letterText.Formatted(pack.NameColored), LetterDefOf.PositiveEvent, new TargetInfo(spot, organizer.Map));
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, nameof(def));
            Scribe_References.Look(ref pack, nameof(pack));
        }
    }
}
