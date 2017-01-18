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
    public class GenStep_CaveInitializeWeatherAndLight : GenStep
    {
		public override void Generate(Map map)
		{
            if (map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Nothing to do in other biomes.
                return;
            }
            // To avoid starting with standard Clear weather, immediately force to reselect a cave biome weather.
            map.weatherDecider.StartNextWeather();

            MapCondition condition = MapConditionMaker.MakeConditionPermanent(Util_CaveBiome.CaveEnvironmentMapConditionDef);
            map.mapConditionManager.RegisterCondition(condition);
        }
    }
}
