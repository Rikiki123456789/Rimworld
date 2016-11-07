using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace CaveworldFlora
{
    /// <summary>
    /// ThingDef_ClusterPlant class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class ThingDef_ClusterPlant : ThingDef
    {
        public int minGrowTemperature = 0; // Plant will enter cryostatis under this temperature.
        public int minOptimalGrowTemperature = 10;
        public int maxOptimalGrowTemperature = 40;
        public int maxGrowTemperature = 50;

        public bool hasStaticGlower = false;  // Glower must be named "defName + Glower".
        public bool hasDynamicGlower = false; // Glower must be named "defName + Small/Medium/Big".

        public bool growOnlyOnRoughRock = false;
        public bool growOnlyUndeRoof = false;
        public bool growOnlyNearNaturalRock = false;
        public float minFertility = 0f;
        public float maxFertility = 999f;

        public float minLight = 0f;
        public float maxLight = 1f;

        public IntRange clusterSizeRange;
        public float clusterSpawnRadius = 1f;
        public float clusterExclusivityRadiusOffset = 1f;
        public float clusterExclusivityRadiusFactor = 0f;

        // When a cluster is mature, it will spawn a new symbiosis cluster.
        // Symbiosis plants cannot spawn new cluster on their own.
        public bool isSymbiosisPlant = false; 
        public ThingDef_ClusterPlant symbiosisPlantDefSource = null; // Symbiosis plant will evolve from this plant.
        public ThingDef_ClusterPlant symbiosisPlantDefEvolution = null; // Plant can evolve into this plant.
    }
}
