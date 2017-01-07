using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace CaveBiome
{
    public class GenStep_CaveRoof : GenStep
    {
        public static int caveWellsNumber = 0;
        public static List<IntVec3> caveWellsPosition = null;

		public override void Generate(Map map)
		{
            if (map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Nothing to do in other biomes.
                return;
            }
            // Compute number of cave wells (5 for standard map 250x250, around 13 for bigest map 400x400).
            caveWellsNumber = Mathf.CeilToInt((map.Size.x * map.Size.z) / 12500);
            
			MapGenFloatGrid elevationGrid = MapGenerator.Elevation;
			foreach (IntVec3 cell in map.AllCells)
			{
			    Thing thing = map.edificeGrid.InnerArray[map.cellIndices.CellToIndex(cell)];
			    if (thing != null && thing.def.holdsRoof)
			    {
                    map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);
			    }
                else
                {
                    // Spawn cave roof holder.
                    GenSpawn.Spawn(Util_CaveBiome.CaveRoofDef, cell, map);
                }
			}

            // Update regions and rooms to be able to use the CanReachMapEdge function to find goo cave well spots.
            DeepProfiler.Start("RebuildAllRegionsBeforeCaveWells");
            map.regionAndRoomUpdater.Enabled = true;
            map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
            map.regionAndRoomUpdater.Enabled = false;
            DeepProfiler.End();

            // Get cave wells position.
            caveWellsPosition = GetCaveWellsPosition(map);

            // Spawn cave wells.
            // First cave well is always dry (to avoid starting thing scattering errors).
            SpawnDryCaveWellWithAnimalCorpsesAt(map, caveWellsPosition[0]);
            for (int caveWellIndex = 1; caveWellIndex < caveWellsNumber; caveWellIndex++)
            {
                if (Rand.Value < 0.5f)
                {
                    // Spawn aqueous cave well.
                    SpawnAqueousCaveWellAt(map, caveWellsPosition[caveWellIndex]);
                }
                else if (Rand.Value < 0.9f)
                {
                    // Spawn dry cave well + fallen animal corpses.
                    SpawnDryCaveWellWithAnimalCorpsesAt(map, caveWellsPosition[caveWellIndex]);
                }
                else
                {
                    // Spawn dry cave well + sacrificial stone.
                    SpawnDryCaveWellWithRitualStoneAt(map, caveWellsPosition[caveWellIndex]);
                }
            }

            // TODO: should correct null region error?
            // Update regions and rooms now that cave wells are spawned.
            DeepProfiler.Start("RebuildAllRegionsAfterCaveWells");
            map.regionAndRoomUpdater.Enabled = true;
            map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
            map.regionAndRoomUpdater.Enabled = false;
            DeepProfiler.End();
		}

        private static List<IntVec3> GetCaveWellsPosition(Map map)
        {
            float distanceFromCenter = 40f;

            List<IntVec3> positionsList = new List<IntVec3>();
            for (int caveWellIndex = 0; caveWellIndex < caveWellsNumber; caveWellIndex++)
            {
                Predicate<IntVec3> validator = delegate(IntVec3 cell)
                {
                    if (caveWellIndex == 0)
                    {
                        // First cave well must be near map center.
                        if (cell.InHorDistOf(map.Center, distanceFromCenter) == false)
                        {
                            return false;
                        }
                    }
                    // Check cave well is not too close from another one.
                    for (int i = 0; i < positionsList.Count; i++)
                    {
                        if (cell.InHorDistOf(positionsList[i], 40f))
                        {
                            return false;
                        }
                    }

                    // Check cave well is connected to map edge.
                    Room room = cell.GetRoom(map);
                    if (room != null
                        && room.TouchesMapEdge)
                    {
                        return true;
                    }
                    return false;
                };

                IntVec3 caveWellCell = IntVec3.Invalid;
                bool caveWellCellIsFound = CellFinderLoose.TryFindRandomNotEdgeCellWith(20, validator, map, out caveWellCell);
                if ((caveWellIndex == 0)
                    && (caveWellCellIsFound == false))
                {
                    // Sometimes, there is no cave touching map edge near the map center. Searh radius is thus greatly increased.
                    distanceFromCenter = 150f;
                    caveWellCellIsFound = CellFinderLoose.TryFindRandomNotEdgeCellWith(20, validator, map, out caveWellCell);
                }
                if (caveWellCellIsFound)
                {
                    positionsList.Add(caveWellCell);
                    if (caveWellIndex == 0)
                    {
                        // Found a good start point. Reuse it later.
                        MapGenerator.PlayerStartSpot = caveWellCell;
                    }
                }
                else
                {
                    CellFinderLoose.TryFindRandomNotEdgeCellWith(20, null, map, out caveWellCell);
                    positionsList.Add(caveWellCell);
                }
            }
            return positionsList;
        }

        private static void SpawnAqueousCaveWellAt(Map map, IntVec3 position)
        {
            // Spawn main hole.
            SetCellsInRadiusNoRoofNoRock(map, position, 10f);
            SpawnCaveWellOpening(map, position);
            SetCellsInRadiusTerrain(map, position, 10f, TerrainDefOf.Gravel);
            SetCellsInRadiusTerrain(map, position, 8f, TerrainDefOf.WaterShallow);

            // Spawn small additional holes.
            int smallHolesNumber = Rand.RangeInclusive(2, 5);
            for (int holeIndex = 0; holeIndex < smallHolesNumber; holeIndex++)
            {
                IntVec3 smallHolePosition = position + (7f * Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360))).ToIntVec3();
                SetCellsInRadiusNoRoofNoRock(map, smallHolePosition, 5f);
                SetCellsInRadiusTerrain(map, smallHolePosition, 3.2f, TerrainDefOf.WaterShallow);
                SetCellsInRadiusTerrain(map, smallHolePosition, 2.1f, TerrainDefOf.WaterDeep);
            }
            SetCellsInRadiusTerrain(map, position, 5.2f, TerrainDefOf.WaterDeep);
        }

        private static void SpawnCaveWellOpening(Map map, IntVec3 position)
        {
            Thing potentialCaveWell = position.GetFirstThing(map, Util_CaveBiome.CaveWellDef);
            if (potentialCaveWell == null)
            {
                GenSpawn.Spawn(Util_CaveBiome.CaveWellDef, position, map);
            }
            foreach (IntVec3 checkedCell in GenAdjFast.AdjacentCells8Way(position))
            {
                potentialCaveWell = checkedCell.GetFirstThing(map, Util_CaveBiome.CaveWellDef);
                if (potentialCaveWell == null)
                {
                    GenSpawn.Spawn(Util_CaveBiome.CaveWellDef, checkedCell, map);
                }
            }
        }

        private static void SpawnDryCaveWellWithAnimalCorpsesAt(Map map, IntVec3 position)
        {
            SpawnDryCaveWellAt(map, position);
            SpawnAnimalCorpsesMaker(map, position);
        }

        private static void SpawnDryCaveWellWithRitualStoneAt(Map map, IntVec3 position)
        {
            SpawnDryCaveWellAt(map, position);
            SpawnRitualStone(map, position);
        }

        private static void SpawnDryCaveWellAt(Map map, IntVec3 position)
        {
            // Spawn main hole.
            SetCellsInRadiusNoRoofNoRock(map, position, 10f);
            SpawnCaveWellOpening(map, position);
            SetCellsInRadiusTerrain(map, position, 10f, TerrainDefOf.Gravel);
            SetCellsInRadiusTerrain(map, position, 8f, TerrainDefOf.Soil);

            // Spawn small additional holes.
            int smallHolesNumber = Rand.RangeInclusive(2, 5);
            for (int holeIndex = 0; holeIndex < smallHolesNumber; holeIndex++)
            {
                IntVec3 smallHolePosition = position + (7f * Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360))).ToIntVec3();
                SetCellsInRadiusNoRoofNoRock(map, smallHolePosition, 5f);
                SetCellsInRadiusTerrain(map, smallHolePosition, 3.2f, TerrainDefOf.Soil);
                SetCellsInRadiusTerrain(map, smallHolePosition, 2.1f, TerrainDef.Named("SoilRich"));
            }
            SetCellsInRadiusTerrain(map, position, 6.5f, TerrainDef.Named("SoilRich"));
        }

        private static void SpawnAnimalCorpsesMaker(Map map, IntVec3 position)
        {
            Thing animalCorpsesGenerator = ThingMaker.MakeThing(Util_CaveBiome.AnimalCorpsesGeneratorDef);
            GenSpawn.Spawn(animalCorpsesGenerator, position, map);
        }

        private static void SpawnRitualStone(Map map, IntVec3 position)
        {
            // Set terrain.
            SetCellsInRadiusTerrain(map, position, 2.5f, TerrainDef.Named("TileSlate"));
            // Spawn ritual stone.
            Thing thing = ThingMaker.MakeThing(ThingDef.Named("Sarcophagus"), ThingDef.Named("BlocksSlate"));
            GenSpawn.Spawn(thing, position + new IntVec3(0, 0, -1), map);
            (thing as Building_Sarcophagus).GetStoreSettings().filter.SetDisallowAll();
            // Spawn offerings.
            thing = ThingMaker.MakeThing(ThingDef.Named("MeleeWeapon_Shiv"), ThingDef.Named("Jade"));
            GenSpawn.Spawn(thing, position + new IntVec3(0, 0, -1), map);
            thing = ThingMaker.MakeThing(ThingDefOf.HerbalMedicine);
            thing.stackCount = Rand.Range(5, 12);
            GenSpawn.Spawn(thing, position + new IntVec3(-1, 0, 0), map);
            thing = ThingMaker.MakeThing(ThingDefOf.Gold);
            thing.stackCount = Rand.Range(7, 25);
            GenSpawn.Spawn(thing, position + new IntVec3(1, 0, 0), map);
            thing = ThingMaker.MakeThing(ThingDef.Named("Campfire"));
            GenSpawn.Spawn(thing, position + new IntVec3(0, 0, 1), map, Rot4.South);
            // Spawn blood.
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, 2f, true))
            {
                if (cell.InBounds(map) == false)
                {
                    continue;
                }
                int bloodQuantity = Rand.Range(2, 5);
                for (int bloodFilthIndex = 0; bloodFilthIndex < bloodQuantity; bloodFilthIndex++)
                {
                    GenSpawn.Spawn(ThingDefOf.FilthBlood, cell, map);
                }
            }
            // Spawn torches.
            GenSpawn.Spawn(Util_CaveBiome.CrystalLampDef, position + new IntVec3(1, 0, 3), map);
            GenSpawn.Spawn(Util_CaveBiome.CrystalLampDef, position + new IntVec3(3, 0, 1), map);
            GenSpawn.Spawn(Util_CaveBiome.CrystalLampDef, position + new IntVec3(3, 0, -1), map);
            GenSpawn.Spawn(Util_CaveBiome.CrystalLampDef, position + new IntVec3(1, 0, -3), map);
            GenSpawn.Spawn(Util_CaveBiome.CrystalLampDef, position + new IntVec3(-1, 0, -3), map);
            GenSpawn.Spawn(Util_CaveBiome.CrystalLampDef, position + new IntVec3(-3, 0, -1), map);
            GenSpawn.Spawn(Util_CaveBiome.CrystalLampDef, position + new IntVec3(-3, 0, 1), map);
            GenSpawn.Spawn(Util_CaveBiome.CrystalLampDef, position + new IntVec3(-1, 0, 3), map);
            // Spawn corpses generator.
            if (Rand.Value < 0.5f)
            {
                Thing villagerCorpsesGenerator = ThingMaker.MakeThing(Util_CaveBiome.VillagerCorpsesGeneratorDef);
                GenSpawn.Spawn(villagerCorpsesGenerator, position, map);
            }
        }

        private static void SetCellsInRadiusNoRoofNoRock(Map map, IntVec3 position, float radius)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true))
            {
                if (cell.InBounds(map) == false)
                {
                    continue;
                }
                // Unroof cell.
                if (cell.Roofed(map))
                {
                    map.roofGrid.SetRoof(cell, null);
                }
                // Remove rock from cell.
                Building rock = map.edificeGrid.InnerArray[map.cellIndices.CellToIndex(cell)];
                if (rock != null)
                {
                    rock.Destroy();
                }
                // Remove cave roof.
                List<Thing> thingList = cell.GetThingList(map);
                for (int thingIndex = 0; thingIndex < thingList.Count; thingIndex++)
                {
                    Thing thing = thingList[thingIndex];
                    if (thing.def == Util_CaveBiome.CaveRoofDef)
                    {
                        thing.Destroy();
                    }
                }
            }
        }
        
        private static void SetCellsInRadiusTerrain(Map map, IntVec3 position, float radius, TerrainDef terrain)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true))
            {
                if (cell.InBounds(map) == false)
                {
                    continue;
                }
                if (terrain != TerrainDefOf.WaterDeep)
                {
                    // Excepted when adding deep water, do not touch to water/marsh patches.
                    TerrainDef cellTerrain = map.terrainGrid.TerrainAt(cell);
                    if ((cellTerrain == TerrainDefOf.WaterDeep)
                        || (cellTerrain == TerrainDefOf.WaterShallow)
                        || (cellTerrain == TerrainDef.Named("Marsh")))
                    {
                        continue;
                    }
                }
                map.terrainGrid.SetTerrain(cell, terrain);
            }
        }
    }
}
