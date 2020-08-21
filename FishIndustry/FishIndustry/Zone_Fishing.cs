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
    /// Zone_Fishing class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    class Zone_Fishing : Zone
    {
        public const float baseFishSpawnMtbZone = 7f * GenDate.TicksPerDay;

        public int nextUpdateTick = 0;

        public bool allowFishing = true;
        public int oceanCellsCount = 0;
        public int riverCellsCount = 0;
        public int marshCellsCount = 0;
        public bool isAffectedByBiome = false;
        public bool isAffectedByMapCondition = false;
        public bool isAffectedByBadTemperature = false;
        public int maxFishStock = -1; // Will be updated in GetInspectString if set to -1.
        
        public List<IntVec3> fishingSpots = new List<IntVec3>();

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

        // Drawing.
        public const int fishingZoneColorCount = 5;
        public static int nextFishingZoneColorIndex = 0;

        public override bool IsMultiselectable
        {
            get
            {
                return true;
            }
        }

        protected override Color NextZoneColor
        {
            get
            {
                Color color = GetZoneColor(nextFishingZoneColorIndex);
                nextFishingZoneColorIndex++;
                if (nextFishingZoneColorIndex >= fishingZoneColorCount)
                {
                    nextFishingZoneColorIndex = 0;
                }
                return color;
            }
        }

        public Color GetZoneColor(int colorIndex)
        {
            Color color = Color.black;
            switch (colorIndex)
            {
                case 0:
                    color = Color.Lerp(new Color(0f, 0f, 1f), Color.gray, 0.5f);
                    color.a = 0.09f;
                    break;
                case 1:
                    color = Color.Lerp(new Color(0f, 1f, 1f), Color.gray, 0.5f);
                    color.a = 0.09f;
                    break;
                case 2:
                    color = Color.Lerp(new Color(0f, 0.5f, 1f), Color.gray, 0.5f);
                    color.a = 0.09f;
                    break;
                case 3:
                    color = Color.Lerp(new Color(0.5f, 1f, 1f), Color.gray, 0.5f);
                    color.a = 0.09f;
                    break;
                case 4:
                default:
                    color = Color.Lerp(new Color(0.5f, 0.5f, 1f), Color.gray, 0.5f);
                    color.a = 0.09f;
                    break;
            }
            return color;
        }

        // ===================== Setup Work =====================     
		public Zone_Fishing()
		{
		}

		public Zone_Fishing(ZoneManager zoneManager) : base("FishIndustry.FishingZone".Translate(), zoneManager)
        {
		}

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.nextUpdateTick, "nextUpdateTick");
            Scribe_Collections.Look<IntVec3>(ref this.fishingSpots, "fishingSpots", LookMode.Value);
            Scribe_Values.Look<bool>(ref this.allowFishing, "allowFishing");
        }
        
        public override void RemoveCell(IntVec3 c)
        {
            // Look for an adjacent fishing spot.
            foreach (IntVec3 offset in GenAdj.CardinalDirections)
            {
                IntVec3 adjacentCell = c + offset;
                if (this.fishingSpots.Contains(adjacentCell))
                {
                    // adjacentCell is a fishing spot.
                    // Check if it is adjacent to another fishing zone cell.
                    bool isTouchingFishingZone = false;
                    foreach (IntVec3 offset2 in GenAdj.CardinalDirections)
                    {
                        IntVec3 adjacentToFishingSpotCell = adjacentCell + offset2;
                        if ((adjacentToFishingSpotCell != c)
                            && this.cells.Contains(adjacentToFishingSpotCell))
                        {
                            // Fisghing spot is adjacent to another fishing zone cell.
                            isTouchingFishingZone = true;
                            break;
                        }
                    }
                    if (isTouchingFishingZone == false)
                    {
                        this.fishingSpots.Remove(adjacentCell);
                    }
                }
            }
            base.RemoveCell(c);
        }
        
        /// <summary>
        /// Check if a cell is near a fishing zone.
        /// </summary>
        public bool IsNearFishingZone(IntVec3 cell)
        {
            foreach (IntVec3 offset in GenAdj.CardinalDirections)
            {
                IntVec3 adjacentCell = cell + offset;
                if (this.cells.Contains(adjacentCell))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a cell is a bank cell.
        /// </summary>
        public bool IsBankCell(IntVec3 cell)
        {
            if (cell.Standable(this.Map)
                && (Util_Zone_Fishing.IsAquaticTerrain(this.Map, cell) == false))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a cell is a bridge cell.
        /// </summary>
        public bool IsBridgeCell(IntVec3 cell)
        {
            if (cell.Standable(this.Map)
                && (cell.GetTerrain(this.Map) == TerrainDefOf.Bridge))
            {
                return true;
            }
            return false;
        }

        // ===================== Other functions =====================
        public void UpdateZone()
        {
            // Check current zone cells are valid. Remove invalid ones (terrain may have changed with moisture pump or mods).
            UpdateCellsAndFishingSpots();

            // Update zone properties.
            Util_Zone_Fishing.UpdateZoneProperties(this.Map, this.Cells, ref this.oceanCellsCount, ref this.riverCellsCount, ref this.marshCellsCount,
                ref this.isAffectedByBiome, ref this.isAffectedByMapCondition, ref this.isAffectedByBadTemperature, ref this.maxFishStock);
            this.cachedSpeciesInZone = Util_Zone_Fishing.GetSpeciesInZoneText(this.Map.Biome, this.oceanCellsCount, this.riverCellsCount, this.marshCellsCount);

            // Udpdate fish stock. 
            if ((this.fishingSpots.Count < this.maxFishStock)
                && this.viableCellsCount > 0)
            {
                float fishSpawnRateFactor = 0f;
                Util_Zone_Fishing.ComputeFishSpawnRateFactor(this.Map, this.oceanCellsCount, this.riverCellsCount, this.marshCellsCount,
                    this.isAffectedByMapCondition, this.isAffectedByBadTemperature, out fishSpawnRateFactor);
                cachedFishSpawnMtb = baseFishSpawnMtbZone * fishSpawnRateFactor;

                // Check if fishes should be spawned.
                int missingFishesCount = this.maxFishStock - this.fishingSpots.Count;
                int fishesToSpawnCount = 0;
                for (int missingFishIndex = 0; missingFishIndex < missingFishesCount; missingFishIndex++)
                {
                    bool fishShouldBeSpawned = Rand.MTBEventOccurs(cachedFishSpawnMtb, 1, MapComponent_FishingZone.updatePeriodInTicks);
                    if (fishShouldBeSpawned) 
                    {
                        fishesToSpawnCount++;
                    }
                }

                if (fishesToSpawnCount > 0)
                {
                    // Update bank and bridge cells.
                    List<IntVec3> bankCells = new List<IntVec3>();
                    List<IntVec3> bridgeCells = new List<IntVec3>();
                    GetFreeBankAndBridgeCells(missingFishesCount, ref bankCells, ref bridgeCells);
                    // Actually try to spawn fishes.
                    for (int newFishIndex = 0; newFishIndex < fishesToSpawnCount; newFishIndex++)
                    {
                        if ((bankCells.Count == 0)
                            && (bridgeCells.Count == 0))
                        {
                            break;
                        }
                        TrySpawnNewFish(ref bankCells, ref bridgeCells);
                    }
                }
            }
            else if (this.fishingSpots.Count > maxFishStock)
            {
                int surplusFishesCount = this.fishingSpots.Count - maxFishStock;
                for (int surplusFishIndex = 0; surplusFishIndex < surplusFishesCount; surplusFishIndex++)
                {
                    IntVec3 position = fishingSpots.RandomElement();
                    this.fishingSpots.Remove(position);
                }
            }
        }

        // ===================== Other Functions =====================
        /// <summary>
        /// Remove cells and fishing spots that are no more valid.
        /// </summary>
        public void UpdateCellsAndFishingSpots()
        {
            // Remove cells that are no more valid (case of moisture pump near marsh).
            List<IntVec3> cellsToRemove = new List<IntVec3>();
            foreach (IntVec3 cell in this.Cells)
            {
                if ((Util_Zone_Fishing.IsAquaticTerrain(this.Map, cell) == false)
                    || (cell.Walkable(this.Map) == false))
                {
                    cellsToRemove.Add(cell);
                }
            }
            foreach (IntVec3 cell in cellsToRemove)
            {
                this.RemoveCell(cell);
            }

            // Remove fishing spots that are no more valid (case of destroyed bridge).
            cellsToRemove.Clear();
            foreach (IntVec3 cell in this.fishingSpots)
            {
                if (IsNearFishingZone(cell) == false)
                {
                    cellsToRemove.Add(cell);
                }
            }
            foreach (IntVec3 cell in cellsToRemove)
            {
                this.fishingSpots.Remove(cell);
            }
        }

        /// <summary>
        /// Get bank and bridge cells which are not already fishing spots.
        /// </summary>
        public void GetFreeBankAndBridgeCells(int requiredBankCellsCount, ref List<IntVec3> bankCells, ref List<IntVec3> bridgeCells)
        {
            int foundCells = 0;
            bankCells.Clear();
            bridgeCells.Clear();

            foreach (IntVec3 cell in this.cells.InRandomOrder()) // The InRandomOrder ensures that we will not always spawn a fish on the same spot.
            {
                foreach (IntVec3 offset in GenAdj.CardinalDirections.InRandomOrder())
                {
                    IntVec3 adjacentCell = cell + offset;
                    if (IsBankCell(adjacentCell)
                        && (this.fishingSpots.Contains(adjacentCell) == false))
                    {
                        if (bankCells.Contains(adjacentCell) == false)
                        {
                            bankCells.Add(adjacentCell);
                        }
                    }
                    if (IsBridgeCell(adjacentCell)
                        && (this.fishingSpots.Contains(adjacentCell) == false))
                    {
                        if (bridgeCells.Contains(adjacentCell) == false)
                        {
                            bridgeCells.Add(adjacentCell);
                            foundCells++;
                            if (foundCells >= requiredBankCellsCount)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        public void TrySpawnNewFish(ref List<IntVec3> bankCells, ref List<IntVec3> bridgeCells)
        {
            // Look first for a near bridge cell.
            foreach (IntVec3 cell in bridgeCells)
            {
                if (this.fishingSpots.Contains(cell) == false)
                {
                    this.fishingSpots.Add(cell);
                    bridgeCells.Remove(cell);
                    //MoteMaker.PlaceTempRoof(cell, this.Map); // TODO: debug.
                    return;
                }
            }

            // Look then for a near bank cell.
            foreach (IntVec3 cell in bankCells)
            {
                if (this.fishingSpots.Contains(cell) == false)
                {
                    this.fishingSpots.Add(cell);
                    bankCells.Remove(cell);
                    MoteMaker.PlaceTempRoof(cell, this.Map);
                    return;
                }
            }
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            int groupKeyBase = 700000114;

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
            return buttonList;
        }

        // ===================== Inspection pannel functions =====================
        /// <summary>
        /// Get the string displayed in the inspection panel.
        /// </summary>
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
                UpdateCellsAndFishingSpots();
                Util_Zone_Fishing.UpdateZoneProperties(this.Map, this.Cells, ref this.oceanCellsCount, ref this.riverCellsCount, ref this.marshCellsCount,
                    ref this.isAffectedByBiome, ref this.isAffectedByMapCondition, ref this.isAffectedByBadTemperature, ref this.maxFishStock);
                this.cachedSpeciesInZone = Util_Zone_Fishing.GetSpeciesInZoneText(this.Map.Biome, this.oceanCellsCount, this.riverCellsCount, this.marshCellsCount);
            }
            // Fish stock.
            stringBuilder.Append("FishIndustry.FishStock".Translate(this.fishingSpots.Count));
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
                || this.isAffectedByMapCondition
                || this.isAffectedByBadTemperature)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("FishIndustry.AffectedBy".Translate());
                StringBuilder effects = new StringBuilder();
                if (this.isAffectedByBiome)
                {
                    effects.AppendWithComma("FishIndustry.AffectedByBiome".Translate());
                }
                if (this.isAffectedByMapCondition)
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
