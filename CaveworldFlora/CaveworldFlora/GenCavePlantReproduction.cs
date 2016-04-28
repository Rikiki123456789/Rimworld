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
    /// GenCavePlantReproduction class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class GenCavePlantReproduction
    {
        public const float chanceToReproducePerDay = 10;
        public const float minGrowthPercentToReproduce = 0.6f;

        /// <summary>
        /// Try to spawn another cave plant in the cluster or to spawn a new cluster.
        /// </summary>
        public static void TryToReproduce(CavePlant cavePlant)
        {
            // Test if it is growing on a fungiponics.
            if (CavePlant.IsOnCavePlantGrower(cavePlant.Position))
            {
                return;
            }
            if (cavePlant.growth < minGrowthPercentToReproduce)
            {
                return;
            }
            if (CavePlant.IsTemperatureConditionOk(cavePlant.Position) == false)
            {
                return;
            }

            float chanceToReproduce = chanceToReproducePerDay / (30000f / GenTicks.TickLongInterval);
            if (Rand.Value < chanceToReproduce)
            {
                int cavePlantsInCluster = CountPlantsOfDefInRange(cavePlant.Position, cavePlant.GetClusterExclusivityRadius(), cavePlant.def);
                // Cluster is not mature, growth the cluster.
                if (cavePlantsInCluster < cavePlant.clusterSize)
                {
                    TrySpawnCavePlantInThisCluster(cavePlant);
                }
                // Cluster is mature, spawn a new cluster.
                else
                {
                    TrySpawnCavePlantInNewCluster(cavePlant);
                }
            }
        }

        /// <summary>
        /// Count the number of cave plant of def cavePlantDef in range.
        /// </summary>
        public static int CountPlantsOfDefInRange(IntVec3 checkedPosition, float radius, ThingDef cavePlantDef)
        {
            int plantsInRange = 0;
            IEnumerable<IntVec3> cellsInCluster = GenRadial.RadialCellsAround(checkedPosition, radius, true);
            foreach (IntVec3 cell in cellsInCluster)
            {
                if ((cell.GetRoom() == checkedPosition.GetRoom())
                    && (Find.ThingGrid.ThingAt(cell, cavePlantDef) != null))
                {
                    plantsInRange++;
                }
            }
            return plantsInRange;
        }

        /// <summary>
        /// Try to spawn another cave plant in this cluster.
        /// </summary>
        public static void TrySpawnCavePlantInThisCluster(CavePlant cavePlant)
        {
            IntVec3 spawnPosition;
            if (GetRandomValidCellNearbyCluster(cavePlant, out spawnPosition))
            {
                CavePlant newPlant = ThingMaker.MakeThing(cavePlant.def) as CavePlant;
                GenSpawn.Spawn(newPlant, spawnPosition);
                newPlant.clusterSize = cavePlant.clusterSize;
            }
        }

        /// <summary>
        /// Get a valid cell in this cluster to spawn another cave plant.
        /// </summary>
        public static bool GetRandomValidCellNearbyCluster(CavePlant cavePlant, out IntVec3 validCell)
        {
            List<IntVec3> validCellsNearbyCluster = new List<IntVec3>();

            IEnumerable<IntVec3> cellsNearbyCluster = GenAdj.CellsAdjacent8Way(cavePlant);
            foreach (IntVec3 cell in cellsNearbyCluster)
            {
                if ((cell.GetRoom() == cavePlant.GetRoom())
                    && IsCellValidToSPawnANewCavePlant(cell))
                {
                    validCellsNearbyCluster.Add(cell);
                }
            }
            if (validCellsNearbyCluster.Count == 0)
            {
                validCell = new IntVec3(0, 0, 0);
                return false;
            }
            validCell = validCellsNearbyCluster.RandomElement<IntVec3>();
            return true;
        }

        /// <summary>
        /// Try to spawn a new cluster away from cavePlant.
        /// </summary>
        public static void TrySpawnCavePlantInNewCluster(CavePlant cavePlant)
        {
            IntVec3 spawnPosition;
            int newClusterSize = Rand.RangeInclusive(cavePlant.def.plant.wildClusterSizeRange.min, cavePlant.def.plant.wildClusterSizeRange.max);
            if (GetRandomValidCellAwayFromCluster(cavePlant, newClusterSize, out spawnPosition))
            {
                CavePlant newPlant = ThingMaker.MakeThing(cavePlant.def) as CavePlant;
                GenSpawn.Spawn(newPlant, spawnPosition);
                newPlant.clusterSize = newClusterSize;
            }
        }

        /// <summary>
        /// Get a valid cell to spawn a new cluster away from cavePlant.
        /// </summary>
        public static bool GetRandomValidCellAwayFromCluster(CavePlant cavePlant, int newClusterSize, out IntVec3 validCell)
        {
            float newClusterExclusivityRadius = CavePlant.GetClusterExclusivityRadius(cavePlant.def, newClusterSize);
            List<IntVec3> validCellsAwayFromCluster = new List<IntVec3>();

            float newClusterMinDistance = cavePlant.GetClusterExclusivityRadius() + newClusterExclusivityRadius;
            float newClusterMaxDistance = 2f * newClusterMinDistance;

            IEnumerable<IntVec3> potentialCellsForNewCluster = GenRadial.RadialCellsAround(cavePlant.Position, newClusterMaxDistance, false);
            foreach (IntVec3 newClusterCell in potentialCellsForNewCluster)
            {
                if ((newClusterCell.InHorDistOf(cavePlant.Position, newClusterMinDistance) == false)
                    && (newClusterCell.GetRoom() == cavePlant.GetRoom())
                    && IsCellValidToSPawnANewCavePlant(newClusterCell))
                {
                    float cavePlantSearchRadius = CavePlant.GetMaxClusterExclusivityRadius(cavePlant.def) * 2f;
                    IEnumerable<IntVec3> cellsAroundNewCluster = GenRadial.RadialCellsAround(newClusterCell, cavePlantSearchRadius, false);
                    bool anotherClusterIsTooClose = false;
                    foreach (IntVec3 cell in cellsAroundNewCluster)
                    {
                        Thing potentialDistantCavePlant = Find.ThingGrid.ThingAt(cell, cavePlant.def);
                        if (potentialDistantCavePlant != null)
                        {
                            CavePlant distantCavePlant = potentialDistantCavePlant as CavePlant;
                            if (distantCavePlant.Position.InHorDistOf(cell, newClusterExclusivityRadius + distantCavePlant.GetClusterExclusivityRadius()))
                            {
                                anotherClusterIsTooClose = true;
                                break;
                            }
                        }
                    }
                    if (anotherClusterIsTooClose == false)
                    {
                        validCellsAwayFromCluster.Add(newClusterCell);
                    }
                }
            }
            if (validCellsAwayFromCluster.Count == 0)
            {
                validCell = new IntVec3(0, 0, 0);
                return false;
            }
            validCell = validCellsAwayFromCluster.RandomElement<IntVec3>();
            return true;
        }

        /// <summary>
        /// Check if cell is valid to spawn a new cave plant.
        /// </summary>
        private static bool IsCellValidToSPawnANewCavePlant(IntVec3 cell)
        {
            bool cellIsValid = false;

            cellIsValid = Find.RoofGrid.Roofed(cell)
                && ((Find.ThingGrid.ThingsListAt(cell).Count == 0)
                || ((Find.ThingGrid.ThingAt(cell, ThingDef.Named("RockRubble")) != null)
                && (Find.ThingGrid.ThingAt(cell, ThingCategory.Plant) == null)))
                && CavePlant.IsLightConditionOk(cell)
                && CavePlant.IsNearNaturalRockBlock(cell)
                && CavePlant.IsTerrainConditionOk(cell)
                && CavePlant.IsTemperatureConditionOk(cell);

            return cellIsValid;
        }
    }
}
