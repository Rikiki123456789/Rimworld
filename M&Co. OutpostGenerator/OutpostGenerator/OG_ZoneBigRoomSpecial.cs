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
    /// OG_ZoneBigRoomSpecial class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_ZoneBigRoomSpecial
    {
        public static void GenerateBigRoomRefectory(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin, Genstep_GenerateOutpost.zoneSideSize, Genstep_GenerateOutpost.zoneSideSize, rotation, TerrainDefOf.Concrete, TerrainDef.Named("CarpetDark"), ref outpostData);

            // Spawn doors.
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 10).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(10, 0, 5).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 0).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(0, 0, 5).RotatedBy(rotation), ref outpostData);

            // Spawn table and dining chairs.
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(4, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(4, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("TableShort"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(6, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(6, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(9, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(9, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("TableShort"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            
            // Spawn NPD, hoppers and lamp.
            OG_Common.TrySpawnThingAt(ThingDefOf.NutrientPasteDispenser, null, rotatedOrigin + new IntVec3(2, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Hopper, null, rotatedOrigin + new IntVec3(4, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnResourceAt(ThingDefOf.RawPotatoes, ThingDefOf.RawPotatoes.stackLimit, rotatedOrigin + new IntVec3(4, 0, 9).RotatedBy(rotation), true);
            OG_Common.TrySpawnThingAt(ThingDefOf.Hopper, null, rotatedOrigin + new IntVec3(4, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnResourceAt(ThingDef.Named("RawCorn"), ThingDef.Named("RawCorn").stackLimit, rotatedOrigin + new IntVec3(4, 0, 8).RotatedBy(rotation), true);
            OG_Common.TrySpawnThingAt(ThingDefOf.Hopper, null, rotatedOrigin + new IntVec3(4, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnResourceAt(ThingDef.Named("RawBerries"), ThingDef.Named("RawBerries").stackLimit, rotatedOrigin + new IntVec3(4, 0, 7).RotatedBy(rotation), true);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(4, 0, 6).RotatedBy(rotation), Color.white, ref outpostData);
            for (int xOffset = 1; xOffset <= 4; xOffset++)
            {
                for (int zOffset = 6; zOffset <= 9; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("SterileTile"));
                }
            }

            // Generate food racks.
            for (int xOffset = 6; xOffset <= 9; xOffset++)
            {
                for (int zOffset = 6; zOffset <= 9; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("SterileTile"));
                }
            }
            OG_Common.TrySpawnThingAt(OG_Util.FoodRackDef, ThingDefOf.Steel, rotatedOrigin + new IntVec3(6, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.FoodRackDef, ThingDefOf.Steel, rotatedOrigin + new IntVec3(6, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.FoodRackDef, ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.FoodRackDef, ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.FoodRackDef, ThingDefOf.Steel, rotatedOrigin + new IntVec3(9, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.FoodRackDef, ThingDefOf.Steel, rotatedOrigin + new IntVec3(9, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            for (int xOffset = 6; xOffset <= 7; xOffset++)
            {
                for (int zOffset = 6; zOffset <= 9; zOffset++)
                {
                    IntVec3 cell = rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation);
                    OG_Common.SpawnResourceAt(ThingDefOf.MealSurvivalPack, ThingDefOf.MealSurvivalPack.stackLimit, cell, true);
                }
            }
            for (int zOffset = 6; zOffset <= 9; zOffset++)
            {
                IntVec3 cell = rotatedOrigin + new IntVec3(9, 0, zOffset).RotatedBy(rotation);
                OG_Common.SpawnResourceAt(ThingDefOf.Beer, ThingDefOf.Beer.stackLimit, cell, true);
            }

            // Spawn lamp and temperature control.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(6, 0, 4).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(9, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(0, 0, 1).RotatedBy(rotation), new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(0, 0, 2).RotatedBy(rotation), new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(10, 0, 1).RotatedBy(rotation), new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(10, 0, 2).RotatedBy(rotation), new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.GenerateHorizontalAndVerticalPavedAlleys(origin);
        }

        public static void GenerateBigRoomBatteryRoom(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ZoneProperties[,] zoneMap, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin, Genstep_GenerateOutpost.zoneSideSize, Genstep_GenerateOutpost.zoneSideSize, rotation, TerrainDefOf.Concrete, TerrainDef.Named("MetalTile"), ref outpostData);

            // Spawn doors.
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 10).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(10, 0, 5).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 0).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(0, 0, 5).RotatedBy(rotation), ref outpostData);

            // Spawn generators.
            OG_Common.TrySpawnThingAt(OG_Util.CompactAutonomousGeneratorDef, null, rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.CompactAutonomousGeneratorDef, null, rotatedOrigin + new IntVec3(1, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.CompactAutonomousGeneratorDef, null, rotatedOrigin + new IntVec3(8, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            // Spawn batteries.
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(1, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(3, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(1, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(3, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(9, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(7, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);

            // Spawn power controller.
            OG_Common.TrySpawnThingAt(ThingDef.Named("MultiAnalyzer"), null, rotatedOrigin + new IntVec3(7, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            // Spawn lamp and vents.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(3, 0, 7).RotatedBy(rotation), Color.blue, ref outpostData);
            for (int cardinalAsInt = 0; cardinalAsInt < 4; cardinalAsInt++)
            {
                Rot4 cardinal = new Rot4(cardinalAsInt);
                int adjacentZoneAbs = 0;
                int adjacentZoneOrd = 0;
                Zone.GetAdjacentZone(zoneAbs, zoneOrd, cardinal, out adjacentZoneAbs, out adjacentZoneOrd);
                if (zoneMap[adjacentZoneOrd, adjacentZoneAbs].zoneType == ZoneType.SolarPanelZone)
                {
                    if (cardinal == Rot4.North)
                    {
                        OG_Common.SpawnVentAt(origin + new IntVec3(4, 0, 10), Rot4.North, ref outpostData);
                        OG_Common.SpawnVentAt(origin + new IntVec3(6, 0, 10), Rot4.North, ref outpostData);
                    }
                    else if (cardinal == Rot4.East)
                    {
                        OG_Common.SpawnVentAt(origin + new IntVec3(10, 0, 4), Rot4.East, ref outpostData);
                        OG_Common.SpawnVentAt(origin + new IntVec3(10, 0, 6), Rot4.East, ref outpostData);
                    }
                    else if (cardinal == Rot4.South)
                    {
                        OG_Common.SpawnVentAt(origin + new IntVec3(4, 0, 0), Rot4.South, ref outpostData);
                        OG_Common.SpawnVentAt(origin + new IntVec3(6, 0, 0), Rot4.West, ref outpostData);
                    }
                    else if (cardinal == Rot4.West)
                    {
                        OG_Common.SpawnVentAt(origin + new IntVec3(0, 0, 4), Rot4.West, ref outpostData);
                        OG_Common.SpawnVentAt(origin + new IntVec3(0, 0, 6), Rot4.West, ref outpostData);
                    }
                }
            }

            OG_Common.GenerateHorizontalAndVerticalPavedAlleys(origin);
        }

        public static Building_OutpostCommandConsole GenerateBigRoomCommandRoom(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            Building_OutpostCommandConsole commandConsole = null;

            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin, Genstep_GenerateOutpost.zoneSideSize, Genstep_GenerateOutpost.zoneSideSize, rotation, TerrainDefOf.Concrete, TerrainDef.Named("CarpetDark"), ref outpostData);

            // Spawn doors.
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 10).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(10, 0, 5).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 0).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(0, 0, 5).RotatedBy(rotation), ref outpostData);
            
            // Spawn weapon racks.
            Building_Storage rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData) as Building_Storage;
            OG_Common.TrySpawnWeaponOnRack(rack);
            rack.GetStoreSettings().filter.SetAllow(ThingCategoryDef.Named("WeaponsMelee"), false);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData) as Building_Storage;
            OG_Common.TrySpawnWeaponOnRack(rack);
            rack.GetStoreSettings().filter.SetAllow(ThingCategoryDef.Named("WeaponsMelee"), false);

            // Spawn lamps.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(9, 0, 9).RotatedBy(rotation), Color.white, ref outpostData);

            // Spawn table and stools.
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("TableShort"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            // Spawn outpost command console, battery and spare parts cabinet.
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(1, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.SparePartsCabinetDef, null, rotatedOrigin + new IntVec3(1, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnResourceAt(ThingDefOf.Components, 10, rotatedOrigin + new IntVec3(1, 0, 7).RotatedBy(rotation), true);
            OG_Common.SpawnResourceAt(ThingDefOf.Components, 10, rotatedOrigin + new IntVec3(1, 0, 6).RotatedBy(rotation), true);
            commandConsole = OG_Common.TrySpawnThingAt(OG_Util.OutpostCommandConsoleDef, null, rotatedOrigin + new IntVec3(3, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData) as Building_OutpostCommandConsole;

            // Spawn workbenches.
            OG_Common.TrySpawnThingAt(ThingDef.Named("TableMachining"), null, rotatedOrigin + new IntVec3(7, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("ElectricSmithy"), null, rotatedOrigin + new IntVec3(9, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);

            // Spawn heaters and coolers.
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(1, 0, 4).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(4, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(0, 0, 1).RotatedBy(rotation), new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(1, 0, 0).RotatedBy(rotation), new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);

            // Spawn floor and tactical computer.
            for (int xOffset = 0; xOffset <= 10; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 5).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            CellRect rect = new CellRect(origin.x + 3, origin.z + 3, 5, 5);
            foreach (IntVec3 cell in rect)
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDef.Named("PavedTile"));
            }
            foreach (IntVec3 cell in rect.ContractedBy(1))
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDef.Named("CarpetRed"));
            }
            if (ModsConfig.IsActive("Miscellaneous_Incidents"))
            {
                OG_Common.TrySpawnThingAt(ThingDef.Named("TacticalComputer"), null, rotatedOrigin + new IntVec3(5, 0, 5).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            }

            return commandConsole;
        }

        public static void GenerateBigRoomBarracks(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin, Genstep_GenerateOutpost.zoneSideSize, Genstep_GenerateOutpost.zoneSideSize, rotation, TerrainDefOf.Concrete, TerrainDef.Named("CarpetGreen"), ref outpostData);

            // Spawn doors.
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 10).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(10, 0, 5).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 0).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(0, 0, 5).RotatedBy(rotation), ref outpostData);
            
            // Spawn beds.
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(4, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(6, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(9, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(9, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(9, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(4, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            
            // Generate shower room.
            for (int xOffset = 6; xOffset <= 9; xOffset++)
            {
                OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(xOffset, 0, 4).RotatedBy(rotation), ref outpostData);
            }
            for (int zOffset = 1; zOffset <= 4; zOffset++)
            {
                OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(6, 0, zOffset).RotatedBy(rotation), ref outpostData);
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(6, 0, 2).RotatedBy(rotation), ref outpostData);
            for (int xOffset = 7; xOffset <= 9; xOffset++)
            {
                for (int zOffset = 1; zOffset <= 3; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("SterileTile"));
                }
            }
            for (int xOffset = 6; xOffset <= 8; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 2).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }

            // Spawn lamps.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(4, 0, 6).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(9, 0, 2).RotatedBy(rotation), Color.white, ref outpostData);

            // Spawn heaters and coolers.
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(1, 0, 9).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(9, 0, 9).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(0, 0, 1).RotatedBy(rotation), new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(0, 0, 9).RotatedBy(rotation), new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(10, 0, 9).RotatedBy(rotation), new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.GenerateHorizontalAndVerticalPavedAlleys(origin);
        }

        public static void GenerateBigRoomHydroponics(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin, Genstep_GenerateOutpost.zoneSideSize, Genstep_GenerateOutpost.zoneSideSize, rotation, TerrainDefOf.Concrete, TerrainDef.Named("SterileTile"), ref outpostData);

            // Spawn doors.
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 10).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(10, 0, 5).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 0).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(0, 0, 5).RotatedBy(rotation), ref outpostData);

            // Spawn sun lamp.
            OG_Common.TrySpawnThingAt(ThingDef.Named("SunLamp"), null, rotatedOrigin + new IntVec3(5, 0, 5).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);

            // Spawn hydroponics basins.
            Building_PlantGrower hydroponics = OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), true, rotation, ref outpostData) as Building_PlantGrower;
            hydroponics.SetPlantDefToGrow(ThingDef.Named("PlantStrawberry"));
            hydroponics = OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(3, 0, 2).RotatedBy(rotation), true, rotation, ref outpostData) as Building_PlantGrower;
            hydroponics.SetPlantDefToGrow(ThingDef.Named("PlantCorn"));
            hydroponics = OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(4, 0, 2).RotatedBy(rotation), true, rotation, ref outpostData) as Building_PlantGrower;
            hydroponics.SetPlantDefToGrow(ThingDef.Named("PlantCorn"));
            hydroponics = OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(6, 0, 2).RotatedBy(rotation), true, rotation, ref outpostData) as Building_PlantGrower;
            hydroponics.SetPlantDefToGrow(ThingDef.Named("PlantCorn"));
            hydroponics = OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(7, 0, 2).RotatedBy(rotation), true, rotation, ref outpostData) as Building_PlantGrower;
            hydroponics.SetPlantDefToGrow(ThingDef.Named("PlantCorn"));
            hydroponics = OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(9, 0, 2).RotatedBy(rotation), true, rotation, ref outpostData) as Building_PlantGrower;
            hydroponics.SetPlantDefToGrow(ThingDef.Named("PlantStrawberry"));

            OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(1, 0, 7).RotatedBy(rotation), true, rotation, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(3, 0, 7).RotatedBy(rotation), true, rotation, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(4, 0, 7).RotatedBy(rotation), true, rotation, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(6, 0, 7).RotatedBy(rotation), true, rotation, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(7, 0, 7).RotatedBy(rotation), true, rotation, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("HydroponicsBasin"), null, rotatedOrigin + new IntVec3(9, 0, 7).RotatedBy(rotation), true, rotation, ref outpostData);

            // Spawn heaters and coolers.
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(8, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(2, 0, 9).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(8, 0, 9).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(2, 0, 0).RotatedBy(rotation), new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(8, 0, 0).RotatedBy(rotation), new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(2, 0, 10).RotatedBy(rotation), new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(8, 0, 10).RotatedBy(rotation), new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.GenerateHorizontalAndVerticalPavedAlleys(origin);
        }
    }
}
