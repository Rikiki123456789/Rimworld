using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// Building_FishingPier class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class Building_FishingPier : Building_WorkTable
    {
        public string riverTerrainCellDefAsString;
        public string middleTerrainCellDefAsString;

        public IntVec3 fishingSpotCell = new IntVec3(0, 0, 0);
        public IntVec3 riverCell = new IntVec3(0, 0, 0);
        public IntVec3 middleCell = new IntVec3(0, 0, 0);
        public IntVec3 bankCell = new IntVec3(0, 0, 0);

        private const int maxFishStockDefault = 5;
        private int maxFishStock = maxFishStockDefault;
        public int fishStock = 1;
        private const int fishStockRespawnInterval = (2 * GenDate.TicksPerDay) / maxFishStockDefault;
        private int fishStockRespawnTick = 0;

        /// <summary>
        /// Convert the cells under the fishing pier into fishing pier cells (technically just water cells with movespeed = 100%).
        /// </summary>
        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);

            // Compute max fish stock according to terrain and biome.
            UpdateMaxFishStock();

            bankCell = this.Position + new IntVec3(0, 0, -1).RotatedBy(this.Rotation);
            middleCell = this.Position + new IntVec3(0, 0, 0).RotatedBy(this.Rotation);
            riverCell = this.Position + new IntVec3(0, 0, 1).RotatedBy(this.Rotation);
            fishingSpotCell = this.Position + new IntVec3(0, 0, 2).RotatedBy(this.Rotation);

            // On first spawning, save the terrain defs and apply the fishing pier equivalent.
            TerrainDef middleCellTerrainDef = map.terrainGrid.TerrainAt(middleCell);
            if (middleCellTerrainDef == TerrainDef.Named("Marsh"))
            {
                middleTerrainCellDefAsString = middleCellTerrainDef.ToString();
                map.terrainGrid.SetTerrain(middleCell, Util_FishIndustry.FishingPierFloorMarshDef);
            }
            else if (middleCellTerrainDef == TerrainDef.Named("WaterShallow"))
            {
                middleTerrainCellDefAsString = middleCellTerrainDef.ToString();
                map.terrainGrid.SetTerrain(middleCell, Util_FishIndustry.FishingPierFloorShallowWaterDef);
            }
            else if (middleCellTerrainDef == TerrainDef.Named("WaterDeep"))
            {
                middleTerrainCellDefAsString = middleCellTerrainDef.ToString();
                map.terrainGrid.SetTerrain(middleCell, Util_FishIndustry.FishingPierFloorDeepWaterDef);
            }

            TerrainDef riverCellTerrainDef = map.terrainGrid.TerrainAt(riverCell);
            if (riverCellTerrainDef == TerrainDef.Named("Marsh"))
            {
                riverTerrainCellDefAsString = riverCellTerrainDef.ToString();
                map.terrainGrid.SetTerrain(riverCell, Util_FishIndustry.FishingPierFloorMarshDef);
            }
            else if (riverCellTerrainDef == TerrainDef.Named("WaterShallow"))
            {
                riverTerrainCellDefAsString = riverCellTerrainDef.ToString();
                map.terrainGrid.SetTerrain(riverCell, Util_FishIndustry.FishingPierFloorShallowWaterDef);
            }
            else if (riverCellTerrainDef == TerrainDef.Named("WaterDeep"))
            {
                riverTerrainCellDefAsString = riverCellTerrainDef.ToString();
                map.terrainGrid.SetTerrain(riverCell, Util_FishIndustry.FishingPierFloorDeepWaterDef);
            }            
        }

        /// <summary>
        /// Periodically resplenishes the fish stock if necessary.
        /// </summary>
        public override void TickRare()
        {
            base.TickRare();

            if (this.fishStock < this.maxFishStock)
            {
                if (this.fishStockRespawnTick == 0)
                {
                    this.fishStockRespawnTick = Find.TickManager.TicksGame + (int)((float)fishStockRespawnInterval * Rand.Range(0.8f, 1.2f));
                }
                if (Find.TickManager.TicksGame >= this.fishStockRespawnTick)
                {
                    this.fishStock++;
                    this.fishStockRespawnTick = 0;
                }
            }
        }

        /// <summary>
        /// Update the max fishing stock according to terrain and biome (to avoid exploits due to terrain changes).
        /// </summary>
        public void UpdateMaxFishStock()
        {
            int maxFishStockTerrain = maxFishStockDefault;

            // Compute max fish stock according to surrounding aquatic cells.
            float aquaticCellsNumber = 0;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.Position, this.def.specialDisplayRadius, true))
            {
                if (cell.InBounds(this.Map) == false)
                {
                    continue;
                }
                if (Util_FishIndustry.IsAquaticTerrain(this.Map, cell))
                {
                    aquaticCellsNumber++;
                }
            }
            float aquaticCellsNumberThreshold = (float)(GenRadial.NumCellsInRadius(this.def.specialDisplayRadius)) / 2f;
            if (aquaticCellsNumber < aquaticCellsNumberThreshold)
            {
                maxFishStockTerrain = Mathf.CeilToInt((float)maxFishStockDefault * (aquaticCellsNumber / aquaticCellsNumberThreshold));
            }

            // Compute max fish stock according to biome.
            int maxFishStockBiome = maxFishStockDefault;
            if ((this.Map.Biome == BiomeDef.Named("AridShrubland"))
                || (this.Map.Biome == BiomeDef.Named("Tundra")))
            {
                maxFishStockBiome = 3;
            }
            else if ((this.Map.Biome == BiomeDef.Named("IceSheet"))
                || (this.Map.Biome == BiomeDef.Named("Desert")))
            {
                maxFishStockBiome = 2;
            }
            else if ((this.Map.Biome == BiomeDef.Named("SeaIce"))
                || (this.Map.Biome == BiomeDef.Named("ExtremeDesert")))
            {
                maxFishStockBiome = 1;
            }
            this.maxFishStock = Math.Min(maxFishStockTerrain, maxFishStockBiome);
            if (this.maxFishStock < 1)
            {
                this.maxFishStock = 1;
            }
        }

        /// <summary>
        /// Get the inspection string.
        /// </summary>
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            stringBuilder.AppendLine("Fish stock: " + this.fishStock);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Saves and loads internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            // TODO: save it as a TerrainDef if possible.
            Scribe_Values.LookValue<String>(ref this.middleTerrainCellDefAsString, "middleTerrainCellDefAsString");
            Scribe_Values.LookValue<String>(ref this.riverTerrainCellDefAsString, "riverTerrainCellDefAsString");
            Scribe_Values.LookValue<int>(ref this.fishStock, "fishStock", maxFishStockDefault);
            Scribe_Values.LookValue<int>(ref this.fishStockRespawnTick, "fishStockRespawnTick", 0);
        }

        /// <summary>
        /// Destroys the fishing pier and the fishing spot. Restores the original terrain cells.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            this.Map.terrainGrid.SetTerrain(middleCell, TerrainDef.Named(middleTerrainCellDefAsString));
            this.Map.terrainGrid.SetTerrain(riverCell, TerrainDef.Named(riverTerrainCellDefAsString));
            base.Destroy(mode);
        }
    }
}
