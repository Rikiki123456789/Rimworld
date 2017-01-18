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
    public class IncidentWorker_RaidEnemyCave : IncidentWorker_RaidEnemy
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (map.Biome == Util_CaveBiome.CaveBiomeDef)
            {
                parms.raidArrivalMode = PawnsArriveMode.EdgeWalkIn;
                if ((parms.raidStrategy != null)
                    && (parms.raidStrategy.defName == "Siege"))
                {
                    parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                }
                return base.TryExecute(parms);
            }
            else
            {
                return base.TryExecute(parms);
            }
        }
    }
}
