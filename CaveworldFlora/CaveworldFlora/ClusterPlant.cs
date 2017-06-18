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
    /// ClusterPlant class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class ClusterPlant : Plant
    {
        // TODO: use compFlickable instead of spawning glower buildings.

        public const float minGrowthToReproduce = 0.6f;
        private string cachedLabelMouseover = null;

        public ThingDef_ClusterPlant clusterPlantProps
        {
            get
            {
                return (this.def as ThingDef_ClusterPlant);
            }
        }

        // Fertility.
        public float fertilityGrowthRateFactor
        {
            get
            {
                if (this.clusterPlantProps.growOnlyOnRoughRock)
                {
                    return 1f;
                }
                return this.Map.fertilityGrid.FertilityAt(this.Position);
            }
        }
        public bool isFertilityConditionOk
        {
            get
            {
                return (this.fertilityGrowthRateFactor > 0f);
            }
        }

        // Temperature.
        public bool isInCryostasis
        {
            get
            {
                return (this.Position.GetTemperature(this.Map) < clusterPlantProps.minGrowTemperature);
            }
        }
        public float temperatureGrowthRateFactor
        {
            get
            {
                float temperature = this.Position.GetTemperature(this.Map);
                if ((temperature < this.clusterPlantProps.minGrowTemperature)
                    || (temperature > this.clusterPlantProps.maxGrowTemperature))
                {
                    return 0f;
                }
                else if (temperature < this.clusterPlantProps.minOptimalGrowTemperature)
                {
                    return Mathf.InverseLerp(this.clusterPlantProps.minGrowTemperature, this.clusterPlantProps.minOptimalGrowTemperature, temperature);
                }
                else if (temperature > this.clusterPlantProps.maxOptimalGrowTemperature)
                {
                    return Mathf.InverseLerp(this.clusterPlantProps.maxGrowTemperature, this.clusterPlantProps.maxOptimalGrowTemperature, temperature);
                }
                return 1f;
            }
        }
        public bool isTemperatureConditionOk
        {
            get
            {
                return (this.temperatureGrowthRateFactor > 0f);
            }
        }

        // Light.
        public float lightGrowthRateFactor
        {
            get
            {
                float light = this.Map.glowGrid.GameGlowAt(this.Position);
                if ((light >= this.clusterPlantProps.minLight)
                    && (light <= this.clusterPlantProps.maxLight))
                {
                    return 1f;
                }
                return 0f;
            }
        }
        public bool isLightConditionOk
        {
            get
            {
                return (this.lightGrowthRateFactor > 0f);
            }
        }

        // Symbiosis.
        public bool isSymbiosisOk
        {
            get
            {
                if (this.clusterPlantProps.isSymbiosisPlant == false)
                {
                    return true;
                }
                return (this.cluster.symbiosisCluster.DestroyedOrNull() == false);
            }
        }

        // Glower.
        public Thing glower = null;

        // Cluster.
        public Cluster cluster;

        // Growth rate
        public new float GrowthRate
        {
            get
            {
                return this.fertilityGrowthRateFactor * this.temperatureGrowthRateFactor * this.lightGrowthRateFactor;
            }
        }
        public new float GrowthPerTick
        {
            get
            {
                if (this.LifeStage != PlantLifeStage.Growing
                    || this.isInCryostasis)
                {
                    return 0f;
                }
                float growthPerTick = (1f / (GenDate.TicksPerDay * this.def.plant.growDays));
                return growthPerTick * this.GrowthRate;
            }
        }

        // Plant grower and terrain.
        public bool isOnCavePlantGrower
        {
            get
            {
                Building edifice = this.Position.GetEdifice(this.Map);
                if ((edifice != null)
                    && ((edifice.def == Util_CaveworldFlora.fungiponicsBasinDef)
                    || (edifice.def == ThingDef.Named("PlantPot"))))
                {
                    return true;
                }
                return false;
            }
        }

        public bool isOnValidNaturalSpot
        {
            get
            {
                bool isValidSpot = true;
                if (this.clusterPlantProps.growOnlyOnRoughRock)
                {
                    isValidSpot &= this.isOnNaturalRoughRock;
                }
                else
                {
                    isValidSpot &= this.isFertilityConditionOk;
                }
                if (this.clusterPlantProps.growOnlyUndeRoof)
                {
                    isValidSpot &= this.Map.roofGrid.Roofed(this.Position);
                }
                if (this.clusterPlantProps.growOnlyNearNaturalRock)
                {
                    isValidSpot &= this.isNearNaturalRockBlock;
                }
                return isValidSpot;
            }
        }

        /// <summary>
        /// Check if the terrain is valid. Require a rough terrain not constructible by player.
        /// </summary>
        public bool isOnNaturalRoughRock
        {
            get
            {
                TerrainDef terrain = this.Map.terrainGrid.TerrainAt(this.Position);
                if ((terrain.layerable == false)
                    && terrain.defName.Contains("Rough"))
                {
                    return true;
                }
                return false;
            }
        }

        public bool isNearNaturalRockBlock
        {
            get
            {
                for (int xOffset = -2; xOffset <= 2; xOffset++)
                {
                    for (int zOffset = -2; zOffset <= 2; zOffset++)
                    {
                        IntVec3 checkedPosition = this.Position + new IntVec3(xOffset, 0, zOffset);
                        if (checkedPosition.InBounds(this.Map))
                        {
                            Thing potentialRock = this.Map.thingGrid.ThingAt(checkedPosition, ThingCategory.Building);
                            if ((potentialRock != null)
                                && ((potentialRock as Building) != null))
                            {
                                if ((potentialRock as Building).def.building.isNaturalRock)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
        }

        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            //this.UpdateGlowerAccordingToGrowth(); // TODO: disabled to avoid ton of warning messages when getting temperature.
        }

        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Cluster>(ref this.cluster, "cluster");
            Scribe_References.Look<Thing>(ref this.glower, "glower");
        }

        // ===================== Destroy =====================
        /// <summary>
        /// Destroy the plant and the associated glower if existing.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            TryToDestroyGlower();
            if (cluster.DestroyedOrNull() == false)
            {
                cluster.NotifyPlantRemoved();
            }
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - update the glower if necessary.
        /// - verify the cave plant is in good conditions to growth.
        /// - when the cave plant is too old, damage it over time.
        /// </summary>
        public override void TickLong()
        {
            if (this.isGrowingNow)
            {
                bool plantWasAlreadyMature = (this.LifeStage == PlantLifeStage.Mature);
                this.growthInt += this.GrowthPerTick * GenTicks.TickLongInterval;
                if (!plantWasAlreadyMature
                    && (this.LifeStage == PlantLifeStage.Mature))
                {
                    // Plant just became mature.
                    this.Map.mapDrawer.MapMeshDirty(this.Position, MapMeshFlag.Things);
                }
            }

            // Verify the plant is not in cryostasis.
            if (this.isInCryostasis == false)
            {
                if (this.LifeStage == PlantLifeStage.Mature)
                {
                    this.ageInt += GenTicks.TickLongInterval;
                }
                if (this.Dying)
                {
                    base.TakeDamage(new DamageInfo(DamageDefOf.Rotting, 5, -1, null, null, null));
                }
            }

            // Update glower.
            if (!base.Destroyed)
            {
                UpdateGlowerAccordingToGrowth();
            }

            this.cachedLabelMouseover = null;
        }

        public bool isGrowingNow
        {
            get
            {
                return this.LifeStage == PlantLifeStage.Growing
                    && this.isTemperatureConditionOk
                    && this.isLightConditionOk
                    && (this.isOnCavePlantGrower
                       || (this.isOnValidNaturalSpot
                          && (this.cluster.DestroyedOrNull() == false)));
            }
        }
        public new bool Dying
        {
            get
            {
                if (this.isInCryostasis)
                {
                    return false;
                }
                bool plantIsTooOld = this.def.plant.LimitedLifespan && (this.ageInt > this.def.plant.LifespanTicks);
                if (plantIsTooOld)
                {
                    return true;
                }
                float temperature = this.Position.GetTemperature(this.Map);
                bool plantCanGrowHere = this.isOnCavePlantGrower
                    || ((this.cluster.DestroyedOrNull() == false)
                       && this.isOnValidNaturalSpot);
                bool plantIsInHostileConditions = (temperature > this.clusterPlantProps.maxGrowTemperature)
                    || !this.isLightConditionOk
                    || !plantCanGrowHere
                    || !this.isSymbiosisOk;
                if (plantIsInHostileConditions)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Update the glower according to the plant growth, cryostatis state and snow depth.
        /// </summary>
        public void UpdateGlowerAccordingToGrowth()
        {
            if ((this.clusterPlantProps.hasStaticGlower == false)
                && (this.clusterPlantProps.hasDynamicGlower == false))
            {
                return;
            }
            
            if (this.isInCryostasis
                || (this.Position.GetSnowDepth(this.Map) >= this.def.hideAtSnowDepth))
            {
                TryToDestroyGlower();
                return;
            }

            if (this.clusterPlantProps.hasStaticGlower)
            {
                if (this.glower.DestroyedOrNull())
                {
                    this.glower = GenSpawn.Spawn(Util_CaveworldFlora.GetGlowerStaticDef(this.def), this.Position, this.Map);
                }
            }
            else if (this.clusterPlantProps.hasDynamicGlower)
            {
                if (this.Growth < 0.33f)
                {
                    if ((this.glower.DestroyedOrNull())
                        || (this.glower.def != Util_CaveworldFlora.GetGlowerSmallDef(this.def)))
                    {
                        TryToDestroyGlower();
                        this.glower = GenSpawn.Spawn(Util_CaveworldFlora.GetGlowerSmallDef(this.def), this.Position, this.Map);
                    }
                }
                else if (this.Growth < 0.66f)
                {
                    if ((this.glower.DestroyedOrNull())
                        || (this.glower.def != Util_CaveworldFlora.GetGlowerMediumDef(this.def)))
                    {
                        TryToDestroyGlower();
                        this.glower = GenSpawn.Spawn(Util_CaveworldFlora.GetGlowerMediumDef(this.def), this.Position, this.Map);
                    }
                }
                else
                {
                    if ((this.glower.DestroyedOrNull())
                        || (this.glower.def != Util_CaveworldFlora.GetGlowerBigDef(this.def)))
                    {
                        TryToDestroyGlower();
                        this.glower = GenSpawn.Spawn(Util_CaveworldFlora.GetGlowerBigDef(this.def), this.Position, this.Map);
                    }
                }
            }
        }

        protected void TryToDestroyGlower()
        {
            if (this.glower.DestroyedOrNull() == false)
            {
                this.glower.Destroy();
            }
            this.glower = null;
        }

        /// <summary>
        /// Build the inspect string.
        /// </summary>
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            float num = this.Growth * 100f;
            if (num > 100f)
            {
                num = 100.1f;
            }
            stringBuilder.Append(num.ToString("##0") + "% growth");
            if (this.LifeStage == PlantLifeStage.Mature)
            {
                if (this.def.plant.Harvestable)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("Ready to harvest");
                }
                else
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("Mature");
                }
            }
            else if (this.LifeStage == PlantLifeStage.Growing)
            {
                if (this.isInCryostasis)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("In cryostasis");
                }
                else if (this.Dying)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("Dying");
                    if (this.Position.GetTemperature(this.Map) > this.clusterPlantProps.maxGrowTemperature)
                    {
                        stringBuilder.Append(", drying");
                    }
                    if (this.isLightConditionOk == false)
                    {
                        float light = this.Map.glowGrid.GameGlowAt(this.Position);
                        if (light < this.clusterPlantProps.minLight)
                        {
                            stringBuilder.Append(", too dark");
                        }
                        else if (light > this.clusterPlantProps.maxLight)
                        {
                            stringBuilder.Append(", overlit");
                        }
                    }
                    if (this.clusterPlantProps.growOnlyUndeRoof)
                    {
                        if (this.Map.roofGrid.Roofed(this.Position) == false)
                        {
                            stringBuilder.Append(", unroofed");
                        }
                    }
                    if (this.isOnCavePlantGrower == false)
                    {
                        if (this.clusterPlantProps.growOnlyOnRoughRock)
                        {
                            if (this.isOnNaturalRoughRock == false)
                            {
                                stringBuilder.Append(", unadapted soil");
                            }
                        }
                        else if (this.isFertilityConditionOk == false)
                        {
                            stringBuilder.Append(", unadapted soil");
                        }
                        if (this.clusterPlantProps.growOnlyNearNaturalRock)
                        {
                            if (this.isNearNaturalRockBlock == false)
                            {
                                stringBuilder.Append(", too far from rock");
                            }
                        }
                        if (this.cluster.DestroyedOrNull())
                        {
                            stringBuilder.Append(", cluster root removed");
                        }
                        if (this.clusterPlantProps.isSymbiosisPlant)
                        {
                            if (this.isSymbiosisOk == false)
                            {
                                stringBuilder.Append(", broken symbiosis");
                            }
                        }
                    }
                }
            }
            return stringBuilder.ToString();
        }

        public override string LabelMouseover
        {
            get
            {
                if (this.cachedLabelMouseover == null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(this.def.LabelCap);
                    stringBuilder.Append(" (" + "PercentGrowth".Translate(new object[]
			        {
				        this.GrowthPercentString
			        }));
                    if (this.isInCryostasis)
                    {
                        stringBuilder.Append(", cryostasis");
                    }
                    if (this.Dying)
                    {
                        stringBuilder.Append(", " + "DyingLower".Translate());
                    }
                    stringBuilder.Append(")");
                    this.cachedLabelMouseover = stringBuilder.ToString();
                }
                return this.cachedLabelMouseover;
            }
        }

        // ===================== Static exported functions =====================
        public static bool IsFertilityConditionOkAt(ThingDef_ClusterPlant plantDef, Map map, IntVec3 position)
        {
            float fertility = map.fertilityGrid.FertilityAt(position);
            return ((fertility >= plantDef.minFertility)
                && (fertility <= plantDef.maxFertility));
        }

        public static bool IsLightConditionOkAt(ThingDef_ClusterPlant plantDef, Map map, IntVec3 position)
        {
            float light = map.glowGrid.GameGlowAt(position);
            if ((light >= plantDef.minLight)
                && (light <= plantDef.maxLight))
            {
                return true;
            }
            return false;
        }

        public static bool IsTemperatureConditionOkAt(ThingDef_ClusterPlant plantDef, Map map, IntVec3 position)
        {
            float temperature = position.GetTemperature(map);
            return ((temperature >= plantDef.minGrowTemperature)
                && (temperature <= plantDef.maxGrowTemperature));
        }

        public static bool CanTerrainSupportPlantAt(ThingDef_ClusterPlant plantDef, Map map, IntVec3 position)
        {
            bool isValidSpot = true;

            if (plantDef.growOnlyOnRoughRock)
            {
                isValidSpot &= IsNaturalRoughRockAt(map, position);
            }
            else
            {
                isValidSpot &= IsFertilityConditionOkAt(plantDef, map, position);
            }
            if (plantDef.growOnlyUndeRoof)
            {
                isValidSpot &= map.roofGrid.Roofed(position);
            }
            else
            {
                isValidSpot &= (map.roofGrid.Roofed(position) == false);
            }
            if (plantDef.growOnlyNearNaturalRock)
            {
                isValidSpot &= IsNearNaturalRockBlock(map, position);
            }
            return isValidSpot;
        }

        public static bool IsNaturalRoughRockAt(Map map, IntVec3 position)
        {
            TerrainDef terrain = map.terrainGrid.TerrainAt(position);
            if ((terrain.layerable == false)
                && terrain.defName.Contains("Rough"))
            {
                return true;
            }
            return false;
        }
        
        public static bool IsNearNaturalRockBlock(Map map, IntVec3 position)
        {
            for (int xOffset = -2; xOffset <= 2; xOffset++)
            {
                for (int zOffset = -2; zOffset <= 2; zOffset++)
                {
                    IntVec3 checkedPosition = position + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(map))
                    {
                        Thing potentialRock = map.thingGrid.ThingAt(checkedPosition, ThingCategory.Building);
                        if ((potentialRock != null)
                            && ((potentialRock as Building) != null))
                        {
                            if ((potentialRock as Building).def.building.isNaturalRock)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
