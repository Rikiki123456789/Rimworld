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
        private List<ThingDef_ClusterPlant> cavePlantDefsInternal = null;
        public int randomSpawnPeriodInTicks = 0;
        public int nextRandomSpawnTick = 10;

        public List<ThingDef_ClusterPlant> cavePlantDefs
        {
            get
            {
                //Log.Message("MapComponent_ClusterPlant: get cavePlantDefs");
                if (cavePlantDefsInternal.NullOrEmpty())
                {
                    //Log.Message("MapComponent_ClusterPlant: cavePlantDefs is null or empty");
                    cavePlantDefsInternal = new List<ThingDef_ClusterPlant>();
                    foreach (ThingDef plantDef in DefDatabase<ThingDef>.AllDefs)
                    {
                        if (plantDef.category == ThingCategory.Plant)
                        {
                            //Log.Message("plantDef = " + plantDef);
                            //Log.Message("plantDef.thingClass = " + plantDef.thingClass.ToString());
                            if ((plantDef as ThingDef_ClusterPlant) != null)
                            /*if ((plantDef.thingClass == typeof(CaveworldFlora.ClusterPlant))
                                || (plantDef.thingClass == typeof(CaveworldFlora.ClusterPlant_Gleamcap))
                                || (plantDef.thingClass == typeof(CaveworldFlora.ClusterPlant_DevilTongue)))*/
                            {
                                //Log.Message("adding plantDef = " + plantDef);
                                cavePlantDefsInternal.Add(plantDef as ThingDef_ClusterPlant);
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
                int mapSurfaceCoefficient = Find.Map.Size.x * 2 + Find.Map.Size.z * 2;
                randomSpawnPeriodInTicks = 200000 / (mapSurfaceCoefficient / 100);                
                randomSpawnPeriodInTicks = 1000; // TODO: fastEcology debug.
                //Log.Message("randomSpawnPeriodInTicks = " + randomSpawnPeriodInTicks);
            }
            randomSpawnPeriodInTicks = 1000; // TODO: fastEcology debug.
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
            //Log.Message("TrySpawnNewClusterAtRandomPosition");
            for (int defindex = 0; defindex < cavePlantDefs.Count; defindex++)
            {
                //Log.Message("cavePlantDefs: " + cavePlantDefs[defindex].ToString());
            }
            ThingDef_ClusterPlant cavePlantDef = cavePlantDefs.RandomElementByWeight((ThingDef_ClusterPlant plantDef) => plantDef.plant.wildCommonalityMaxFraction / plantDef.clusterSizeRange.Average);
            //Log.Message("selected cavePlantDef = " + cavePlantDef.ToString());

            int newDesiredClusterSize = cavePlantDef.clusterSizeRange.RandomInRange;
            IntVec3 spawnCell = IntVec3.Invalid;
            GenClusterPlantReproduction.TryGetRandomClusterSpawnCell(cavePlantDef, newDesiredClusterSize, true, out spawnCell);
            if (spawnCell.IsValid)
            {
                Cluster.SpawnNewClusterAt(spawnCell, cavePlantDef, newDesiredClusterSize);
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.LookValue<int>(ref nextRandomSpawnTick, "nextRandomSpawnTick");
        }
    }
}
