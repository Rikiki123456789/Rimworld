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
    /// OG_WarfieldEffects class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_WarfieldEffects
    {
        public static void GenerateWarfieldEffects(ZoneProperties[,] zoneMap, int horizontalZonesNumber, int verticalZonesNumber, OG_OutpostData outpostData)
        {
            bool battleZoneIsFound = false;
            int battleZoneAbs = 0;
            int battleZoneOrd = 0;

            if (outpostData.battleOccured == false)
            {
                return;
            }
            battleZoneIsFound = GetBattleZoneAbsAndOrd(zoneMap, horizontalZonesNumber, verticalZonesNumber, out battleZoneAbs, out battleZoneOrd);
            if (battleZoneIsFound == false)
            {
                Log.Warning("M&Co. OutpostGenerator: failed to find an appropriate zone to generate warfield.");
                return;
            }

            Building_WarfieldGenerator warfieldGenerator = ThingMaker.MakeThing(ThingDef.Named("WarfieldGenerator")) as Building_WarfieldGenerator;
            warfieldGenerator.battleZoneAbs = battleZoneAbs;
            warfieldGenerator.battleZoneOrd = battleZoneOrd;
            warfieldGenerator.outpostData = outpostData;
            IntVec3 warfieldCenter = Zone.GetZoneOrigin(outpostData.areaSouthWestOrigin, battleZoneAbs, battleZoneOrd) + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset);
            GenSpawn.Spawn(warfieldGenerator, warfieldCenter);
        }

        private static bool GetBattleZoneAbsAndOrd(ZoneProperties[,] zoneMap, int horizontalZonesNumber, int verticalZonesNumber, out int battleZoneAbs, out int battleZoneOrd)
        {
            battleZoneAbs = 0;
            battleZoneOrd = 0;

            // Look for an entranched zone.
            for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
            {
                for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
                {
                    if ((zoneMap[zoneOrd, zoneAbs].zoneType == ZoneType.SecondaryEntrance)
                        || (zoneMap[zoneOrd, zoneAbs].zoneType == ZoneType.MainEntrance))
                    {
                        battleZoneAbs = zoneAbs;
                        battleZoneOrd = zoneOrd;
                        return true;
                    }
                }
            }
            // Else, look for an empty zone.
            for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
            {
                for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
                {
                    if ((zoneMap[zoneOrd, zoneAbs].zoneType == ZoneType.NotYetGenerated)
                        || (zoneMap[zoneOrd, zoneAbs].zoneType == ZoneType.Empty))
                    {
                        battleZoneAbs = zoneAbs;
                        battleZoneOrd = zoneOrd;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
