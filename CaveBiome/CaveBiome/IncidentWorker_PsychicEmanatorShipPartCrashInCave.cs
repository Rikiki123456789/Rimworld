using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;

namespace CaveBiome
{
    public class IncidentWorker_PsychicEmanatorShipPartCrashInCave : IncidentWorker_ShipPartCrashInCave
    {
        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            return !map.gameConditionManager.ConditionIsActive(GameConditionDefOf.PsychicDrone) && base.CanFireNowSub(target);
        }
    }
}
