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
    public class GenStep_CaveFindPlayerStartSpot : GenStep_FindPlayerStartSpot
    {
		public override void Generate(Map map)
		{
            if (map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Use standard base function.
                base.Generate(map);
                return;
            }
            if (MapGenerator.PlayerStartSpot.IsValid)
            {
                return;
            }
            else
            {
                base.Generate(map);
            }
        }
    }
}
