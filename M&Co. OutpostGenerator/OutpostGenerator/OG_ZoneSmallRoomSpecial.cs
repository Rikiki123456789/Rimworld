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
    /// OG_ZoneSmallRoomSpecial class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_ZoneSmallRoomSpecial
    {
        const int smallRoomWallOffset = 2;

        public static void GenerateBatteryRoomZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation, Rot4 linkedZoneRelativeRotation, ref OG_OutpostData outpostData)
        {
            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, rotation);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(smallRoomWallOffset, 0, smallRoomWallOffset).RotatedBy(rotation), Genstep_GenerateOutpost.zoneSideSize - 2 * smallRoomWallOffset, Genstep_GenerateOutpost.zoneSideSize - 2 * smallRoomWallOffset, rotation, TerrainDefOf.Concrete, null, ref outpostData);

            // Generate batteries.
            OG_Common.TrySpawnThingAt(ThingDef.Named("CompactAutonomousGenerator"), null, rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(smallRoomWallOffset + 4, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 1).RotatedBy(rotation), true, new Rot4(Rot4.North.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(smallRoomWallOffset + 2, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(smallRoomWallOffset + 4, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            OG_Common.TrySpawnThingAt(ThingDefOf.Battery, null, rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 5).RotatedBy(rotation), true, new Rot4(Rot4.South.AsInt + rotation.AsInt), ref outpostData);
            // Generate vertical alley and door.
            for (int zOffset = smallRoomWallOffset; zOffset <= Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
            OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset - 1).RotatedBy(rotation), ref outpostData);
            // Generate horizontal alley, door, lamp and power conduits.
            if (linkedZoneRelativeRotation == Rot4.West)
            {
                for (int xOffset = smallRoomWallOffset - 1; xOffset <= Genstep_GenerateOutpost.zoneSideCenterOffset; xOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
                }
                OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(smallRoomWallOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset).RotatedBy(rotation), ref outpostData);
                OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 3).RotatedBy(rotation), Color.red, ref outpostData);
                for (int xOffset = 0; xOffset <= 1; xOffset++)
                {
                    OG_Common.SpawnFireproofPowerConduitAt(rotatedOrigin + new IntVec3(xOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset).RotatedBy(rotation), ref outpostData);
                }
            }
            else if (linkedZoneRelativeRotation == Rot4.East)
            {
                for (int xOffset = Genstep_GenerateOutpost.zoneSideCenterOffset; xOffset <= Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset; xOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
                }
                OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset - 1, 0, Genstep_GenerateOutpost.zoneSideCenterOffset).RotatedBy(rotation), ref outpostData);
                OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 3).RotatedBy(rotation), Color.red, ref outpostData);
                for (int xOffset = Genstep_GenerateOutpost.zoneSideSize - 2; xOffset <= Genstep_GenerateOutpost.zoneSideSize - 1; xOffset++)
                {
                    OG_Common.SpawnFireproofPowerConduitAt(rotatedOrigin + new IntVec3(xOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset).RotatedBy(rotation), ref outpostData);
                }
            }
        }

        public static Building_OutpostCommandConsole GenerateCommandRoomZone(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 cardinal, Rot4 linkedZoneRelativeRotation, ref OG_OutpostData outpostData)
        {
            Building_OutpostCommandConsole commandConsole = null;

            IntVec3 origin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 rotatedOrigin = Zone.GetZoneRotatedOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd, cardinal);

            OG_Common.GenerateEmptyRoomAt(rotatedOrigin + new IntVec3(smallRoomWallOffset, 0, smallRoomWallOffset).RotatedBy(cardinal), Genstep_GenerateOutpost.zoneSideSize - 2 * smallRoomWallOffset, Genstep_GenerateOutpost.zoneSideSize - 2 * smallRoomWallOffset, cardinal, TerrainDefOf.Concrete, null, ref outpostData);
            // Spawn command console.
            commandConsole = OG_Common.TrySpawnThingAt(ThingDef.Named("OutpostCommandConsole"), null, rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset + 2).RotatedBy(cardinal), true, new Rot4(Rot4.South.AsInt + cardinal.AsInt), ref outpostData) as Building_OutpostCommandConsole;
            // Generate vertical alley.
            for (int zOffset = smallRoomWallOffset; zOffset < Genstep_GenerateOutpost.zoneSideCenterOffset; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, zOffset).RotatedBy(cardinal), TerrainDef.Named("PavedTile"));
            }
            // Generate horizontal alley, door and lamp.
            if (linkedZoneRelativeRotation == Rot4.West)
            {
                for (int xOffset = smallRoomWallOffset - 2; xOffset <= Genstep_GenerateOutpost.zoneSideCenterOffset; xOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset).RotatedBy(cardinal), TerrainDef.Named("PavedTile"));
                }
                OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(smallRoomWallOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset).RotatedBy(cardinal), ref outpostData);
            }
            else if (linkedZoneRelativeRotation == Rot4.East)
            {
                for (int xOffset = Genstep_GenerateOutpost.zoneSideCenterOffset; xOffset <= Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset + 1; xOffset++)
                {
                    Find.TerrainGrid.SetTerrain(rotatedOrigin + new IntVec3(xOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset).RotatedBy(cardinal), TerrainDef.Named("PavedTile"));
                }
                OG_Common.SpawnDoorAt(rotatedOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideSize - smallRoomWallOffset - 1, 0, Genstep_GenerateOutpost.zoneSideCenterOffset).RotatedBy(cardinal), ref outpostData);
            }
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 1, 0, smallRoomWallOffset + 1).RotatedBy(cardinal), Color.red, ref outpostData);
            OG_Common.TrySpawnLampAt(rotatedOrigin + new IntVec3(smallRoomWallOffset + 5, 0, smallRoomWallOffset + 1).RotatedBy(cardinal), Color.red, ref outpostData);

            return commandConsole;
        }

    }
}
