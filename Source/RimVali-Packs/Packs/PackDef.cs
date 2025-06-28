using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs;

/// <summary>
///     In RimVali, the Avali can form <see cref="Pack"/>s, and these may have a set of different attributes, buffs and debuffs.
///     To realize this, this <see cref="Def"/> class describes those attributes.
/// </summary>
public class PackDef : Def
{
    public List<HediffDef> memberHediffs = new List<HediffDef>();
    public Type packWorkerType = typeof(PackWorker);

    public int minSizeToSustain = 2;

    public int minSizeToCreate = 3;

    public int maxSize = 5;

    public int minGroupOpinionNeededCreation = 60;

    public int minGroupOpinionNeededSustain = 40;

    public bool unique = false;

    /// <summary>
    ///     The maximum size of a <see cref="Pack"/>
    /// </summary>
    public int MaxSize => maxSize;

    /// <summary>
    ///     The minimum size to sustain a <see cref="Pack"/>
    /// </summary>
    public int MinSizeToSustain => minSizeToSustain;

    /// <summary>
    ///     The minimum size to create a <see cref="Pack"/>
    /// </summary>
    public int MinSizeToCreate => minSizeToCreate;

    /// <summary>
    ///     Creates a new object of type <see cref="PackWorker"/> with the type as stated in the def file at <see cref="packWorkerType"/>
    /// </summary>
    public PackWorker GetNewPackWorker => (PackWorker)packWorkerType.GetConstructor([typeof(PackDef)]).Invoke([this]);
    public List<string> tags = [];
    public override IEnumerable<string> ConfigErrors()
    {   
        foreach (string error in base.ConfigErrors()) 
        {
            yield return error;
        }

        if (!(packWorkerType.IsSubclassOf(typeof(PackWorker)) || packWorkerType == typeof(PackWorker)))
        {
            yield return $"The PackDef {defName ?? "<missing defname>"} has no valid {nameof(packWorkerType)}! The given type must inherit from {typeof(PackWorker).Name}!";
        }

        if (minSizeToSustain > maxSize)
        {
            yield return $"The PackDef {defName ?? "<missing defname>"} has an invalid {nameof(minSizeToSustain)}! It must be bigger or equal {nameof(maxSize)}!";
        }

        if (maxSize <= 0)
        {
            yield return $"The PackDef {defName ?? "<missing defname>"} has an invalid {nameof(maxSize)}! It must be bigger or equal to 1!";
        }
    }
}
