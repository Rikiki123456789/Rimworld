using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace MobileMineralSonar
{
    /// <summary>
    /// Mobile mineral sonar class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    [StaticConstructorOnStartup]
    class Building_MobileMineralSonar : Building
    {
        // ===================== Variables =====================
        public const int baseMaxScanRange = 30;
        public const int enhancedMaxScanRange = 50;
        public static int maxScanRange = baseMaxScanRange;

        public const float baseDetectionChance = 0.2f;
        public const float enhancedDetectionChance = 0.4f;
        public static float detectionChance = baseDetectionChance;

        public const int powerConsumption = 3000;
        public int scanRange = 1;
        public int scanProgress = 0;
        private const int scanProgressThresholdPerCell = 240;
        public float satelliteDishRotation = 0;
        private bool isInstalled = false;

        public List<ThingDef> detectedDefList = null;
        
        // Components references.
        public CompPowerTrader powerComp;

        // Textures.
        public static Material scanRange10 = MaterialPool.MatFrom("Effects/ScanRange10");
        public static Material scanRange20 = MaterialPool.MatFrom("Effects/ScanRange20");
        public static Material scanRange30 = MaterialPool.MatFrom("Effects/ScanRange30");
        public static Material scanRange40 = MaterialPool.MatFrom("Effects/ScanRange40");
        public static Material scanRange50 = MaterialPool.MatFrom("Effects/ScanRange50");
        public static Material satelliteDish = MaterialPool.MatFrom("Things/Building/SatelliteDish");
        public static Material scanRayDynamic = MaterialPool.MatFrom("Effects/ScanRay50x50", ShaderDatabase.MetaOverlay);
        public static Material scanSpot = MaterialPool.MatFrom("Effects/ScanSpot", ShaderDatabase.Transparent);
        public Material scanRangeDynamic;
        public Matrix4x4 scanRangeMatrix10 = default(Matrix4x4);
        public Matrix4x4 scanRangeMatrix20 = default(Matrix4x4);
        public Matrix4x4 scanRangeMatrix30 = default(Matrix4x4);
        public Matrix4x4 scanRangeMatrix40 = default(Matrix4x4);
        public Matrix4x4 scanRangeMatrix50 = default(Matrix4x4);
        public Matrix4x4 scanRangeDynamicMatrix = default(Matrix4x4);
        public Matrix4x4 scanRayDynamicMatrix = default(Matrix4x4);
        public Matrix4x4 satelliteDishMatrix = default(Matrix4x4);
        public Matrix4x4 scanSpotMatrix = default(Matrix4x4);
        public Vector3 scanRangeScale10 = new Vector3(20f, 1f, 20f);
        public Vector3 scanRangeScale20 = new Vector3(40f, 1f, 40f);
        public Vector3 scanRangeScale30 = new Vector3(60f, 1f, 60f);
        public Vector3 scanRangeScale40 = new Vector3(80f, 1f, 80f);
        public Vector3 scanRangeScale50 = new Vector3(100f, 1f, 100f);
        public Vector3 scanRangeDynamicScale = new Vector3(1f, 1f, 1f);
        public Vector3 scanRayDynamicScale = new Vector3(1f, 1f, 1f);
        public Vector3 satelliteDishScale = new Vector3(2f, 1f, 2f);
        public Vector3 scanSpotScale = new Vector3(1f, 1f, 1f);

        // ===================== Static functions =====================
        public static void TryUpdateScanParameters()
        {
            ResearchProjectDef mmsResearch = ResearchProjectDef.Named("ResearchMobileMineralSonarEnhancedScan");
            if (Find.ResearchManager.GetProgress(mmsResearch) >= mmsResearch.CostApparent)
            {
                maxScanRange = enhancedMaxScanRange;
                detectionChance = enhancedDetectionChance;
            }
        }
        
        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        /// 
        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            
            detectedDefList = new List<ThingDef>();
            foreach (ThingDef metallicDef in ((ThingDef_MobileMineralSonar)this.def).scannedThingDefs)
            {
                detectedDefList.Add(metallicDef);
            }

            // Components initialization.
            powerComp = base.GetComp<CompPowerTrader>();
            powerComp.powerOutputInt = 0;
            
            // The 10f offset on Y axis is mandatory to be over the fog of war.
            scanRangeMatrix10.SetTRS(base.DrawPos + new Vector3(0f, 10f, 0f) + Altitudes.AltIncVect, (0f).ToQuat(), scanRangeScale10);
            scanRangeMatrix20.SetTRS(base.DrawPos + new Vector3(0f, 10f, 0f) + Altitudes.AltIncVect, (0f).ToQuat(), scanRangeScale20);
            scanRangeMatrix30.SetTRS(base.DrawPos + new Vector3(0f, 10f, 0f) + Altitudes.AltIncVect, (0f).ToQuat(), scanRangeScale30);
            scanRangeMatrix40.SetTRS(base.DrawPos + new Vector3(0f, 10f, 0f) + Altitudes.AltIncVect, (0f).ToQuat(), scanRangeScale40);
            scanRangeMatrix50.SetTRS(base.DrawPos + new Vector3(0f, 10f, 0f) + Altitudes.AltIncVect, (0f).ToQuat(), scanRangeScale50);
            satelliteDishMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect, satelliteDishRotation.ToQuat(), satelliteDishScale);

            TryUpdateScanParameters();

            if (this.isInstalled == false)
            {
                // The MMS has just been moved or is spawned for the first time.
                this.scanRange = 1;
                this.scanProgress = 0;
                this.satelliteDishRotation = 0;
            }
            else
            {
                this.isInstalled = false;
            }
        }
        
        /// <summary>
        /// Save and load mobile mineral sonar internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode != LoadSaveMode.Saving)
            {
                this.isInstalled = true;
            }
            // Save and load the work variables, so they don't default after loading.
            Scribe_Values.LookValue<int>(ref scanRange, "scanRange", 1);
            Scribe_Values.LookValue<int>(ref scanProgress, "scanProgress", 1);
            Scribe_Values.LookValue<float>(ref satelliteDishRotation, "satelliteDishRotation", 0f);
        }

        // ===================== Destroy =====================
        /// <summary>
        /// Destroy the mobile mineral sonar and reset its state when deconstructed.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);

            if (mode == DestroyMode.Deconstruct)
            {
                this.scanRange = 1;
                this.scanProgress = 0;
                this.satelliteDishRotation = 0;
            }
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - update the scan range,
        /// - draw the satellite dish,
        /// - draw the scan range only when the mobile mineral sonar is selected.
        /// </summary>
        public override void Tick()
        {
            base.Tick();            
            PerformScanUpdate();
        }
                
        /// <summary>
        /// Perform the scan update and update the satellite dish rotation.
        /// </summary>
        public void PerformScanUpdate()
        {
            // Updates the satellite dish rotation.
            if (powerComp.PowerOn)
            {
                satelliteDishRotation = (satelliteDishRotation + 1f) % 360f;
            }
            else
            {
                satelliteDishRotation = (satelliteDishRotation + 0.2f) % 360f;
            }

            if (scanRange == maxScanRange)
            {
                powerComp.powerOutputInt = 0;
                return;
            }

            if ((Find.TickManager.TicksGame % GenTicks.TicksPerRealSecond) == 0)
            {
                // Increment the scan progress according to the available power input.
                if (powerComp.PowerOn)
                {
                    scanProgress += 5 * (int)GenTicks.TicksPerRealSecond;
                }
                else
                {
                    scanProgress += (int)GenTicks.TicksPerRealSecond;
                }
                if (scanProgress >= (this.scanRange * scanProgressThresholdPerCell))
                {
                    foreach (ThingDef detectedDef in detectedDefList)
                    {
                        UnfogSomeRandomThingAtScanRange(detectedDef);
                    }

                    // Reset the scan progress and increase the next scan duration.
                    scanRange++;
                    scanProgress = 0;
                }
            }
        }

        /// <summary>
        /// Unfog some of the things of type thingDefParameter at scanRange.
        /// </summary>
        public void UnfogSomeRandomThingAtScanRange(ThingDef thingDefParameter)
        {
            // Get the mineral blocks at current scan range.
            IEnumerable<Thing> thingsInTheArea = this.Map.listerThings.ThingsOfDef(thingDefParameter);
            if (thingsInTheArea != null)
            {
                IEnumerable<Thing> thingsAtScanRange = thingsInTheArea.Where(thing => thing.Position.InHorDistOf(this.Position, scanRange)
                    && (thing.Position.InHorDistOf(this.Position, scanRange - 1) == false));
                // Remove the fog on those mineral blocks.
                foreach (Thing thing in thingsAtScanRange)
                {
                    // Chance to unfog a thing.
                    float detectionThreshold = detectionChance + detectionChance * (1 - (float)scanRange / (float)enhancedMaxScanRange);
                    if (Rand.Range(0f, 1f) <= detectionThreshold)
                    {
                        this.Map.fogGrid.Unfog(thing.Position);
                    }
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
            DrawSatelliteDish();

            if (Find.Selector.IsSelected(this) == true)
            {
                DrawMaxScanRange();
                DrawDynamicScanRangeAndScanRay();
                foreach (ThingDef detectedDef in detectedDefList)
                {
                    DrawScanSpotOnThingsWithinScanRange(detectedDef);
                }
            }
        }

        /// <summary>
        /// Draw the satellite dish.
        /// </summary>
        public void DrawSatelliteDish()
        {
            satelliteDishMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect, satelliteDishRotation.ToQuat(), satelliteDishScale);
            Graphics.DrawMesh(MeshPool.plane10, satelliteDishMatrix, satelliteDish, 0);
        }

        /// <summary>
        /// Draw the max scan range.
        /// </summary>
        public void DrawMaxScanRange()
        {
            if (maxScanRange == baseMaxScanRange)
            {
                Graphics.DrawMesh(MeshPool.plane10, scanRangeMatrix30, scanRange30, 0);
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, scanRangeMatrix50, scanRange50, 0);
            }
        }

        /// <summary>
        /// Draw the dynamic scan range and scan ray.
        /// </summary>
        public void DrawDynamicScanRangeAndScanRay()
        {
            if (scanRange <= 10)
            {
                scanRangeDynamic = scanRange10;
            }
            else if (scanRange <= 20)
            {
                scanRangeDynamic = scanRange20;
            }
            else if (scanRange <= 30)
            {
                scanRangeDynamic = scanRange30;
            }
            else if (scanRange <= 40)
            {
                scanRangeDynamic = scanRange40;
            }
            else
            {
                scanRangeDynamic = scanRange50;
            }
            scanRangeDynamicScale = new Vector3(2f * scanRange, 1f, 2f * scanRange);
            scanRangeDynamicMatrix.SetTRS(base.DrawPos + new Vector3(0f, 10f, 0f) + Altitudes.AltIncVect, (0f).ToQuat(), scanRangeDynamicScale);
            Graphics.DrawMesh(MeshPool.plane10, scanRangeDynamicMatrix, scanRangeDynamic, 0);

            scanRayDynamicScale = new Vector3(2f * scanRange, 1f, 2f * scanRange);
            scanRayDynamicMatrix.SetTRS(base.DrawPos + new Vector3(0f, 10f, 0f) + Altitudes.AltIncVect, satelliteDishRotation.ToQuat(), scanRayDynamicScale);
            Graphics.DrawMesh(MeshPool.plane10, scanRayDynamicMatrix, scanRayDynamic, 0);
        }

        /// <summary>
        /// Draw the scan spots on things of def thingDefParameter within scan range.
        /// </summary>
        public void DrawScanSpotOnThingsWithinScanRange(ThingDef thingDefParameter)
        {
            float scanSpotDrawingIntensity = 0f;

            // Get the things within current scan range.
            IEnumerable<Thing> thingsInTheArea = this.Map.listerThings.ThingsOfDef(thingDefParameter);
            if (thingsInTheArea != null)
            {
                thingsInTheArea = thingsInTheArea.Where(thing => thing.Position.InHorDistOf(this.Position, scanRange));
                foreach (Thing thing in thingsInTheArea)
                {
                    if (this.Map.fogGrid.IsFogged(thing.Position) == false)
                    {
                        // Set spot intensity proportional to the dynamic scan ray rotation.
                        Vector3 sonarToMineralVector = thing.Position.ToVector3Shifted() - this.Position.ToVector3Shifted();
                        float orientation = sonarToMineralVector.AngleFlat();
                        scanSpotDrawingIntensity = 1f - (((satelliteDishRotation - orientation + 360) % 360f) / 360f);
                        scanSpotMatrix.SetTRS(thing.Position.ToVector3Shifted() + new Vector3(0f, 10f, 0f) + Altitudes.AltIncVect, (0f).ToQuat(), scanSpotScale);
                        Graphics.DrawMesh(MeshPool.plane10, scanSpotMatrix, FadedMaterialPool.FadedVersionOf(scanSpot, scanSpotDrawingIntensity), 0);
                    }
                }
            }
        }

        /// <summary>
        /// Build the string giving some basic information that is shown when the mobile mineral sonar is selected.
        /// </summary>
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            float powerNeeded = -powerComp.powerOutputInt;
            float powerProduction = 0f;
            float powerStored = 0f;
            float scanProgressInPercent = 0;

            if (powerComp.PowerNet != null)
            {
                powerProduction = powerComp.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
                powerStored = powerComp.PowerNet.CurrentStoredEnergy();
            }
            stringBuilder.AppendLine("Optional power needed/generated/stored:\n"
                + "(" + powerNeeded.ToString() + ")/" + powerProduction.ToString("F0") + "/" + powerStored.ToString("F0"));

            scanProgressInPercent = ((float)this.scanProgress / (float)(this.scanRange * scanProgressThresholdPerCell)) * 100;
            stringBuilder.AppendLine("Scan progress: " + ((int)scanProgressInPercent).ToString() + "%");

            stringBuilder.AppendLine("Current/max scan range: " + this.scanRange.ToString() + "/" + maxScanRange.ToString());

            return stringBuilder.ToString();
        }
    }
}
