using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace CaveworldFlora
{
    /// <summary>
    /// MapComponent_ClusterPlant class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class MapComponent_ClusterPlant : MapComponent
    {
        public List<ThingDef_ClusterPlant> cavePlantDefsInternal = null;
        public int randomSpawnPeriodInTicks = 0;
        public int nextRandomSpawnTick = 10;

        public MapComponent_ClusterPlant(Map map) : base(map)
        {
        }

        public List<ThingDef_ClusterPlant> cavePlantDefs
        {
            get
            {
                if (cavePlantDefsInternal.NullOrEmpty())
                {
                    cavePlantDefsInternal = new List<ThingDef_ClusterPlant>();
                    foreach (ThingDef plantDef in DefDatabase<ThingDef>.AllDefs)
                    {
                        if (plantDef.category == ThingCategory.Plant)
                        {
                            ThingDef_ClusterPlant clusterPlantDef = (plantDef as ThingDef_ClusterPlant);
                            if ((clusterPlantDef != null)
                                && (clusterPlantDef.isSymbiosisPlant == false)
                                && ((clusterPlantDef.growsOnlyInCaveBiome == false)
                                   || (this.map.Biome.defName == "Cave")))
                            {
                                cavePlantDefsInternal.Add(clusterPlantDef);
                            }
                        }
                    }
                }
                return cavePlantDefsInternal;
            }
        }

        public override void MapComponentTick()
        {
            if (randomSpawnPeriodInTicks == 0)
            {
                // Occurs when loading a savegame.
                int mapSurfaceCoefficient = this.map.Size.x * 2 + this.map.Size.z * 2;
                randomSpawnPeriodInTicks = 200000 / (mapSurfaceCoefficient / 100);
            }
            if (Find.TickManager.TicksGame > nextRandomSpawnTick)
            {
                nextRandomSpawnTick = Find.TickManager.TicksGame + randomSpawnPeriodInTicks;
                TrySpawnNewClusterAtRandomPosition();
            }
        }

        /// <summary>
        /// Tries to spawn a new cluster at a random position on the map. The exclusivity radius still applies.
        /// </summary>
        public void TrySpawnNewClusterAtRandomPosition()
        {
            ThingDef_ClusterPlant cavePlantDef = cavePlantDefs.RandomElementByWeight((ThingDef_ClusterPlant plantDef) => plantDef.plant.wildCommonalityMaxFraction / plantDef.clusterSizeRange.Average);
            
            int newDesiredClusterSize = cavePlantDef.clusterSizeRange.RandomInRange;
            IntVec3 spawnCell = IntVec3.Invalid;
            GenClusterPlantReproduction.TryGetRandomClusterSpawnCell(cavePlantDef, newDesiredClusterSize, true, this.map, out spawnCell);
            if (spawnCell.IsValid)
            {
                Cluster.SpawnNewClusterAt(this.map, spawnCell, cavePlantDef, newDesiredClusterSize);
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref nextRandomSpawnTick, "nextRandomSpawnTick");
        }
    }
}
