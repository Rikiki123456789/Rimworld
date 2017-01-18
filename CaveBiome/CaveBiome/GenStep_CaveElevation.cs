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
    public class GenStep_CaveElevation : GenStep
    {
        public const float ElevationFreq = 0.03f;
        public const float ElevationFactorCave = 1.5f;
        
		public override void Generate(Map map)
		{
            if (map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Nothing to do in other biomes.
                return;
            }
            
            // Generate basic map elevation.
			NoiseRenderer.renderSize = new IntVec2(map.Size.x, map.Size.z);
            ModuleBase perlinMap = new Perlin(ElevationFreq, 1.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			perlinMap = new ScaleBias(0.5, 0.5, perlinMap);
            NoiseDebugUI.StoreNoiseRender(perlinMap, "Cave: elev base");
            perlinMap = new Multiply(perlinMap, new Const((double)ElevationFactorCave));
            NoiseDebugUI.StoreNoiseRender(perlinMap, "Cave: elev cave-factored");
            
            // Override base elevation grid so the GenStep_Terrain.Generate function uses this one.
            MapGenFloatGrid mapGenFloatGrid = MapGenerator.FloatGridNamed("Elevation", map);
			foreach (IntVec3 current in map.AllCells)
			{
                mapGenFloatGrid[current] = perlinMap.GetValue(current);
			}
		}
    }
}
