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

        public const float optimalAquaticAreaRadius = 10f;
        public const float optimalAquaticCellsProportion = 0.5f;
        private const int maxFishStockDefault = 5;
        private int maxFishStock = maxFishStockDefault;
        public int fishStock = 1;
        private int fishStockRespawnInterval = (2 * GenDate.TicksPerDay) / maxFishStockDefault;
        private int fishStockRespawnTick = 0;

        /// <summary>
        /// Convert the cells under the fishing pier into fishing pier cells (technically just water cells with movespeed = 100%).
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, true);

            // Compute max fish stock and respawn period according to terrain and biome.
            ComputeMaxFishStockAndRespawnPeriod();

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
            else if ((middleCellTerrainDef == TerrainDefOf.WaterShallow)
                || (middleCellTerrainDef == TerrainDefOf.WaterMovingShallow)
                || (middleCellTerrainDef == TerrainDefOf.WaterOceanShallow))
            {
                middleTerrainCellDefAsString = middleCellTerrainDef.ToString();
                map.terrainGrid.SetTerrain(middleCell, Util_FishIndustry.FishingPierFloorShallowWaterDef);
            }
            else if ((middleCellTerrainDef == TerrainDefOf.WaterDeep)
                || (middleCellTerrainDef == TerrainDefOf.WaterMovingDeep)
                || (middleCellTerrainDef == TerrainDefOf.WaterOceanDeep))
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
            else if ((middleCellTerrainDef == TerrainDefOf.WaterShallow)
                || (middleCellTerrainDef == TerrainDefOf.WaterMovingShallow)
                || (middleCellTerrainDef == TerrainDefOf.WaterOceanShallow))
            {
                riverTerrainCellDefAsString = riverCellTerrainDef.ToString();
                map.terrainGrid.SetTerrain(riverCell, Util_FishIndustry.FishingPierFloorShallowWaterDef);
            }
            else if ((middleCellTerrainDef == TerrainDefOf.WaterDeep)
                || (middleCellTerrainDef == TerrainDefOf.WaterMovingDeep)
                || (middleCellTerrainDef == TerrainDefOf.WaterOceanDeep))
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
                    this.fishStockRespawnTick = Find.TickManager.TicksGame + Mathf.CeilToInt((float)this.fishStockRespawnInterval * Rand.Range(0.8f, 1.2f));
                }
                if (Find.TickManager.TicksGame >= this.fishStockRespawnTick)
                {
                    this.fishStock++;
                    this.fishStockRespawnTick = 0;
                }
            }
        }

        /// <summary>
        /// Compute the max fishing stock and fish respawn rate according to terrain and biome (to avoid exploits due to terrain changes).
        /// </summary>
        public void ComputeMaxFishStockAndRespawnPeriod()
        {
            // Compute max fish stock according to biome.
            this.maxFishStock = maxFishStockDefault;
            if (this.Map.Biome == BiomeDefOf.BorealForest)
            {
                this.maxFishStock = 4;
            }
            else if ((this.Map.Biome == BiomeDefOf.Tundra)
                || (this.Map.Biome == BiomeDefOf.AridShrubland))
            {
                this.maxFishStock = 3;
            }
            else if ((this.Map.Biome == BiomeDefOf.IceSheet)
                || (this.Map.Biome == BiomeDefOf.Desert))
            {
                this.maxFishStock = 2;
            }
            else if ((this.Map.Biome == BiomeDefOf.SeaIce)
                || (this.Map.Biome == BiomeDef.Named("ExtremeDesert")))
            {
                this.maxFishStock = 1;
            }

            // Compute fish stock respawn period factor according  to surrounding aquatic cells.
            float aquaticCellsProportion = Util_FishIndustry.GetAquaticCellsProportionInRadius(this.Position, this.Map, Building_FishingPier.optimalAquaticAreaRadius);
            float fishRespawnFactor = 1f;
            if (aquaticCellsProportion < optimalAquaticCellsProportion)
            {
                fishRespawnFactor = (aquaticCellsProportion / optimalAquaticCellsProportion);
            }
            if (fishRespawnFactor <= 0)
            {
                // Avoid division by 0.
                fishRespawnFactor = 0.05f;
            }

            this.fishStockRespawnInterval = Mathf.CeilToInt((2f * GenDate.TicksPerDay) / ((float)this.maxFishStock * fishRespawnFactor));
        }

        /// <summary>
        /// Get the inspection string.
        /// </summary>
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            stringBuilder.Append("FishIndustry.FishStock".Translate(this.fishStock));

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Saves and loads internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            // TODO: save it as a TerrainDef if possible.
            Scribe_Values.Look<String>(ref this.middleTerrainCellDefAsString, "middleTerrainCellDefAsString");
            Scribe_Values.Look<String>(ref this.riverTerrainCellDefAsString, "riverTerrainCellDefAsString");
            Scribe_Values.Look<int>(ref this.fishStock, "fishStock", maxFishStockDefault);
            Scribe_Values.Look<int>(ref this.fishStockRespawnTick, "fishStockRespawnTick", 0);
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
