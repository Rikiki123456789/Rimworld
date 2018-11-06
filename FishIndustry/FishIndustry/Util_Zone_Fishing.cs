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

namespace FishIndustry
{
    /// <summary>
    /// Util_Zone_Fishing utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public static class Util_Zone_Fishing
    {
        public const int minCellsToSpawnFish = 50;

        public static bool IsAquaticTerrain(Map map, IntVec3 position)
        {
            TerrainDef terrainDef = map.terrainGrid.TerrainAt(position);
            if ((terrainDef == TerrainDefOf.WaterShallow)
                || (terrainDef == TerrainDefOf.WaterOceanShallow)
                || (terrainDef == TerrainDefOf.WaterMovingShallow)
                || (terrainDef == TerrainDefOf.WaterDeep)
                || (terrainDef == TerrainDefOf.WaterOceanDeep)
                || (terrainDef == TerrainDefOf.WaterMovingChestDeep)
                || (terrainDef == TerrainDef.Named("Marsh")))
            {
                return true;
            }
            return false;
        }

        public static void UpdateZoneProperties(Map map, List<IntVec3> aquaticCells,
            ref int oceanCellsCount, ref int riverCellsCount, ref int marshCellsCount,
            ref bool isAffectedByBiome, ref bool isAffectedByToxicFallout, ref bool isAffectedByBadTemperature,
            ref int maxFishStock)
        {
            bool toxicFalloutIsActive = map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout);
            bool volcanicWinterIsActive = map.GameConditionManager.ConditionIsActive(GameConditionDefOf.VolcanicWinter);
            bool zoneIsOutdoor = false;
            bool zoneIsUnroofed = false;

            oceanCellsCount = 0;
            riverCellsCount = 0;
            marshCellsCount = 0;
            isAffectedByBiome = false;
            isAffectedByToxicFallout = false;
            isAffectedByBadTemperature = false;

            UpdateViableCells(map, aquaticCells, ref oceanCellsCount, ref riverCellsCount, ref marshCellsCount, ref zoneIsOutdoor, ref zoneIsUnroofed);
            UpdateAffectionIndicators(map, zoneIsOutdoor, zoneIsUnroofed, ref isAffectedByBiome, ref isAffectedByToxicFallout, ref isAffectedByBadTemperature);
            UpdateMaxFishStock(map.Biome, oceanCellsCount + riverCellsCount + marshCellsCount, isAffectedByBiome, isAffectedByToxicFallout, isAffectedByBadTemperature, ref maxFishStock);
        }

        public static void UpdateViableCells(Map map, List<IntVec3> aquaticCells,
            ref int oceanCellsCount, ref int riverCellsCount, ref int marshCellsCount,
            ref bool zoneIsOutdoor, ref bool zoneIsUnroofed)
        {
            bool toxicFalloutIsActive = map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout);

            // Valid cells include aquatic cells not affected by toxic fallout event (or under a roof).
            foreach (IntVec3 cell in aquaticCells)
            {
                if (cell.UsesOutdoorTemperature(map))
                {
                    zoneIsOutdoor = true;
                }
                if (cell.Roofed(map) == false)
                {
                    zoneIsUnroofed = true;
                    if (toxicFalloutIsActive)
                    {
                        // Do not count unroofed cells during toxic fallout.
                        continue;
                    }
                }
                TerrainDef waterDef = map.terrainGrid.TerrainAt(cell);
                if ((waterDef == TerrainDefOf.WaterOceanShallow)
                    || (waterDef == TerrainDefOf.WaterOceanDeep))
                {
                    oceanCellsCount++;
                }
                else if ((waterDef == TerrainDefOf.WaterMovingShallow)
                    || (waterDef == TerrainDefOf.WaterShallow)
                    || (waterDef == TerrainDefOf.WaterDeep))
                {
                    riverCellsCount++;
                }
                else if (waterDef == TerrainDef.Named("Marsh"))
                {
                    marshCellsCount++;
                }
            }
        }

        public static void UpdateAffectionIndicators(Map map, bool zoneIsOutdoor, bool zoneIsUnroofed,
            ref bool isAffectedByBiome, ref bool isAffectedByToxicFallout, ref bool isAffectedByBadTemperature)
        {
            // Update biome indicator.
            if (GetBiomeMaxFishStockFactor(map.Biome) < 1f)
            {
                isAffectedByBiome = true;
            }
            // Update toxic fallout indicator.
            if (zoneIsUnroofed
                && map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
            {
                isAffectedByToxicFallout = true;
            }
            // Update temperature indicator.
            if (zoneIsOutdoor
                && (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.VolcanicWinter)
                    || (map.weatherManager.growthSeasonMemory.GrowthSeasonOutdoorsNow == false)))
            {
                isAffectedByBadTemperature = true;
            }
        }

        public static void UpdateMaxFishStock(BiomeDef biome, int viableCellsCount, bool isAffectedByBiome, bool isAffectedByToxicFallout, bool isAffectedByBadTemperature, ref int maxFishStock)
        {
            const float minCellsPerFish = 50f;
            float environmentFactor = 1f; // [0.5f; 1f].
            float biomeFactor = 1f; // [0.2f; 1f].

            if (viableCellsCount < minCellsPerFish)
            {
                maxFishStock = 0;
                return;
            }
            biomeFactor = GetBiomeMaxFishStockFactor(biome);
            if (isAffectedByToxicFallout)
            {
                environmentFactor -= 0.25f;
            }
            if (isAffectedByBadTemperature)
            {
                environmentFactor -= 0.25f;
            }
            float maxFishStockFloat = ((float)viableCellsCount / minCellsPerFish) * biomeFactor * environmentFactor;
            maxFishStock = Math.Max(1, Mathf.RoundToInt(maxFishStockFloat));
        }

        // FishSpawnRateFactor in [1f; 4f]. This modifies the average time necessary to spawn a new fish.
        public static void ComputeFishSpawnRateFactor(Map map, int oceanCellsCount, int riverCellsCount, int marshCellsCount, bool isAffectedByToxicFallout, bool isAffectedByBadTemperature, out float spawnRateFactor)
        {
            // Fishstock spawn rate in ocean > river > marsh.
            const float oceanCellFactor = 1f;
            const float riverCellFactor = 0.75f;
            const float marshCellFactor = 0.5f;
            
            spawnRateFactor = 0f;
            int viableCellsCount = oceanCellsCount + riverCellsCount + marshCellsCount;
            if (viableCellsCount == 0)
            {
                return;
            }

            float biomeFactor = GetBiomeFishSpawnRateFactor(map.Biome);
            float waterQualityFactor = viableCellsCount / (oceanCellsCount * oceanCellFactor + riverCellsCount * riverCellFactor + marshCellsCount * marshCellFactor); // [1f; 2f].
            float seasonFactor = 1f; // [1f; 2f].
            if (isAffectedByBadTemperature
                && map.gameConditionManager.ConditionIsActive(GameConditionDefOf.VolcanicWinter))
            {
                seasonFactor += 0.5f;
            }
            if (isAffectedByBadTemperature
                && (map.weatherManager.growthSeasonMemory.GrowthSeasonOutdoorsNow == false))
            {
                seasonFactor += 0.5f;
            }
            spawnRateFactor = biomeFactor * waterQualityFactor * seasonFactor;
            spawnRateFactor /= Settings.fishRespawnRateFactor;
            return;
        }

        public static float GetBiomeMaxFishStockFactor(BiomeDef biome)
        {
            float biomeFactor = 1f;
            if (biome == BiomeDefOf.BorealForest)
            {
                biomeFactor = 0.8f;
            }
            else if ((biome == BiomeDefOf.Tundra)
                || (biome == BiomeDefOf.AridShrubland))
            {
                biomeFactor = 0.6f;
            }
            else if ((biome == BiomeDefOf.SeaIce)
                || (biome == BiomeDefOf.Desert))
            {
                biomeFactor = 0.4f;
            }
            else if ((biome == BiomeDefOf.IceSheet)
                || (biome == BiomeDef.Named("ExtremeDesert")))
            {
                biomeFactor = 0.2f;
            }
            return biomeFactor;
        }

        // This impacts negatively the respawn rate.
        public static float GetBiomeFishSpawnRateFactor(BiomeDef biome)
        {
            float biomeFactor = 1f;
            if (biome == BiomeDefOf.BorealForest)
            {
                biomeFactor = 1.2f;
            }
            else if ((biome == BiomeDefOf.Tundra)
                || (biome == BiomeDefOf.AridShrubland))
            {
                biomeFactor = 1.4f;
            }
            else if ((biome == BiomeDefOf.SeaIce)
                || (biome == BiomeDefOf.Desert))
            {
                biomeFactor = 1.6f;
            }
            else if ((biome == BiomeDefOf.IceSheet)
                || (biome == BiomeDef.Named("ExtremeDesert")))
            {
                biomeFactor = 1.8f;
            }
            return biomeFactor;
        }
        
        public static string GetSpeciesInZoneText(BiomeDef biome, int oceanCellsCount, int riverCellsCount, int marshCellsCount)
        {
            StringBuilder speciesInZone = new StringBuilder();
            foreach (PawnKindDef_FishSpecies species in Util_FishIndustry.GetFishSpeciesList(biome))
            {
                if ((oceanCellsCount > 0)
                    && (species.livesInOcean))
                {
                    speciesInZone.AppendWithComma(species.label);
                }
                else if ((riverCellsCount > 0)
                    && (species.livesInRiver))
                {
                    speciesInZone.AppendWithComma(species.label);
                }
                else if ((marshCellsCount > 0)
                    && (species.livesInMarsh))
                {
                    speciesInZone.AppendWithComma(species.label);
                }
            }
            if (speciesInZone.Length == 0)
            {
                speciesInZone.Append("none");
            }
            return speciesInZone.ToString();
        }
    }
}
