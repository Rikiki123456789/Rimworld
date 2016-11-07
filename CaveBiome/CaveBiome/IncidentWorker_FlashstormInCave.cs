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
    public class IncidentWorker_FlashstormInCave : IncidentWorker_Flashstorm
    {
        protected override bool CanFireNowSub()
        {
            if (Find.Map.Biome == Util_CaveBiome.CaveBiomeDef)
            {
                return false;
            }
            return base.CanFireNowSub();
        }
    }
}
