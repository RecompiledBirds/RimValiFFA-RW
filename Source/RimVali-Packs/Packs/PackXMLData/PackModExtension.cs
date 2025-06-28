using RimValiFFARW.Packs;
using RimWorld;
using System.Diagnostics.CodeAnalysis;
using Verse;

namespace RimValiFFARW.Packs;
public class PackModExtension : DefModExtension
{
    [AllowNull]
    public PackDataDef packXMLInfo;
}
public class TraitPackPrevention
{
    [AllowNull]
    public TraitDef trait;
    [AllowNull]
    public string reasonKey;
}
public class HediffPackPrevention
{
    [AllowNull]
    public HediffDef hediff;
    [AllowNull]
    public string reasonKey;
}
