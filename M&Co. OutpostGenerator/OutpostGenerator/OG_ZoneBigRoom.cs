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
    /// OG_ZoneBigRoom class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_ZoneBigRoom
    {
        public static void GenerateBigRoomLivingRoom(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin, Genstep_GenerateOutpost.zoneSideSize, Genstep_GenerateOutpost.zoneSideSize, rotation, TerrainDefOf.Concrete, null, ref outpostData);

            // Spawn table and stools.
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 4).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("TableShort"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 2).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            // Spawn NPD, hoppers and lamp.
            OG_Common.TrySpawnThingAt(ThingDefOf.NutrientPasteDispenser, null, rotatedOrigin + new IntVec3(2, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(2, 0, 6).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            OG_Common.TrySpawnThingAt(ThingDefOf.Hopper, null, rotatedOrigin + new IntVec3(4, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Hopper, null, rotatedOrigin + new IntVec3(4, 0, 8).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Hopper, null, rotatedOrigin + new IntVec3(4, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(4, 0, 7).RotatedBy(rotation), Color.white, ref outpostData);
            // Spawn TV and dining chairs.
            OG_Common.TrySpawnThingAt(ThingDef.Named("FlatscreenTelevision"), null, rotatedOrigin + new IntVec3(8, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(9, 0, 6).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(7, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("DiningChair"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(9, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            // Spawn lamp and temperature control.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(6, 0, 4).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(9, 0, 3).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(9, 0, 1).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(10, 0, 3).RotatedBy(rotation), new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(10, 0, 1).RotatedBy(rotation), new Rot4(Rot4.East.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.GenerateHorizontalAndVerticalPavedAlleys(origin);
        }

        public static void GenerateBigRoomWarehouse(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            const int stockWidth = 4;
            const int stockHeight = 4;

            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin, Genstep_GenerateOutpost.zoneSideSize, Genstep_GenerateOutpost.zoneSideSize, rotation, TerrainDefOf.Concrete, null, ref outpostData);
            // Spawn metal stock and lamp.
            OG_Common.TrySpawnResourceStock(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), rotation, ThingDefOf.Steel, stockWidth, stockHeight, 0.6f, 10, 55);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), Color.white, ref outpostData);
            // Spawn wood stock and lamp.
            OG_Common.TrySpawnResourceStock(rotatedOrigin + new IntVec3(6, 0, 1).RotatedBy(rotation), rotation, ThingDefOf.WoodLog, stockWidth, stockHeight, 0.8f, 20, 75);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(9, 0, 1).RotatedBy(rotation), Color.white, ref outpostData);
            // Spawn gold stock and lamp.
            OG_Common.TrySpawnResourceStock(rotatedOrigin + new IntVec3(1, 0, 6).RotatedBy(rotation), rotation, ThingDefOf.Gold, stockWidth, stockHeight, 0.3f, 2, 25);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(1, 0, 9).RotatedBy(rotation), Color.white, ref outpostData);
            // Spawn plasteel stock and lamp.
            OG_Common.TrySpawnResourceStock(rotatedOrigin + new IntVec3(6, 0, 6).RotatedBy(rotation), rotation, ThingDefOf.Plasteel, stockWidth, stockHeight, 0.5f, 5, 35);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(9, 0, 9).RotatedBy(rotation), Color.white, ref outpostData);

            OG_Common.GenerateHorizontalAndVerticalPavedAlleys(origin);
        }

        public static void GenerateBigRoomPrison(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin, Genstep_GenerateOutpost.zoneSideSize, Genstep_GenerateOutpost.zoneSideSize, rotation, TerrainDefOf.Concrete, null, ref outpostData);
            // Spawn table and stools.
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(4, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.West.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("Stool"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(3, 0, 9).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDef.Named("TableShort"), ThingDefOf.Steel, rotatedOrigin + new IntVec3(2, 0, 7).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            // Spawn lamp and temperature control.
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(6, 0, 6).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(7, 0, 9).RotatedBy(rotation), ref outpostData);
            OG_Common.TrySpawnHeaterAt(rotatedOrigin + new IntVec3(9, 0, 9).RotatedBy(rotation), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(7, 0, 10).RotatedBy(rotation), new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.SpawnCoolerAt(rotatedOrigin + new IntVec3(9, 0, 10).RotatedBy(rotation), new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            // Spawn prison cells' walls, door, bed, lamp and vent.
            // Cell 1.
            OG_Common.GenerateEmptyRoomAt(rotatedOrigin, 5, 5, rotation, TerrainDefOf.Concrete, null, ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(2, 0, 4).RotatedBy(rotation), ref outpostData);
            Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(2, 0, 5).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            Thing bed = OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(2, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            //(bed as Building_Bed).ForPrisoners = true; // TODO: can't do it while generating map...
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(1, 0, 1).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.SpawnVentAt(rotatedOrigin + new IntVec3(1, 0, 4).RotatedBy(rotation), new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            // Cell 2.
            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(6, 0, 0).RotatedBy(rotation), 5, 5, rotation, TerrainDefOf.Concrete, null, ref outpostData);
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(8, 0, 4).RotatedBy(rotation), ref outpostData);
            Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(7, 0, 5).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            bed = OG_Common.TrySpawnThingAt(ThingDef.Named("Bed"), ThingDefOf.WoodLog, rotatedOrigin + new IntVec3(8, 0, 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            //(bed as Building_Bed).ForPrisoners = true; // TODO: can't do it while generating map...
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(9, 0, 1).RotatedBy(rotation), Color.white, ref outpostData);
            OG_Common.SpawnVentAt(rotatedOrigin + new IntVec3(9, 0, 4).RotatedBy(rotation), new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);

            OG_Common.GenerateHorizontalAndVerticalPavedAlleys(origin);
        }
    }
}
