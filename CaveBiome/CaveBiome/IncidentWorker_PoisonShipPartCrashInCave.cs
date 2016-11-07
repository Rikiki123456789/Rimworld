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
    public class IncidentWorker_PoisonShipPartCrashInCave : IncidentWorker_ShipPartCrashInCave
    {
        protected override int CountToSpawn
        {
            get
            {
                return Rand.RangeInclusive(1, 1);
            }
        }
    }
}
