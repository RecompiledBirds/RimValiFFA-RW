using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Symbols
{
    public class Resolver_WreckedWalls : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            ThingDef stuff = ThingDefOf.Steel;
            foreach(IntVec3 c in rp.rect.EdgeCells)
            {
                if(c.InBounds(map) && Rand.Range(0, 100) > 15)
                {
                    ThingDef wallDef = ThingDefOf.Wall;
                    Thing wall = ThingMaker.MakeThing(wallDef, stuff);
                    GenSpawn.Spawn(wall, c, map);
                }
            }
        }
    }
}
