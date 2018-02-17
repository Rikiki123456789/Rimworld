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
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class Zone_Fishing : Zone
    {
        public const float baseFishSpawnMtbZone = 9f * GenDate.TicksPerDay;

        public int nextUpdateTick = 0;

        public bool allowFishing = true;
        public int oceanCellsCount = 0;
        public int riverCellsCount = 0;
        public int marshCellsCount = 0;
        public bool isAffectedByBiome = false;
        public bool isAffectedByToxicFallout = false;
        public bool isAffectedByBadTemperature = false;
        public int maxFishStock = -1; // Will be updated in GetInspectString if set to -1.

        public List<IntVec3> fishesPosition = new List<IntVec3>();

        public int viableCellsCount
        {
            get
            {
                return (this.oceanCellsCount + this.riverCellsCount + this.marshCellsCount);
            }
        }

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
            Scribe_Collections.Look<IntVec3>(ref this.fishesPosition, "fishesPosition", LookMode.Value);
            Scribe_Values.Look<bool>(ref this.allowFishing, "allowFishing");
        }

        public override void RemoveCell(IntVec3 c)
        {
            if (this.fishesPosition.Contains(c))
            {
                this.fishesPosition.Remove(c);
            }
            base.RemoveCell(c);
        }

        // ===================== Other functions =====================
        public void UpdateZone()
        {
            // Check current zone cells are valid. Remove invalid ones (terrain may have changed with moisture pump or mods).
            UpdateAquaticCellsAround();
            // Update zone properties.
            Util_Zone_Fishing.UpdateZoneProperties(this.Map, this.Cells, ref this.oceanCellsCount, ref this.riverCellsCount, ref this.marshCellsCount,
                ref this.isAffectedByBiome, ref this.isAffectedByToxicFallout, ref this.isAffectedByBadTemperature, ref this.maxFishStock);

            // Udpdate fish stock. 
            if ((this.fishesPosition.Count < this.maxFishStock)
                && viableCellsCount > 0)
            {
                float fishSpawnRateFactor = 1f;
                Util_Zone_Fishing.UpdateFishSpawnRateFactor(this.Map, this.oceanCellsCount, this.riverCellsCount, this.marshCellsCount,
                    this.isAffectedByToxicFallout, this.isAffectedByBadTemperature, ref fishSpawnRateFactor);
                float fishSpawnMtb = baseFishSpawnMtbZone * fishSpawnRateFactor;
                int missingFishesCount = this.maxFishStock - this.fishesPosition.Count;
                for (int missingFishIndex = 0; missingFishIndex < missingFishesCount; missingFishIndex++)
                {
                    bool fishShouldBeSpawned = Rand.MTBEventOccurs(fishSpawnMtb, 1, MapComponent_FishingZone.updatePeriodInTicks);
                    if (fishShouldBeSpawned) 
                    {
                        // Try to spawn a new fish.
                        for (int tryIndex = 0; tryIndex < 5; tryIndex++)
                        {
                            IntVec3 spawnCell = this.Cells.RandomElement();
                            if (this.fishesPosition.Contains(spawnCell) == false)
                            {
                                this.fishesPosition.Add(spawnCell);
                                break;
                            }
                        }
                    }
                }
            }
            else if (this.fishesPosition.Count > maxFishStock)
            {
                int surplusFishesCount = this.fishesPosition.Count - maxFishStock;
                for (int surplusFishIndex = 0; surplusFishIndex < surplusFishesCount; surplusFishIndex++)
                {
                    IntVec3 fishPos = fishesPosition.RandomElement();
                    this.fishesPosition.Remove(fishPos);
                }
            }
        }

        // ===================== Other Functions =====================
        public void UpdateAquaticCellsAround()
        {
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
                UpdateAquaticCellsAround();
                Util_Zone_Fishing.UpdateZoneProperties(this.Map, this.Cells, ref this.oceanCellsCount, ref this.riverCellsCount, ref this.marshCellsCount,
                    ref this.isAffectedByBiome, ref this.isAffectedByToxicFallout, ref this.isAffectedByBadTemperature, ref this.maxFishStock);
            }
            // Fish stock.
            stringBuilder.Append("FishIndustry.FishStock".Translate(this.fishesPosition.Count));
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
            if (this.Cells.Count < Util_Zone_Fishing.minCellsToSpawnFish)
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
                    effects.AppendWithSeparator("FishIndustry.AffectedByToxicFallout".Translate(), "FishIndustry.AffectedBySeparator".Translate());
                }
                if (this.isAffectedByBadTemperature)
                {
                    effects.AppendWithSeparator("FishIndustry.AffectedByBadTemperature".Translate(), "FishIndustry.AffectedBySeparator".Translate());
                }
                stringBuilder.Append(effects);
            }
            return stringBuilder.ToString();
        }
    }
}
