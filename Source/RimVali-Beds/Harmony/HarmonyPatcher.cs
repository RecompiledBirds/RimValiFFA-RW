using HarmonyLib;
using RimWorld;
using RVCRestructured;
using Verse;

namespace RimValiFFARW
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            Harmony harmony = new("RimvaliFFA.Bed.Patches");
            RVCLog.Log("Running RimValiFFA Bed Patches");
            harmony.Patch(AccessTools.Method(typeof(BedUtility), nameof(BedUtility.WillingToShareBed)), postfix: new HarmonyMethod(typeof(BedUtilityPatch), nameof(BedUtilityPatch.Postfix)));
            RVCLog.Log("RVFFA Patches completed.");
        }



    }

}
