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
    // TODO: add solar panel placed near glowing crystal?
    // TODO : add some insect hives with a limit of size? Modify hive class to limit reproduction?
    // TODO: Try to correct incoming caravans establishing a new colony without visibility.

    public class BiomeWorker_Cave : BiomeWorker
    {
        public override float GetScore(Tile tile)
        {
            if (tile.elevation <= 0f)
            {
                return -100f;
            }
            if (tile.temperature < -10f)
            {
                return 0f;
            }
            if ((tile.elevation < 1000f)
                || (tile.elevation > 3000f))
            {
                return 0f;
            }
            if (Rand.Value < 0.05f)
            {
                return 100f;
            }
            else
            {
                return -100f;
            }
        }
    }
}
