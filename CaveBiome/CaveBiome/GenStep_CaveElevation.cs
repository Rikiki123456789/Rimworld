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

        // TODO: remove this unsuccessful test...
        //private const float ElevationFreq = 0.1f;
        //private const float ElevationFactorCave = 1.5f;
        //ModuleBase moduleBase = new Voronoi(ElevationFreq, 25, Rand.Range(0, 2147483647), false);

		public override void Generate()
		{
            Log.Message("GenStep_CaveElevationFertility.Generate");

            if (Find.Map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Nothing to do in other biomes.
                return;
            }
			NoiseRenderer.renderSize = new IntVec2(Find.Map.Size.x, Find.Map.Size.z);
            ModuleBase moduleBase = new Perlin(ElevationFreq, 1.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
            NoiseDebugUI.StoreNoiseRender(moduleBase, "Cave: elev base");
            moduleBase = new Multiply(moduleBase, new Const((double)ElevationFactorCave));
            NoiseDebugUI.StoreNoiseRender(moduleBase, "Cave: elev cave-factored");
            // Override base elevation grid so the GenStep_Terrain.Generate function uses this one.
            MapGenFloatGrid mapGenFloatGrid = MapGenerator.FloatGridNamed("Elevation");
			foreach (IntVec3 current in Find.Map.AllCells)
			{
                mapGenFloatGrid[current] = moduleBase.GetValue(current);
			}
		}
    }
}
