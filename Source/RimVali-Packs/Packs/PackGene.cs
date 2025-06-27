using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
namespace RimValiFFARW.Packs;
public class PackGene : Gene
{
    private PackInfoContainer packInfoContainer = new();
    public PackInfoContainer PackInfoContainer
    {
        get
        {
            return packInfoContainer;
        }
    }
    public override void ExposeData()
    {
        Scribe_Deep.Look(ref packInfoContainer, nameof(packInfoContainer));
        base.ExposeData();
    }
}
