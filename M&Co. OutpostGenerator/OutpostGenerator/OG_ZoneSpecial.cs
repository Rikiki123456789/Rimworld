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
    /// OG_ZoneSpecial class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_ZoneSpecial
    {
        const int smallRoomWallOffset = 2;

        public static void GenerateSolarPanelZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, ref OG_OutpostData outpostData)
        {
            IntVec3 solarPanelZoneOrigin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);

            OG_Common.TrySpawnThingAt(ThingDefOf.SolarGenerator, null, solarPanelZoneOrigin + new IntVec3(2, 0, 2), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.SolarGenerator, null, solarPanelZoneOrigin + new IntVec3(7, 0, 2), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.SolarGenerator, null, solarPanelZoneOrigin + new IntVec3(2, 0, 7), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.SolarGenerator, null, solarPanelZoneOrigin + new IntVec3(7, 0, 7), false, Rot4.North, ref outpostData);
            OG_Common.SpawnFireproofPowerConduitAt(solarPanelZoneOrigin + new IntVec3(5, 0, 1), ref outpostData);
            OG_Common.SpawnFireproofPowerConduitAt(solarPanelZoneOrigin + new IntVec3(5, 0, 0), ref outpostData);
            OG_Common.SpawnFireproofPowerConduitAt(solarPanelZoneOrigin + new IntVec3(1, 0, 5), ref outpostData);
            OG_Common.SpawnFireproofPowerConduitAt(solarPanelZoneOrigin + new IntVec3(0, 0, 5), ref outpostData);
            OG_Common.SpawnFireproofPowerConduitAt(solarPanelZoneOrigin + new IntVec3(9, 0, 5), ref outpostData);
            OG_Common.SpawnFireproofPowerConduitAt(solarPanelZoneOrigin + new IntVec3(10, 0, 5), ref outpostData);
            OG_Common.SpawnFireproofPowerConduitAt(solarPanelZoneOrigin + new IntVec3(5, 0, 9), ref outpostData);
            OG_Common.SpawnFireproofPowerConduitAt(solarPanelZoneOrigin + new IntVec3(5, 0, 10), ref outpostData);
        }

        public static void GenerateDropZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, ref OG_OutpostData outpostData)
        {
            IntVec3 dropZoneOrigin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 sandbagsOrigin = dropZoneOrigin + new IntVec3(1, 0, 1);

            CellRect rect = new CellRect(sandbagsOrigin.x, sandbagsOrigin.z, Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset, Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset);
            foreach (IntVec3 cell in rect.Cells)
            {
                if (((cell.x == rect.minX) || (cell.x == rect.maxX) || (cell.z == rect.minZ) || (cell.z == rect.maxZ))
                    && ((cell.x != rect.Center.x) && (cell.z != rect.Center.z)))
                {
                    OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.North, ref outpostData);
                }
                Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
            }
            OG_Common.GenerateHorizontalAndVerticalPavedAlleys(dropZoneOrigin);
            OG_Common.TrySpawnThingAt(ThingDef.Named("OrbitalTradeBeacon"), null, rect.Center, false, Rot4.North, ref outpostData);
            Find.TerrainGrid.SetTerrain(rect.Center, TerrainDef.Named("MetalTile"));

            outpostData.dropZoneCenter = rect.Center;
        }

        public static void GenerateLandingPadBottom(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            TerrainDef c = TerrainDefOf.Concrete;
            TerrainDef mt = TerrainDef.Named("MetalTile");
            TerrainDef pt = TerrainDef.Named("PavedTile");

            TerrainDef[,] landingPadBottomPattern = new TerrainDef[11, 11]
            {
                {null, null, null, null, null, null, null, null, null, null, null},
                {null, null, mt,   mt,   mt,   mt,   mt,   mt,   mt,   null, null},
                {null, null, mt,   c,    c,    c,    c,    c,    mt,   null, null},
                {null, mt,   mt,   c,    c,    c,    c,    c,    mt,   mt,   null},
                {null, mt,   c,    c,    c,    c,    c,    c,    c,    mt,   null},
                {pt,   pt,   c,    c,    c,    c,    c,    c,    c,    pt,   pt},
                {null, mt,   c,    mt,   c,    c,    c,    mt,   c,    mt,   null},
                {mt,   mt,   c,    c,    c,    c,    c,    c,    c,    mt,   mt},
                {mt,   c,    c,    c,    c,    c,    c,    c,    c,    c,    mt},
                {mt,   c,    c,    c,    c,    c,    c,    c,    c,    c,    mt},
                {mt,   c,    c,    c,    c,    c,    c,    c,    c,    c,    mt}
            };

            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            // Spawn floor.
            for (int xOffset = 0; xOffset <= 10; xOffset++)
            {
                for (int zOffset = 0; zOffset <= 10; zOffset++)
                {
                    TerrainDef terrain = landingPadBottomPattern[zOffset, xOffset];
                    if (terrain != null)
                    {
                        Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), terrain);
                    }
                }
            }

            // Spawn power conduits.
            for (int xOffset = 0; xOffset <= 10; xOffset++)
            {
                OG_Common.SpawnFireproofPowerConduitAt(rotatedOrigin + new IntVec3(xOffset, 0, 0).RotatedBy(rotation), ref outpostData);
            }
            for (int zOffset = 0; zOffset <= 10; zOffset++)
            {
                OG_Common.SpawnFireproofPowerConduitAt(rotatedOrigin + new IntVec3(0, 0, zOffset).RotatedBy(rotation), ref outpostData);
                OG_Common.SpawnFireproofPowerConduitAt(rotatedOrigin + new IntVec3(10, 0, zOffset).RotatedBy(rotation), ref outpostData);
            }

            // Spawn landing pad lamps.
            Building_LandingPadBeacon beacon = OG_Common.TrySpawnThingAt(OG_Util.LandingPadBeaconDef, null, rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData) as Building_LandingPadBeacon;
            beacon = OG_Common.TrySpawnThingAt(OG_Util.LandingPadBeaconDef, null, rotatedOrigin + new IntVec3(9, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData) as Building_LandingPadBeacon;

            beacon = OG_Common.TrySpawnThingAt(OG_Util.LandingPadBeaconDef, null, rotatedOrigin + new IntVec3(1, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData) as Building_LandingPadBeacon;
            beacon.SetFlashStartOffset(Building_LandingPadBeacon.flashDurationInTicks);
            beacon = OG_Common.TrySpawnThingAt(OG_Util.LandingPadBeaconDef, null, rotatedOrigin + new IntVec3(9, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData) as Building_LandingPadBeacon;
            beacon.SetFlashStartOffset(Building_LandingPadBeacon.flashDurationInTicks);
            beacon = OG_Common.TrySpawnThingAt(OG_Util.LandingPadBeaconDef, null, rotatedOrigin + new IntVec3(1, 0, 7).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData) as Building_LandingPadBeacon;
            beacon.SetFlashStartOffset(2 * Building_LandingPadBeacon.flashDurationInTicks);
            beacon = OG_Common.TrySpawnThingAt(OG_Util.LandingPadBeaconDef, null, rotatedOrigin + new IntVec3(9, 0, 7).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData) as Building_LandingPadBeacon;
            beacon.SetFlashStartOffset(2 * Building_LandingPadBeacon.flashDurationInTicks);
            beacon = OG_Common.TrySpawnThingAt(OG_Util.LandingPadBeaconDef, null, rotatedOrigin + new IntVec3(1, 0, 10).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData) as Building_LandingPadBeacon;
            beacon.SetFlashStartOffset(3 * Building_LandingPadBeacon.flashDurationInTicks);
            beacon = OG_Common.TrySpawnThingAt(OG_Util.LandingPadBeaconDef, null, rotatedOrigin + new IntVec3(9, 0, 10).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData) as Building_LandingPadBeacon;
            beacon.SetFlashStartOffset(3 * Building_LandingPadBeacon.flashDurationInTicks);
        }
    }
}
