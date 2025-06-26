using Verse;

namespace RimValiFFARW.Packs
{
    public class PawnDeathPatches
    {
        public static void KillPostfix(DamageInfo? dinfo, Pawn __instance, Hediff? exactCulprit = null)
        {
            if (!__instance.IsInPack(out Pack? pack)) return;
            foreach (Pawn packmate in pack.Members)
            {

                if (packmate == __instance) continue;
                if (!packmate.TryGetComp(out PackInfoComp packInfoComp)) continue;
                if (packmate.Map == __instance.Map && __instance.GetRoom() == packmate.GetRoom())
                {
                    packInfoComp.PackLossProgression += 10;
                    continue;
                }
                packInfoComp.PackLossProgression += 5;
            }

        }
    }
}
