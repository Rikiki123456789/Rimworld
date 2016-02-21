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
            // TODO: this is a door step of a little sas, not an empty zone!!! ;-)
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);
            for (int xOffset = Genstep_GenerateOutpost.zoneSideCenterOffset - 2; xOffset <= Genstep_GenerateOutpost.zoneSideCenterOffset + 2; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 3).RotatedBy(rotation), TerrainDefOf.Concrete);
            }
            Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, 3).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
        }

        public static void GenerateMainEntranceZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);
            IntVec3 cell = rotatedOrigin;

            // Spawn concrete floor.
            for (int xOffset = -1; xOffset <= 11; xOffset++)
            {
                for (int zOffset = -1; zOffset <= 5; zOffset++)
                {
                    cell = rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation);
                    Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
                }
            }

            // Generate central paved alley.
            for (int xOffset = 4; xOffset <= 6; xOffset++)
            {
                for (int zOffset = 0; zOffset <= 10; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            for (int zOffset = -1; zOffset <= 10; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }

            // Generate south west turret.
            IntVec3 southWestOrigin = rotatedOrigin;
            for (int xOffset = 0; xOffset < 5; xOffset++)
            {
                for (int zOffset = 0; zOffset < 5; zOffset++)
                {
                    cell = southWestOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation);
                    if ((xOffset == 0)
                        || (zOffset == 0))
                    {
                        OG_Common.TrySpawnWallAt(cell, ref outpostData);
                        OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                    }
                }
            }
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southWestOrigin + new IntVec3(1, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southWestOrigin + new IntVec3(2, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southWestOrigin + new IntVec3(3, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southWestOrigin + new IntVec3(4, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southWestOrigin + new IntVec3(4, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.VulcanTurretDef, ThingDefOf.Steel, southWestOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            // Generate south east turret.
            IntVec3 southEastOrigin = rotatedOrigin;
            for (int xOffset = 6; xOffset < 11; xOffset++)
            {
                for (int zOffset = 0; zOffset < 5; zOffset++)
                {
                    cell = southEastOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation);
                    if ((xOffset == 10)
                        || (zOffset == 0))
                    {
                        OG_Common.TrySpawnWallAt(cell, ref outpostData);
                        OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                    }
                }
            }
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southEastOrigin + new IntVec3(6, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southEastOrigin + new IntVec3(6, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southEastOrigin + new IntVec3(7, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southEastOrigin + new IntVec3(8, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southEastOrigin + new IntVec3(9, 0, 4).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.VulcanTurretDef, ThingDefOf.Steel, southEastOrigin + new IntVec3(7, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            // Generate central door.
            cell = rotatedOrigin + new IntVec3(5, 0, 0).RotatedBy(rotation);
            OG_Common.SpawnDoorAt(cell, ref outpostData);
            OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);


            // Generate ironed roof.
            for (int xOffset = 0; xOffset < 11; xOffset++)
            {
                for (int zOffset = 0; zOffset < 5; zOffset++)
                {
                    Find.RoofGrid.SetRoof(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), OG_Util.IronedRoofDef);
                }
            }
        }

        public static void GenerateSecondaryEntranceZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            // Generate central paved alley.
            for (int xOffset = 4; xOffset <= 6; xOffset++)
            {
                for (int zOffset = 0; zOffset < Genstep_GenerateOutpost.zoneSideSize; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            for (int zOffset = 0; zOffset < Genstep_GenerateOutpost.zoneSideSize; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }

            // Generate north west turret.
            IntVec3 northWestOrigin = rotatedOrigin + new IntVec3(1, 0, 6).RotatedBy(rotation);
            for (int xOffset = 0; xOffset < 4; xOffset++)
            {
                for (int zOffset = 0; zOffset < 4; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(northWestOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(0, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(3, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(0, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(1, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(2, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(3, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.VulcanTurretDef, ThingDefOf.Steel, northWestOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            // Generate north east turret.
            IntVec3 norththEastOrigin = rotatedOrigin + new IntVec3(6, 0, 6).RotatedBy(rotation);
            for (int xOffset = 0; xOffset < 4; xOffset++)
            {
                for (int zOffset = 0; zOffset < 4; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(norththEastOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, norththEastOrigin + new IntVec3(0, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, norththEastOrigin + new IntVec3(3, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, norththEastOrigin + new IntVec3(0, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, norththEastOrigin + new IntVec3(1, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, norththEastOrigin + new IntVec3(2, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, norththEastOrigin + new IntVec3(3, 0, 3).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(OG_Util.VulcanTurretDef, ThingDefOf.Steel, norththEastOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            // Spawn south west sandbags and floor.
            IntVec3 southWestOrigin = rotatedOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation);
            for (int xOffset = 0; xOffset < 3; xOffset++)
            {
                for (int zOffset = 0; zOffset < 3; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(southWestOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southWestOrigin + new IntVec3(0, 0, 0).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southWestOrigin + new IntVec3(0, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southWestOrigin + new IntVec3(0, 0, 2).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southWestOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southWestOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), false, Rot4.North, ref outpostData);

            // Spawn south east sandbags and floor.
            IntVec3 southEastOrigin = rotatedOrigin + new IntVec3(6, 0, 1).RotatedBy(rotation);
            for (int xOffset = 0; xOffset < 3; xOffset++)
            {
                for (int zOffset = 0; zOffset < 3; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(southEastOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southEastOrigin + new IntVec3(2, 0, 0).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southEastOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southEastOrigin + new IntVec3(0, 0, 2).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southEastOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), false, Rot4.North, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, southEastOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), false, Rot4.North, ref outpostData);

        }

        public static void GenerateEntranchedZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);
            IntVec3 cell = rotatedOrigin;

            // Generate south west battery shelter.
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(0, 0, 0).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(3, 0, 0).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(0, 0, 3).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(3, 0, 3).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            for (int xOffset = 0; xOffset < 4; xOffset++)
            {
                for (int zOffset = 0; zOffset < 4; zOffset++)
                {
                    cell = rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation);
                    Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
                    Find.RoofGrid.SetRoof(cell, OG_Util.IronedRoofDef);
                }
            }

            // Generate south east battery shelter.
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(7, 0, 0).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(10, 0, 0).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(7, 0, 3).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnWallAt(rotatedOrigin + new IntVec3(10, 0, 3).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(8, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(9, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            for (int xOffset = 7; xOffset < 11; xOffset++)
            {
                for (int zOffset = 0; zOffset < 4; zOffset++)
                {
                    cell = rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation);
                    Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
                    Find.RoofGrid.SetRoof(cell, OG_Util.IronedRoofDef);
                }
            }

            // Generate central paved alley.
            for (int xOffset = 4; xOffset <= 6; xOffset++)
            {
                for (int zOffset = 0; zOffset < 7; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            for (int zOffset = 0; zOffset < 7; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            
            // Generate north west force field.
            IntVec3 northWestOrigin = rotatedOrigin + new IntVec3(0, 0, 6).RotatedBy(rotation);
            for (int xOffset = 0; xOffset < 5; xOffset++)
            {
                for (int zOffset = 0; zOffset < 2; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(northWestOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            for (int xOffset = 1; xOffset <= 3; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(northWestOrigin + new IntVec3(xOffset, 0, 2).RotatedBy(rotation), TerrainDefOf.Concrete);
            }
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(0, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(3, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(3, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northWestOrigin + new IntVec3(4, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            if (ModsConfig.IsActive("M&Co. ForceField"))
            {
                OG_Common.TrySpawnThingAt(ThingDef.Named("ForceFieldGenerator"), null, northWestOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            }
            for (int zOffset = -3; zOffset <= 3; zOffset++)
            {
                OG_Common.SpawnFireproofPowerConduitAt(northWestOrigin + new IntVec3(2, 0, zOffset).RotatedBy(rotation), ref outpostData);
            }

            // Generate central sandbag.
            cell = rotatedOrigin + new IntVec3(5, 0, 7).RotatedBy(rotation);
            Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
            
            // Generate north east force field.
            IntVec3 northEastOrigin = rotatedOrigin + new IntVec3(6, 0, 6).RotatedBy(rotation);
            for (int xOffset = 0; xOffset < 5; xOffset++)
            {
                for (int zOffset = 0; zOffset < 2; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(northEastOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            for (int xOffset = 1; xOffset <= 3; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(northEastOrigin + new IntVec3(xOffset, 0, 2).RotatedBy(rotation), TerrainDefOf.Concrete);
            }
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northEastOrigin + new IntVec3(0, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northEastOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northEastOrigin + new IntVec3(1, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northEastOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northEastOrigin + new IntVec3(3, 0, 2).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northEastOrigin + new IntVec3(3, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, northEastOrigin + new IntVec3(4, 0, 1).RotatedBy(rotation), false, Rot4.Invalid, ref outpostData);
            if (ModsConfig.IsActive("M&Co. ForceField"))
            {
                OG_Common.TrySpawnThingAt(ThingDef.Named("ForceFieldGenerator"), null, northEastOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            }
            for (int zOffset = -3; zOffset <= 3; zOffset++)
            {
                OG_Common.SpawnFireproofPowerConduitAt(northEastOrigin + new IntVec3(2, 0, zOffset).RotatedBy(rotation), ref outpostData);
            }
        }

        public static void GenerateSamSiteZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            // Spawn SAM site surrounded by sandbags.
            IntVec3 samSitePosition = rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset);
            OG_Common.TrySpawnThingAt(OG_Util.SamSiteDef, null, samSitePosition, false, Rot4.Invalid, ref outpostData);
            CellRect sandbagRect = new CellRect(rotatedOrigin.x + 3, rotatedOrigin.z + 3, 5, 5);
            foreach (IntVec3 cell in sandbagRect.Cells)
            {
                if ((cell.x == sandbagRect.minX) || (cell.x == sandbagRect.maxX) || (cell.z == sandbagRect.minZ) || (cell.z == sandbagRect.maxZ))
                {
                    OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
                }
            }
            for (int rotationAsInt = Rot4.North.AsInt; rotationAsInt <= Rot4.West.AsInt; rotationAsInt++)
            {
                IntVec3 sandbagPosition = samSitePosition + new IntVec3(0, 0, 3).RotatedBy(new Rot4(rotationAsInt));
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, sandbagPosition, false, Rot4.Invalid, ref outpostData);
                foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(sandbagPosition))
                {
                    Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
                }
            }

            // Generate concrete ground.
            CellRect concreteRect = new CellRect(rotatedOrigin.x + 2, rotatedOrigin.z + 2, 7, 7);
            foreach (IntVec3 cell in concreteRect.Cells)
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
            }
        }

        public static void GenerateBigSasZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, bool generateSecondarySas, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);
            IntVec3 cell = rotatedOrigin;

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(3, 0, -1).RotatedBy(rotation), 5, 13, rotation, null, null, ref outpostData);
            
            // Spawn lamps.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(4, 0, 2).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(4, 0, 8).RotatedBy(rotation), Color.white, ref outpostData);

            // Spawn floor and roof.
            for (int xOffset = 2; xOffset <= 8; xOffset++)
            {
                for (int zOffset = 0; zOffset < 11; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            for (int zOffset = 0; zOffset < 11; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }

            // Spawn doors.
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, -1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(5, 0, 11).RotatedBy(rotation), ref outpostData);

            // Spawn secondary sas.
            if (generateSecondarySas)
            {
                OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(3, 0, 4).RotatedBy(rotation), 3, 6, new Rot4(rotation.AsInt + Rot4.West.AsInt), TerrainDefOf.Concrete, null, ref outpostData);
                
                // Spawn paved alley.
                for (int xOffset = -2; xOffset <= 4; xOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, 5).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
                }
                // Spawn doors.
                OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(-2, 0, 5).RotatedBy(rotation), ref outpostData);
                OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(3, 0, 5).RotatedBy(rotation), ref outpostData);
            }
        }

        public static void GeneratePlazaZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            // Generate paved floor.
            for (int xOffset = 3; xOffset <= 7; xOffset++)
            {
                for (int zOffset = 0; zOffset <= 10; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset), TerrainDef.Named("TileGranite"));
                }
            }
            for (int xOffset = 2; xOffset <= 8; xOffset++)
            {
                for (int zOffset = 1; zOffset <= 9; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset), TerrainDef.Named("TileGranite"));
                }
            }
            for (int xOffset = 1; xOffset <= 9; xOffset++)
            {
                for (int zOffset = 2; zOffset <= 8; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset), TerrainDef.Named("TileGranite"));
                }
            }
            for (int xOffset = 0; xOffset <= 10; xOffset++)
            {
                for (int zOffset = 3; zOffset <= 7; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, zOffset), TerrainDef.Named("TileGranite"));
                }
            }

            // Generate central paved alley.
            IntVec3 centralPylonPosition = rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset);
            for (int zOffset = 0; zOffset < Genstep_GenerateOutpost.zoneSideSize; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, zOffset), TerrainDef.Named("PavedTile"));
            }
            for (int xOffset = 0; xOffset < Genstep_GenerateOutpost.zoneSideSize; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset), TerrainDef.Named("PavedTile"));
            }
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(centralPylonPosition))
            {
                Find.TerrainGrid.SetTerrain(cell, TerrainDef.Named("PavedTile"));
            }

            // Generate pylon and alert speakers.
            OG_Common.TrySpawnWallAt(centralPylonPosition, ref outpostData);
            if (ModsConfig.IsActive("M&Co. AlertSpeaker"))
            {
                for (int rotationAsInt = 0; rotationAsInt < 4; rotationAsInt++)
                {
                    Rot4 alertSpeakerRotation = new Rot4(rotationAsInt);
                    OG_Common.TrySpawnThingAt(ThingDef.Named("AlertSpeaker"), null, centralPylonPosition + new IntVec3(0, 0, 1).RotatedBy(alertSpeakerRotation), true, alertSpeakerRotation, ref outpostData);
                }
            }

            // Generate sandbags and power conduit.
            for (int rotationAsInt = 0; rotationAsInt < 4; rotationAsInt++)
            {
                Rot4 quarterRotation = new Rot4(rotationAsInt);
                IntVec3 quarterSouthWestOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, quarterRotation);

                IntVec3 cell = quarterSouthWestOrigin + new IntVec3(0, 0, 4).RotatedBy(quarterRotation);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                cell = quarterSouthWestOrigin + new IntVec3(0, 0, 3).RotatedBy(quarterRotation);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                cell = quarterSouthWestOrigin + new IntVec3(1, 0, 3).RotatedBy(quarterRotation);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                cell = quarterSouthWestOrigin + new IntVec3(1, 0, 2).RotatedBy(quarterRotation);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                cell = quarterSouthWestOrigin + new IntVec3(2, 0, 2).RotatedBy(quarterRotation);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                cell = quarterSouthWestOrigin + new IntVec3(2, 0, 1).RotatedBy(quarterRotation);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                cell = quarterSouthWestOrigin + new IntVec3(3, 0, 1).RotatedBy(quarterRotation);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                cell = quarterSouthWestOrigin + new IntVec3(3, 0, 0).RotatedBy(quarterRotation);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                cell = quarterSouthWestOrigin + new IntVec3(4, 0, 0).RotatedBy(quarterRotation);
                OG_Common.TrySpawnThingAt(ThingDefOf.Sandbags, null, cell, false, Rot4.Invalid, ref outpostData);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                cell = quarterSouthWestOrigin + new IntVec3(5, 0, 0).RotatedBy(quarterRotation);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
            }
        }

        public static void GenerateStraightAlleyZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            // Spawn lamps.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(4, 0, 2).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(4, 0, 8).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(6, 0, 2).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(6, 0, 8).RotatedBy(rotation), Color.white, ref outpostData);

            // Generate central paved alley.
            for (int zOffset = 0; zOffset < Genstep_GenerateOutpost.zoneSideSize; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(5, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }

            // Generate concrete border and power conduit.
            for (int zOffset = 0; zOffset < Genstep_GenerateOutpost.zoneSideSize; zOffset++)
            {
                IntVec3 cell = rotatedOrigin + new IntVec3(4, 0, zOffset).RotatedBy(rotation);
                Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
            }
            for (int zOffset = 0; zOffset < Genstep_GenerateOutpost.zoneSideSize; zOffset++)
            {
                IntVec3 cell = rotatedOrigin + new IntVec3(6, 0, zOffset).RotatedBy(rotation);
                Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
                OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
            }
        }
    }
}
