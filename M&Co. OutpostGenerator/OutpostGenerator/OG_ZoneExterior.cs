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

            // Spawn floor.
            for (int xOffset = 0; xOffset < 11; xOffset++)
            {
                for (int zOffset = 0; zOffset < 11; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Soil);
                }
            }
            for (int xOffset = 0; xOffset < 11; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 5).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            for (int zOffset = 0; zOffset < 11; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
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

            // Spawn floor.
            for (int xOffset = 0; xOffset < 11; xOffset++)
            {
                for (int zOffset = 0; zOffset < 11; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Soil);
                }
            }
            for (int xOffset = 4; xOffset <= 6; xOffset++)
            {
                for (int zOffset = 0; zOffset < 11; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("TileGranite"));
                }
            }
            for (int zOffset = 0; zOffset < 11; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }

            // Spawn graves and tombstones.
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), ref outpostData);
            Building_Grave grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(1, 0, 5).RotatedBy(rotation), ref outpostData);
            grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(2, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(1, 0, 8).RotatedBy(rotation), ref outpostData);
            grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(2, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();

            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(9, 0, 2).RotatedBy(rotation), ref outpostData);
            grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(8, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(9, 0, 5).RotatedBy(rotation), ref outpostData);
            grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(8, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(9, 0, 8).RotatedBy(rotation), ref outpostData);
            grave = OG_Common.TrySpawnThingAt(ThingDefOf.Grave, null, rotatedOrigin + new IntVec3(8, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave.GetStoreSettings().filter.SetDisallowAll();

            // Spawn flower pots.
            grave = OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), outpostData.structureStuffDef, rotatedOrigin + new IntVec3(2, 0, 3).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave = OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), outpostData.structureStuffDef, rotatedOrigin + new IntVec3(3, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave = OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), outpostData.structureStuffDef, rotatedOrigin + new IntVec3(8, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
            grave = OG_Common.TrySpawnThingAt(ThingDef.Named("PlantPot"), outpostData.structureStuffDef, rotatedOrigin + new IntVec3(8, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData) as Building_Grave;
        }
    }
}
