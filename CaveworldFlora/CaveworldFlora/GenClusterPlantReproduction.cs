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
    /// GenClusterPlantReproduction class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class GenClusterPlantReproduction
    {
        /// <summary>
        /// Try to spawn another cave plant in the cluster or to spawn a new cluster.
        /// </summary>
        public static ClusterPlant TryToReproduce(ClusterPlant plant)
        {
            // Test if it is growing on a fungiponics basin.
            if (plant.isOnCavePlantGrower)
            {
                return null;
            }

            if (plant.cluster.actualSize < plant.cluster.desiredSize)
            {
                // Cluster is not mature, grow the cluster.
                return TryGrowCluster(plant.cluster);
            }
            else
            {
                // Cluster is mature: try to spawn a new cluster and/or a symbiosis cluster.
                if (plant.clusterPlantProps.isSymbiosisPlant == false)
                {
                    return TrySpawnNewClusterAwayFrom(plant.cluster);
                }
                if (plant.clusterPlantProps.symbiosisPlantDefEvolution != null)
                {
                    TryToSpawnNewSymbiosisCluster(plant.cluster);
                }
            }
            return null;
        }

        /// <summary>
        /// Try to get a valid cell to spawn a new cluster anywhere on the map.
        /// </summary>
        public static void TryGetRandomClusterSpawnCell(ThingDef_ClusterPlant plantDef, int newDesiredClusterSize, bool checkTemperature, out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;

            Predicate<IntVec3> validator = delegate(IntVec3 cell)
            {
                // Check a plant can be spawned here.
                if (GenClusterPlantReproduction.IsValidPositionToGrowPlant(plantDef, cell) == false)
                {
                    return false;
                }
                // Check there is no third cluster nearby.
                if (GenClusterPlantReproduction.IsClusterAreaClear(plantDef, newDesiredClusterSize, cell) == false)
                {
                    return false;
                }
                return true;
            };

            bool validCellIsFound = CellFinderLoose.TryGetRandomCellWith(validator, 1000, out spawnCell);
            if (validCellIsFound == false)
            {
                // Just for robustness, TryGetRandomCellWith set result to IntVec3.Invalid if no valid cell is found.
                spawnCell = IntVec3.Invalid;
            }
        }

        /// <summary>
        /// Get the center of a cluster and its actual size.
        /// </summary>
        /*public static void GetClusterCenterAndActualSize(ClusterPlant plant, out IntVec3 clusterCenter, out int actualClusterSize)
        {
            // Default values.
            clusterCenter = plant.Position;
            actualClusterSize = 1;

            if (plant.desiredClusterSize == 1)
            {
                return;
            }
            else
            {
                clusterCenter = IntVec3.Zero;
                actualClusterSize = 0;
                Room plantRoom = plant.Position.GetRoom();
                // We only check with clusterSpawnRadius (+ a small offset). No need to check entire cluster exclusivity area.
                IEnumerable<IntVec3> cellsInCluster = GenRadial.RadialCellsAround(plant.Position, plant.clusterPlantProps.clusterSpawnRadius + 3f, true);
                foreach (IntVec3 cell in cellsInCluster)
                {
                    if ((cell.GetRoom() == plantRoom)
                        && (Find.ThingGrid.ThingAt(cell, plant.def) != null))
                    {
                        actualClusterSize++;
                        clusterCenter += cell;
                    }
                }
                clusterCenter.x = (int)Mathf.Round(clusterCenter.x / (float)actualClusterSize);
                clusterCenter.z = (int)Mathf.Round(clusterCenter.z / (float)actualClusterSize);
                if (clusterCenter.x < 0)
                {
                    clusterCenter.x = 0;
                }
                else if (clusterCenter.x > Find.Map.Size.x)
                {
                    clusterCenter.x = Find.Map.Size.x;
                }
                if (clusterCenter.z < 0)
                {
                    clusterCenter.z = 0;
                }
                else if (clusterCenter.z > Find.Map.Size.z)
                {
                    clusterCenter.z = Find.Map.Size.z;
                }
                //Log.Message("GetClusterActualSizeAndCenter: " + plant.Position.ToString() + "/" + clusterActualSize + "/" + clusterCenter);
            }
        }*/

        /// <summary>
        /// Try to spawn another plant in this cluster.
        /// </summary>
        public static ClusterPlant TryGrowCluster(Cluster cluster)
        {
            IntVec3 spawnCell = IntVec3.Invalid;
            TryGetRandomSpawnCellNearCluster(cluster, out spawnCell);
            if (spawnCell.IsValid)
            {
                ClusterPlant newPlant = ThingMaker.MakeThing(cluster.plantDef) as ClusterPlant;
                GenSpawn.Spawn(newPlant, spawnCell);
                newPlant.cluster = cluster;
                cluster.NotifyPlantAdded();
                if (cluster.plantDef.isSymbiosisPlant)
                {
                    // Destroy source symbiosis plant.
                    Thing sourceSymbiosisPlant = spawnCell.GetFirstThing(cluster.plantDef.symbiosisPlantDefSource);
                    if (sourceSymbiosisPlant != null)
                    {
                        sourceSymbiosisPlant.Destroy();
                    }
                }
                return newPlant;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get a valid cell in this cluster to spawn another cave plant.
        /// </summary>
        public static void TryGetRandomSpawnCellNearCluster(Cluster cluster, out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;

            float maxSpawnDistance = GenRadial.RadiusOfNumCells(cluster.actualSize + 1); // Min radius to hold cluster's plants + new plant.
            //Log.Message("Cluster at " + clusterCenter.ToString() + ": actualClusterSize/desiredClusterSize => maxSpawnDistance " + actualClusterSize + "/" + desiredClusterSize + " => " + maxSpawnDistance);
            maxSpawnDistance += 2f; // Add a margin so the cluster does not have a perfect circle shape.
            /* = plantDef.clusterSpawnRadius;
            if (plantDef.clusterSizeRange.max > plantDef.clusterSizeRange.min)
            {
                maxSpawnDistance *= clusterMaturity;
                maxSpawnDistance = Math.Max(2f, maxSpawnDistance);
            }
            Log.Message("maxSpawnDistance at " + clusterCenter.ToString() + ": maturity/maxSpawnDistance => " + clusterMaturity + "/" + maxSpawnDistance);*/
            Predicate<IntVec3> validator = delegate(IntVec3 cell)
            {
                // Check cell is not too far away from current cluster.
                if (cell.InHorDistOf(cluster.Position, maxSpawnDistance) == false)
                {
                    return false;
                }
                // Check cell is in the same room.
                if (cell.GetRoom() != cluster.GetRoom())
                {
                    return false;
                }
                return IsValidPositionToGrowPlant(cluster.plantDef, cell);
            };
            bool validCellIsFound = CellFinder.TryFindRandomCellNear(cluster.Position, (int)maxSpawnDistance, validator, out spawnCell);
            if (validCellIsFound == false)
            {
                // Note that TryFindRandomCellNear set result to root if no valid cell is found!
                spawnCell = IntVec3.Invalid;
            }
        }

        /// <summary>
        /// Try to spawn a new cluster away from plant.
        /// </summary>
        public static ClusterPlant TrySpawnNewClusterAwayFrom(Cluster cluster)
        {
            //Log.Message("TrySpawnNewClusterAwayFrom");
            IntVec3 spawnCell = IntVec3.Invalid;
            int newDesiredClusterSize = cluster.plantDef.clusterSizeRange.RandomInRange;
            TryGetRandomSpawnCellAwayFromCluster(cluster, newDesiredClusterSize, out spawnCell);
            if (spawnCell.IsValid)
            {
                return Cluster.SpawnNewClusterAt(spawnCell, cluster.plantDef, newDesiredClusterSize);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Try to get a valid cell to spawn a new cluster away from plant.
        /// </summary>
        public static void TryGetRandomSpawnCellAwayFromCluster(Cluster cluster, int newDesiredClusterSize, out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;
            float newClusterExclusivityRadius = Cluster.GetExclusivityRadius(cluster.plantDef, newDesiredClusterSize);
            //Log.Message("newCluster size/ExclusivityRadius = " + newClusterSize + "/" + newClusterExclusivityRadius);

            // Current cluster and new cluster zones are exclusive and should not overlap.
            float newClusterMinDistance = cluster.exclusivityRadius + newClusterExclusivityRadius;
            float newClusterMaxDistance = 2f * newClusterMinDistance;
            //Log.Message("newClusterMinDistance/newClusterMaxDistance = " + newClusterMinDistance + "/" + newClusterMaxDistance);

            Predicate<IntVec3> validator = delegate(IntVec3 cell)
            {
                // Check cell is not too close from current cluster.
                if (cell.InHorDistOf(cluster.Position, newClusterMinDistance))
                {
                    return false;
                }
                //Log.Message("not too close");
                // Check cell is not too distant from current cluster.
                if (cell.InHorDistOf(cluster.Position, newClusterMaxDistance) == false)
                {
                    return false;
                }
                //Log.Message("not too distant");
                // Check cell is in the same room.
                if (cell.GetRoom() != cluster.GetRoom())
                {
                    return false;
                }
                // Check a plant can be spawned here.
                if (IsValidPositionToGrowPlant(cluster.plantDef, cell) == false)
                {
                    return false;
                }
                //Log.Message("IsValidPositionToSpawnPlant OK");
                // Check there is no third cluster nearby.
                if (IsClusterAreaClear(cluster.plantDef, newDesiredClusterSize, cell) == false)
                {
                    return false;
                }
                return true;
            };

            bool validCellIsFound = CellFinder.TryFindRandomCellNear(cluster.Position, (int)newClusterMaxDistance, validator, out spawnCell);
            if (validCellIsFound == false)
            {
                // Note that TryFindRandomCellNear set result to root if no valid cell is found!
                spawnCell = IntVec3.Invalid;
            }
        }

        /// <summary>
        /// Check if there is another cluster too close.
        /// </summary>
        public static bool IsClusterAreaClear(ThingDef_ClusterPlant plantDef, int newDesiredClusterSize, IntVec3 position)
        {
            float newClusterExclusivityRadius = Cluster.GetExclusivityRadius(plantDef, newDesiredClusterSize);
            foreach (Thing thing in Find.ListerThings.ThingsOfDef(Util_CaveworldFlora.ClusterDef))
            {
                Cluster cluster = thing as Cluster;
                if (cluster.plantDef != plantDef)
                {
                    continue;
                }
                if (cluster.Position.InHorDistOf(position, cluster.exclusivityRadius + newClusterExclusivityRadius))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if position is valid to grow a plant. Does not check cluster exclusivity!
        /// </summary>
        public static bool IsValidPositionToGrowPlant(ThingDef_ClusterPlant plantDef, IntVec3 position, bool checkTemperature = true)
        {
            if (position.InBounds() == false)
            {
                return false;
            }
            if (plantDef.isSymbiosisPlant)
            {
                // For symbiosis plant, only check there is a source symbiosis plant.
                if (position.GetFirstThing(plantDef.symbiosisPlantDefSource) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            // Check there is no building or cover.
            if ((position.GetEdifice() != null)
                || (position.GetCover() != null))
            {
                return false;
            }
            // Check terrain condition.
            if (ClusterPlant.CanTerrainSupportPlantAt(plantDef, position) == false)
            {
                return false;
            }
            // Check temperature conditions.
            if (ClusterPlant.IsTemperatureConditionOkAt(plantDef, position) == false)
            {
                return false;
            }
            // Check light conditions.
            if (ClusterPlant.IsLightConditionOkAt(plantDef, position) == false)
            {
                return false;
            }
            // Check there is no other plant.
            if (Find.ThingGrid.ThingAt(position, ThingCategory.Plant) != null)
            {
                return false;
            }
            // Check the cell is not blocked by a plant, an item, a pawn, a rock...
	        List<Thing> thingList = Find.ThingGrid.ThingsListAt(position);
	        for (int thingIndex = 0; thingIndex < thingList.Count; thingIndex++)
	        {
                Thing thing = thingList[thingIndex];
                //Log.Message("checking thing + " + thing.ToString() + " at " + position.ToString());
		        if (thing.def.BlockPlanting)
		        {
			        return false;
		        }
		        if (plantDef.passability == Traversability.Impassable
                    && (thing.def.category == ThingCategory.Pawn
                        || thing.def.category == ThingCategory.Item
                        || thing.def.category == ThingCategory.Building
                        || thing.def.category == ThingCategory.Plant))
		        {
			        return false;
		        }
	        }
            // Check snow level.
            if (GenPlant.SnowAllowsPlanting(position) == false)
            {
                return false;
            }
            return true;
        }

        public static ClusterPlant TryToSpawnNewSymbiosisCluster(Cluster cluster)
        {
            // Check there is not already a symbiosis cluster.
            if (cluster.symbiosisCluster == null)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(cluster.Position, cluster.plantDef.clusterSpawnRadius, false).InRandomOrder())
                {
                    if (cell.InBounds() == false)
                    {
                        continue;
                    }
                    ClusterPlant plant = cell.GetFirstThing(cluster.plantDef) as ClusterPlant;
                    if (plant != null)
                    {
                        plant.Destroy();
                        ClusterPlant symbiosisPlant = Cluster.SpawnNewClusterAt(cell, cluster.plantDef.symbiosisPlantDefEvolution, cluster.plantDef.symbiosisPlantDefEvolution.clusterSizeRange.RandomInRange);
                        cluster.NotifySymbiosisClusterAdded(symbiosisPlant.cluster);
                        return symbiosisPlant;
                    }
                }
            }
            return null;
        }
    }
}
