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
		public override void Generate()
		{
            Log.Message("GenStep_CaveFindPlayerStartSpot.Generate");
            if (Find.Map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Use standard base function.
                base.Generate();
                return;
            }
            if (MapGenerator.PlayerStartSpot.IsValid)
            {
                Log.Message("PlayerStartSpot = " + MapGenerator.PlayerStartSpot.ToString());
                return;
            }
            else
            {
                Log.Warning("Calling vanilla GenStep_FindPlayerStartSpot");
                base.Generate();
            }
        }
    }
}
