using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using CaveworldFlora;

namespace CaveBiome
{
    public class GenStep_CavePlants : GenStep_Plants
    {
        public const float plantMinGrowth = 0.07f;
        public const float plantMaxGrowth = 1.0f;

        public override void Generate(Map map)
		{
            if (map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Use standard base function.
                base.Generate(map);
                return;
            }
            
            map.regionAndRoomUpdater.Enabled = false;
            List<ThingDef_ClusterPlant> wildCavePlants = new List<ThingDef_ClusterPlant>();
            Dictionary<ThingDef_ClusterPlant, float> wildCavePlantsWeighted = new Dictionary<ThingDef_ClusterPlant, float>();
            foreach (ThingDef def in map.Biome.AllWildPlants)
            {
                ThingDef_ClusterPlant cavePlantDef = def as ThingDef_ClusterPlant;
                if (cavePlantDef != null)
                {
                    wildCavePlants.Add(cavePlantDef);
                    wildCavePlantsWeighted.Add(cavePlantDef, map.Biome.CommonalityOfPlant(cavePlantDef) / cavePlantDef.clusterSizeRange.Average);
                }
            }

            int spawnTriesNumber = 10000;
            int totalSuccessfulSpawns = 0;
            int failedSpawns = 0;
            int totalFailedSpawns = 0;
            for (int tryIndex = 0; tryIndex < spawnTriesNumber; tryIndex++)
            {
                ThingDef_ClusterPlant cavePlantDef = wildCavePlants.RandomElementByWeight((ThingDef_ClusterPlant def) => wildCavePlantsWeighted[def]);

                int newDesiredClusterSize = cavePlantDef.clusterSizeRange.RandomInRange;
                IntVec3 spawnCell = IntVec3.Invalid;
                GenClusterPlantReproduction.TryGetRandomClusterSpawnCell(cavePlantDef, newDesiredClusterSize, false, map, out spawnCell); // Ignore temperature condition.
                if (spawnCell.IsValid)
                {
                    totalSuccessfulSpawns++;
                    failedSpawns = 0;
                    ClusterPlant newPlant = Cluster.SpawnNewClusterAt(map, spawnCell, cavePlantDef, newDesiredClusterSize);
                    newPlant.Growth = Rand.Range(ClusterPlant.minGrowthToReproduce, plantMaxGrowth);

                    bool clusterIsMature = (Rand.Value < 0.7f);
                    GrowCluster(newPlant, clusterIsMature);
                }
                else
                {
                    failedSpawns++;
                    totalFailedSpawns++;
                    if (failedSpawns >= 50)
                    {
                        break;
                    }
                }
            }
            map.regionAndRoomUpdater.Enabled = true;
        }

        public static void GrowCluster(ClusterPlant plant, bool clusterIsMature)
        {
            Cluster cluster = plant.cluster;
            int seedPlantsNumber = 0;
            if (clusterIsMature)
            {
                seedPlantsNumber = cluster.desiredSize - 1; // The first plant is already spawned.
            }
            else
            {
                seedPlantsNumber = (int)((float)cluster.desiredSize * Rand.Range(0.25f, 0.75f));
            }
            if (seedPlantsNumber == 0)
            {
                return;
            }
            for (int seedPlantIndex = 0; seedPlantIndex < seedPlantsNumber; seedPlantIndex++)
            {
                ClusterPlant seedPlant = GenClusterPlantReproduction.TryToReproduce(plant);
                if (seedPlant != null)
                {
                    seedPlant.Growth = Rand.Range(plantMinGrowth, plantMaxGrowth);
                }
            }
            if (clusterIsMature
                && cluster.plantDef.symbiosisPlantDefEvolution != null)
            {
                ClusterPlant symbiosisPlant = GenClusterPlantReproduction.TryToSpawnNewSymbiosisCluster(cluster);
                if (symbiosisPlant != null)
                {
                    symbiosisPlant.Growth = Rand.Range(plantMinGrowth, plantMaxGrowth);
                    GrowCluster(symbiosisPlant, clusterIsMature);
                }
            }
        }
    }
}
