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
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    class Building_FishingPier : Building_WorkTable
    {
        public string bankCellTerrainDefAsString;
        public string middleCellTerrainDefAsString;
        public string riverCellTerrainDefAsString;

        public IntVec3 fishingSpotCell = new IntVec3(0, 0, 0);
        public IntVec3 riverCell = new IntVec3(0, 0, 0);
        public IntVec3 middleCell = new IntVec3(0, 0, 0);
        public IntVec3 bankCell = new IntVec3(0, 0, 0);

        // Parameters.
        public bool allowFishing = true;
        public bool allowUsingGrain = true;

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

        // Inspect string.
        public string cachedSpeciesInZone = "";
        public float cachedFishSpawnMtb = 0;

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

            if (respawningAfterLoad == false)
            {
                // On first spawning, save the terrain defs and apply the fishing pier equivalent.
                TerrainDef bankCellTerrainDef = map.terrainGrid.TerrainAt(bankCell);
                bankCellTerrainDefAsString = bankCellTerrainDef.ToString();
                if (bankCellTerrainDef == TerrainDef.Named("Marsh"))
                {
                    map.terrainGrid.SetTerrain(bankCell, Util_FishIndustry.FishingPierFloorMarshDef);
                }
                else
                {
                    map.terrainGrid.SetTerrain(bankCell, Util_FishIndustry.FishingPierFloorShallowWaterDef);
                }

                TerrainDef middleCellTerrainDef = map.terrainGrid.TerrainAt(middleCell);
                middleCellTerrainDefAsString = middleCellTerrainDef.ToString();
                if (middleCellTerrainDef == TerrainDef.Named("Marsh"))
                {
                    map.terrainGrid.SetTerrain(middleCell, Util_FishIndustry.FishingPierFloorMarshDef);
                }
                else
                {
                    map.terrainGrid.SetTerrain(middleCell, Util_FishIndustry.FishingPierFloorShallowWaterDef);
                }

                TerrainDef riverCellTerrainDef = map.terrainGrid.TerrainAt(riverCell);
                riverCellTerrainDefAsString = middleCellTerrainDef.ToString();
                if (riverCellTerrainDef == TerrainDef.Named("Marsh"))
                {
                    map.terrainGrid.SetTerrain(riverCell, Util_FishIndustry.FishingPierFloorMarshDef);
                }
                else
                {
                    map.terrainGrid.SetTerrain(riverCell, Util_FishIndustry.FishingPierFloorShallowWaterDef);
                }
            }
        }

        /// <summary>
        /// Saves and loads internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<String>(ref this.bankCellTerrainDefAsString, "bankTerrainCellDefAsString");
            Scribe_Values.Look<String>(ref this.middleCellTerrainDefAsString, "middleTerrainCellDefAsString");
            Scribe_Values.Look<String>(ref this.riverCellTerrainDefAsString, "riverTerrainCellDefAsString");
            Scribe_Values.Look<int>(ref this.fishStock, "fishStock");
            Scribe_Values.Look<bool>(ref this.allowFishing, "allowFishing");
            Scribe_Values.Look<bool>(ref this.allowUsingGrain, "allowUsingGrain");
        }

        /// <summary>
        /// Destroys the fishing pier and the fishing spot. Restores the original terrain cells.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            this.Map.terrainGrid.SetTerrain(bankCell, TerrainDef.Named(bankCellTerrainDefAsString));
            this.Map.terrainGrid.SetTerrain(middleCell, TerrainDef.Named(middleCellTerrainDefAsString));
            this.Map.terrainGrid.SetTerrain(riverCell, TerrainDef.Named(riverCellTerrainDefAsString));
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
            UpdateCells();
            Util_Zone_Fishing.UpdateZoneProperties(this.Map, this.aquaticCells, ref this.oceanCellsCount, ref this.riverCellsCount, ref this.marshCellsCount,
                ref this.isAffectedByBiome, ref this.isAffectedByToxicFallout, ref this.isAffectedByBadTemperature, ref this.maxFishStock);
            this.cachedSpeciesInZone = Util_Zone_Fishing.GetSpeciesInZoneText(this.Map.Biome, this.oceanCellsCount, this.riverCellsCount, this.marshCellsCount);

            // Udpdate fish stock. 
            if ((this.fishStock < this.maxFishStock)
                && viableCellsCount > 0)
            {
                float fishSpawnRateFactor = 0f;
                Util_Zone_Fishing.ComputeFishSpawnRateFactor(this.Map, this.oceanCellsCount, this.riverCellsCount, this.marshCellsCount,
                    this.isAffectedByToxicFallout, this.isAffectedByBadTemperature, out fishSpawnRateFactor);
                cachedFishSpawnMtb = baseFishSpawnMtbPier * fishSpawnRateFactor;
                int missingFishesCount = this.maxFishStock - this.fishStock;
                for (int missingFishIndex = 0; missingFishIndex < missingFishesCount; missingFishIndex++)
                {
                    bool fishShouldBeSpawned = Rand.MTBEventOccurs(cachedFishSpawnMtb, 1, MapComponent_FishingZone.updatePeriodInTicks);
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
        /// <summary>
        /// Update valid aquatic cells around the fishing pier.
        /// </summary>
        public void UpdateCells()
        {
            this.aquaticCells.Clear();
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.fishingSpotCell, aquaticAreaRadius, false))
            {
                // Same room cannot be checked for deep water.
                if (cell.InBounds(this.Map) == false)
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

            Command_Toggle allowUsingGrainButton = new Command_Toggle();
            allowUsingGrainButton.icon = ContentFinder<Texture2D>.Get(ThingDef.Named("RawCorn").graphicData.texPath);
            allowUsingGrainButton.defaultLabel = "FishIndustry.AllowUsingGrainLabel".Translate();
            allowUsingGrainButton.defaultDesc = "FishIndustry.AllowUsingGrainDesc".Translate();
            allowUsingGrainButton.isActive = (() => this.allowUsingGrain);
            allowUsingGrainButton.toggleAction = delegate
            {
                this.allowUsingGrain = !this.allowUsingGrain;
            };
            allowUsingGrainButton.groupKey = groupKeyBase + 2;
            buttonList.Add(allowUsingGrainButton);

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
                UpdateCells();
                Util_Zone_Fishing.UpdateZoneProperties(this.Map, this.aquaticCells, ref this.oceanCellsCount, ref this.riverCellsCount, ref this.marshCellsCount,
                    ref this.isAffectedByBiome, ref this.isAffectedByToxicFallout, ref this.isAffectedByBadTemperature, ref this.maxFishStock);
                this.cachedSpeciesInZone = Util_Zone_Fishing.GetSpeciesInZoneText(this.Map.Biome, this.oceanCellsCount, this.riverCellsCount, this.marshCellsCount);
            }
            // Fish stock.
            stringBuilder.Append("FishIndustry.FishStock".Translate(this.fishStock));
            if (Prefs.DevMode)
            {
                stringBuilder.Append("/" + this.maxFishStock);
            }
            // Status.
            stringBuilder.AppendLine();
            if (this.viableCellsCount < Util_Zone_Fishing.minCellsToSpawnFish)
            {
                stringBuilder.Append("FishIndustry.NotEnoughViableSpace".Translate());
            }
            else
            {
                stringBuilder.Append("FishIndustry.SpeciesInZone".Translate() + this.cachedSpeciesInZone);
            }
            // Affections.
            if (this.isAffectedByBiome
                || this.isAffectedByToxicFallout
                || this.isAffectedByBadTemperature)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("FishIndustry.AffectedBy".Translate());
                StringBuilder effects = new StringBuilder();
                if (this.isAffectedByBiome)
                {
                    effects.AppendWithComma("FishIndustry.AffectedByBiome".Translate());
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
            // Debug.
            if (Prefs.DevMode)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Spawn mean time: " + GenDate.ToStringTicksToPeriod((int)cachedFishSpawnMtb));
            }
            return stringBuilder.ToString();
        }
    }
}
