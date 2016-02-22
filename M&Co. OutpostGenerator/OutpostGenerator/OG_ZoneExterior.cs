using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// OG_ZoneExterior class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_ZoneExterior
    {
        public static void GenerateFarmZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            // Spawn floor (excepted in zone corners).
            for (int xOffset = 1; xOffset <= 9; xOffset++)
            {
                for (int zOffset = 0; zOffset <= 10; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Soil);
                }
            }
            for (int xOffset = 0; xOffset <= 10; xOffset++)
            {
                for (int zOffset = 1; zOffset <= 9; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Soil);
                }
            }
            for (int xOffset = 0; xOffset < 11; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 5).RotatedBy(rotation), OG_Util.DirtFloorDef);
            }
            for (int zOffset = 0; zOffset < 11; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), OG_Util.DirtFloorDef);
            }

            // Generate crops.
            ThingDef plantDef = ThingDef.Named("PlantXerigium");
            if (Rand.Value < 0.5f)
            {
                plantDef = ThingDef.Named("PlantDevilstrand");
            }
            for (int xOffset = 0; xOffset < 5; xOffset++)
            {
                for (int zOffset = 0; zOffset < 5; zOffset++)
                {
                    if ((xOffset == 0) && (zOffset == 0))
                        continue;
                    GenSpawn.Spawn(plantDef, rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation));
                }
            }
            plantDef = ThingDef.Named("PlantCorn");
            if (Rand.Value < 0.5f)
            {
                plantDef = ThingDef.Named("PlantRice");
            }
            for (int xOffset = 6; xOffset < 11; xOffset++)
            {
                for (int zOffset = 0; zOffset < 5; zOffset++)
                {
                    if ((xOffset == 10) && (zOffset == 0))
                        continue;
                    GenSpawn.Spawn(plantDef, rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation));
                }
            }
            plantDef = ThingDefOf.PlantPotato;
            if (Rand.Value < 0.5f)
            {
                plantDef = ThingDef.Named("PlantStrawberry");
            }
            for (int xOffset = 0; xOffset < 5; xOffset++)
            {
                for (int zOffset = 6; zOffset < 11; zOffset++)
                {
                    if ((xOffset == 0) && (zOffset == 10))
                        continue;
                    GenSpawn.Spawn(plantDef, rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation));
                }
            }
            plantDef = ThingDef.Named("PlantHops");
            if (Rand.Value < 0.5f)
            {
                plantDef = ThingDef.Named("PlantCotton");
            }
            for (int xOffset = 6; xOffset < 11; xOffset++)
            {
                for (int zOffset = 6; zOffset < 11; zOffset++)
                {
                    if ((xOffset == 10) && (zOffset == 10))
                        continue;
                    GenSpawn.Spawn(plantDef, rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation));
                }
            }
        }

        public static void GenerateCemeteryZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            // Spawn floor (excepted in zone corners).
            for (int xOffset = 1; xOffset <= 9; xOffset++)
            {
                for (int zOffset = 0; zOffset <= 10; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Soil);
                }
            }
            for (int xOffset = 0; xOffset <= 10; xOffset++)
            {
                for (int zOffset = 1; zOffset <= 9; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Soil);
                }
            }
            for (int xOffset = 4; xOffset <= 6; xOffset++)
            {
                for (int zOffset = 1; zOffset <= 9; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("TileGranite"));
                }
            }
            for (int zOffset = 1; zOffset <= 9; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            for (int xOffset = 4; xOffset <= 6; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 2).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 5).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 8).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }

            // Spawn graves and tombstones.
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), ref outpostData);
            Building_Grave grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(1, 0, 5).RotatedBy(rotation), ref outpostData);
            grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(2, 0, 5).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(1, 0, 8).RotatedBy(rotation), ref outpostData);
            grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(2, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();

            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(9, 0, 2).RotatedBy(rotation), ref outpostData);
            grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(8, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(9, 0, 5).RotatedBy(rotation), ref outpostData);
            grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(8, 0, 5).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(9, 0, 8).RotatedBy(rotation), ref outpostData);
            grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(8, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();

            // Spawn flower pots.
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(2, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(3, 0, 7).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(8, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(8, 0, 6).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
        }

        public static void GenerateExteriorRecZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            // Spawn floor (excepted in zone corners).
            for (int xOffset = 2; xOffset <= 8; xOffset++)
            {
                for (int zOffset = 1; zOffset <= 9; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("WoodPlankFloor"));
                }
            }
            for (int xOffset = 1; xOffset <= 9; xOffset++)
            {
                for (int zOffset = 2; zOffset <= 8; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("WoodPlankFloor"));
                }
            }

            // Spawn table and chairs.
            OG_Common.TrySpawnThingAt(ThingDef.Named("TableShort"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(3, 0, 6).RotatedBy(rotation), true, rotation, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(4, 0, 5).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(3, 0, 5).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(2, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(3, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(5, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(6, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("ChessTable"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(7, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(8, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            
            OG_Common.TrySpawnThingAt(ThingDef.Named("HorseshoesPin"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(7, 0, 2).RotatedBy(rotation), TerrainDef.Named("PavedTile"));

            // Spawn flower pots.
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(1, 0, 8).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(2, 0, 9).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(8, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(8, 0, 9).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(9, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), ThingDef.Named("BlocksGranite"), rotatedOrigin + new IntVec3(9, 0, 8).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
        }

        public static void GenerateWaterPoolZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);
            IntVec3 firstPatchCenter = rotatedOrigin + new IntVec3(4, 0, 4).RotatedBy(rotation);
            IntVec3 secondPatchCenter = rotatedOrigin + new IntVec3(7, 0, 7).RotatedBy(rotation);
            IntVec3 thirdPatchCenter = rotatedOrigin + new IntVec3(6, 0, 6).RotatedBy(rotation);

            // Generate water patches.
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(firstPatchCenter, 4.2f, true))
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
            }
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(secondPatchCenter, 3.2f, true))
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
            }
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(firstPatchCenter, 3.2f, true))
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDef.Named("WaterShallow"));
            }
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(secondPatchCenter, 2f, true))
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDef.Named("WaterShallow"));
            }
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(firstPatchCenter, 2f, true))
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDef.Named("WaterDeep"));
            }
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(secondPatchCenter, 1f, true))
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDef.Named("WaterDeep"));
            }
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(thirdPatchCenter, 1f, true))
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDef.Named("WaterDeep"));
            }

            // Spawn fishing pier.
            if (ModsConfig.IsActive("FishIndustry"))
            {
                OG_Common.TrySpawnThingAt(ThingDef.Named("FishingPier"), null, rotatedOrigin + new IntVec3(4, 0, 1).RotatedBy(rotation), true, rotation, ref outpostData);
            }
        }

        public static void GenerateShootingRangeZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            IntVec3 shootingPosition = rotatedOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation);
            GenerateOneShootingLine(shootingPosition, rotation, ref outpostData);

            shootingPosition = rotatedOrigin + new IntVec3(8, 0, 1).RotatedBy(rotation);
            GenerateOneShootingLine(shootingPosition, rotation, ref outpostData);
        }

        private static void GenerateOneShootingLine(IntVec3 shootingPosition, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            // Spawn floor.
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int zOffset = -1; zOffset <= 9; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(shootingPosition + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            Find.TerrainGrid.SetTerrain(shootingPosition + new IntVec3(0, 0, 2).RotatedBy(rotation), TerrainDef.Named("MetalTile"));
            Find.TerrainGrid.SetTerrain(shootingPosition + new IntVec3(0, 0, 3).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            Find.TerrainGrid.SetTerrain(shootingPosition + new IntVec3(0, 0, 4).RotatedBy(rotation), TerrainDef.Named("MetalTile"));
            Find.TerrainGrid.SetTerrain(shootingPosition + new IntVec3(0, 0, 5).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            Find.TerrainGrid.SetTerrain(shootingPosition + new IntVec3(0, 0, 6).RotatedBy(rotation), TerrainDef.Named("MetalTile"));
            Find.TerrainGrid.SetTerrain(shootingPosition + new IntVec3(0, 0, 7).RotatedBy(rotation), TerrainDef.Named("PavedTile"));

            // Spawn sandbags.
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, shootingPosition + new IntVec3(-1, 0, 0).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, shootingPosition + new IntVec3(-1, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, shootingPosition + new IntVec3(0, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, shootingPosition + new IntVec3(1, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, shootingPosition + new IntVec3(1, 0, 0).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);

            // Spawn target.
            // TODO: spawn a real target. From Miscellaneous?
            OG_Common.TrySpawnWallAt(shootingPosition + new IntVec3(0, 0, 8).RotatedBy(rotation), ref outpostData);
        }

        public static void GenerateMortarZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            // Spawn floor.
            for (int xOffset = 0; xOffset <= 10; xOffset++)
            {
                for (int zOffset = 0; zOffset <= 10; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            OG_Common.GenerateHorizontalAndVerticalPavedAlleys(origin);
            for (int xOffset = 7; xOffset <= 9; xOffset++)
            {
                for (int zOffset = 1; zOffset <= 3; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("MetalTile"));
                }
            }
            for (int xOffset = 6; xOffset <= 9; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 2).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }

            // Spawn sandbags.
            for (int xOffset = 0; xOffset <= 4; xOffset++)
            {
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, rotatedOrigin + new IntVec3(xOffset, 0, 0).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, rotatedOrigin + new IntVec3(xOffset, 0, 10).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            }
            for (int xOffset = 6; xOffset <= 10; xOffset++)
            {
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, rotatedOrigin + new IntVec3(xOffset, 0, 10).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            }
            for (int zOffset = 0; zOffset <= 4; zOffset++)
            {
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, rotatedOrigin + new IntVec3(0, 0, zOffset).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            }
            for (int zOffset = 6; zOffset <= 10; zOffset++)
            {
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, rotatedOrigin + new IntVec3(0, 0, zOffset).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, rotatedOrigin + new IntVec3(10, 0, zOffset).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            }

            // Spawn mortars.
            OG_Common.TrySpawnThingAt(ThingDef.Named("Turret_MortarBomb"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Turret_MortarBomb"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Turret_MortarBomb"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            // Generate ammunition stockpile.
            for (int xOffset = 6; xOffset <= 10; xOffset++)
            {
                OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(xOffset, 0, 0).RotatedBy(rotation), ref outpostData);
                OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(xOffset, 0, 4).RotatedBy(rotation), ref outpostData);
            }
            for (int zOffset = 0; zOffset <= 4; zOffset++)
            {
                OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(6, 0, zOffset).RotatedBy(rotation), ref outpostData);
                OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(10, 0, zOffset).RotatedBy(rotation), ref outpostData);
            }
            for (int xOffset = 6; xOffset <= 10; xOffset++)
            {
                for (int zOffset = 0; zOffset <= 4; zOffset++)
                {
                    Find.RoofGrid.SetRoof(rotatedOrigin + new IntVec3(10, 0, zOffset).RotatedBy(rotation), OG_Util.IronedRoofDef);
                }
            }

            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(6, 0, 2).RotatedBy(rotation), ref outpostData);
            for (int xOffset = 7; xOffset <= 9; xOffset++)
            {
                Thing shell = OG_Common.TrySpawnThingAt(ThingDef.Named("ArtilleryShell"), null, rotatedOrigin + new IntVec3(xOffset, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
                shell.stackCount = ThingDef.Named("ArtilleryShell").stackLimit;
                shell = OG_Common.TrySpawnThingAt(ThingDef.Named("ArtilleryShell"), null, rotatedOrigin + new IntVec3(xOffset, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
                shell.stackCount = ThingDef.Named("ArtilleryShell").stackLimit;
            }

            OG_Common.GenerateHorizontalAndVerticalPavedAlleys(origin);
        }
    }
}
