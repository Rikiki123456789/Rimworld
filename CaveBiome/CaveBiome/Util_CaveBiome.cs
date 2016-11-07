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
    public static class Util_CaveBiome
    {
        // Plant glowers.
        public static ThingDef StoneGrassGlowerDef
        {
            get
            {
                return ThingDef.Named("StoneGrassGlower");
            }
        }

        // Roof and cave well.
        public static ThingDef CaveRoofDef
        {
            get
            {
                return ThingDef.Named("CaveRoof");
            }
        }

        public static ThingDef CaveWellDef
        {
            get
            {
                return ThingDef.Named("CaveWell");
            }
        }

        // Weather and light.
        public static WeatherDef CaveCalmWeatherDef
        {
            get
            {
                return WeatherDef.Named("CaveCalm");
            }
        }

        public static MapConditionDef CaveEnvironmentMapConditionDef
        {
            get
            {
                return MapConditionDef.Named("CaveEnvironment");
            }
        }

        // Biome.
        public static BiomeDef CaveBiomeDef
        {
            get
            {
                return BiomeDef.Named("Cave");
            }
        }

        // Corpses generators.
        public static ThingDef AnimalCorpsesGeneratorDef
        {
            get
            {
                return ThingDef.Named("AnimalCorpsesGenerator");
            }
        }

        public static ThingDef VillagerCorpsesGeneratorDef
        {
            get
            {
                return ThingDef.Named("VillagerCorpsesGenerator");
            }
        }
    }
}
