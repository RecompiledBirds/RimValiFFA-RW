using RimWorld;
using RimWorld.Planet;
using RVCRestructured;
using Verse;

namespace Rimvali_Terraforming
{
    public class TerraformingCompProperties : CompProperties
    {
        public TerraformingCompProperties()
        {
            compClass = typeof(TerraformingComp);
        }
    }
    public class TerraformingComp : ThingComp
    {
        List<SurfaceTile> tilesForTerraforming = [];
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            tilesForTerraforming = [.. Find.WorldGrid.Tiles.Where(x =>x.PrimaryBiome == BiomeDefOf.IceSheet || x.PrimaryBiome == BiomeDefOf.SeaIce)];
            VineLog.Log("Spawned terraformer");
            base.PostSpawnSetup(respawningAfterLoad);
        }
        public override void CompTickInterval(int delta)
        {
            if (!parent.IsHashIntervalTick(100, delta)) return;
           
            void updateTask()
            {
                Tile? last = null;
                List<SurfaceTile> newTiles = [];
                for (int i = 0; i < tilesForTerraforming.Count; i++)
                {

                    Tile tile = tilesForTerraforming[i];
                    List<PlanetTile> neighbors = [];
                    Find.WorldGrid.GetTileNeighbors(tile.tile, neighbors);
                    bool skippedAny = false;
                    float count = 6;
                    foreach (PlanetTile n in neighbors.InRandomOrder())
                    {
                        if (n.Tile.PrimaryBiome == BiomeDefOf.IceSheet || n.Tile.PrimaryBiome == BiomeDefOf.SeaIce) continue;
                        if (n.Tile.Layer.Def != PlanetLayerDefOf.Surface) continue;
                        if (n.Tile is not SurfaceTile surfaceTile) continue;
                        if (Rand.Chance(count / 7.0f))
                        {
                            skippedAny = true;
                            continue;
                        }
                        count--;
                        newTiles.Add(surfaceTile);
                        if (surfaceTile.Biomes.Contains(BiomeDefOf.Ocean))
                        {

                            surfaceTile.PrimaryBiome = BiomeDefOf.SeaIce;

                            continue;
                        }
                        surfaceTile.PrimaryBiome = BiomeDefOf.IceSheet;
                        Settlement? settlement = Find.WorldObjects.SettlementAt(n);
                        if (settlement == null) continue;
                        if (settlement.Faction == Faction.OfPlayer) continue;
                        if (settlement.Faction.def.allowedArrivalTemperatureRange.min > BiomeDefOf.SeaIce.constantOutdoorTemperature) continue;
                        settlement.Destroy();
                        //WorldObject abandoned = SiteMaker.MakeSite(null, n, settlement.Faction, true, WorldObjectDefOf.AbandonedSettlement);
                        //Find.WorldObjects.Add(abandoned);
                    }
                    if (skippedAny) newTiles.Add((SurfaceTile)tile);
                    last = tile;
                }

                tilesForTerraforming = newTiles;
                if (!WorldRendererUtility.WorldSelected)
                    last?.Layer.SetAllLayersDirty();
            }
            Task.Run(updateTask);
           
            //    last?.Layer.RegenerateAllLayersNow();
            base.CompTickInterval(delta);
        }
    }
}
