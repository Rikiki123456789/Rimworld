using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;

namespace Spaceship
{
    public static class Util_LandingPad
    {
        // Note: "available landing pad" means powered and not reserved.
        // Note: "free landing pad" means only unreserved.
        
        // Return the primary landing pad if available
        //     else a random one
        //     else null.
        public static Building_LandingPad GetBestAvailableLandingPad(Map map)
        {
            List<Building_LandingPad> allAvailableLandingPads = GetAllFreeAndPoweredLandingPads(map);
            if (allAvailableLandingPads == null)
            {
                // No available landing pad on the map.
                return null;
            }
            foreach (Building_LandingPad landingPad in allAvailableLandingPads)
            {
                if (landingPad.isPrimary)
                {
                    // Primary landing pad is available.
                    return landingPad;
                }
            }
            return allAvailableLandingPads.RandomElement();
        }

        // Return the primary landing pad if available and reaching map edge
        //     else a random one
        //     else null.
        public static Building_LandingPad GetBestAvailableLandingPadReachingMapEdge(Map map)
        {
            IntVec3 exitSpot = IntVec3.Invalid;
            // Check pawns can reach map edge from best landing pad.
            Building_LandingPad bestLandingPad = GetBestAvailableLandingPad(map);
            if (bestLandingPad != null)
            {
                if (Expedition.TryFindRandomExitSpot(map, bestLandingPad.Position, out exitSpot))
                {
                    return bestLandingPad;
                }
            }
            // Check pawns can exit map from any landing pad.
            List<Building_LandingPad> allAvailableLandingPads = Util_LandingPad.GetAllFreeAndPoweredLandingPads(map);
            if (allAvailableLandingPads != null)
            {
                foreach (Building_LandingPad landingPad in allAvailableLandingPads.InRandomOrder())
                {
                    if (Expedition.TryFindRandomExitSpot(map, landingPad.Position, out exitSpot))
                    {
                        return landingPad;
                    }
                }
            }
            return null;
        }

        // Return a list of all free and powered landing pads.
        public static List<Building_LandingPad> GetAllFreeAndPoweredLandingPads(Map map)
        {
            if (map.listerBuildings.ColonistsHaveBuilding(Util_ThingDefOf.LandingPad) == false)
            {
                // No landing pad on the map.
                return null;
            }
            List<Building_LandingPad> allFreeAndPoweredLandingPads = new List<Building_LandingPad>();
            foreach (Building building in map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPad))
            {
                Building_LandingPad landingPad = building as Building_LandingPad;
                if (landingPad.isFreeAndPowered)
                {
                    allFreeAndPoweredLandingPads.Add(landingPad);
                }
            }
            if (allFreeAndPoweredLandingPads.Count > 0)
            {
                return allFreeAndPoweredLandingPads;
            }
            return null;
        }

        // Return a list of all free landing pads.
        public static List<Building_LandingPad> GetAllFreeLandingPads(Map map)
        {
            if (map.listerBuildings.ColonistsHaveBuilding(Util_ThingDefOf.LandingPad) == false)
            {
                // No landing pad on the map.
                return null;
            }
            List<Building_LandingPad> allFreeLandingPads = new List<Building_LandingPad>();
            foreach (Building building in map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPad))
            {
                Building_LandingPad landingPad = building as Building_LandingPad;
                if (landingPad.isFree)
                {
                    allFreeLandingPads.Add(landingPad);
                }
            }
            if (allFreeLandingPads.Count > 0)
            {
                return allFreeLandingPads;
            }
            return null;
        }
    }
}
