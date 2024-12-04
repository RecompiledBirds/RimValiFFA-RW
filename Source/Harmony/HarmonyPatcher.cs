using HarmonyLib;
using RimWorld;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Source.Harmony
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            HarmonyLib.Harmony harmony = new("RimvaliFFA.Patches");
            RVCLog.Log("Running RimValiFFA Patches");
            harmony.Patch(AccessTools.Method(typeof(BedUtility), nameof(BedUtility.WillingToShareBed)),postfix: new HarmonyMethod(typeof(BedUtilityPatch),nameof(BedUtilityPatch.Postfix)));
            RVCLog.Log("RVFFA Patches completed.");
        }



    }

}
