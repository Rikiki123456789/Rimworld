using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace MiningTurret
{
    public class WorkGiver_OperateDrillTurret : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(Util_DrillTurret.drillTurretDef);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(Util_DrillTurret.drillTurretDef).Cast<Thing>();
        }

        public override bool ShouldSkip(Pawn pawn)
        {
            if (pawn.Map.listerBuildings.AllBuildingsColonistOfDef(Util_DrillTurret.drillTurretDef).Count() == 0)
            {
                return true;
            }
            return false;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            Building building = t as Building;
            if (building == null)
            {
                return false;
            }
            if (building.IsForbidden(pawn))
            {
                return false;
            }
            if (!pawn.CanReserve(building, 1, -1, null, forced))
            {
                return false;
            }
            if (building.IsBurning())
            {
                return false;
            }
            return (building as Building_MiningTurret).targetPosition.IsValid;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(Util_DrillTurret.operateDrillTurretJobDef, t, 1500, true);
        }
    }
}

