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
            Building_Bed bed = OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData) as Building_Bed;
            bed.Medical = false; // Set as non medical or pawns may want to use it even if it is not owned by colony.
            OG_Common.TrySpawnThingAt(ThingDef.Named("VitalsMonitor"), null, rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            bed = OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData) as Building_Bed;
            bed.Medical = false;
            OG_Common.TrySpawnThingAt(ThingDef.Named("VitalsMonitor"), null, rotatedOrigin + new IntVec3(2, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);

            bed = OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Bed;
            bed.Medical = false;
            OG_Common.TrySpawnThingAt(ThingDef.Named("VitalsMonitor"), null, rotatedOrigin + new IntVec3(8, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            bed = OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(8, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Bed;
            bed.Medical = false;
            OG_Common.TrySpawnThingAt(ThingDef.Named("VitalsMonitor"), null, rotatedOrigin + new IntVec3(8, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);

            // Spawn medicine and lamp.
            OG_Common.TrySpawnResourceAt(ThingDefOf.Medicine, Rand.RangeInclusive(14, 48), rotatedOrigin + new IntVec3(8, 0, 5).RotatedBy(rotation));
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
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(1, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
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

        public static void GenerateMediumRoomWeaponRoom(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(origin + new IntVec3(1, 0, 1), 5, 9, rotation, TerrainDefOf.Concrete, null, ref outpostData);
            OG_Common.GenerateEmptyRoomAt(origin + new IntVec3(4, 0, 1), 3, 9, rotation, TerrainDefOf.Concrete, null, ref outpostData);
            OG_Common.GenerateEmptyRoomAt(origin + new IntVec3(6, 0, 1), 5, 9, rotation, TerrainDefOf.Concrete, null, ref outpostData);

            // Spawn weapon racks, weapons and lamps.
            /*Thing rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(smallRoomWallOffset + 4, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(smallRoomWallOffset + 2, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, ThingDefOf.Steel, rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            TrySpawnWeaponOnRack(rack);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 3).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 3).RotatedBy(rotation), Color.white, ref outpostData);
            // Spawn vertical alley and door.
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 9).RotatedBy(rotation), ref outpostData);*/
        }

        public static void TrySpawnWeaponOnRack(Thing rack)
        {
            foreach (IntVec3 cell in rack.OccupiedRect())
            {
                if (Rand.Value < 0.33f)
                {
                    float weaponSelector = Rand.Value;
                    ThingDef thingDef = ThingDefOf.Gun_Pistol;
                    const float weaponsRatio = 1f / 7f;
                    if (weaponSelector < weaponsRatio * 1f)
                    {
                        thingDef = ThingDef.Named("Gun_PumpShotgun");
                    }
                    else if (weaponSelector < weaponsRatio * 2f)
                    {
                        thingDef = ThingDef.Named("Gun_AssaultRifle");
                    }
                    else if (weaponSelector < weaponsRatio * 3f)
                    {
                        thingDef = ThingDef.Named("Gun_SniperRifle");
                    }
                    else if (weaponSelector < weaponsRatio * 4f)
                    {
                        thingDef = ThingDef.Named("Gun_IncendiaryLauncher");
                    }
                    else if (weaponSelector < weaponsRatio * 5f)
                    {
                        thingDef = ThingDef.Named("Gun_LMG");
                    }
                    else if (weaponSelector < weaponsRatio * 6f)
                    {
                        thingDef = ThingDef.Named("Bullet_ChargeRifle");
                    }
                    else
                    {
                        thingDef = ThingDefOf.Gun_Pistol;
                    }

                    Thing weapon = ThingMaker.MakeThing(thingDef);
                    CompQuality qualityComp = weapon.TryGetComp<CompQuality>();
                    if (qualityComp != null)
                    {
                        QualityCategory quality = (QualityCategory)Rand.RangeInclusive((int)QualityCategory.Normal, (int)QualityCategory.Superior);
                        qualityComp.SetQuality(quality, ArtGenerationSource.Outsider);
                    }
                    GenSpawn.Spawn(weapon, cell);
                }
            }
        }
    }
}
