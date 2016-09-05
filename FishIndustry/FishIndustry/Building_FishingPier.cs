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

        private const int maxFishStock = 5;
        public int fishStock = maxFishStock;
        private const int fishStockRespawnInterval = 60000 / maxFishStock;
        private int fishStockRespawnTick = 0;

        /// <summary>
        /// Convert the cells under the fishing pier into fishing pier cells (technically just water cells with movespeed = 100%).
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            bankCell = this.Position + new IntVec3(0, 0, -1).RotatedBy(this.Rotation);
            middleCell = this.Position + new IntVec3(0, 0, 0).RotatedBy(this.Rotation);
            riverCell = this.Position + new IntVec3(0, 0, 1).RotatedBy(this.Rotation);
            fishingSpotCell = this.Position + new IntVec3(0, 0, 2).RotatedBy(this.Rotation);

            // On first spawning, save the terrain defs and apply the fishing pier equivalent.
            TerrainDef middleCellTerrainDef = Find.TerrainGrid.TerrainAt(middleCell);
            if (middleCellTerrainDef == TerrainDef.Named("Marsh"))
            {
                middleTerrainCellDefAsString = middleCellTerrainDef.ToString();
                Find.TerrainGrid.SetTerrain(middleCell, Util_FishIndustry.FishingPierFloorMarshDef);
            }
            else if (middleCellTerrainDef == TerrainDef.Named("WaterShallow"))
            {
                middleTerrainCellDefAsString = middleCellTerrainDef.ToString();
                Find.TerrainGrid.SetTerrain(middleCell, Util_FishIndustry.FishingPierFloorShallowWaterDef);
            }
            else if (middleCellTerrainDef == TerrainDef.Named("WaterDeep"))
            {
                middleTerrainCellDefAsString = middleCellTerrainDef.ToString();
                Find.TerrainGrid.SetTerrain(middleCell, Util_FishIndustry.FishingPierFloorDeepWaterDef);
            }

            TerrainDef riverCellTerrainDef = Find.TerrainGrid.TerrainAt(riverCell);
            if (riverCellTerrainDef == TerrainDef.Named("Marsh"))
            {
                riverTerrainCellDefAsString = riverCellTerrainDef.ToString();
                Find.TerrainGrid.SetTerrain(riverCell, Util_FishIndustry.FishingPierFloorMarshDef);
            }
            else if (riverCellTerrainDef == TerrainDef.Named("WaterShallow"))
            {
                riverTerrainCellDefAsString = riverCellTerrainDef.ToString();
                Find.TerrainGrid.SetTerrain(riverCell, Util_FishIndustry.FishingPierFloorShallowWaterDef);
            }
            else if (riverCellTerrainDef == TerrainDef.Named("WaterDeep"))
            {
                riverTerrainCellDefAsString = riverCellTerrainDef.ToString();
                Find.TerrainGrid.SetTerrain(riverCell, Util_FishIndustry.FishingPierFloorDeepWaterDef);
            }
        }

        /// <summary>
        /// Periodically resplenishes the fish stock if necessary.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (this.fishStock < maxFishStock)
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
            Scribe_Values.LookValue<int>(ref this.fishStock, "fishStock", 5);
            Scribe_Values.LookValue<int>(ref this.fishStockRespawnTick, "fishStockRespawnTick", 0);
        }

        /// <summary>
        /// Destroys the fishing pier and the fishing spot. Restores the original terrain cells.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            Find.TerrainGrid.SetTerrain(middleCell, TerrainDef.Named(middleTerrainCellDefAsString));
            Find.TerrainGrid.SetTerrain(riverCell, TerrainDef.Named(riverTerrainCellDefAsString));
        }
    }
}
