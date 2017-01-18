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
            if (plant.cluster == null)
            {
                Log.Warning("CaveworldFlora:TryToReproduce: cluster is null at " + plant.Position);
            }

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
        public static void TryGetRandomClusterSpawnCell(ThingDef_ClusterPlant plantDef, int newDesiredClusterSize, bool checkTemperature, Map map, out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;

            Predicate<IntVec3> validator = delegate(IntVec3 cell)
            {
                // Check a plant can be spawned here.
                if (GenClusterPlantReproduction.IsValidPositionToGrowPlant(plantDef, map, cell, checkTemperature) == false)
                {
                    return false;
                }
                // Check there is no third cluster nearby.
                if (GenClusterPlantReproduction.IsClusterAreaClear(plantDef, newDesiredClusterSize, map, cell) == false)
                {
                    return false;
                }
                return true;
            };

            bool validCellIsFound = CellFinderLoose.TryGetRandomCellWith(validator, map, 1000, out spawnCell);
            if (validCellIsFound == false)
            {
                // Just for robustness, TryGetRandomCellWith set result to IntVec3.Invalid if no valid cell is found.
                spawnCell = IntVec3.Invalid;
            }
        }
        
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
                GenSpawn.Spawn(newPlant, spawnCell, cluster.Map);
                newPlant.cluster = cluster;
                cluster.NotifyPlantAdded();
                if (cluster.plantDef.isSymbiosisPlant)
                {
                    // Destroy source symbiosis plant.
                    Thing sourceSymbiosisPlant = spawnCell.GetFirstThing(cluster.Map, cluster.plantDef.symbiosisPlantDefSource);
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
            maxSpawnDistance += 2f; // Add a margin so the cluster does not have a perfect circle shape.
            Predicate<IntVec3> validator = delegate(IntVec3 cell)
            {
                // Check cell is not too far away from current cluster.
                if (cell.InHorDistOf(cluster.Position, maxSpawnDistance) == false)
                {
                    return false;
                }
                // Check cell is in the same room.
                Room clusterRoom = cluster.GetRoom();
                Room cellRoom = cell.GetRoom(cluster.Map);
                if (clusterRoom == null)
                {
                    if (Current.ProgramState == ProgramState.Playing)
                    {
                        Log.Warning("clusterRoom at " + cluster.Position + " is null.");
                        if (cellRoom == null)
                        {
                            Log.Message("cellRoom at " + cell + " is null.");
                        }
                    }
                }
                if ((cellRoom == null)
                    || (cellRoom != clusterRoom))
                {
                    return false;
                }
                return IsValidPositionToGrowPlant(cluster.plantDef, cluster.Map, cell);
            };
            bool validCellIsFound = CellFinder.TryFindRandomCellNear(cluster.Position, cluster.Map, (int)maxSpawnDistance, validator, out spawnCell);
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
            IntVec3 spawnCell = IntVec3.Invalid;
            int newDesiredClusterSize = cluster.plantDef.clusterSizeRange.RandomInRange;
            TryGetRandomSpawnCellAwayFromCluster(cluster, newDesiredClusterSize, out spawnCell);
            if (spawnCell.IsValid)
            {
                return Cluster.SpawnNewClusterAt(cluster.Map, spawnCell, cluster.plantDef, newDesiredClusterSize);
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

            // Current cluster and new cluster zones are exclusive and should not overlap.
            float newClusterMinDistance = cluster.exclusivityRadius + newClusterExclusivityRadius;
            float newClusterMaxDistance = 2f * newClusterMinDistance;

            Predicate<IntVec3> validator = delegate(IntVec3 cell)
            {
                // Check cell is not too close from current cluster.
                if (cell.InHorDistOf(cluster.Position, newClusterMinDistance))
                {
                    return false;
                }
                // Check cell is not too distant from current cluster.
                if (cell.InHorDistOf(cluster.Position, newClusterMaxDistance) == false)
                {
                    return false;
                }
                // Check cell is in the same room.
                if (cell.GetRoom(cluster.Map) != cluster.GetRoom())
                {
                    return false;
                }
                // Check a plant can be spawned here.
                if (IsValidPositionToGrowPlant(cluster.plantDef, cluster.Map, cell) == false)
                {
                    return false;
                }
                // Check there is no third cluster nearby.
                if (IsClusterAreaClear(cluster.plantDef, newDesiredClusterSize, cluster.Map, cell) == false)
                {
                    return false;
                }
                return true;
            };

            bool validCellIsFound = CellFinder.TryFindRandomCellNear(cluster.Position, cluster.Map, (int)newClusterMaxDistance, validator, out spawnCell);
            if (validCellIsFound == false)
            {
                // Note that TryFindRandomCellNear set result to root if no valid cell is found!
                spawnCell = IntVec3.Invalid;
            }
        }

        /// <summary>
        /// Check if there is another cluster too close.
        /// </summary>
        public static bool IsClusterAreaClear(ThingDef_ClusterPlant plantDef, int newDesiredClusterSize, Map map, IntVec3 position)
        {
            float newClusterExclusivityRadius = Cluster.GetExclusivityRadius(plantDef, newDesiredClusterSize);
            foreach (Thing thing in map.listerThings.ThingsOfDef(Util_CaveworldFlora.ClusterDef))
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
        public static bool IsValidPositionToGrowPlant(ThingDef_ClusterPlant plantDef, Map map, IntVec3 position, bool checkTemperature = true)
        {
            if (position.InBounds(map) == false)
            {
                return false;
            }
            if (plantDef.isSymbiosisPlant)
            {
                // For symbiosis plant, only check there is a source symbiosis plant.
                if (position.GetFirstThing(map, plantDef.symbiosisPlantDefSource) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            // Check there is no building or cover.
            if ((position.GetEdifice(map) != null)
                || (position.GetCover(map) != null))
            {
                return false;
            }
            // Check terrain condition.
            if (ClusterPlant.CanTerrainSupportPlantAt(plantDef, map, position) == false)
            {
                return false;
            }
            // Check temperature conditions.
            if (ClusterPlant.IsTemperatureConditionOkAt(plantDef, map, position) == false)
            {
                return false;
            }
            // Check light conditions.
            if (ClusterPlant.IsLightConditionOkAt(plantDef, map, position) == false)
            {
                return false;
            }
            // Check there is no other plant.
            if (map.thingGrid.ThingAt(position, ThingCategory.Plant) != null)
            {
                return false;
            }
            // Check the cell is not blocked by a plant, an item, a pawn, a rock...
	        List<Thing> thingList = map.thingGrid.ThingsListAt(position);
	        for (int thingIndex = 0; thingIndex < thingList.Count; thingIndex++)
	        {
                Thing thing = thingList[thingIndex];
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
            if (GenPlant.SnowAllowsPlanting(position, map) == false)
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
                    if (cell.InBounds(cluster.Map) == false)
                    {
                        continue;
                    }
                    ClusterPlant plant = cell.GetFirstThing(cluster.Map, cluster.plantDef) as ClusterPlant;
                    if (plant != null)
                    {
                        plant.Destroy();
                        ClusterPlant symbiosisPlant = Cluster.SpawnNewClusterAt(cluster.Map, cell, cluster.plantDef.symbiosisPlantDefEvolution, cluster.plantDef.symbiosisPlantDefEvolution.clusterSizeRange.RandomInRange);
                        cluster.NotifySymbiosisClusterAdded(symbiosisPlant.cluster);

                        return symbiosisPlant;
                    }
                }
            }
            return null;
        }
    }
}
