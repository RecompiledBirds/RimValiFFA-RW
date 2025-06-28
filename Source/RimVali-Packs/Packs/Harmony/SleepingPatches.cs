using RimWorld;
using Verse;

namespace RimValiFFARW.Packs
{
    public class SleepingPatches
    {
        public static void SleepingPostfix(Pawn actor, Building_Bed bed)
        {
            if (!actor.IsInPack(out Pack? pack) || !actor.TryGetPackInfoContainer(out PackInfoContainer? packInfoContainer)) return;
            if (packInfoContainer.PackXMLInfo == null) return;
            if(packInfoContainer.PackXMLInfo.sleptAloneThought!=null)
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(packInfoContainer.PackXMLInfo.sleptAloneThought);
            if (packInfoContainer.PackXMLInfo.sleptNotAloneThought != null)
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(packInfoContainer.PackXMLInfo.sleptNotAloneThought);
            if (packInfoContainer.PackXMLInfo.sleptWithPackThought != null)
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(packInfoContainer.PackXMLInfo.sleptWithPackThought);
            if (bed == null) return;
            Room room = bed.GetRoom(RegionType.Set_All);
            if (room == null) return;

            int roomScore = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
            List<Pawn> pawns = [];

            foreach (Building_Bed foundBed in room.ContainedBeds)
            {
                pawns.AddRange(foundBed.CurOccupants.Where(other => other != actor));
            }
            if (pawns.Count == 0)
            {
                if (packInfoContainer.PackXMLInfo.sleptAloneThought != null)
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(packInfoContainer.PackXMLInfo.sleptAloneThought, roomScore), null);
                return;
            }
            if (pawns.Any(other => (Packmanager.GetLastActivePackmanager.TryGetPackForPawn(other, out Pack? otherPack) && otherPack == pack)))
            {
                if (packInfoContainer.PackXMLInfo.sleptWithPackThought != null)
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(packInfoContainer.PackXMLInfo.sleptWithPackThought, roomScore), null);
                return;
            }
            if (packInfoContainer.PackXMLInfo.sleptNotAloneThought != null)
                actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(packInfoContainer.PackXMLInfo.sleptNotAloneThought, roomScore), null);
            return;
        }
    }
}
