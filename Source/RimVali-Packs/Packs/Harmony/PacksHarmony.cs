using HarmonyLib;
using RimWorld;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            VineLog.Log($"{harmony.GetPatchedMethods().Count()} Packs patches completed.");
        }
    }
}
