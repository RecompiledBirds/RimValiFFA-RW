using HarmonyLib;
using RimWorld;
using RVCRestructured;
using Verse;

namespace RimValiFFARW.Packs
{
    [StaticConstructorOnStartup]
    public static class PacksHarmony
    {
        static PacksHarmony()
        {
            Harmony harmony = new Harmony("RimvaliFFA.Packs");
            VineLog.Log("[RimValiPacks Patches]: Doing patches!");
            harmony.Patch(AccessTools.Method(typeof(Toils_LayDown), "ApplyBedThoughts"), postfix: new HarmonyMethod(typeof(SleepingPatches), nameof(SleepingPatches.SleepingPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill)), postfix: new HarmonyMethod(typeof(PawnDeathPatches), nameof(PawnDeathPatches.KillPostfix)));
            harmony.Patch(AccessTools.Method(typeof(NegativeInteractionUtility), nameof(NegativeInteractionUtility.NegativeInteractionChanceFactor)), postfix: new HarmonyMethod(typeof(NegativeInteractionChancePatch), nameof(NegativeInteractionChancePatch.Postfix)));
            VineLog.Log($"{harmony.GetPatchedMethods().Count()} Packs patches completed.");
        }
    }
}
