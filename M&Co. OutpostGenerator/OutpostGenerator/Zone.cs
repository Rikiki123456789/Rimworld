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
    /// Zone class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Zone
    {
        public static void GetAdjacentZone(int zoneAbs, int zoneOrd, Rot4 direction, out int adjacentZoneAbs, out int adjacentZoneOrd)
        {
            adjacentZoneAbs = 0;
            adjacentZoneOrd = 0;
            if (direction == Rot4.North)
            {
                adjacentZoneAbs = zoneAbs;
                adjacentZoneOrd = zoneOrd + 1;
            }
            else if (direction == Rot4.East)
            {
                adjacentZoneAbs = zoneAbs + 1;
                adjacentZoneOrd = zoneOrd;
            }
            else if (direction == Rot4.South)
            {
                adjacentZoneAbs = zoneAbs;
                adjacentZoneOrd = zoneOrd - 1;
            }
            else if (direction == Rot4.West)
            {
                adjacentZoneAbs = zoneAbs - 1;
                adjacentZoneOrd = zoneOrd;
            }
        }

        public static bool GetRandomFreeCardinalZoneAdjacentTo(int zoneAbs, int zoneOrd, out Rot4 cardinal, ZoneProperties[,] zoneMap, int horizontalZonesNumber, int verticalZonesNumber)
        {
            // Get the free zones.
            List<Rot4> cardinalList = new List<Rot4>();
            for (int cardinalAsInt = 0; cardinalAsInt < 4; cardinalAsInt++)
            {
                int testedZoneAbs = 0;
                int testedZoneOrd = 0;
                Rot4 testedCardinal = new Rot4(cardinalAsInt);
                Zone.GetAdjacentZone(zoneAbs, zoneOrd, testedCardinal, out testedZoneAbs, out testedZoneOrd);
                if (ZoneIsInArea(testedZoneAbs, testedZoneOrd, horizontalZonesNumber, verticalZonesNumber))
                {
                    if (zoneMap[testedZoneOrd, testedZoneAbs].zoneType == ZoneType.NotYetGenerated)
                    {
                        cardinalList.Add(testedCardinal);
                    }
                }
            }
            if (cardinalList.Count == 0)
            {
                cardinal = Rot4.North;
                return false;
            }
            else
            {
                cardinal = cardinalList.RandomElement<Rot4>();
                return true;
            }
        }

        public static bool ZoneIsInArea(int zoneAbs, int zoneOrd, int horizontalZonesNumber, int verticalZonesNumber)
        {
            bool zoneIsInArea = false;

            zoneIsInArea = ((zoneAbs >= 0) && (zoneAbs < horizontalZonesNumber))
                && ((zoneOrd >= 0) && (zoneOrd < verticalZonesNumber));
            return zoneIsInArea;
        }

        public static IntVec3 GetZoneOrigin(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd)
        {
            return (areaSouthWestOrigin + new IntVec3(zoneAbs * Genstep_GenerateOutpost.zoneSideSize, 0, zoneOrd * Genstep_GenerateOutpost.zoneSideSize));
        }

        public static IntVec3 GetZoneRotatedOrigin(IntVec3 areaSouthWestOrigin, int zoneAbs, int zoneOrd, Rot4 rotation)
        {
            IntVec3 zoneOrigin = Zone.GetZoneOrigin(areaSouthWestOrigin, zoneAbs, zoneOrd);
            IntVec3 zoneRotatedOrigin = zoneOrigin;
            if (rotation == Rot4.North)
            {
                zoneRotatedOrigin = zoneOrigin;
            }
            else if (rotation == Rot4.East)
            {
                zoneRotatedOrigin = zoneOrigin + new IntVec3(0, 0, Genstep_GenerateOutpost.zoneSideSize - 1);
            }
            else if (rotation == Rot4.South)
            {
                zoneRotatedOrigin = zoneOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideSize - 1, 0, Genstep_GenerateOutpost.zoneSideSize - 1);
            }
            else if (rotation == Rot4.West)
            {
                zoneRotatedOrigin = zoneOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideSize - 1, 0, 0);
            }
            return zoneRotatedOrigin;
        }

        public static bool IsZoneMediumRoom(ZoneType zoneType)
        {
            return ((zoneType == ZoneType.MediumRoomMedibay)
                || (zoneType == ZoneType.MediumRoomPrison)
                || (zoneType == ZoneType.MediumRoomKitchen)
                || (zoneType == ZoneType.MediumRoomWarehouse)
                || (zoneType == ZoneType.MediumRoomWeaponRoom)
                || (zoneType == ZoneType.MediumRoomBarn)
                || (zoneType == ZoneType.MediumRoomLaboratory)
                || (zoneType == ZoneType.MediumRoomRecRoom));
        }
    }
}
