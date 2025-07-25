﻿using RimWorld;
using RVCRestructured;
using Verse;

namespace RimValiFFARW.Packs;

public class PackDataDef : Def
{
    public ThoughtDef? packLossThought;
    public ThoughtDef? sleptAloneThought;
    public ThoughtDef? sleptNotAloneThought;
    public ThoughtDef? sleptWithPackThought;

    public List<string> packDefsFromMod = [];
    public List<PackDef> allowedPackDefs = [];
    public List<string> packDefsWithTags = [];

    public List<TraitPackPrevention> traitsThatPreventPacking = [];
    public List<HediffPackPrevention> hediffsThatPreventPacking = [];
    public override void ResolveReferences()
    {
        foreach (string id in packDefsFromMod)
        {
            ModContentPack? mod = LoadedModManager.RunningModsListForReading.FirstOrFallback(x => x?.PackageId == id, null);
            if (mod == null)
            {
                VineLog.Warn($"{id} is not a loaded mod, but we tried to get packdefs from it!");
                continue;
            }
            allowedPackDefs.AddRange((IEnumerable<PackDef>)mod.AllDefs.Where(x => x is PackDef def && !allowedPackDefs.Contains(def)));

        }

        foreach (string tag in packDefsWithTags)
        {

            allowedPackDefs.AddRange(DefDatabase<PackDef>.AllDefsListForReading.Where(x => x.tags.Contains(tag) && !allowedPackDefs.Contains(x)));

        }
    }

}