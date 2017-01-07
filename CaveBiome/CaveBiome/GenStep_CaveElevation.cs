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
        private const float ElevationFreq = 0.03f;
        private const float ElevationFactorCave = 1.5f;
        
		public override void Generate(Map map)
		{
            if (map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Nothing to do in other biomes.
                return;
            }
			NoiseRenderer.renderSize = new IntVec2(map.Size.x, map.Size.z);
            ModuleBase moduleBase = new Perlin(ElevationFreq, 1.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
            NoiseDebugUI.StoreNoiseRender(moduleBase, "Cave: elev base");
            moduleBase = new Multiply(moduleBase, new Const((double)ElevationFactorCave));
            NoiseDebugUI.StoreNoiseRender(moduleBase, "Cave: elev cave-factored");
            // Override base elevation grid so the GenStep_Terrain.Generate function uses this one.
            MapGenFloatGrid mapGenFloatGrid = MapGenerator.FloatGridNamed("Elevation", map);
			foreach (IntVec3 current in map.AllCells)
			{
                mapGenFloatGrid[current] = moduleBase.GetValue(current);
			}
		}
    }
}
