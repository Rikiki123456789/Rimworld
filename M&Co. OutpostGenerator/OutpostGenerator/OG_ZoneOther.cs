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
    /// OG_ZoneOther class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_ZoneOther
    {
        public static void GenerateEmptyZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);
            for (int xOffset = Genstep_GenerateOutpost.zoneSideCenterOffset - 2; xOffset <= Genstep_GenerateOutpost.zoneSideCenterOffset + 2; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 3).RotatedBy(rotation), TerrainDefOf.Concrete);
            }
            Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, 3).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
        }

        public static void GenerateSecondaryEntranceZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            // Spawn sandbags and floor.
            IntVec3 sandbagCornerCenterPosition = rotatedOrigin + new IntVec3(3, 0, 3).RotatedBy(rotation);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagCornerCenterPosition + new IntVec3(-1, 0, -1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagCornerCenterPosition + new IntVec3(-1, 0, 0).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagCornerCenterPosition + new IntVec3(-1, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagCornerCenterPosition + new IntVec3(0, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagCornerCenterPosition + new IntVec3(1, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            for (int i = 0; i < GenAdj.AdjacentCellsAndInside.Length; i++)
            {
                Find.TerrainGrid.SetTerrain(sandbagCornerCenterPosition + GenAdj.AdjacentCellsAndInside[i], TerrainDefOf.Concrete);
            }
            sandbagCornerCenterPosition = rotatedOrigin + new IntVec3(7, 0, 3).RotatedBy(rotation);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagCornerCenterPosition + new IntVec3(1, 0, -1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagCornerCenterPosition + new IntVec3(1, 0, 0).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagCornerCenterPosition + new IntVec3(1, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagCornerCenterPosition + new IntVec3(0, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagCornerCenterPosition + new IntVec3(-1, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            for (int i = 0; i < GenAdj.AdjacentCellsAndInside.Length; i++)
            {
                Find.TerrainGrid.SetTerrain(sandbagCornerCenterPosition + GenAdj.AdjacentCellsAndInside[i], TerrainDefOf.Concrete);
            }
            // Spawn improvised turret and sandbags.
            IntVec3 turretPosition = rotatedOrigin + new IntVec3(2, 0, 7).RotatedBy(rotation);
            OG_Common.TrySpawnThingAt(ThingDef.Named("TurretGun"), ThingDefOf.Steel, turretPosition, true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, turretPosition + new IntVec3(-1, 0, 0).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, turretPosition + new IntVec3(-1, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, turretPosition + new IntVec3(0, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, turretPosition + new IntVec3(1, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, turretPosition + new IntVec3(1, 0, 0).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            for (int i = 0; i < GenAdj.AdjacentCellsAndInside.Length; i++)
            {
                Find.TerrainGrid.SetTerrain(turretPosition + GenAdj.AdjacentCellsAndInside[i], TerrainDefOf.Concrete);
            }
            turretPosition = rotatedOrigin + new IntVec3(8, 0, 7).RotatedBy(rotation);
            OG_Common.TrySpawnThingAt(ThingDef.Named("TurretGun"), ThingDefOf.Steel, turretPosition, true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, turretPosition + new IntVec3(-1, 0, 0).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, turretPosition + new IntVec3(-1, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, turretPosition + new IntVec3(0, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, turretPosition + new IntVec3(1, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, turretPosition + new IntVec3(1, 0, 0).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            for (int i = 0; i < GenAdj.AdjacentCellsAndInside.Length; i++)
            {
                Find.TerrainGrid.SetTerrain(turretPosition + GenAdj.AdjacentCellsAndInside[i], TerrainDefOf.Concrete);
            }
            // Spawn floor.
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int zOffset = 3; zOffset < Genstep_GenerateOutpost.zoneSideSize; zOffset++)
                {
                    TerrainDef terrain = TerrainDefOf.Concrete;
                    if (xOffset == 0)
                    {
                        terrain = TerrainDef.Named("PavedTile");
                    }
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset + xOffset, 0, zOffset).RotatedBy(rotation), terrain);
                }
            }
        }
    }
}
