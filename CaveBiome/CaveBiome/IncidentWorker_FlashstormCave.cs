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
    public class IncidentWorker_FlashstormCave : IncidentWorker_Flashstorm
    {
        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            if (map.Biome == Util_CaveBiome.CaveBiomeDef)
            {
                return false;
            }
            return base.CanFireNowSub(target);
        }
    }
}
