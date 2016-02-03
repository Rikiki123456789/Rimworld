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
    /// CavePlant class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class CavePlant : Plant
    {
        // Temperature.
        public const float minTempToGrow = 0f;
        public const float lowerOptimalTempToGrow = 10f;
        public const float upperOptimalTempToGrow = 30f;
        public const float maxTempToGrow = 58f;
        public bool isInCryostasis
        {
            get
            {
                return (this.Position.GetTemperature() < minTempToGrow);
            }
        }

        // Glower.
        public Thing glowerBuilding = null;

        // Cluster size and radius.
        public int clusterSize; // Target number of plants in this cluster.        
        public float GetClusterExclusivityRadius() // No new cluster should spawn in this radius.
        {
            return (float)(this.clusterSize * this.def.plant.wildClusterRadius);
        }
        public static float GetClusterExclusivityRadius(ThingDef cavePlantDef, int clusterSize)
        {
            return (float)(clusterSize * cavePlantDef.plant.wildClusterRadius);
        }
        public static float GetMaxClusterExclusivityRadius(ThingDef cavePlantDef)
        {
            return (float)(cavePlantDef.plant.wildClusterSizeRange.max * cavePlantDef.plant.wildClusterRadius);
        }

        // Plant properties: growing conditions, growth rate, growth temperature efficiency, rotting conditions...
        public bool GrowingNow
        {
            get
            {
                return (this.LifeStage == PlantLifeStage.Growing
                    && IsTemperatureConditionOk(this.Position)
                    && IsLightConditionOk(this.Position)
                    && IsTerrainConditionOk(this.Position)
                    && (IsNearNaturalRockBlock(this.Position)
                    || IsOnFungiponicsBasin(this.Position)));
            }
        }
        public float GrowthPerTick
        {
            get
            {
                return (1f / (30000f * this.def.plant.growDays));
            }
        }
        public float GrowthPerTickLong
        {
            get
            {
                return (this.GrowthPerTick * GenTicks.TickLongInterval);
            }
        }
        public float TemperatureEfficiency
        {
            get
            {
                float temperature = this.Position.GetTemperature();
                if ((temperature > minTempToGrow) && (temperature < lowerOptimalTempToGrow))
                    return 0.3f;
                else if ((temperature > lowerOptimalTempToGrow) && (temperature < upperOptimalTempToGrow))
                    return 1f;
                else if ((temperature > upperOptimalTempToGrow) && (temperature < maxTempToGrow))
                    return 0.6f;
                else
                    return 0;
            }
        }
        public new bool Rotting
        {
            get
            {
                bool plantIsTooOld = this.def.plant.LimitedLifespan && (this.age > this.def.plant.LifespanTicks);
                float temperature = this.Position.GetTemperature();
                bool plantIsInHostileConditions = (temperature > maxTempToGrow)
                    || !IsLightConditionOk(this.Position)
                    || !IsTerrainConditionOk(this.Position)
                    || !(IsNearNaturalRockBlock(this.Position)
                    || IsOnFungiponicsBasin(this.Position));
                return ((plantIsTooOld || plantIsInHostileConditions)
                    && (this.isInCryostasis == false));
            }
        }

        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            // Set the default cluster radius. This should be overriden by the spawner.
            this.clusterSize = Rand.Range(this.def.plant.wildClusterSizeRange.min, this.def.plant.wildClusterSizeRange.max);
            if ((this.glowerBuilding == null)
                && (this.isInCryostasis == false))
            {
                this.glowerBuilding = GenSpawn.Spawn(Util_CavePlant.GetGlowerSmallDef(this.def), this.Position);
            }
        }

        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.clusterSize, "clusterSize");
            Scribe_References.LookReference<Thing>(ref this.glowerBuilding, "glowerBuilding");
        }

        // ===================== Destroy =====================
        /// <summary>
        /// Destroy the plant and the associated glower if existing.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            TryToDestroyGlowerBulding();
            base.Destroy(mode);
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - update the glower if necessary.
        /// - verify the cave plant is in good conditions to growth.
        /// - when the cave plant is too old, damage it over time.
        /// - when the cave plant is mature, try to reproduce.
        /// </summary>
        public override void TickLong()
        {
            UpdateGlowerBuildingAccordingToGrowth();
            if (this.GrowingNow)
            {
                this.growth += this.GrowthPerTickLong * this.TemperatureEfficiency;
                if (this.LifeStage == PlantLifeStage.Mature)
                {
                    Find.MapDrawer.WholeMapChanged(MapMeshFlag.Things);
                }
            }

            // Verify the plant is not in cryostasis.
            if (this.isInCryostasis == false)
            {
                if (this.LifeStage == PlantLifeStage.Mature)
                {
                    this.age += GenTicks.TickLongInterval;
                }
                if (this.Rotting)
                {
                    int amount = Mathf.CeilToInt(1.25f);
                    base.TakeDamage(new DamageInfo(DamageDefOf.Rotting, amount, null, null, null));
                }
                if (!base.Destroyed)
                {
                    GenCavePlantReproduction.TryToReproduce(this);
                }
            }
        }

        /// <summary>
        /// Update the glower building according to the plant growth.
        /// </summary>
        public void UpdateGlowerBuildingAccordingToGrowth()
        {
            if (this.isInCryostasis)
            {
                TryToDestroyGlowerBulding();
                return;
            }
            
            if (this.growth < 0.33f)
            {
                if ((this.glowerBuilding == null)
                    || (this.glowerBuilding.def != Util_CavePlant.GetGlowerSmallDef(this.def)))
                {
                    TryToDestroyGlowerBulding();
                    this.glowerBuilding = GenSpawn.Spawn(Util_CavePlant.GetGlowerSmallDef(this.def), this.Position);
                }
            }
            else if (this.growth < 0.66f)
            {
                if ((this.glowerBuilding == null)
                    || (this.glowerBuilding.def != Util_CavePlant.GetGlowerMediumDef(this.def)))
                {
                    TryToDestroyGlowerBulding();
                    this.glowerBuilding = GenSpawn.Spawn(Util_CavePlant.GetGlowerMediumDef(this.def), this.Position);
                }
            }
            else
            {
                if ((this.glowerBuilding == null)
                    || (this.glowerBuilding.def != Util_CavePlant.GetGlowerBigDef(this.def)))
                {
                    TryToDestroyGlowerBulding();
                    this.glowerBuilding = GenSpawn.Spawn(Util_CavePlant.GetGlowerBigDef(this.def), this.Position);
                }
            }
        }

        private void TryToDestroyGlowerBulding()
        {
            if ((this.glowerBuilding != null)
                && (this.glowerBuilding.Destroyed == false))
            {
                this.glowerBuilding.Destroy();
            }
            this.glowerBuilding = null;
        }

        /// <summary>
        /// Check the temperature is in a valid range.
        /// </summary>
        public static bool IsTemperatureConditionOk(IntVec3 cavePlantPosition)
        {
            float temperature = cavePlantPosition.GetTemperature();
            if (temperature > minTempToGrow && temperature < maxTempToGrow)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check the plant is not overlit.
        /// </summary>
        public static bool IsLightConditionOk(IntVec3 cavePlantPosition)
        {
            PsychGlow light = Find.GlowGrid.PsychGlowAt(cavePlantPosition);
            if ((light == PsychGlow.Dark)
                || (light == PsychGlow.Lit))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if cavePlantPosition is near a natural rock block.
        /// </summary>
        public static bool IsNearNaturalRockBlock(IntVec3 cavePlantPosition)
        {
            IntVec3 checkedPosition = new IntVec3(0, 0, 0);
            for (int xOffset = -2; xOffset <= 2; xOffset++)
            {
                for (int zOffset = -2; zOffset <= 2; zOffset++)
                {
                    checkedPosition = cavePlantPosition + new IntVec3(xOffset, 0, zOffset);
                    Thing potentialRock = Find.ThingGrid.ThingAt(checkedPosition, ThingCategory.Building);
                    if (potentialRock != null)
                    {
                        if ((potentialRock as Building).def.building.isNaturalRock)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the terrain is valid.
        /// </summary>
        public static bool IsTerrainConditionOk(IntVec3 cavePlantPosition)
        {
            TerrainDef terrain = Find.TerrainGrid.TerrainAt(cavePlantPosition);
            if (terrain.defName.Contains("Rough"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the plant is on a fungiponics.
        /// </summary>
        public static bool IsOnFungiponicsBasin(IntVec3 cavePlantPosition)
        {
            Building edifice = cavePlantPosition.GetEdifice();
            if ((edifice != null)
                && (edifice.def == Util_CavePlant.fungiponicsBasinDef))
            {
                return true;
            }
            return false;

        }

        /// <summary>
        /// Build the inspect string.
        /// </summary>
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            float num = this.growth * 100f;
            if (num > 100f)
            {
                num = 100.1f;
            }
            stringBuilder.AppendLine(num.ToString("##0") + "% growth");
            if (this.LifeStage == PlantLifeStage.Mature)
            {
                if (this.def.plant.Harvestable)
                {
                    stringBuilder.AppendLine("Ready to harvest");
                }
                else
                {
                    stringBuilder.AppendLine("Mature");
                }
            }
            else if (this.LifeStage == PlantLifeStage.Growing)
            {
                if (this.Rotting)
                {
                    if (this.Position.GetTemperature() > maxTempToGrow)
                    {
                        stringBuilder.AppendLine("Drying");
                    }
                    if (IsLightConditionOk(this.Position) == false)
                    {
                        stringBuilder.AppendLine("Overlit");
                    }
                    if (IsNearNaturalRockBlock(this.Position) == false)
                    {
                        stringBuilder.AppendLine("Too far from a rock");
                    }
                    if (IsTerrainConditionOk(this.Position) == false)
                    {
                        stringBuilder.AppendLine("Unadapted soil");
                    }
                }
                else
                {
                    if (this.isInCryostasis)
                    {
                        stringBuilder.AppendLine("In cryostasis");
                    }
                }
            }
            return stringBuilder.ToString();
        }
    }
}
