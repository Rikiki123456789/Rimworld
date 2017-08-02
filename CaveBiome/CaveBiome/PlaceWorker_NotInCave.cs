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
    public class PlaceWorker_NotInCave : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
        {
            if (this.Map.Biome == Util_CaveBiome.CaveBiomeDef)
            {
                return new AcceptanceReport("CaveBiome.CanNotBuildInCave".Translate());
            }
            return true;
        }
    }
}
