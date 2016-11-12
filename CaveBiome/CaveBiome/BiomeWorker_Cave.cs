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
    // TODO: mortars need line of sight
    // TODO: add solar panel placed near glowing crystal?
    // TODO: set caveWell light according to mapCondition (volcanoe, eclipse, toxic fallout, ...).

    public class BiomeWorker_Cave : BiomeWorker
    {
        public override float GetScore(WorldSquare square)
        {
            if (square.elevation <= 0f)
            {
                return -100f;
            }
            if (square.temperature < -10f)
            {
                return 0f;
            }
            if ((square.elevation < 1000f)
                || (square.elevation > 3000f))
            {
                return 0f;
            }
            // TODO: debug.
            return 200;
            /*if (Rand.Value < 0.05f)
            {
                return 100f;
            }
            else
            {
                return -100f;
            }*/
        }
    }
}
