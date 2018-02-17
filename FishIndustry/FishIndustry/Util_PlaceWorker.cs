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

namespace FishIndustry
{
    /// <summary>
    /// Util_PlaceWorker utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class Util_PlaceWorker
    {
        public const int minDistanceBetweenTwoFishingSpots = 15;

        public static bool IsNearFishingPier(Map map, IntVec3 position, float distance)
        {
            // Check if another fishing pier is not too close (mind the test on "fishing pier" def and "fishing pier spawner" blueprint and frame defs.
            List<Thing> fishingPiers = new List<Thing>();
            fishingPiers.AddRange(map.listerThings.ThingsOfDef(Util_FishIndustry.FishingPierDef));
            fishingPiers.AddRange(map.listerThings.ThingsOfDef(Util_FishIndustry.FishingPierSpawnerDef.blueprintDef));
            fishingPiers.AddRange(map.listerThings.ThingsOfDef(Util_FishIndustry.FishingPierSpawnerDef.frameDef));
            fishingPiers.AddRange(map.listerThings.ThingsOfDef(Util_FishIndustry.FishingPierSpawnerOnMudDef.blueprintDef));
            fishingPiers.AddRange(map.listerThings.ThingsOfDef(Util_FishIndustry.FishingPierSpawnerOnMudDef.frameDef));
            foreach (Thing thing in fishingPiers)
            {
                if (thing.Position.InHorDistOf(position, distance))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsNearFishingZone(Map map, IntVec3 position, float distance)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, distance, true))
            {
                if (cell.InBounds(map) == false)
                {
                    continue;
                }
                Zone zone = map.zoneManager.ZoneAt(cell);
                if ((zone != null)
                    && (zone is Zone_Fishing))
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetAquaticCellsInRadius(Map map, IntVec3 position, float radius)
        {
            int aquaticCellsNumber = 0;

            if (radius <= 0)
            {
                return 0;
            }
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true))
            {
                if (cell.InBounds(map) == false)
                {
                    continue;
                }
                if (Util_Zone_Fishing.IsAquaticTerrain(map, cell))
                {
                    aquaticCellsNumber++;
                }
            }
            return aquaticCellsNumber;
        }

    }
}
