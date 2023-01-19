using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimValiFFARW.Nexus
{
    public class WreckedAreaGen : GenStep
    {
        public override int SeedPart => Find.World.ConstantRandSeed;
        public override void Generate(Map map, GenStepParams parms)
        {
            BaseGen.globalSettings.map = map;
            GenerateRects();
            foreach (CellRect rect in rooms)
            {
                ResolveParams resolve = default(ResolveParams);
                resolve.rect = rect;
                BaseGen.symbolStack.Push("RVFFA_WreckedShipWalls", resolve);
                BaseGen.symbolStack.Push("RVFFA_ClearArea", resolve);
            }
            foreach(CellRect rect in rooms)
            {
                ResolveParams resolve = default(ResolveParams);
                resolve.rect = rect;
                BaseGen.symbolStack.Push("doors", resolve);
            }
        }
        private List<CellRect> rooms = new List<CellRect>();
        private void GenerateRects()
        {
            Map map = BaseGen.globalSettings.map;
            CellRect center = CellRect.SingleCell(map.Center);
            CellRect baseRoom = new CellRect(center.minX - 3, center.minZ - 3, 5, 5);
            rooms.Add(baseRoom);
            int roomCount = Rand.Range(10, 20);
            for(int i =0; i<roomCount; i++)
            {
                CellRect root = rooms.RandomElement();
                IntVec3 pos =GetDoorPos(root);
                CellRect newRoom = GenRoom(pos);
                bool overlaps=CheckRoom(newRoom);
                int attempts =0;

                while (overlaps && attempts < 5)
                {
                    root = rooms.RandomElement();
                    pos=GetDoorPos(root);
                    newRoom = GenRoom(pos);
                    if (CheckRoom(newRoom))
                    {
                        attempts++;
                    }
                    else
                    {
                        overlaps = false;
                        attempts = 0;
                    }
                }

                if (attempts == 0)
                {
                    rooms.Add(newRoom);
                }
               
            }

            CellRect mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
            rooms.RemoveAll(x => !x.FullyContainedWithin(mapRect));
        }
        private IntVec3 GetDoorPos(CellRect rect)
        {
            return rect.EdgeCells.Where(x => !rect.Corners.Contains(x)).RandomElement();
        }
        private CellRect GenRoom(IntVec3 linkPos)
        {
            int sizeX = Rand.Range(10, 25);
            int sizeZ = Rand.Range(10, 25);
            CellRect rect = new CellRect(linkPos.x, linkPos.z, sizeX, sizeZ);
            return rect.ContractedBy(1);
        }

        private bool CheckRoom(CellRect rect)
        {
            return rooms.Any(x => x.Overlaps(rect));
        }
    }
}
