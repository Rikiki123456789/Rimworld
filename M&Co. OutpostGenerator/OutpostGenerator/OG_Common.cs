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
    /// OG_Common class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_Common
    {
        public static void GenerateEmptyRoomAt(IntVec3 rotatedOrigin,
            int width,
            int height,
            Rot4 rotation,
            TerrainDef defaultFloorDef,
            TerrainDef interiorFloorDef,
            ref OG_OutpostData outpostData)
        {
            CellRect rect = new CellRect(rotatedOrigin.x, rotatedOrigin.z, width, height);
            if (rotation == Rot4.North)
            {
                rect = new CellRect(rotatedOrigin.x, rotatedOrigin.z, width, height);
            }
            else if (rotation == Rot4.East)
            {
                rect = new CellRect(rotatedOrigin.x, rotatedOrigin.z - width + 1, height, width);
            }
            else if (rotation == Rot4.South)
            {
                rect = new CellRect(rotatedOrigin.x - width + 1, rotatedOrigin.z - height + 1, width, height);
            }
            else
            {
                rect = new CellRect(rotatedOrigin.x - height + 1, rotatedOrigin.z, height, width);
            }
            // Generate 4 walls and power conduits.
            foreach (IntVec3 cell in rect.Cells)
            {
                if ((cell.x == rect.minX) || (cell.x == rect.maxX) || (cell.z == rect.minZ) || (cell.z == rect.maxZ))
                {
                    OG_Common.TrySpawnWallAt(cell, ref outpostData);
                    OG_Common.SpawnPowerConduitAt(cell, ref outpostData);
                }
            }
            // Trigger to unfog area when a pawn enters the room.
            SpawnRectTriggerUnfogArea(rect, ref outpostData.triggerIntrusion);
            // Generate roof.
            foreach (IntVec3 cell in rect.Cells)
            {
                Find.RoofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
            }
            // Generate room default floor.
            if (defaultFloorDef != null)
            {
                foreach (IntVec3 cell in rect.ExpandedBy(1).Cells)
                {
                    Find.TerrainGrid.SetTerrain(cell, defaultFloorDef);
                }
            }
            // Generate room interior floor.
            if (interiorFloorDef != null)
            {
                foreach (IntVec3 cell in rect.ContractedBy(1).Cells)
                {
                    Find.TerrainGrid.SetTerrain(cell, interiorFloorDef);
                }
            }
        }

        public static void GenerateHorizontalAndVerticalPavedAlleys(IntVec3 origin)
        {
            for (int xOffset = 0; xOffset < Genstep_GenerateOutpost.zoneSideSize; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(origin + new IntVec3(xOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset), TerrainDef.Named("PavedTile"));
            }
            for (int zOffset = 0; zOffset < Genstep_GenerateOutpost.zoneSideSize; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(origin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, zOffset), TerrainDef.Named("PavedTile"));
            }
        }

        public static void SpawnRectTriggerUnfogArea(CellRect rect, ref TriggerIntrusion triggerIntrusion)
        {
            RectTriggerUnfogArea rectTrigger = (RectTriggerUnfogArea)ThingMaker.MakeThing(ThingDef.Named("RectTriggerUnfogArea"));
            rectTrigger.Rect = rect;
            GenSpawn.Spawn(rectTrigger, rect.Center);

            // Update the trigger intrusion watched cells.
            foreach (IntVec3 cell in rect.Cells)
            {
                if (triggerIntrusion.watchedCells.Contains(cell) == false)
                {
                    triggerIntrusion.watchedCells.Add(cell);
                }
            }
        }

        public static void TrySpawnResourceStock(IntVec3 rotatedOrigin, Rot4 rotation, ThingDef resourceDef, int stockWidth, int stockHeight, float spawnStockChance, int minQuantity, int maxQuantity)
        {
            for (int xOffset = 0; xOffset < stockWidth; xOffset++)
            {
                for (int zOffset = 0; zOffset < stockHeight; zOffset++)
                {
                    if (Rand.Value < spawnStockChance)
                    {
                        TrySpawnResourceAt(resourceDef, Rand.RangeInclusive(minQuantity, maxQuantity), rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation));
                    }
                }
            }
        }

        public static void TrySpawnResourceAt(ThingDef resourceDef, int quantity, IntVec3 position)
        {
            if (position.GetEdifice() != null)
            {
                return;
            }
            Thing thing = ThingMaker.MakeThing(resourceDef);
            thing.stackCount = quantity;
            thing.SetForbidden(true);
            GenSpawn.Spawn(thing, position);
        }

        public static Thing TrySpawnThingAt(ThingDef thingDef,
            ThingDef stuffDef,
            IntVec3 position,
            bool rotated,
            Rot4 rotation,
            ref OG_OutpostData outpostData,
            bool destroyThings = false,
            bool replaceStructure = false)
        {
            if (destroyThings)
            {
                List<Thing> thingList = position.GetThingList();
                for (int j = thingList.Count - 1; j >= 0; j--)
                {
                    thingList[j].Destroy(DestroyMode.Vanish);
                }
            }
            Building building = position.GetEdifice();
            if (building != null)
            {
                if (replaceStructure)
                {
                    if (outpostData.outpostThingList.Contains(building))
                    {
                        outpostData.outpostThingList.Remove(building);
                    }
                    building.Destroy(DestroyMode.Vanish);
                }
                else
                {
                    return null;
                }
            }
            Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);
            outpostData.outpostThingList.Add(thing);
            thing.SetFaction(outpostData.faction);
            if (rotated && thingDef.rotatable)
            {
                return GenSpawn.Spawn(thing, position, rotation);
            }
            else
            {
                // TODO: improve this special case treatment.
                if (thingDef == ThingDef.Named("TableShort"))
                {
                    if (rotation == Rot4.East)
                    {
                        position += new IntVec3(0, 0, -1);
                    }
                    else if (rotation == Rot4.South)
                    {
                        position += new IntVec3(-1, 0, -1);
                    }
                    else if (rotation == Rot4.West)
                    {
                        position += new IntVec3(-1, 0, 0);
                    }
                }
                return GenSpawn.Spawn(thing, position);
            }
        }

        public static void TrySpawnLampAt(IntVec3 position, Color color, ref OG_OutpostData outpostData)
        {
            if (position.GetEdifice() != null)
            {
                return;
            }
            ThingDef lampDef = null;
            if (color == Color.red)
            {
                lampDef = ThingDef.Named("StandingLamp_Red");
            }
            else if (color == Color.green)
            {
                lampDef = ThingDef.Named("StandingLamp_Green");
            }
            else if (color == Color.blue)
            {
                lampDef = ThingDef.Named("StandingLamp_Blue");
            }
            else
            {
                lampDef = ThingDef.Named("StandingLamp");
            }
            OG_Common.TrySpawnThingAt(lampDef, null, position, false, Rot4.North, ref outpostData, true, false);
            Find.TerrainGrid.SetTerrain(position, TerrainDef.Named("MetalTile"));
        }

        public static void TrySpawnHeaterAt(IntVec3 position, ref OG_OutpostData outpostData)
        {
            ThingDef heaterDef = ThingDef.Named("Heater");
            OG_Common.TrySpawnThingAt(heaterDef, null, position, false, Rot4.North, ref outpostData);
            Find.TerrainGrid.SetTerrain(position, TerrainDef.Named("MetalTile"));
        }

        public static void TrySpawnWallAt(IntVec3 position, ref OG_OutpostData outpostData)
        {
            ThingDef wallDef = ThingDefOf.Wall;
            OG_Common.TrySpawnThingAt(wallDef, outpostData.structureStuffDef, position, false, Rot4.North, ref outpostData);
        }

        public static void SpawnPowerConduitAt(IntVec3 position, ref OG_OutpostData outpostData)
        {
            Thing powerConduit = ThingMaker.MakeThing(ThingDefOf.PowerConduit);
            powerConduit.SetFaction(outpostData.faction);
            foreach (Thing thing in position.GetThingList())
            {
                if (thing.def == ThingDefOf.PowerConduit)
                {
                    return;
                }
            }
            outpostData.outpostThingList.Add(powerConduit);
            GenSpawn.Spawn(powerConduit, position);
        }

        public static void SpawnDoorAt(IntVec3 position, ref OG_OutpostData outpostData)
        {
            ThingDef autodoorDef = ThingDef.Named("Autodoor");
            OG_Common.TrySpawnThingAt(autodoorDef, outpostData.structureStuffDef, position, false, Rot4.North, ref outpostData, false, true);
        }

        public static void SpawnCoolerAt(IntVec3 position, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            ThingDef coolerDef = ThingDef.Named("Cooler");
            OG_Common.TrySpawnThingAt(coolerDef, null, position, true, rotation, ref outpostData, false, true);
        }

        public static void SpawnVentAt(IntVec3 position, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            ThingDef ventDef = ThingDef.Named("Vent");
            OG_Common.TrySpawnThingAt(ventDef, null, position, true, rotation, ref outpostData, false, true);
        }

        public static LaserFence.Building_LaserFencePylon TrySpawnLaserFencePylonAt(IntVec3 position, ref OG_OutpostData outpostData)
        {
            ThingDef laserFencePylonDef = ThingDef.Named("LaserFencePylon");
            return (OG_Common.TrySpawnThingAt(laserFencePylonDef, null, position, false, Rot4.North, ref outpostData) as LaserFence.Building_LaserFencePylon);
        }

    }
}
