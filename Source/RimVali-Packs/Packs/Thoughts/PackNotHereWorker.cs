using RimWorld;
using Verse;

namespace RimValiFFARW.Packs
{
    public class PackNotHereWorker : ThoughtWorker
    {

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {

            if (!p.TryGetPackInfoContainer(out PackInfoContainer? container)) return ThoughtState.Inactive;
            int stage = container.PackLossProgression;

            if (stage >= 15)
            {
                return ThoughtState.ActiveAtStage(3);
            }
            if (stage >= 10)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            if (stage >= 6)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            if (stage >= 3)
            {
                return ThoughtState.ActiveAtStage(0);
            }

            return ThoughtState.Inactive;
        }
    }
}
