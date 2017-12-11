using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace DrillTurret
{
    /// <summary>
    /// Building_DrillTurret class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    [StaticConstructorOnStartup]
    class Building_DrillTurret : Building
    {
        public enum MiningMode
        {
            Ores,
            Rocks,
            OresAndRocks
        }

        // ===================== Variables =====================
        public const int updatePeriodInTicks = 30;
        public int updateOffsetInTicks = 0;

        // Components references.
        public CompPowerTrader powerComp;

        // Drill efficiency.
        public bool isManned = false;
        public float operatorEfficiency = 0f;
        public int drillEfficiencyInPercent = 0;

        // Other.
        public IntVec3 targetPosition = IntVec3.Invalid;
        public MiningMode miningMode = MiningMode.OresAndRocks;

        // Rotation.
        public float turretTopRotation = 0f;

        // Sound.
        public Sustainer laserDrillSoundSustainer = null;

        // Effecter.
        public Effecter laserDrillEffecter = null;

        // Textures.
        public static Material turretTopOnTexture = MaterialPool.MatFrom("Things/Building/DrillTurret_On");
        public static Material turretTopOffTexture = MaterialPool.MatFrom("Things/Building/DrillTurret_Off");
        public Matrix4x4 turretTopMatrix = default(Matrix4x4);
        public Vector3 turretTopScale = new Vector3(4f, 1f, 4f);
        public Matrix4x4 laserBeamMatrix = default(Matrix4x4);
        public Vector3 laserBeamScale = new Vector3(1f, 1f, 1f);
        public static Material laserBeamTexture = MaterialPool.MatFrom("Effects/DrillTurret_LaserBeam", ShaderDatabase.Transparent);
        public static Material targetLineTexture = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 1f, 1f));

        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.powerComp = base.GetComp<CompPowerTrader>();
            this.updateOffsetInTicks = Rand.RangeInclusive(0, updatePeriodInTicks);

            turretTopMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect, this.turretTopRotation.ToQuat(), turretTopScale);
        }

        /// <summary>
        /// Reset target when turret is despawned (when minified for example).
        /// </summary>
        public override void DeSpawn()
        {
            base.DeSpawn();
            ResetTarget();
        }

        /// <summary>
        /// Reset target and stop effecter.
        /// </summary>
        public void ResetTarget()
        {
            this.targetPosition = IntVec3.Invalid;
            StopLaserDrillEffecter();
            this.drillEfficiencyInPercent = 0;
        }
        
        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<IntVec3>(ref this.targetPosition, "targetPosition");
            Scribe_Values.Look<MiningMode>(ref this.miningMode, "MiningMode");
            Scribe_Values.Look<float>(ref this.turretTopRotation, "turretTopRotation");
        }

        // ===================== Drill efficiency function =====================
        /// <summary>
        /// Set the operator manning efficiency.
        /// </summary>
        public void SetOperatorEfficiency(float operatorEfficiency)
        {
            this.isManned = true;
            this.operatorEfficiency = operatorEfficiency;
        }

        /// <summary>
        /// Compute the drill efficiency according to the operating miner skill and available power.
        /// </summary>
        public float ComputeDrillEfficiency()
        {
            const float baseWeight = 0.25f;
            const float operatorWeight = 0.5f;
            const float researchWeight = 0.25f;
            const float noPowerFactor = 0.5f;

            float drillEfficiency = baseWeight;

            if (this.isManned)
            {
                this.isManned = false; // Will be set again by the manning pawn's JobDriver.
                drillEfficiency += operatorWeight * this.operatorEfficiency;
            }
            if (Util_DrillTurret.researchDrillTurretEfficientDrillingDef.IsFinished)
            {
                drillEfficiency += researchWeight;
            }
            if (powerComp.PowerOn == false)
            {
                drillEfficiency *= noPowerFactor;
            }

            return drillEfficiency;
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - look for a target,
        /// - rotate turret top if needed,
        /// - drill it.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if ((Find.TickManager.TicksGame % updatePeriodInTicks) == this.updateOffsetInTicks)
            {
                // Check locked target is still valid.
                if (this.targetPosition.IsValid)
                {
                    if (IsValidTargetAt(this.targetPosition) == false)
                    {
                        // Target is no more valid.
                        ResetTarget();
                    }
                }

                // Target is invalid. Look for a valid one.
                if (this.targetPosition.IsValid == false)
                {
                    LookForNewTarget(out this.targetPosition);
                }

                // Compute drill efficiency.
                float drillEfficiency = ComputeDrillEfficiency();
                this.drillEfficiencyInPercent = Mathf.RoundToInt(Mathf.Clamp(drillEfficiency * 100f, 0f, 100f));
            }

            if (this.targetPosition.IsValid)
            {
                // Drill and maintain effecter.
                DrillRock();
                StartOrMaintainLaserDrillEffecter();
            }
            ComputeDrawingParameters();
        }

        // ===================== Utility Function =====================
        /// <summary>
        /// Look for a valid target to drill: ore deposit or natural wall to mine within direct line of sight.
        /// </summary>
        public void LookForNewTarget(out IntVec3 newTargetPosition)
        {
            newTargetPosition = IntVec3.Invalid;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.Position, this.def.specialDisplayRadius, false).InRandomOrder())
            {
                if (IsValidTargetAt(cell))
                {
                    newTargetPosition = cell;
                    break;
                }
            }
            if (newTargetPosition.IsValid)
            {
                this.turretTopRotation = Mathf.Repeat(Mathf.Round((this.targetPosition.ToVector3Shifted() - this.TrueCenter()).AngleFlat()), 360f);
            }
        }

        /// <summary>
        /// Check if a cell contain a valid target to drill.
        /// </summary>
        public bool IsValidTargetAt(IntVec3 position)
        {
            if (GenSight.LineOfSight(this.Position, position, this.Map))
            {
                if ((this.miningMode == MiningMode.Ores)
                    || (this.miningMode == MiningMode.OresAndRocks))
                {
                    // Look for valid ore deposit.
                    Building building = position.GetEdifice(this.Map);
                    if ((building != null)
                        && building.def.building.isResourceRock
                        && building.def.mineable)
                    {
                        return true;
                    }
                }

                if ((this.miningMode == MiningMode.Rocks)
                    || (this.miningMode == MiningMode.OresAndRocks))
                {
                    // Look for valid designation.
                    Designation mineDesignation = this.Map.designationManager.DesignationAt(position, DesignationDefOf.Mine);
                    if (mineDesignation != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a cell contain a valid target to drill for gizmo target selection.
        /// </summary>
        public bool IsValidTargetAtForGizmo(IntVec3 position)
        {
            if (GenSight.LineOfSight(this.Position, position, this.Map))
            {
                Building building = position.GetEdifice(this.Map);
                if ((building != null)
                    && building.def.mineable)
                {
                    // Look for valid ore deposit or natural rock.
                    if (building.def.building.isResourceRock
                        || building.def.building.isNaturalRock)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Rotate turret top or drill.
        /// </summary>
        public void DrillRock()
        {
            const float researchUnfinishedFactor = 0.75f;
            const int damagePerTick = 4;
            Building rock = this.targetPosition.GetEdifice(this.Map);
            if (rock != null)
            {
                // Drill rock.
                if ((this.drillEfficiencyInPercent == 100)
                    || ((Find.TickManager.TicksGame % 100) <= this.drillEfficiencyInPercent))
                {
                    if (rock.HitPoints > damagePerTick)
                    {
                        // Only damage rock.
                        rock.TakeDamage(new DamageInfo(DamageDefOf.Mining, damagePerTick));
                    }
                    else
                    {
                        // Drill is finsihed.
                        if (rock.def.building.isResourceRock
                            && (rock.def.building.mineableThing != null))
                        {
                            int oreQuantity = rock.def.building.mineableYield;
                            if (Util_DrillTurret.researchDrillTurretEfficientDrillingDef.IsFinished == false)
                            {
                                oreQuantity = Mathf.RoundToInt((float)oreQuantity * researchUnfinishedFactor);
                            }
                            Thing ore = ThingMaker.MakeThing(rock.def.building.mineableThing);
                            ore.stackCount = oreQuantity;
                            GenSpawn.Spawn(ore, rock.Position, this.Map);
                            rock.Destroy(DestroyMode.Vanish);
                        }
                        else
                        {
                            rock.Destroy(DestroyMode.KillFinalize);
                        }
                    }
                    if (rock.DestroyedOrNull())
                    {
                        ResetTarget();
                        LookForNewTarget(out this.targetPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Stop the laser drill effecter.
        /// </summary>
        public void StopLaserDrillEffecter()
        {
            if (this.laserDrillEffecter != null)
            {
                this.laserDrillEffecter.Cleanup();
                this.laserDrillEffecter = null;
            }
        }

        /// <summary>
        /// Start or maintain the laser drill effecter.
        /// </summary>
        public void StartOrMaintainLaserDrillEffecter()
        {
            if (this.laserDrillEffecter == null)
            {
                this.laserDrillEffecter = new Effecter(EffecterDef.Named("LaserDrill"));
            }
            else
            {
                this.laserDrillEffecter.EffectTick(new TargetInfo(this.targetPosition, this.Map), new TargetInfo(this.Position, this.Map));
            }
        }

        /// <summary>
        /// Compute the laser beam parameters.
        /// </summary>
        public void ComputeDrawingParameters()
        {
            if (this.targetPosition.IsValid)
            {
                // Drilling laser beam.
                Vector3 turretTargetVector = (this.targetPosition.ToVector3Shifted() - this.TrueCenter());
                turretTargetVector.y = 0f;
                this.laserBeamScale.z = turretTargetVector.magnitude - 0.8f;
                Vector3 positionOffset = turretTargetVector / 2f;
                laserBeamMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect + positionOffset, this.turretTopRotation.ToQuat(), this.laserBeamScale);
            }
            else
            {
                // Idle laser beam.
                this.laserBeamScale.z = 1.5f;
                Vector3 positionOffset = new Vector3(0f, 0f, this.laserBeamScale.z / 2f).RotatedBy(this.turretTopRotation);
                laserBeamMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect + positionOffset, this.turretTopRotation.ToQuat(), this.laserBeamScale);
            }
        }

        /// <summary>
        /// Draw the turret top, a laser beam when drilling and a line to the targeted rock.
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            this.turretTopMatrix.SetTRS(base.DrawPos + 1.1f * Altitudes.AltIncVect, this.turretTopRotation.ToQuat(), this.turretTopScale);
            Graphics.DrawMesh(MeshPool.plane10, this.turretTopMatrix, turretTopOnTexture, 0);
            Graphics.DrawMesh(MeshPool.plane10, this.laserBeamMatrix, laserBeamTexture, 0);

            if (Find.Selector.IsSelected(this)
                && (this.targetPosition.IsValid))
            {
                Vector3 lineOrigin = this.TrueCenter();
                Vector3 lineTarget = this.targetPosition.ToVector3Shifted();
                lineTarget.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
                lineOrigin.y = lineTarget.y;
                GenDraw.DrawLineBetween(lineOrigin, lineTarget, targetLineTexture);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder(base.GetInspectString());
            stringBuilder.AppendLine();
            stringBuilder.Append("Drill efficiency: " + drillEfficiencyInPercent + "%");
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000103;

            Command_Action miningModeButton = new Command_Action();
            switch (this.miningMode)
            {
                case (MiningMode.Ores):
                    miningModeButton.defaultLabel = "Mining mode: ORES";
                    miningModeButton.defaultDesc = "Mining mode: ORES. In this mode, the mining turret automatically drill nearby ores.";
                    break;
                case (MiningMode.OresAndRocks):
                    miningModeButton.defaultLabel = "Mining mode: ORES and ROCKS";
                    miningModeButton.defaultDesc = "Mining mode: ORES and ROCKS. In this mode, the mining turret automatically drill nearby ores and designated rocks.";
                    break;
                case (MiningMode.Rocks):
                    miningModeButton.defaultLabel = "Mining mode: ROCKS";
                    miningModeButton.defaultDesc = "Mining mode: ROCKS. In this mode, the mining turret automatically drill nearby designated rocks.";
                    break;
            }
            miningModeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_SwitchMode");
            miningModeButton.activateSound = SoundDef.Named("Click");
            miningModeButton.action = new Action(SwitchMiningMode);
            miningModeButton.groupKey = groupKeyBase + 1;
            buttonList.Add(miningModeButton);

            Command_Action setTargetButton = new Command_Action();
            setTargetButton.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack");
            setTargetButton.defaultLabel = "Set target";
            setTargetButton.defaultDesc = "Order the turret to mine a specific rock. Can only target rocks in range with line of sight.";
            setTargetButton.activateSound = SoundDef.Named("Click");
            setTargetButton.action = new Action(SelectTarget);
            setTargetButton.groupKey = groupKeyBase + 6;
            buttonList.Add(setTargetButton);

            IEnumerable<Gizmo> resultButtonList;
            IEnumerable<Gizmo> basebuttonList = base.GetGizmos();
            if (basebuttonList != null)
            {
                resultButtonList = basebuttonList.Concat(buttonList);
            }
            else
            {
                resultButtonList = buttonList;
            }
            return resultButtonList;
        }

        /// <summary>
        /// Switch mining mode.
        /// </summary>
        public void SwitchMiningMode()
        {
            switch (this.miningMode)
            {
                case MiningMode.Ores:
                    this.miningMode = MiningMode.Rocks;
                    break;
                case MiningMode.Rocks:
                    this.miningMode = MiningMode.OresAndRocks;
                    break;
                case MiningMode.OresAndRocks:
                    this.miningMode = MiningMode.Ores;
                    break;
            }
            ResetTarget();
        }

        /// <summary>
        /// Manually select a target to mine.
        /// </summary>
        public void SelectTarget()
        {
            TargetingParameters targetingParams = new TargetingParameters();
            targetingParams.canTargetPawns = false;
            targetingParams.canTargetBuildings = true;
            targetingParams.canTargetLocations = true;
            targetingParams.validator = delegate (TargetInfo targ)
            {
                if (IsValidTargetAtForGizmo(targ.Cell)
                    && targ.Cell.InHorDistOf(this.Position, this.def.specialDisplayRadius))
                {
                    return true;
                }
                return false;
            };
            Find.Targeter.BeginTargeting(targetingParams, SetForcedTarget);
        }

        public void SetForcedTarget(LocalTargetInfo forcedTarget)
        {
            this.targetPosition = forcedTarget.Cell;
            if (this.Map.designationManager.DesignationAt(forcedTarget.Cell, DesignationDefOf.Mine) == null)
            {
                this.Map.designationManager.AddDesignation(new Designation(forcedTarget, DesignationDefOf.Mine));
            }
            this.turretTopRotation = Mathf.Repeat(Mathf.Round((this.targetPosition.ToVector3Shifted() - this.Position.ToVector3Shifted()).AngleFlat()), 360f);
        }
    }
}
