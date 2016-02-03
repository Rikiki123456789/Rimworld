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
    /// MapComponent_CavePlant class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class MapComponent_CavePlant : MapComponent
    {
        public List<ThingDef> cavePlantDefs = new List<ThingDef>();
        public float commonalitySum = 0;
        public int randomSpawnPeriodInTicks = 0;

        public override void ExposeData()
        {
            if (this.cavePlantDefs.Count == 0)
            {
                GetCavePlantDefsList();
            }
        }

        public override void MapComponentTick()
        {
            // On map initialization treatment.
            if (Find.TickManager.TicksGame == 1)
            {
                GetCavePlantDefsList();
                
                for (int spawnTryIndex = 0; spawnTryIndex < 100; spawnTryIndex++)
                {
                    TrySpawnNewClusterAtRandomPosition();
                }
            }


            // Normal treatment.
            if (randomSpawnPeriodInTicks == 0)
            {
                // Occurs when loading a savegame.
                int mapSurfaceCoefficient = Find.Map.Size.x * 2 + Find.Map.Size.z * 2;
                randomSpawnPeriodInTicks = 50000 / (mapSurfaceCoefficient / 100);
            }
	        if (Find.TickManager.TicksGame % randomSpawnPeriodInTicks == 0)
            {
                TrySpawnNewClusterAtRandomPosition();
            }
        }

        /// <summary>
        /// Tries to spawn a new cluster at a random position on the map. The exclusivity radius still applies.
        /// </summary>
        public bool TrySpawnNewClusterAtRandomPosition()
        {
            ThingDef cavePlantDef = GetRandomCavePlantDef();
            int newClusterSize = Rand.RangeInclusive(cavePlantDef.plant.wildClusterSizeRange.min, cavePlantDef.plant.wildClusterSizeRange.max);
            float newClusterExclusivityRadius = CavePlant.GetClusterExclusivityRadius(cavePlantDef, newClusterSize);
            int checkedCellsNumber = 0;
            for (checkedCellsNumber = 0; checkedCellsNumber < 1000; checkedCellsNumber++)
            {
                IntVec3 newClusterCell = new IntVec3(Rand.Range(0, Find.Map.Size.x), 0, Rand.Range(0, Find.Map.Size.z));
                if (Find.RoofGrid.Roofed(newClusterCell)
                    && ((Find.ThingGrid.ThingsListAt(newClusterCell).Count == 0)
                    || ((Find.ThingGrid.ThingAt(newClusterCell, ThingDef.Named("RockRubble")) != null)
                     && (Find.ThingGrid.ThingAt(newClusterCell, ThingCategory.Plant) == null)))
                    && CavePlant.IsLightConditionOk(newClusterCell)
                    && CavePlant.IsNearNaturalRockBlock(newClusterCell)
                    && CavePlant.IsTerrainConditionOk(newClusterCell)
                    && CavePlant.IsTemperatureConditionOk(newClusterCell))
                {
                    float cavePlantSearchRadius = CavePlant.GetMaxClusterExclusivityRadius(cavePlantDef) * 2f;
                    IEnumerable<IntVec3> cellsAroundNewCluster = GenRadial.RadialCellsAround(newClusterCell, cavePlantSearchRadius, false);
                    bool anotherClusterIsTooClose = false;
                    foreach (IntVec3 cell in cellsAroundNewCluster)
                    {
                        Thing potentialDistantCavePlant = Find.ThingGrid.ThingAt(cell, cavePlantDef);
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
                        GenSpawn.Spawn(cavePlantDef, newClusterCell);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get the list of cave plant defs and compute the sum of their wildCommonalityMaxFraction.
        /// </summary>
        public void GetCavePlantDefsList()
        {
            foreach (ThingDef potentialCavePlantDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (potentialCavePlantDef.thingClass.Name.Contains("CavePlant"))
                {
                    this.cavePlantDefs.Add(potentialCavePlantDef);
                }
            }

            if (this.cavePlantDefs.Count != 0)
            {
                foreach (ThingDef cavePlantDef in this.cavePlantDefs)
                {
                    this.commonalitySum += cavePlantDef.plant.wildCommonalityMaxFraction;
                }
            }
        }

        /// <summary>
        /// Get a random cave plant def from the list built in GetCavePlantDefsList function.
        /// </summary>
        public ThingDef GetRandomCavePlantDef()
        {
            float randomPlantIndex = Rand.Range(0f, this.commonalitySum);
            float localCommonalitySum = 0;
            ThingDef randomCavePlantDef = null;
            foreach (ThingDef cavePlantDef in this.cavePlantDefs)
            {
                localCommonalitySum += cavePlantDef.plant.wildCommonalityMaxFraction;
                if (randomPlantIndex < localCommonalitySum)
                {
                    randomCavePlantDef = cavePlantDef;
                    break;
                }
            }
            return randomCavePlantDef;
        }
    }
}
