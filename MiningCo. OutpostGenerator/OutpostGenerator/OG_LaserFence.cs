using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// OG_LaserFence class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_LaserFence
    {
        public static void GenerateLaserFence(ZoneProperties[,] zoneMap, ref OG_OutpostData outpostData)
        {
            if (OG_Util.IsModActive("MiningCo. LaserFence") == false)
            {
                Log.Warning("MiningCo. OutpostGenerator: MiningCo. LaserFence mod is not active. Cannot generate laser fences.");
                return;
            }

            int horizontalZonesNumber = 0;
            int verticalZonesNumber = 0;

            if (outpostData.size == OG_OutpostSize.SmallOutpost)
            {
                horizontalZonesNumber = OG_SmallOutpost.horizontalZonesNumber;
                verticalZonesNumber = OG_SmallOutpost.verticalZonesNumber;

                GenerateLaseFenceForSmallOutpost(zoneMap, horizontalZonesNumber, verticalZonesNumber, ref outpostData);
            }
            else
            {
                horizontalZonesNumber = OG_BigOutpost.horizontalZonesNumber;
                verticalZonesNumber = OG_BigOutpost.verticalZonesNumber;
                GenerateLaseFenceForBigOutpost(zoneMap, horizontalZonesNumber, verticalZonesNumber, ref outpostData);
            }
        }
        
        private static void GenerateLaseFenceForSmallOutpost(ZoneProperties[,] zoneMap, int horizontalZonesNumber, int verticalZonesNumber, ref OG_OutpostData outpostData)
        {
            for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
            {
                for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
                {
                    // Standard case: only generate laser fence on border zones.
                    if ((zoneOrd == 0) || (zoneOrd == verticalZonesNumber - 1) || (zoneAbs == 0) || (zoneAbs == horizontalZonesNumber - 1))
                    {
                        ZoneProperties zone = zoneMap[zoneOrd, zoneAbs];
                        IntVec3 zoneOrigin = Zone.GetZoneOrigin(outpostData.areaSouthWestOrigin, zoneAbs, zoneOrd);
                        IntVec3 zoneRotatedOrigin = Zone.GetZoneRotatedOrigin(outpostData.areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation);
                        IntVec3 laserFenceOrigin;
                        switch (zone.zoneType)
                        {
                            case ZoneType.NotYetGenerated:
                                if (zoneAbs == 0)
                                {
                                    // Try to spawn laser fence along absolute east side.
                                    laserFenceOrigin = zoneOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideSize, 0, 0);
                                    SpawnLaserFenceWithoutEntrance(laserFenceOrigin, Rot4.West, ref outpostData);
                                }
                                else if (zoneAbs == horizontalZonesNumber - 1)
                                {
                                    // Try to spawn laser fence along absolute west side.
                                    laserFenceOrigin = zoneOrigin + new IntVec3(-1, 0, Genstep_GenerateOutpost.zoneSideSize - 1);
                                    SpawnLaserFenceWithoutEntrance(laserFenceOrigin, Rot4.East, ref outpostData);
                                }
                                if (zoneOrd == 0)
                                {
                                    // Try to spawn laser fence along absolute north side.
                                    laserFenceOrigin = zoneOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideSize - 1, 0, Genstep_GenerateOutpost.zoneSideSize);
                                    SpawnLaserFenceWithoutEntrance(laserFenceOrigin, Rot4.South, ref outpostData);
                                }
                                else if (zoneOrd == verticalZonesNumber - 1)
                                {
                                    // Try to spawn laser fence along absolute south side.
                                    laserFenceOrigin = zoneOrigin + new IntVec3(0, 0, -1);
                                    SpawnLaserFenceWithoutEntrance(laserFenceOrigin, Rot4.North, ref outpostData);
                                }
                                break;

                            case ZoneType.SmallRoomBarracks:
                            case ZoneType.SmallRoomMedibay:
                            case ZoneType.SmallRoomWeaponRoom:
                            case ZoneType.SmallRoomBatteryRoom:
                                laserFenceOrigin = zoneRotatedOrigin + new IntVec3(0, 0, Genstep_GenerateOutpost.zoneSideSize - 1).RotatedBy(new Rot4(zone.rotation.AsInt));
                                SpawnLaserFenceWithEntrance(laserFenceOrigin, zone.rotation, 1, ref outpostData);
                                break;

                            case ZoneType.SmallRoomCommandRoom:
                                laserFenceOrigin = zoneRotatedOrigin + new IntVec3(0, 0, Genstep_GenerateOutpost.zoneSideSize - 1).RotatedBy(new Rot4(zone.rotation.AsInt));
                                SpawnLaserFenceWithoutEntrance(laserFenceOrigin, zone.rotation, ref outpostData);
                                // Try to spawn laser fence along linked side.
                                if (zone.linkedZoneRelativeRotation == Rot4.West)
                                {
                                    laserFenceOrigin = zoneRotatedOrigin;
                                }
                                else if (zone.linkedZoneRelativeRotation == Rot4.East)
                                {
                                    laserFenceOrigin = zoneRotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideSize - 1, 0, Genstep_GenerateOutpost.zoneSideSize - 1).RotatedBy(new Rot4(zone.rotation.AsInt));
                                }
                                SpawnLaserFenceWithEntrance(laserFenceOrigin, new Rot4(zone.rotation.AsInt + zone.linkedZoneRelativeRotation.AsInt), 1, ref outpostData);
                                break;

                            case ZoneType.SolarPanelZone:
                                laserFenceOrigin = zoneRotatedOrigin + new IntVec3(0, 0, Genstep_GenerateOutpost.zoneSideSize - 1).RotatedBy(new Rot4(zone.rotation.AsInt));
                                SpawnLaserFenceWithoutEntrance(laserFenceOrigin, zone.rotation, ref outpostData);
                                // Try to spawn laser fence along opposite linked side.
                                laserFenceOrigin = zoneRotatedOrigin;
                                if (zone.linkedZoneRelativeRotation == Rot4.West)
                                {
                                    laserFenceOrigin = zoneRotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideSize - 1, 0, Genstep_GenerateOutpost.zoneSideSize - 1).RotatedBy(new Rot4(zone.rotation.AsInt));
                                    SpawnLaserFenceWithoutEntrance(laserFenceOrigin, new Rot4(zone.rotation.AsInt + Rot4.East.AsInt), ref outpostData);
                                }
                                else if (zone.linkedZoneRelativeRotation == Rot4.East)
                                {
                                    laserFenceOrigin = zoneRotatedOrigin;
                                    SpawnLaserFenceWithoutEntrance(laserFenceOrigin, new Rot4(zone.rotation.AsInt + Rot4.West.AsInt), ref outpostData);
                                }
                                break;
                            case ZoneType.DropZone:
                                // Try to spawn laser fence along relative south side.
                                laserFenceOrigin = zoneRotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideSize - 1, 0, -1).RotatedBy(new Rot4(zone.rotation.AsInt));
                                SpawnLaserFenceWithoutEntrance(laserFenceOrigin, new Rot4(zone.rotation.AsInt + Rot4.South.AsInt), ref outpostData);
                                break;

                            case ZoneType.Empty:
                                laserFenceOrigin = zoneRotatedOrigin + new IntVec3(0, 0, Genstep_GenerateOutpost.zoneSideSize - 1).RotatedBy(new Rot4(zone.rotation.AsInt));
                                SpawnLaserFenceWithEntrance(laserFenceOrigin, zone.rotation, 1, ref outpostData);
                                break;
                            case ZoneType.SecondaryEntrance:
                                laserFenceOrigin = zoneRotatedOrigin + new IntVec3(0, 0, Genstep_GenerateOutpost.zoneSideSize - 1).RotatedBy(new Rot4(zone.rotation.AsInt));
                                SpawnLaserFenceWithEntrance(laserFenceOrigin, zone.rotation, 3, ref outpostData);
                                break;
                        }
                    }
                }
            }
        }

        private static void GenerateLaseFenceForBigOutpost(ZoneProperties[,] zoneMap, int horizontalZonesNumber, int verticalZonesNumber, ref OG_OutpostData outpostData)
        {
            for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
            {
                for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
                {
                    ZoneProperties zone = zoneMap[zoneOrd, zoneAbs];
                    IntVec3 zoneOrigin = Zone.GetZoneOrigin(outpostData.areaSouthWestOrigin, zoneAbs, zoneOrd);
                    IntVec3 zoneRotatedOrigin = Zone.GetZoneRotatedOrigin(outpostData.areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation);
                    if (zone.zoneType == ZoneType.MainEntrance)
                    {
                        SpawnLaserFenceTwoPylons(zoneRotatedOrigin + new IntVec3(0, 0, 5).RotatedBy(zone.rotation), new Rot4(Rot4.North.AsInt + zone.rotation.AsInt), 4, ref outpostData);
                        SpawnLaserFenceTwoPylons(zoneRotatedOrigin + new IntVec3(10, 0, 5).RotatedBy(zone.rotation), new Rot4(Rot4.North.AsInt + zone.rotation.AsInt), 4, ref outpostData);
                    }
                    else if (zone.zoneType == ZoneType.SecondaryEntrance)
                    {
                        SpawnLaserFenceThreePylons(zoneRotatedOrigin, new Rot4(Rot4.North.AsInt + zone.rotation.AsInt), 4, ref outpostData);
                        SpawnLaserFenceThreePylons(zoneRotatedOrigin + new IntVec3(10, 0, 0).RotatedBy(zone.rotation), new Rot4(Rot4.North.AsInt + zone.rotation.AsInt), 4, ref outpostData);
                        OG_Common.SpawnFireproofPowerConduitAt(zoneRotatedOrigin + new IntVec3(0, 0, -1).RotatedBy(zone.rotation), ref outpostData);
                        OG_Common.SpawnFireproofPowerConduitAt(zoneRotatedOrigin + new IntVec3(10, 0, -1).RotatedBy(zone.rotation), ref outpostData);
                    }
                    else
                    {
                        if (zoneOrd == 0)
                        {
                            if (Zone.IsZoneMediumRoom(zone.zoneType))
                            {
                                SpawnLaserFenceTwoPylons(zoneOrigin, Rot4.East, 2, ref outpostData);
                                SpawnLaserFenceTwoPylons(zoneOrigin + new IntVec3(10, 0, 0), Rot4.West, 2, ref outpostData);
                            }
                            else
                            {
                                SpawnLaserFenceThreePylons(zoneOrigin, Rot4.East, 4, ref outpostData);
                            }
                        }
                        if (zoneOrd == verticalZonesNumber - 1)
                        {
                            if (Zone.IsZoneMediumRoom(zone.zoneType))
                            {
                                SpawnLaserFenceTwoPylons(zoneOrigin + new IntVec3(0, 0, 10), Rot4.East, 2, ref outpostData);
                                SpawnLaserFenceTwoPylons(zoneOrigin + new IntVec3(10, 0, 10), Rot4.West, 2, ref outpostData);
                            }
                            else
                            {
                                SpawnLaserFenceThreePylons(zoneOrigin + new IntVec3(0, 0, 10), Rot4.East, 4, ref outpostData);
                            }
                        }
                        if (zoneAbs == 0)
                        {
                            if (Zone.IsZoneMediumRoom(zone.zoneType))
                            {
                                SpawnLaserFenceTwoPylons(zoneOrigin, Rot4.North, 2, ref outpostData);
                                SpawnLaserFenceTwoPylons(zoneOrigin + new IntVec3(0, 0, 10), Rot4.South, 2, ref outpostData);
                            }
                            else
                            {
                                SpawnLaserFenceThreePylons(zoneOrigin, Rot4.North, 4, ref outpostData);
                            }
                        }
                        if (zoneAbs == horizontalZonesNumber - 1)
                        {
                            if (Zone.IsZoneMediumRoom(zone.zoneType))
                            {
                                SpawnLaserFenceTwoPylons(zoneOrigin + new IntVec3(10, 0, 0), Rot4.North, 2, ref outpostData);
                                SpawnLaserFenceTwoPylons(zoneOrigin + new IntVec3(10, 0, 10), Rot4.South, 2, ref outpostData);
                            }
                            else
                            {
                                SpawnLaserFenceThreePylons(zoneOrigin + new IntVec3(10, 0, 0), Rot4.North, 4, ref outpostData);
                            }
                        }
                    }
                }
            }
        }
        
        private static void SpawnLaserFenceTwoPylons(IntVec3 laserFenceLeftOrigin, Rot4 rotation, int distanceBetweenPylons, ref OG_OutpostData outpostData)
        {
            OG_Common.TrySpawnLaserFencePylonAt(laserFenceLeftOrigin, ref outpostData);
            OG_Common.TrySpawnLaserFencePylonAt(laserFenceLeftOrigin + new IntVec3(0, 0, distanceBetweenPylons + 1).RotatedBy(rotation), ref outpostData);

            for (int zOffset = 0; zOffset <= 5; zOffset++)
            {
                IntVec3 position = laserFenceLeftOrigin + new IntVec3(0, 0, zOffset).RotatedBy(rotation);
                OG_Common.SpawnFireproofPowerConduitAt(position, ref outpostData);
                Find.TerrainGrid.SetTerrain(position, TerrainDefOf.Concrete);
            }
        }

        private static void SpawnLaserFenceThreePylons(IntVec3 laserFenceLeftOrigin, Rot4 rotation, int distanceBetweenPylons, ref OG_OutpostData outpostData)
        {
            OG_Common.TrySpawnLaserFencePylonAt(laserFenceLeftOrigin, ref outpostData);
            OG_Common.TrySpawnLaserFencePylonAt(laserFenceLeftOrigin + new IntVec3(0, 0, distanceBetweenPylons + 1).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnLaserFencePylonAt(laserFenceLeftOrigin + new IntVec3(0, 0, 2 * (distanceBetweenPylons + 1)).RotatedBy(rotation), ref outpostData);

            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                IntVec3 position = laserFenceLeftOrigin + new IntVec3(0, 0, zOffset).RotatedBy(rotation);
                OG_Common.SpawnFireproofPowerConduitAt(position, ref outpostData);
                Find.TerrainGrid.SetTerrain(position, TerrainDefOf.Concrete);
            }
        }

        private static void SpawnLaserFenceWithoutEntrance(IntVec3 laserFenceLeftOrigin, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            // Note: the fence is orthogonal to the parameter rotation.
            for (int xOffset = 0; xOffset < Genstep_GenerateOutpost.zoneSideSize; xOffset++)
            {
                IntVec3 position = laserFenceLeftOrigin + new IntVec3(xOffset, 0, 0).RotatedBy(new Rot4(rotation.AsInt));
                if ((xOffset == 0)
                    || (xOffset == Genstep_GenerateOutpost.zoneSideCenterOffset)
                    || (xOffset == Genstep_GenerateOutpost.zoneSideSize - 1))
                {
                    OG_Common.TrySpawnLaserFencePylonAt(position, ref outpostData);
                }
                OG_Common.SpawnFireproofPowerConduitAt(position, ref outpostData);
                Find.TerrainGrid.SetTerrain(position, TerrainDefOf.Concrete);
            }
        }

        private static void SpawnLaserFenceWithEntrance(IntVec3 laserFenceLeftOrigin, Rot4 rotation, int entranceWidth, ref OG_OutpostData outpostData)
        {
            // Note1: the fence is orthogonal to the parameter rotation.
            // Note2: the entrance width must be an odd number.
            int entranceWidthOffset = 1 + (entranceWidth / 2);

            for (int xOffset = 0; xOffset < Genstep_GenerateOutpost.zoneSideSize; xOffset++)
            {
                IntVec3 position = laserFenceLeftOrigin + new IntVec3(xOffset, 0, 0).RotatedBy(new Rot4(rotation.AsInt));
                if ((xOffset == 0)
                    || (xOffset == Genstep_GenerateOutpost.zoneSideCenterOffset - entranceWidthOffset)
                    || (xOffset == Genstep_GenerateOutpost.zoneSideCenterOffset + entranceWidthOffset)
                    || (xOffset == Genstep_GenerateOutpost.zoneSideSize - 1))
                {
                    LaserFence.Building_LaserFencePylon pylon = OG_Common.TrySpawnLaserFencePylonAt(position, ref outpostData);
                    if (pylon != null)
                    {
                        if (xOffset == Genstep_GenerateOutpost.zoneSideCenterOffset - entranceWidthOffset)
                        {
                            if (rotation == Rot4.North)
                            {
                                pylon.ToggleEastFenceStatus();
                            }
                            else if (rotation == Rot4.East)
                            {
                                pylon.ToggleSouthFenceStatus();
                            }
                            else if (rotation == Rot4.South)
                            {
                                pylon.ToggleWestFenceStatus();
                            }
                            else if (rotation == Rot4.West)
                            {
                                pylon.ToggleNorthFenceStatus();
                            }
                            pylon.SwitchLaserFence();
                        }
                        if (xOffset == Genstep_GenerateOutpost.zoneSideCenterOffset + entranceWidthOffset)
                        {
                            if (rotation == Rot4.North)
                            {
                                pylon.ToggleWestFenceStatus();
                            }
                            else if (rotation == Rot4.East)
                            {
                                pylon.ToggleNorthFenceStatus();
                            }
                            else if (rotation == Rot4.South)
                            {
                                pylon.ToggleEastFenceStatus();
                            }
                            else if (rotation == Rot4.West)
                            {
                                pylon.ToggleSouthFenceStatus();
                            }
                            pylon.SwitchLaserFence();
                        }
                    }
                }
                OG_Common.SpawnFireproofPowerConduitAt(position, ref outpostData);
                Find.TerrainGrid.SetTerrain(position, TerrainDefOf.Concrete);
                if (xOffset == Genstep_GenerateOutpost.zoneSideCenterOffset)
                {
                    Find.TerrainGrid.SetTerrain(position, TerrainDef.Named("PavedTile"));
                }
            }
        }
    }
}
