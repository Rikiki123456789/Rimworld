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

		public override void Generate()
		{
            Log.Message("GenStep_CaveRoof.Generate");
            if (Find.Map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Nothing to do in other biomes.
                return;
            }
            // Compute number of cave wells (5 for standard map 250x250, around 13 for bigest map 400x400).
            caveWellsNumber = Mathf.CeilToInt((Find.Map.Size.x * Find.Map.Size.z) / 12500);
            Log.Message("caveWellsNumber = " + caveWellsNumber);
            
			MapGenFloatGrid elevationGrid = MapGenerator.Elevation;
			foreach (IntVec3 cell in Find.Map.AllCells)
			{
			    Thing thing = Find.EdificeGrid.InnerArray[CellIndices.CellToIndex(cell)];
			    if (thing != null && thing.def.holdsRoof)
			    {
                    Find.RoofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);
			    }
                else
                {
                    // Spawn cave roof holder.
                    GenSpawn.Spawn(Util_CaveBiome.CaveRoofDef, cell);
                }
			}

            // Update region and room to be able to use the CanReachMapEdge function.
            DeepProfiler.Start("RebuildAllRegionsBeforeCaveWells");
            RegionAndRoomUpdater.Enabled = true;
            RegionAndRoomUpdater.RebuildAllRegionsAndRooms();
            RegionAndRoomUpdater.Enabled = false;
            DeepProfiler.End();

            // Get cave wells position.
            caveWellsPosition = GetCaveWellsPosition();

            // Spawn cave wells.
            // First cave well is always dry (to avoid starting thing scattering errors).
            SpawnDryCaveWellAt(caveWellsPosition[0]);
            SpawnAnimalCorpsesMaker(caveWellsPosition[0]);
            for (int caveWellIndex = 1; caveWellIndex < caveWellsNumber; caveWellIndex++)
            {
                if (Rand.Value < 0.8f)
                {
                    // Spawn aqueous cave well.
                    SpawnAqueousCaveWellAt(caveWellsPosition[caveWellIndex]);
                }
                else if (Rand.Value < 0.9f)
                {
                    // Spawn dry cave well + fallen animal corpses.
                    SpawnDryCaveWellAt(caveWellsPosition[caveWellIndex]);
                    SpawnAnimalCorpsesMaker(caveWellsPosition[caveWellIndex]);
                }
                else
                {
                    // Spawn dry cave well + sacrificial stone.
                    SpawnDryCaveWellAt(caveWellsPosition[caveWellIndex]);
                    SpawnRitualStone(caveWellsPosition[caveWellIndex]);
                }
            }
		}

        private static List<IntVec3> GetCaveWellsPosition()
        {
            List<IntVec3> positionsList = new List<IntVec3>();
            for (int caveWellIndex = 0; caveWellIndex < caveWellsNumber; caveWellIndex++)
            {
                Predicate<IntVec3> validator = delegate(IntVec3 cell)
                {
                    if (caveWellIndex == 0)
                    {
                        // First cave well must be near map center.
                        if (cell.InHorDistOf(Find.Map.Center, 40f) == false)
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
                    Room room = cell.GetRoom();
                    if (room != null
                        && room.TouchesMapEdge)
                    {
                        return true;
                    }
                    return false;
                };

                IntVec3 caveWellCell = IntVec3.Invalid;
                bool caveWellCellIsFound = CellFinderLoose.TryFindRandomNotEdgeCellWith(20, validator, out caveWellCell);
                if (caveWellCellIsFound)
                {
                    //Log.Message("valid caveWellsPosition[" + caveWellIndex + "] = " + caveWellCell.ToString());
                    positionsList.Add(caveWellCell);
                    if (caveWellIndex == 0)
                    {
                        // Found a good start point. Reuse it later.
                        MapGenerator.PlayerStartSpot = caveWellCell;
                    }
                }
                else
                {
                    CellFinderLoose.TryFindRandomNotEdgeCellWith(20, null, out caveWellCell);
                    //Log.Message("default caveWellsPosition[" + caveWellIndex + "] = " + caveWellCell.ToString());
                    positionsList.Add(caveWellCell);
                }
            }
            return positionsList;
        }

        private static void SpawnAqueousCaveWellAt(IntVec3 position)
        {
            // Spawn main hole.
            SetCellsInRadiusNoRoofNoRock(position, 10f);
            SetCellsInRadiusCaveWell(position, 10f);
            SetCellsInRadiusTerrain(position, 10f, TerrainDefOf.Gravel);
            SetCellsInRadiusTerrain(position, 8f, TerrainDefOf.WaterShallow);

            // Spawn small additional holes.
            int smallHolesNumber = Rand.RangeInclusive(2, 5);
            for (int holeIndex = 0; holeIndex < smallHolesNumber; holeIndex++)
            {
                IntVec3 smallHolePosition = position + (7f * Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360))).ToIntVec3();
                SetCellsInRadiusNoRoofNoRock(smallHolePosition, 5f);
                SetCellsInRadiusCaveWell(position, 5f);
                SetCellsInRadiusTerrain(smallHolePosition, 3.2f, TerrainDefOf.WaterShallow);
                SetCellsInRadiusTerrain(smallHolePosition, 2.1f, TerrainDefOf.WaterDeep);
            }
            SetCellsInRadiusTerrain(position, 5.2f, TerrainDefOf.WaterDeep);
        }

        private static void SpawnDryCaveWellAt(IntVec3 position)
        {
            // Spawn main hole.
            SetCellsInRadiusNoRoofNoRock(position, 10f);
            SetCellsInRadiusCaveWell(position, 10f);
            SetCellsInRadiusTerrain(position, 10f, TerrainDefOf.Gravel);
            SetCellsInRadiusTerrain(position, 8f, TerrainDefOf.Soil);

            // Spawn small additional holes.
            int smallHolesNumber = Rand.RangeInclusive(2, 5);
            for (int holeIndex = 0; holeIndex < smallHolesNumber; holeIndex++)
            {
                IntVec3 smallHolePosition = position + (7f * Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360))).ToIntVec3();
                SetCellsInRadiusNoRoofNoRock(smallHolePosition, 5f);
                SetCellsInRadiusCaveWell(position, 5f);
                SetCellsInRadiusTerrain(smallHolePosition, 3.2f, TerrainDefOf.Soil);
                SetCellsInRadiusTerrain(smallHolePosition, 2.1f, TerrainDef.Named("SoilRich"));
            }
            SetCellsInRadiusTerrain(position, 6.5f, TerrainDef.Named("SoilRich"));
        }

        private static void SpawnChipChunksAt(IntVec3 position)
        {
            int partsNumber = Rand.Range(2, 5);
            for (int partIndex = 0; partIndex < partsNumber; partIndex++)
            {
                IntVec3 spawnCell = position + Vector3Utility.RandomHorizontalOffset(5f).ToIntVec3();
                GenSpawn.Spawn(ThingDefOf.ShipChunk, spawnCell);
            }
        }

        private static void SpawnAnimalCorpsesMaker(IntVec3 position)
        {
            Thing animalCorpsesGenerator = ThingMaker.MakeThing(Util_CaveBiome.AnimalCorpsesGeneratorDef);
            GenSpawn.Spawn(animalCorpsesGenerator, position);
        }

        private static void SpawnRitualStone(IntVec3 position)
        {
            // Set terrain.
            SetCellsInRadiusTerrain(position, 2.5f, TerrainDef.Named("TileSlate"));
            // Spawn ritual stone.
            Thing thing = ThingMaker.MakeThing(ThingDef.Named("Sarcophagus"), ThingDef.Named("BlocksSlate"));
            GenSpawn.Spawn(thing, position + new IntVec3(0, 0, -1));
            (thing as Building_Sarcophagus).GetStoreSettings().filter.SetDisallowAll();
            // Spawn offerings.
            thing = ThingMaker.MakeThing(ThingDef.Named("MeleeWeapon_Shiv"), ThingDef.Named("Jade"));
            GenSpawn.Spawn(thing, position + new IntVec3(0, 0, -1));
            thing = ThingMaker.MakeThing(ThingDefOf.HerbalMedicine);
            thing.stackCount = Rand.Range(5, 12);
            GenSpawn.Spawn(thing, position + new IntVec3(-1, 0, 0));
            thing = ThingMaker.MakeThing(ThingDefOf.Gold);
            thing.stackCount = Rand.Range(7, 25);
            GenSpawn.Spawn(thing, position + new IntVec3(1, 0, 0));
            thing = ThingMaker.MakeThing(ThingDef.Named("Campfire"));
            GenSpawn.Spawn(thing, position + new IntVec3(0, 0, 1), Rot4.South);
            // Spawn blood.
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, 2f, true))
            {
                if (cell.InBounds() == false)
                {
                    continue;
                }
                int bloodQuantity = Rand.Range(2, 5);
                for (int bloodFilthIndex = 0; bloodFilthIndex < bloodQuantity; bloodFilthIndex++)
                {
                    GenSpawn.Spawn(ThingDefOf.FilthBlood, cell);
                }
            }
            // Spawn torches.
            GenSpawn.Spawn(ThingDef.Named("TorchLamp"), position + new IntVec3(1, 0, 3));
            GenSpawn.Spawn(ThingDef.Named("TorchLamp"), position + new IntVec3(3, 0, 1));
            GenSpawn.Spawn(ThingDef.Named("TorchLamp"), position + new IntVec3(3, 0, -1));
            GenSpawn.Spawn(ThingDef.Named("TorchLamp"), position + new IntVec3(1, 0, -3));
            GenSpawn.Spawn(ThingDef.Named("TorchLamp"), position + new IntVec3(-1, 0, -3));
            GenSpawn.Spawn(ThingDef.Named("TorchLamp"), position + new IntVec3(-3, 0, -1));
            GenSpawn.Spawn(ThingDef.Named("TorchLamp"), position + new IntVec3(-3, 0, 1));
            GenSpawn.Spawn(ThingDef.Named("TorchLamp"), position + new IntVec3(-1, 0, 3));
            // Spawn corpses generator.
            if (Rand.Value < 0.5f)
            {
                Thing villagerCorpsesGenerator = ThingMaker.MakeThing(Util_CaveBiome.VillagerCorpsesGeneratorDef);
                GenSpawn.Spawn(villagerCorpsesGenerator, position);
            }
        }

        private static void SetCellsInRadiusNoRoofNoRock(IntVec3 position, float radius)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true))
            {
                if (cell.InBounds() == false)
                {
                    continue;
                }
                // Unroof cell.
                if (cell.Roofed())
                {
                    Find.RoofGrid.SetRoof(cell, null);
                }
                // Remove rock from cell.
                Building rock = Find.EdificeGrid.InnerArray[CellIndices.CellToIndex(cell)];
                if (rock != null)
                {
                    rock.Destroy();
                }
                // Remove cave roof.
                List<Thing> thingList = cell.GetThingList();
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

        private static void SetCellsInRadiusCaveWell(IntVec3 position, float radius)
        {
            bool caveWellIsAlreadySpawned = false;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true))
            {
                if (cell.InBounds() == false)
                {
                    continue;
                }
                caveWellIsAlreadySpawned = false;
                List<Thing> thingList = cell.GetThingList();
                for (int thingIndex = 0; thingIndex < thingList.Count; thingIndex++)
                {
                    Thing thing = thingList[thingIndex];
                    if (thing.def == Util_CaveBiome.CaveWellDef)
                    {
                        // A cave well has already be spawned on this cell.
                        caveWellIsAlreadySpawned = true;
                        break;
                    }
                }
                if (caveWellIsAlreadySpawned == false)
                {
                    GenSpawn.Spawn(Util_CaveBiome.CaveWellDef, cell);
                }
            }
        }

        private static void SetCellsInRadiusTerrain(IntVec3 position, float radius, TerrainDef terrain)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true))
            {
                if (cell.InBounds() == false)
                {
                    continue;
                }
                if (terrain != TerrainDefOf.WaterDeep)
                {
                    // Excepted when adding deep water, do not touch to water/marsh patches.
                    TerrainDef cellTerrain = Find.TerrainGrid.TerrainAt(cell);
                    if ((cellTerrain == TerrainDefOf.WaterDeep)
                        || (cellTerrain == TerrainDefOf.WaterShallow)
                        || (cellTerrain == TerrainDef.Named("Marsh")))
                    {
                        continue;
                    }
                }
                Find.TerrainGrid.SetTerrain(cell, terrain);
            }
        }
    }
}
