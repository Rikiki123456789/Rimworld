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
    // TODO: redefine event which drop something: refugee, raids, ... ?
    // TODO: remove solar panels with events (and associated tech?) and add an other one (placed near glowing glowtree).
    // TODO: add violent winds weather?
    // TODO: add animals skeleton in dry cave wells.
    // TODO: set caveWell light according to time of day (overlit glower / glower / no glower).
    // TODO: set caveWell light according to mapCondition (volcanoe, eclipse, toxic fallout, ...).
    // Override Rat, cave bear and Megatherium to allow herd spawn.
    // TODO: add giant insectoïd.
    // TODO: deactivate map border plants spawn.

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
