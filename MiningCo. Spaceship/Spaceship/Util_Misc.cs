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
    public static class Util_Misc
    {
        // Air strike target selection.
        public static void SelectAirStrikeTarget(Map map, Action<LocalTargetInfo> actionOnValidTarget)
        {
            TargetingParameters targetingParams = new TargetingParameters();
            targetingParams.canTargetPawns = false;
            targetingParams.canTargetBuildings = true;
            targetingParams.canTargetLocations = true;
            targetingParams.validator = delegate(TargetInfo targ)
            {
                if (map.fogGrid.IsFogged(targ.Cell))
                {
                    return false;
                }
                return true;
            };
            Find.Targeter.BeginTargeting(targetingParams, actionOnValidTarget);
        }

        // Partnership component.
        public static WorldComponent_Partnership Partnership
        {
            get
            {
                WorldComponent_Partnership partnership = Find.World.GetComponent(typeof(WorldComponent_Partnership)) as WorldComponent_Partnership;
                if (partnership != null)
                {
                    return partnership;
                }
                Log.ErrorOnce("MiningCo. Spaceship: did not found WorldComponent_Partnership.", 123456788);
                return null;
            }
        }

        // OrbitalHealing component.
        public static WorldComponent_OrbitalHealing OrbitalHealing
        {
            get
            {
                WorldComponent_OrbitalHealing orbitalHealing = Find.World.GetComponent(typeof(WorldComponent_OrbitalHealing)) as WorldComponent_OrbitalHealing;
                if (orbitalHealing != null)
                {
                    return orbitalHealing;
                }
                Log.ErrorOnce("MiningCo. Spaceship: did not found WorldComponent_OrbitalHealing.", 123456787);
                return null;
            }
        }

        public static bool IsModActive(string packageId)
        {
            foreach (ModMetaData mod in ModLister.AllInstalledMods)
            {
                if (mod.Active
                    && (mod.PackageId == packageId))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
