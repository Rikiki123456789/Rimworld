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
		public override void Generate()
		{
            if (Find.Map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Nothing to do in other biomes.
                return;
            }
            // To avoid starting with standard Clear weather, immediately force to reselect a cave biome weather.
            Find.Storyteller.weatherDecider.StartNextWeather();

            MapCondition condition = MapConditionMaker.MakeConditionPermanent(Util_CaveBiome.CaveEnvironmentMapConditionDef);
            Find.MapConditionManager.RegisterCondition(condition);

            Current.Game.Rules.SetAllowBuilding(ThingDefOf.SolarGenerator, false);
            Current.Game.Rules.SetAllowBuilding(ThingDef.Named("Turret_MortarBomb"), false);
            Current.Game.Rules.SetAllowBuilding(ThingDef.Named("Turret_MortarIncendiary"), false);
            Current.Game.Rules.SetAllowBuilding(ThingDef.Named("Turret_MortarEMP"), false);
        }
    }
}
