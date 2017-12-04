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
    public static class Util_OrbitalRelay
    {
        public static Building_OrbitalRelay GetOrbitalRelay(Map map)
        {
            if (map.listerBuildings.ColonistsHaveBuilding(Util_ThingDefOf.OrbitalRelay) == false)
            {
                // No orbital relay on the map.
                return null;
            }
            return map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.OrbitalRelay).First() as Building_OrbitalRelay;
        }

        public static void TryUpdateLandingPadAvailability(Map map)
        {
            Building_OrbitalRelay orbitalRelay = GetOrbitalRelay(map);
            if (orbitalRelay != null)
            {
                orbitalRelay.UpdateLandingPadAvailability();
            }
        }

    }
}
