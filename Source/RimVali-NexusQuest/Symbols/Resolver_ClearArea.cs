using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Symbols
{
    public class Resolver_ClearArea : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            CellRect rect = rp.rect.ContractedBy(1);
            foreach(IntVec3 pos in rect.Cells)
            {
               IEnumerable<Thing> things = map.thingGrid.ThingsAt(pos);
                foreach(Thing thing in things)
                {
                    thing.DeSpawn();
                }
            }
        }
    }
}
