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
            OG_Common.SpawnPowerConduitAt(solarPanelZoneOrigin + new IntVec3(5, 0, 1), ref outpostData);
            OG_Common.SpawnPowerConduitAt(solarPanelZoneOrigin + new IntVec3(1, 0, 5), ref outpostData);
            OG_Common.SpawnPowerConduitAt(solarPanelZoneOrigin + new IntVec3(9, 0, 5), ref outpostData);
            OG_Common.SpawnPowerConduitAt(solarPanelZoneOrigin + new IntVec3(5, 0, 9), ref outpostData);
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
    }
}
