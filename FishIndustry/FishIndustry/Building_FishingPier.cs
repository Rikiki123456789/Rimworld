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

        // Parameters.
        public bool allowFishing = true;
        public bool allowUsingCorn = true;

        public const float baseFishSpawnMtbPier = 6f * GenDate.TicksPerDay;
        public List<IntVec3> aquaticCells = new List<IntVec3>();
        
        public const float aquaticAreaRadius = 10f;
        public int oceanCellsCount = 0;
        public int riverCellsCount = 0;
        public int marshCellsCount = 0;
        public bool isAffectedByBiome = false;
        public bool isAffectedByToxicFallout = false;
        public bool isAffectedByBadTemperature = false;
        public int maxFishStock = -1; // Will be updated in GetInspectString if set to -1.
        public int fishStock = 1;

        public int viableCellsCount
        {
            get
            {
                return (this.oceanCellsCount + this.riverCellsCount + this.marshCellsCount);
            }
        }

        // ===================== Setup Work =====================
        /// <summary>
        /// Convert the cells under the fishing pier into fishing pier cells (technically just water cells with movespeed = 100%).
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, true);

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
        /// Saves and loads internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            // TODO: save it as a TerrainDef if possible.
            Scribe_Values.Look<String>(ref this.middleTerrainCellDefAsString, "middleTerrainCellDefAsString");
            Scribe_Values.Look<String>(ref this.riverTerrainCellDefAsString, "riverTerrainCellDefAsString");
            Scribe_Values.Look<int>(ref this.fishStock, "fishStock");
            Scribe_Values.Look<bool>(ref this.allowFishing, "allowFishing");
            Scribe_Values.Look<bool>(ref this.allowUsingCorn, "allowUsingCorn");
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

        // ===================== Main Work Function =====================
        /// <summary>
        /// Periodically resplenishes the fish stock if possible.
        /// </summary>
        public override void TickRare()
        {
            base.TickRare();

            // Update zone properties.
            UpdateAquaticCellsAround();
            Util_Zone_Fishing.UpdateZoneProperties(this.Map, this.aquaticCells, ref this.oceanCellsCount, ref this.riverCellsCount, ref this.marshCellsCount,
                ref this.isAffectedByBiome, ref this.isAffectedByToxicFallout, ref this.isAffectedByBadTemperature, ref this.maxFishStock);

            // Udpdate fish stock. 
            if ((this.fishStock < this.maxFishStock)
                && viableCellsCount > 0)
            {
                float fishSpawnRateFactor = 1f;
                Util_Zone_Fishing.UpdateFishSpawnRateFactor(this.Map, this.oceanCellsCount, this.riverCellsCount, this.marshCellsCount,
                    this.isAffectedByToxicFallout, this.isAffectedByBadTemperature, ref fishSpawnRateFactor);
                float fishSpawnMtb = baseFishSpawnMtbPier * fishSpawnRateFactor;
                int missingFishesCount = this.maxFishStock - this.fishStock;
                for (int missingFishIndex = 0; missingFishIndex < missingFishesCount; missingFishIndex++)
                {
                    bool fishShouldBeSpawned = Rand.MTBEventOccurs(fishSpawnMtb, 1, MapComponent_FishingZone.updatePeriodInTicks);
                    if (fishShouldBeSpawned)
                    {
                        this.fishStock++;
                    }
                }
            }
            else if (this.fishStock > this.maxFishStock)
            {
                int surplusFishesCount = this.fishStock - this.maxFishStock;
                this.fishStock -= surplusFishesCount;
            }
        }

        // ===================== Other Functions =====================
        public void UpdateAquaticCellsAround()
        {
            this.aquaticCells.Clear();
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.Position, aquaticAreaRadius, false))
            {
                if ((cell.InBounds(this.Map) == false)
                    || (cell.GetRoom(this.Map) != this.GetRoom()))
                {
                    continue;
                }
                if (Util_Zone_Fishing.IsAquaticTerrain(this.Map, cell))
                {
                    this.aquaticCells.Add(cell);
                }
            }
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            int groupKeyBase = 700000115;

            IList<Gizmo> buttonList = new List<Gizmo>();
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                buttonList.Add(gizmo);
            }
            Command_Toggle allowFishingButton = new Command_Toggle();
            allowFishingButton.icon = ContentFinder<Texture2D>.Get(Util_FishIndustry.BluebladeTexturePath);
            allowFishingButton.defaultLabel = "FishIndustry.AllowFishingLabel".Translate();
            allowFishingButton.defaultDesc = "FishIndustry.AllowFishingDesc".Translate();
            allowFishingButton.isActive = (() => this.allowFishing);
            allowFishingButton.toggleAction = delegate
            {
                this.allowFishing = !this.allowFishing;
            };
            allowFishingButton.groupKey = groupKeyBase + 1;
            buttonList.Add(allowFishingButton);

            Command_Toggle allowUsingCornButton = new Command_Toggle();
            allowUsingCornButton.icon = ContentFinder<Texture2D>.Get(ThingDef.Named("RawCorn").graphicData.texPath);
            allowUsingCornButton.defaultLabel = "FishIndustry.AllowUsingCornLabel".Translate();
            allowUsingCornButton.defaultDesc = "FishIndustry.AllowUsingCornDesc".Translate();
            allowUsingCornButton.isActive = (() => this.allowUsingCorn);
            allowUsingCornButton.toggleAction = delegate
            {
                this.allowUsingCorn = !this.allowUsingCorn;
            };
            allowUsingCornButton.groupKey = groupKeyBase + 2;
            buttonList.Add(allowUsingCornButton);

            return buttonList;
        }

        // ===================== Inspection pannel functions =====================
        /// <summary>
        /// Get the inspection string.
        /// </summary>
        /// 
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (Util_FishIndustry.GetFishSpeciesList(this.Map.Biome).NullOrEmpty())
            {
                stringBuilder.Append("FishIndustry.FishingPier_InvalidBiome".Translate());
                return stringBuilder.ToString();
            }
            
            if (this.maxFishStock < 0)
            {
                // Update after a savegame loading for example.
                UpdateAquaticCellsAround();
                Util_Zone_Fishing.UpdateZoneProperties(this.Map, this.aquaticCells, ref this.oceanCellsCount, ref this.riverCellsCount, ref this.marshCellsCount,
                    ref this.isAffectedByBiome, ref this.isAffectedByToxicFallout, ref this.isAffectedByBadTemperature, ref this.maxFishStock);
            }
            // Fish stock.
            stringBuilder.Append("FishIndustry.FishStock".Translate(this.fishStock));
            // Status.
            stringBuilder.AppendLine();
            if (this.viableCellsCount < Util_Zone_Fishing.minCellsToSpawnFish)
            {
                stringBuilder.Append("FishIndustry.NotViableNow".Translate());
            }
            else
            {
                stringBuilder.Append("FishIndustry.SpeciesInZone".Translate() + Util_Zone_Fishing.GetSpeciesInZoneText(this.Map, this.oceanCellsCount, this.riverCellsCount, this.marshCellsCount));
            }
            // Affections.
            if (this.aquaticCells.Count < Util_Zone_Fishing.minCellsToSpawnFish)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("FishIndustry.TooSmallZone".Translate());
            }
            if (this.isAffectedByBiome
                || this.isAffectedByToxicFallout
                || this.isAffectedByBadTemperature)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("FishIndustry.AffectedBy".Translate());
                StringBuilder effects = new StringBuilder();
                if (this.isAffectedByBiome)
                {
                    effects.Append("FishIndustry.AffectedByBiome".Translate());
                }
                if (this.isAffectedByToxicFallout)
                {
                    effects.AppendWithComma("FishIndustry.AffectedByToxicFallout".Translate());
                }
                if (this.isAffectedByBadTemperature)
                {
                    effects.AppendWithComma("FishIndustry.AffectedByBadTemperature".Translate());
                }
                stringBuilder.Append(effects);
            }
            return stringBuilder.ToString();
        }
    }
}
