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
    /// OG_ZoneSmallRoom class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_ZoneSmallRoom
    {
        const int smallRoomWallOffset = 2;

        public static void GenerateSmallRoomBarracks(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(smallRoomWallOffset, 0, smallRoomWallOffset).RotatedBy(rotation), Genstep_GenerateOutpost.zoneSideSize - 2 * smallRoomWallOffset, Genstep_GenerateOutpost.zoneSideSize - 2 * smallRoomWallOffset, rotation, TerrainDefOf.Concrete, TerrainDef.Named("WoodPlankFloor"), ref outpostData);

            // Spawn beds, lamp and temperature control.
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 3).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 4, 0, smallRoomWallOffset + 3).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 3).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 6, 0, smallRoomWallOffset + 3).RotatedBy(rotation), new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            // Spawn vertical alley and door.
            for (int zOffset = smallRoomWallOffset; zOffset <= Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset - 1).RotatedBy(rotation), ref outpostData);
        }

        public static void GenerateSmallRoomMedibay(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(smallRoomWallOffset, 0, smallRoomWallOffset).RotatedBy(rotation), Genstep_GenerateOutpost.zoneSideSize - 2 * smallRoomWallOffset, Genstep_GenerateOutpost.zoneSideSize - 2 * smallRoomWallOffset, rotation, TerrainDefOf.Concrete, TerrainDef.Named("CarpetBlue"), ref outpostData);

            // Spawn medbeds and lamp.
            Building_Bed bed = OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData) as Building_Bed;
            bed.Medical = false; // Set as non medical or pawns may want to use it even if it is not owned by colony.
            bed = OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData) as Building_Bed;
            bed.Medical = false;
            bed = OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData) as Building_Bed;
            bed.Medical = false;
            bed = OG_Common.TrySpawnThingAt(ThingDef.Named("HospitalBed"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData) as Building_Bed;
            bed.Medical = false;
            OG_Common.TrySpawnResourceAt(ThingDefOf.Medicine, Rand.RangeInclusive(7, 32), rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 3).RotatedBy(rotation));
            Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 3).RotatedBy(rotation), TerrainDef.Named("MetalTile"));
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 4, 0, smallRoomWallOffset + 3).RotatedBy(rotation), Color.blue, ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 3).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 6, 0, smallRoomWallOffset + 3).RotatedBy(rotation), new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            // Spawn vertical alley and door.
            for (int zOffset = smallRoomWallOffset; zOffset <= Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset - 1).RotatedBy(rotation), ref outpostData);
        }

        public static void GenerateSmallRoomWeaponRoom(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(smallRoomWallOffset, 0, smallRoomWallOffset).RotatedBy(rotation), Genstep_GenerateOutpost.zoneSideSize - 2 * smallRoomWallOffset, Genstep_GenerateOutpost.zoneSideSize - 2 * smallRoomWallOffset, rotation, TerrainDefOf.Concrete, TerrainDef.Named("MetalTile"), ref outpostData);

            // Spawn weapon racks, weapons and lamps.
            Thing rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, outpostData.furnitureStuffDef, rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, outpostData.furnitureStuffDef, rotatedOrigin + new IntVec3(smallRoomWallOffset + 4, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, outpostData.furnitureStuffDef, rotatedOrigin + new IntVec3(smallRoomWallOffset + 2, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            TrySpawnWeaponOnRack(rack);
            rack = OG_Common.TrySpawnThingAt(ThingDefOf.EquipmentRack, outpostData.furnitureStuffDef, rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            TrySpawnWeaponOnRack(rack);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 3).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 3).RotatedBy(rotation), Color.white, ref outpostData);
            // Spawn vertical alley and door.
            for (int zOffset = smallRoomWallOffset; zOffset <= Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset - 1).RotatedBy(rotation), ref outpostData);
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
