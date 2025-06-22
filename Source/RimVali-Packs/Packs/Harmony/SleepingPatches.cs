using RimWorld;
using Verse;

namespace RimValiFFARW.Packs
{
    public class SleepingPatches
    {
        public static void SleepingPostfix(Pawn actor, Building_Bed bed)
        {
            if (!Packmanager.GetLastActivePackmanager.TryGetPackForPawn(actor, out Pack pack)) return;
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(RVFFA_Defs.RVFFA_AvaliSleptAlone);
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(RVFFA_Defs.RVFFA_AvaliSleptNotAlone);
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(RVFFA_Defs.RVFFA_AvaliSleptWithPack);
            Room room = bed.GetRoom(RegionType.Set_All);
            if (room == null) return;

            int roomScore = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
            List<Pawn> pawns = [];
            foreach(Building_Bed foundBed in room.ContainedBeds)
            {
                pawns.AddRange(foundBed.CurOccupants.Where(other => other != actor));
            }
            if (pawns.Count==0)
            {
                actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RVFFA_Defs.RVFFA_AvaliSleptAlone,roomScore), null);
                return;
            }
            if(pawns.Any(other=>(Packmanager.GetLastActivePackmanager.TryGetPackForPawn(other,out Pack otherPack) && otherPack == pack)))
            {
                actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RVFFA_Defs.RVFFA_AvaliSleptWithPack, roomScore), null);
                return;
            }
            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(RVFFA_Defs.RVFFA_AvaliSleptNotAlone, roomScore), null);
            return;
        }
    }
}
