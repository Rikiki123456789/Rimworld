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
    /// OG_ZoneMediumRoom class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_ZoneMediumRoom
    {
        public static void GenerateMediumRoomMedibay(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), 9, 9, rotation, TerrainDefOf.Concrete, TerrainDef.Named("SterileTile"), ref outpostData);

            // Spawn medbeds and vitals monitors.
            OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("VitalsMonitor"), null, rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("VitalsMonitor"), null, rotatedOrigin + new IntVec3(2, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("VitalsMonitor"), null, rotatedOrigin + new IntVec3(8, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("VitalsMonitor"), null, rotatedOrigin + new IntVec3(8, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);

            // Spawn medicine and lamp.
            OG_Common.SpawnResourceAt(ThingDefOf.Medicine, Rand.RangeInclusive(14, 48), rotatedOrigin + new IntVec3(8, 0, 5).RotatedBy(rotation));
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(2, 0, 5).RotatedBy(rotation), Color.blue, ref outpostData);

            // Spawn heaters and coolers.
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(2, 0, 4).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(2, 0, 6).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(1, 0, 4).RotatedBy(rotation), new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(1, 0, 6).RotatedBy(rotation), new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);

            // Spawn vertical alley and doors.
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 9).RotatedBy(rotation), ref outpostData);
        }
        
        public static void GenerateMediumRoomPrison(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(0, 0, 1).RotatedBy(rotation), 5, 9, rotation, TerrainDefOf.Concrete, null, ref outpostData);
            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(4, 0, 1).RotatedBy(rotation), 3, 9, rotation, TerrainDefOf.Concrete, null, ref outpostData);
            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(6, 0, 1).RotatedBy(rotation), 5, 9, rotation, TerrainDefOf.Concrete, null, ref outpostData);

            // Spawn prisoner beds.
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(9, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            // Spawn lamps, heaters and coolers.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(3, 0, 2).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(7, 0, 8).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(9, 0, 8).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(0, 0, 2).RotatedBy(rotation), new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(10, 0, 8).RotatedBy(rotation), new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);

            // Spawn vertical alley and door.
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(4, 0, 3).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(6, 0, 7).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 9).RotatedBy(rotation), ref outpostData);
        }

        public static void GenerateMediumRoomKitchen(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), 9, 9, rotation, TerrainDefOf.Concrete, null, ref outpostData);

            // Spawn table and stools.
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("TableShort"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            for (int xOffset = 2; xOffset <= 4; xOffset++)
            {
                for (int zOffset = 2; zOffset <= 5; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("WoodPlankFloor"));
                }
            }

            // Spawn cook stoves and brewery.
            OG_Common.TrySpawnThingAt(ThingDef.Named("CookStove"), null, rotatedOrigin + new IntVec3(8, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("CookStove"), null, rotatedOrigin + new IntVec3(8, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            for (int xOffset = 7; xOffset <= 8; xOffset++)
            {
                for (int zOffset = 2; zOffset <= 8; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("SterileTile"));
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset + 4).RotatedBy(rotation), TerrainDef.Named("SterileTile"));
                }
            }
            OG_Common.TrySpawnThingAt(ThingDef.Named("Brewery"), null, rotatedOrigin + new IntVec3(3, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            for (int xOffset = 2; xOffset <= 4; xOffset++)
            {
                for (int zOffset = 7; zOffset <= 8; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("SterileTile"));
                }
            }

            // Spawn lamps, heater and coolers.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(2, 0, 5).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(8, 0, 5).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(9, 0, 4).RotatedBy(rotation), new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(9, 0, 6).RotatedBy(rotation), new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            
            // Spawn vertical alley and doors.
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 9).RotatedBy(rotation), ref outpostData);
        }

        public static void GenerateMediumRoomWarehouse(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            const int stockWidth = 3;
            const int stockHeight = 3;

            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), 9, 9, rotation, TerrainDefOf.Concrete, null, ref outpostData);
            
            // Spawn metal stock and lamp.
            OG_Common.TrySpawnResourceStock(rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), rotation, ThingDefOf.Steel, stockWidth, stockHeight, 0.6f, 10, 55);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), Color.white, ref outpostData);
            // Spawn wood stock and lamp.
            OG_Common.TrySpawnResourceStock(rotatedOrigin + new IntVec3(6, 0, 2).RotatedBy(rotation), rotation, ThingDefOf.WoodLog, stockWidth, stockHeight, 0.8f, 20, 75);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(8, 0, 2).RotatedBy(rotation), Color.white, ref outpostData);
            // Spawn gold stock and lamp.
            OG_Common.TrySpawnResourceStock(rotatedOrigin + new IntVec3(2, 0, 6).RotatedBy(rotation), rotation, ThingDefOf.Gold, stockWidth, stockHeight, 0.3f, 2, 25);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(2, 0, 8).RotatedBy(rotation), Color.white, ref outpostData);
            // Spawn plasteel stock and lamp.
            OG_Common.TrySpawnResourceStock(rotatedOrigin + new IntVec3(6, 0, 6).RotatedBy(rotation), rotation, ThingDefOf.Plasteel, stockWidth, stockHeight, 0.5f, 5, 35);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(8, 0, 8).RotatedBy(rotation), Color.white, ref outpostData);

            // Spawn vertical and horizontal alley and doors.
            for (int xOffset = 2; xOffset <= 8; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 5).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 9).RotatedBy(rotation), ref outpostData);
        }

        public static void GenerateMediumRoomWeaponRoom(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);
            
            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), 9, 9, rotation, TerrainDefOf.Concrete, null, ref outpostData);

            // Spawn weapon racks, weapons and lamps.
            Thing rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(6, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(4, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnWeaponOnRack(rack);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(2, 0, 5).RotatedBy(rotation), Color.red, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(8, 0, 5).RotatedBy(rotation), Color.red, ref outpostData);

            // Spawn vertical alley and doors.
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 9).RotatedBy(rotation), ref outpostData);
        }

        public static void GenerateMediumRoomBarn(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), 9, 9, rotation, TerrainDefOf.Concrete, TerrainDefOf.Soil, ref outpostData);

            // Generate animal feed room.
            for (int xOffset = 2; xOffset <= 4; xOffset++)
            {
                OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(xOffset, 0, 5).RotatedBy(rotation), ref outpostData);
            }
            for (int zOffset = 2; zOffset <= 5; zOffset++)
            {
                OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(4, 0, zOffset).RotatedBy(rotation), ref outpostData);
            }
            IntVec3 doorPosition = rotatedOrigin + new IntVec3(4, 0, 3).RotatedBy(rotation);
            OG_Common.SpawnDoorAt(doorPosition, ref outpostData);
            Find.TerrainGrid.SetTerrain(doorPosition, TerrainDef.Named("PavedTile"));
            for (int xOffset = 2; xOffset <= 3; xOffset++)
            {
                for (int zOffset = 2; zOffset <= 4; zOffset++)
                {
                    IntVec3 fridgeCell = rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation);
                    Find.TerrainGrid.SetTerrain(fridgeCell, TerrainDef.Named("SterileTile"));
                    if ((zOffset == 2) || (zOffset == 4))
                    {
                        Thing hayStack = ThingMaker.MakeThing(ThingDef.Named("Hay"));
                        hayStack.stackCount = ThingDef.Named("Hay").stackLimit;
                        GenSpawn.Spawn(hayStack, fridgeCell);
                    }
                }
            }
            // Spawn animal beds.
            OG_Common.TrySpawnThingAt(ThingDef.Named("AnimalBed"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(2, 0, 6).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("AnimalBed"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(2, 0, 8).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("AnimalBed"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(4, 0, 8).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);

            OG_Common.TrySpawnThingAt(ThingDef.Named("AnimalBed"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(6, 0, 8).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("AnimalBed"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(8, 0, 8).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("AnimalBed"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(8, 0, 6).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);

            OG_Common.TrySpawnThingAt(ThingDef.Named("AnimalBed"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(8, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("AnimalBed"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(8, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("AnimalBed"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(6, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            
            // Spawn lamp, heater and coolers.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(6, 0, 5).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(8, 0, 5).RotatedBy(rotation), ref outpostData);
            Building_Cooler cooler = OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(1, 0, 3).RotatedBy(rotation), new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            cooler.compTempControl.targetTemperature = 5f;

            // Spawn vertical alley and doors.
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 9).RotatedBy(rotation), ref outpostData);
        }

        public static void GenerateMediumRoomLaboratory(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), 9, 9, rotation, TerrainDefOf.Concrete, TerrainDef.Named("MetalTile"), ref outpostData);

            // Spawn research bench.
            OG_Common.TrySpawnThingAt(ThingDefOf.ResearchBench, ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 5).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 5).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);

            // Spawn tool cabinet and uranium.
            OG_Common.TrySpawnThingAt(ThingDef.Named("ToolCabinet"), null, rotatedOrigin + new IntVec3(2, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            IntVec3 studyObjectPosition = rotatedOrigin + new IntVec3(3, 0, 5).RotatedBy(rotation);
            Find.TerrainGrid.SetTerrain(studyObjectPosition, TerrainDef.Named("SterileTile"));
            OG_Common.SpawnResourceAt(ThingDef.Named("Uranium"), Rand.RangeInclusive(5, 18), studyObjectPosition, true);

            // Spawn lamps, heater and coolers.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(3, 0, 3).RotatedBy(rotation), Color.blue, ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(4, 0, 8).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(1, 0, 5).RotatedBy(rotation), new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);

            // Spawn vertical alley and doors.
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 9).RotatedBy(rotation), ref outpostData);
        }

        public static void GenerateMediumRoomRecRoom(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), 9, 9, rotation, TerrainDefOf.Concrete, TerrainDef.Named("WoodPlankFloor"), ref outpostData);

            // Spawn billiard table, chess table and chairs.
            OG_Common.TrySpawnThingAt(ThingDef.Named("BilliardsTable"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(3, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("ChessTable"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(3, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(4, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            for (int xOffset = 2; xOffset <= 4; xOffset++)
            {
                for (int zOffset = 4; zOffset <= 8; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("CarpetGreen"));
                }
            }

            // Spawn television and armchairs.
            OG_Common.TrySpawnThingAt(ThingDef.Named("Television"), null, rotatedOrigin + new IntVec3(7, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Armchair"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(6, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Armchair"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(7, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Armchair"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(8, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Armchair"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(6, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Armchair"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(7, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Armchair"), ThingDef.Named("Cloth"), rotatedOrigin + new IntVec3(8, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            for (int xOffset = 6; xOffset <= 8; xOffset++)
            {
                for (int zOffset = 4; zOffset <= 8; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("CarpetDark"));
                }
            }

            // Spawn lamps, heater and coolers.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(2, 0, 8).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(8, 0, 2).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(8, 0, 3).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(8, 0, 8).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(7, 0, 1).RotatedBy(rotation), new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(3, 0, 9).RotatedBy(rotation), new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            // Spawn vertical alley and doors.
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 9).RotatedBy(rotation), ref outpostData);
        }
    }
}
