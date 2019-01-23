using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;   
using Verse;      
using Verse.Sound;

namespace Projector
{
    /// <summary>
    /// Building_MobileProjector class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    [StaticConstructorOnStartup]
    public abstract class  Building_MobileProjector : Building
    {
        public enum LightMode
        {
            Conic,
            Automatic,
            Fixed
        }
        
        // ===================== Variables =====================
        public LightMode lightMode = LightMode.Conic;

        public const int targetSearchPeriodInTicks = 30;
        public int nextTargetSearchTick = 0;

        public const int idlePauseDurationInTicks = 3 * GenTicks.TicksPerRealSecond;
        public int idlePauseTicks = 1;

        // Components references.
        public CompPowerTrader powerComp;

        // Target and light references.
        public Pawn target = null;
        public Thing light = null;

        // Projector range and rotation.
        public const float projectorMinRange = 5f;
        public const int projectorRangeRateInTicksIdle = 16;      // Ticks necessary to change range by 1 when idle.
        public const int projectorRangeRateInTicksTargetting = 8; // Ticks necessary to change range by 1 when targetting.
        public int projectorRangeRateInTicks = projectorRangeRateInTicksIdle;
        public float projectorRangeBaseOffset = 15f;
        public float projectorRange = 15f;
        public float projectorRangeTarget = 15f;

        public const int projectorRotationRateInTicksIdle = 4;       // Ticks between 1° rotation when idle.
        public const int projectorRotationRateInTicksTargetting = 1; // Ticks between 1° rotation when targetting.
        public int projectorRotationRateInTicks = projectorRotationRateInTicksIdle;
        public float projectorRotationBaseOffset = 0f;
        public float projectorRotation = 0f;
        public float projectorRotationTarget = 0f;
        public bool projectorRotationClockwise = true;

        // Synchronization.
        public static int nextGroupId = 1;
        public int groupId = 0;
        public bool groupIdJustChanged = false;

        // Textures.
        public static Material projectorOnTexture = MaterialPool.MatFrom("Things/Building/Security/ProjectorTower_ProjectorOn");
        public static Material projectorOffTexture = MaterialPool.MatFrom("Things/Building/Security/ProjectorTower_ProjectorOff");
        public Matrix4x4 projectorMatrix = default(Matrix4x4);
        public Vector3 projectorScale = new Vector3(3.5f, 1f, 3.5f);
        public Matrix4x4 projectorLightEffectMatrix = default(Matrix4x4);
        public Vector3 projectorLightEffectScale = new Vector3(5f, 1f, 5f);
        public static Material projectorLightEffectTexture = MaterialPool.MatFrom("Things/Building/Security/ProjectorTower_LightEffect", ShaderDatabase.Transparent);
        public static Material targetLineTexture = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 1f, 1f));

        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.powerComp = base.GetComp<CompPowerTrader>();
            this.nextTargetSearchTick = Find.TickManager.TicksGame + Rand.Range(0, targetSearchPeriodInTicks);

            FindNextGroupId();
            
            if (respawningAfterLoad == false)
            {
                // Initial spawn. Align projector with turret rotation.
                this.projectorRotation = this.Rotation.AsAngle;
            }
            projectorMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect, this.projectorRotation.ToQuat(), projectorScale);
            this.powerComp.powerStartedAction = OnPoweredOn;
            this.powerComp.powerStoppedAction = OnPoweredOff;
        }

        /// <summary>
        /// Remove the light when minifying.
        /// </summary>
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            SwitchOffLight();
            base.DeSpawn(mode);
        }

        /// <summary>
        /// Remove the light and destroy the object.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            SwitchOffLight();
            base.Destroy(mode);
        }

        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.nextTargetSearchTick, "nextTargetSearchTick");
            Scribe_References.Look<Pawn>(ref this.target, "target");
            Scribe_References.Look<Thing>(ref this.light, "light");
            Scribe_Values.Look<int>(ref this.projectorRangeRateInTicks, "projectorRangeRateInTicks");
            Scribe_Values.Look<int>(ref this.projectorRotationRateInTicks, "projectorRotationRateInTicks");
            Scribe_Values.Look<float>(ref this.projectorRotationBaseOffset, "projectorRotationBaseOffset");
            Scribe_Values.Look<float>(ref this.projectorRotation, "projectorRotation");
            Scribe_Values.Look<float>(ref this.projectorRotationTarget, "projectorRotationTarget");
            Scribe_Values.Look<bool>(ref this.projectorRotationClockwise, "projectorRotationClockwise");
            Scribe_Values.Look<float>(ref this.projectorRangeBaseOffset, "projectorRangeBaseOffset");
            Scribe_Values.Look<float>(ref this.projectorRange, "projectorRange");
            Scribe_Values.Look<float>(ref this.projectorRangeTarget, "projectorRangeTarget");

            Scribe_Values.Look<LightMode>(ref this.lightMode, "lightMode");
            Scribe_Values.Look<int>(ref this.idlePauseTicks, "idlePauseTicks");
            Scribe_Values.Look<int>(ref this.groupId, "groupId");
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - look for a target,
        /// - light it if it exists,
        /// - otherwise, idle turn.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            
            if (CheckAdditionalConditions() == false)
            {
                return;
            }

            // Check if tower is powered.
            if (powerComp.PowerOn == false)
            {
                return;
            }
            
            // Check locked target is still valid.
            if (this.target != null)
            {
                // Check target is still valid: not killed or downed and in sight.
                if (this.target.DestroyedOrNull()
                    || (IsPawnValidTarget(this.target) == false))
                {
                    // Target is no more valid.
                    StopTargetting();
                    Pawn newTarget = LookForNewTarget();
                    if (newTarget != null)
                    {
                        StartTargetting(newTarget);
                    }
                    else
                    {
                        // Only synchronize towers if no new target is found.
                        SynchronizeProjectorsInGroup();
                    }
                }
            }
            
            // Periodically look for a new target if idle or update its position.
            if (Find.TickManager.TicksGame >= this.nextTargetSearchTick)
            {
                this.nextTargetSearchTick = Find.TickManager.TicksGame + targetSearchPeriodInTicks;
                
                if (this.target == null)
                {
                    // No locked target: look for a new target.
                    Pawn newTarget = LookForNewTarget();
                    if (newTarget != null)
                    {
                        StartTargetting(newTarget);
                    }
                }
                
                if (this.target != null)
                {
                    // Target locked: update projecto rotation and range.
                    this.projectorRotationTarget = Mathf.Round((this.target.Position - this.Position).AngleFlat);
                    ComputeRotationDirection();
                    this.projectorRangeTarget = (this.target.Position - this.Position).ToVector3().magnitude;
                }

                TryBlindTarget();
            }
            
            // Update the projector rotation and range.
            ProjectorMotionTick();
            
            // Start a new idle motion when projector is paused for a moment.
            IdleMotionTick();
        }

        // ===================== Utility Function =====================
        /// <summary>
        /// Only perform main treatment if it returns true.
        /// </summary>
        public abstract bool CheckAdditionalConditions();

        /// <summary>
        /// Action when powered on.
        /// </summary>
        public void OnPoweredOn()
        {
            SynchronizeProjectorsInGroup();
        }

        /// <summary>
        /// Action when powered off.
        /// </summary>
        public void OnPoweredOff()
        {
            StopTargetting();
            SwitchOffLight();
        }

        /// <summary>
        /// Reset rotation and range rates.
        /// </summary>
        public void StopTargetting()
        {
            this.target = null;
            this.projectorRotationRateInTicks = projectorRotationRateInTicksIdle;
            this.projectorRangeRateInTicks = projectorRangeRateInTicksIdle;
        }

        /// <summary>
        /// Set target, rotation and range rates and play a sound.
        /// </summary>
        public void StartTargetting(Pawn newTarget)
        {
            this.target = newTarget;
            this.projectorRotationRateInTicks = projectorRotationRateInTicksTargetting;
            this.projectorRangeRateInTicks = projectorRangeRateInTicksTargetting;
            SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(this.Position, this.Map));
            this.nextTargetSearchTick = Find.TickManager.TicksGame; // So target rotation and range will be immediately updated.
        }

        /// <summary>
        /// Look for a valid target to light: an hostile unroofed pawn within range.
        /// </summary>
        public Pawn LookForNewTarget()
        {
            List<Pawn> hostilesInSight = new List<Pawn>();

            if (this.Faction != null)
            {
                foreach (Pawn pawn in this.Map.mapPawns.AllPawnsSpawned)
                {
                    if (IsPawnValidTarget(pawn))
                    {
                        hostilesInSight.Add(pawn);
                    }
                }
                if (hostilesInSight.Count > 0)
                {
                    return hostilesInSight.RandomElement();
                }
            }
            return null;
        }

        /// <summary>
        /// Check if a pawn is a valid target.
        /// </summary>
        public abstract bool IsPawnValidTarget(Pawn pawn);

        /// <summary>
        /// Blind target if it is facing the projector.
        /// </summary>
        public void TryBlindTarget()
        {
            if ((this.target != null)
                && this.target.RaceProps.Humanlike
                && (this.projectorRotation == this.projectorRotationTarget)
                && (this.projectorRange == this.projectorRangeTarget))
            {
                float deltaAngle = ComputeAbsoluteAngleDelta(this.projectorRotation, this.target.Rotation.AsAngle);
                if ((deltaAngle >= 90f)
                    && (deltaAngle <= 270f))
                {
                    bool targetIsWearingMarineHelmet = false;
                    foreach (Apparel apparel in this.target.apparel.WornApparel)
                    {
                        if (apparel.def == ThingDef.Named("Apparel_PowerArmorHelmet"))
                        {
                            targetIsWearingMarineHelmet = true;
                            break;
                        }
                    }
                    bool targetHasBionicEye = false;
                    targetHasBionicEye = this.target.health.hediffSet.HasHediff(HediffDefOf.BionicEye)
                        || this.target.health.hediffSet.HasHediff(HediffDef.Named("ArchotechEye"));
                    
                    if ((targetHasBionicEye == false)
                        && (targetIsWearingMarineHelmet == false))
                    {
                        this.target.health.AddHediff(Util_Projector.BlindedByProjectorDef);
                    }
                }
            }
        }

        /// <summary>
        /// Power off the light.
        /// </summary>
        public void SwitchOffLight()
        {
            if (this.light.DestroyedOrNull() == false)
            {
                this.light.Destroy();
            }
            this.light = null;
        }

        /// <summary>
        /// Light an area at given position.
        /// </summary>
        public void SwitchOnLight(IntVec3 position)
        {
            if (position.InBounds(this.Map) == false)
            {
                // Out of bounds.
                // Known limitation: when a projector near map border is facing outside, it will not light objects that shoud normally block the light.
                SwitchOffLight();
                return;
            }
            // Remove old light if target has moved.
            if ((this.light.DestroyedOrNull() == false)
                && (position != this.light.Position))
            {
                SwitchOffLight();
            }
            // Spawn a new light.
            if (this.light.DestroyedOrNull())
            {
                // Note: we could forbid several lights on the same spot but as glowers stack, it is visually better.
                /*Thing potentialLight = position.GetFirstThing(this.Map, Util_Projector.ProjectorLightDef);
                if (potentialLight == null)*/
                {
                    this.light = GenSpawn.Spawn(Util_Projector.MobileProjectorLightDef, position, this.Map);
                }
            }
        }
        
        /// <summary>
        /// Synchronize a group of projectors.
        /// </summary>
        public void SynchronizeProjectorsInGroup()
        {
            if (this.groupId == 0)
            {
                StartNewIdleMotion();
            }
            foreach (Building_MobileProjector projector in GetPoweredAndIdleProjectorsWithGroupId(this.groupId))
            {
                projector.StartNewIdleMotion(true);
            }
        }

        public List<Building_MobileProjector> GetPoweredAndIdleProjectorsWithGroupId(int groupId)
        {
            List<Building_MobileProjector> projectorsList = new List<Building_MobileProjector>();

            foreach (Building building in this.Map.listerBuildings.AllBuildingsColonistOfDef(Util_Projector.ProjectorTowerDef))
            {
                Building_MobileProjectorTower projector = building as Building_MobileProjectorTower;
                if ((projector != null)
                    && (projector.groupId == groupId)
                    && (projector.target == null)
                    && (projector.isRoofed == false)
                    && projector.powerComp.PowerOn)
                {
                    projectorsList.Add(projector);
                }
            }
            foreach (Building building in this.Map.listerBuildings.AllBuildingsColonistOfDef(Util_Projector.ProjectorTurretDef))
            {
                Building_MobileProjectorTurret projector = building as Building_MobileProjectorTurret;
                if ((projector != null)
                    && (projector.groupId == groupId)
                    && (projector.target == null)
                    && projector.powerComp.PowerOn)
                {
                    projectorsList.Add(projector);
                }
            }
            return projectorsList;
        }

        /// <summary>
        /// Update projector to face the target.
        /// </summary>
        public void ProjectorMotionTick()
        {
            // Update projector rotation.
            if (this.projectorRotation != this.projectorRotationTarget)
            {
                int rotationRate = this.projectorRotationRateInTicks;
                float deltaAngle = ComputeAbsoluteAngleDelta(this.projectorRotation, this.projectorRotationTarget);
                if (deltaAngle < 20f)
                {
                    rotationRate *= 2; // Slow down rotation when reaching target rotation.
                }
                if ((Find.TickManager.TicksGame % rotationRate) == 0)
                {
                    if (this.projectorRotationClockwise)
                    {
                        this.projectorRotation = Mathf.Repeat(this.projectorRotation + 1f, 360f);
                    }
                    else
                    {
                        this.projectorRotation = Mathf.Repeat(this.projectorRotation - 1f, 360f);
                    }
                }
            }

            // Update projector range.
            if (this.projectorRange != this.projectorRangeTarget)
            {
                if ((Find.TickManager.TicksGame % this.projectorRangeRateInTicks) == 0)
                {
                    if (Mathf.Abs(this.projectorRangeTarget - this.projectorRange) < 1f)
                    {
                        this.projectorRange = this.projectorRangeTarget;
                    }
                    else if (this.projectorRange < this.projectorRangeTarget)
                    {
                        this.projectorRange++;
                    }
                    else
                    {
                        this.projectorRange--;
                    }
                }
            }

            // Light the area in front of the projector.
            Vector3 lightVector3 = new Vector3(0, 0, this.projectorRange).RotatedBy(this.projectorRotation);
            IntVec3 lightIntVec3 = new IntVec3(Mathf.RoundToInt(lightVector3.x), 0, Mathf.RoundToInt(lightVector3.z));
            IntVec3 projectorTarget = this.Position + lightIntVec3;
            TryLightTarget(projectorTarget);
        }

        /// <summary>
        /// Try light the target position.
        /// </summary>
        public abstract void TryLightTarget(IntVec3 targetPosition);

        /// <summary>
        /// Start a new idle motion when projector is paused for a moment.
        /// </summary>
        public void IdleMotionTick()
        {
            if ((this.target == null)
                && (this.projectorRotation == this.projectorRotationTarget)
                && (this.projectorRange == this.projectorRangeTarget)
                && (this.idlePauseTicks > 0))
            {
                // Motion is finished, decrement pause counter.
                this.idlePauseTicks--;
            }
            if (this.idlePauseTicks == 0)
            {
                if (this.groupId == 0)
                {
                    // Solo projector.
                    StartNewIdleMotion();
                }
                else
                {
                    // Group of projectors: check all projectors have finished their pause.
                    bool allProjectorsAreIdle = true;
                    foreach (Building_MobileProjector projector in GetPoweredAndIdleProjectorsWithGroupId(this.groupId))
                    {
                        if (projector.idlePauseTicks > 0)
                        {
                            allProjectorsAreIdle = false;
                            break;
                        }
                    }
                    if (allProjectorsAreIdle)
                    {
                        foreach (Building_MobileProjector projector in GetPoweredAndIdleProjectorsWithGroupId(this.groupId))
                        {
                            projector.StartNewIdleMotion();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compute projector target rotation, target range and rotation direction.
        /// </summary>
        public void StartNewIdleMotion(bool startNewCycle = false)
        {
            this.idlePauseTicks = idlePauseDurationInTicks;
            switch (this.lightMode)
            {
                case LightMode.Automatic:
                    this.projectorRotationTarget = (float)Rand.Range(0, 360);
                    this.projectorRangeTarget = Rand.Range(projectorMinRange, this.def.specialDisplayRadius);
                    break;
                case LightMode.Conic:
                    if (startNewCycle
                        || (this.projectorRotation == Mathf.Repeat(this.Rotation.AsAngle + this.projectorRotationBaseOffset - 45f, 360f)))
                    {
                        // Projector is targeting the left. Now, target the right.
                        this.projectorRotationTarget = Mathf.Repeat(this.Rotation.AsAngle + this.projectorRotationBaseOffset + 45f, 360f);
                    }
                    else
                    {
                        // Projector is targeting the right. Now, target the left.
                        this.projectorRotationTarget = Mathf.Repeat(this.Rotation.AsAngle + this.projectorRotationBaseOffset - 45f, 360f);
                    }
                    this.projectorRangeTarget = this.projectorRangeBaseOffset;
                    break;
                case LightMode.Fixed:
                    // Fixed range and rotation.
                    this.projectorRotationTarget = Mathf.Repeat(this.Rotation.AsAngle + this.projectorRotationBaseOffset, 360f);
                    this.projectorRangeTarget = this.projectorRangeBaseOffset;
                    break;
            }
            // Compute rotation direction.
            ComputeRotationDirection();
        }

        /// <summary>
        /// Compute the optimal rotation direction.
        /// </summary>
        public void ComputeRotationDirection()
        {
            if (this.projectorRotationTarget >= this.projectorRotation)
            {
                float dif = this.projectorRotationTarget - this.projectorRotation;
                if (dif <= 180f)
                {
                    this.projectorRotationClockwise = true;
                }
                else
                {
                    this.projectorRotationClockwise = false;
                }
            }
            else
            {
                float dif = this.projectorRotation - this.projectorRotationTarget;
                if (dif <= 180f)
                {
                    this.projectorRotationClockwise = false;
                }
                else
                {
                    this.projectorRotationClockwise = true;
                }
            }
        }

        /// <summary>
        /// Compute the absolute delta angle between two angles.
        /// </summary>
        public float ComputeAbsoluteAngleDelta(float angle1, float angle2)
        {
            float absoluteDeltaAngle = Mathf.Abs(angle2 - angle1);
            return absoluteDeltaAngle;
        }
        
        // ===================== Gizmo =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000102;

            Command_Action lightModeButton = new Command_Action();
            switch (this.lightMode)
            {
                case (LightMode.Conic):
                    lightModeButton.defaultLabel = "Light mode: conic.";
                    lightModeButton.defaultDesc = "In this mode, the projector patrols in a conic area in front of it.";
                    break;
                case (LightMode.Automatic):
                    lightModeButton.defaultLabel = "Light mode: automatic.";
                    lightModeButton.defaultDesc = "In this mode, the projector randomly lights the surroundings.";
                    break;
                case (LightMode.Fixed):
                    lightModeButton.defaultLabel = "Light mode: fixed.";
                    lightModeButton.defaultDesc = "In this mode, the projector lights a fixed area.";
                    break;
            }
            lightModeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_SwitchMode");
            lightModeButton.activateSound = SoundDef.Named("Click");
            lightModeButton.action = new Action(SwitchLightMode);
            lightModeButton.groupKey = groupKeyBase + 1;
            buttonList.Add(lightModeButton);

            if ((this.lightMode == LightMode.Conic)
                || (this.lightMode == LightMode.Fixed))
            {
                Command_Action decreaseRangeButton = new Command_Action();
                decreaseRangeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_DecreaseRange");
                decreaseRangeButton.defaultLabel = "Range: " + this.projectorRangeBaseOffset;
                decreaseRangeButton.defaultDesc = "Decrease range.";
                decreaseRangeButton.activateSound = SoundDef.Named("Click");
                decreaseRangeButton.action = new Action(DecreaseProjectorRange);
                decreaseRangeButton.groupKey = groupKeyBase + 2;
                buttonList.Add(decreaseRangeButton);

                Command_Action increaseRangeButton = new Command_Action();
                increaseRangeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_IncreaseRange");
                increaseRangeButton.defaultLabel = "";
                increaseRangeButton.defaultDesc = "Increase range.";
                increaseRangeButton.activateSound = SoundDef.Named("Click");
                increaseRangeButton.action = new Action(IncreaseProjectorRange);
                increaseRangeButton.groupKey = groupKeyBase + 3;
                buttonList.Add(increaseRangeButton);

                float rotation = Mathf.Repeat(this.Rotation.AsAngle + this.projectorRotationBaseOffset, 360f);
                Command_Action turnLeftButton = new Command_Action();
                turnLeftButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_TurnLeft");
                turnLeftButton.defaultLabel = "Rotation: " + rotation + "°";
                turnLeftButton.defaultDesc = "Turn left.";
                turnLeftButton.activateSound = SoundDef.Named("Click");
                turnLeftButton.action = new Action(AddProjectorBaseRotationLeftOffset);
                turnLeftButton.groupKey = groupKeyBase + 4;
                buttonList.Add(turnLeftButton);

                Command_Action turnRightButton = new Command_Action();
                turnRightButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_TurnRight");
                turnRightButton.defaultLabel = "";
                turnRightButton.defaultDesc = "Turn right.";
                turnRightButton.activateSound = SoundDef.Named("Click");
                turnRightButton.action = new Action(AddProjectorBaseRotationRightOffset);
                turnRightButton.groupKey = groupKeyBase + 5;
                buttonList.Add(turnRightButton);
            }

            Command_Action setTargetButton = new Command_Action();
            setTargetButton.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack");
            setTargetButton.defaultLabel = "Set target";
            setTargetButton.defaultDesc = "Order the tower to light a specific target. Can only target unroofed hostiles in range.";
            setTargetButton.activateSound = SoundDef.Named("Click");
            setTargetButton.action = new Action(SelectTarget);
            setTargetButton.groupKey = groupKeyBase + 6;
            buttonList.Add(setTargetButton);

            Command_Action synchronizeButton = new Command_Action();
            synchronizeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_Synchronize");
            synchronizeButton.defaultLabel = "Group: " + this.groupId;
            synchronizeButton.defaultDesc = "Synchronize the selected projectors and select conic mode.";
            synchronizeButton.activateSound = SoundDef.Named("Click");
            synchronizeButton.action = new Action(SetNewTowersGroup);
            synchronizeButton.groupKey = groupKeyBase + 7;
            buttonList.Add(synchronizeButton);

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
        /// Switch light mode.
        /// </summary>
        public void SwitchLightMode()
        {
            this.groupId = 0;
            switch (this.lightMode)
            {
                case LightMode.Automatic:
                    this.lightMode = LightMode.Conic;
                    break;
                case LightMode.Conic:
                    this.lightMode = LightMode.Fixed;
                    break;
                case LightMode.Fixed:
                    this.lightMode = LightMode.Automatic;
                    break;
            }
            StartNewIdleMotion();
        }

        /// <summary>
        /// Add an offset to the projector base rotation.
        /// </summary>
        public void AddProjectorBaseRotationLeftOffset()
        {
            this.projectorRotationBaseOffset = Mathf.Repeat(this.projectorRotationBaseOffset - 10f, 360f);
            if (this.groupId > 0)
            {
                SynchronizeProjectorsInGroup();
            }
            else
            {
                StartNewIdleMotion();
            }
        }

        /// <summary>
        /// Add an offset to the projector base rotation.
        /// </summary>
        public void AddProjectorBaseRotationRightOffset()
        {
            this.projectorRotationBaseOffset = Mathf.Repeat(this.projectorRotationBaseOffset + 10f, 360f);
            if (this.groupId > 0)
            {
                SynchronizeProjectorsInGroup();
            }
            else
            {
                StartNewIdleMotion();
            }
        }

        /// <summary>
        /// Decrease the projector range.
        /// </summary>
        public void DecreaseProjectorRange()
        {
            if (this.projectorRangeBaseOffset > projectorMinRange)
            {
                this.projectorRangeBaseOffset -= 1f;
                this.projectorRangeTarget = this.projectorRangeBaseOffset;
            }
            if (this.groupId > 0)
            {
                SynchronizeProjectorsInGroup();
            }
        }

        /// <summary>
        /// Increase the projector range.
        /// </summary>
        public void IncreaseProjectorRange()
        {
            if (this.projectorRangeBaseOffset < Mathf.Round(this.def.specialDisplayRadius))
            {
                this.projectorRangeBaseOffset += 1f;
                this.projectorRangeTarget = this.projectorRangeBaseOffset;
            }
            if (this.groupId > 0)
            {
                SynchronizeProjectorsInGroup();
            }
        }

        /// <summary>
        /// Manually select a target to light.
        /// </summary>
        public void SelectTarget()
        {
            TargetingParameters targetingParams = new TargetingParameters();
            targetingParams.canTargetPawns = true;
            targetingParams.canTargetBuildings = false;
            targetingParams.neverTargetIncapacitated = true;
            targetingParams.validator = delegate (TargetInfo targ)
            {
                if ((targ.HasThing)
                    && (targ.Thing is Pawn)
                    && IsPawnValidTarget(targ.Thing as Pawn))
                {
                    return true;
                }
                return false;
            };
            Find.Targeter.BeginTargeting(targetingParams, SetForcedTarget);
        }

        public void SetForcedTarget(LocalTargetInfo forcedTarget)
        {
            StartTargetting(forcedTarget.Thing as Pawn);
        }

        /// <summary>
        /// Group the selected towers so they are synchronized when idle.
        /// </summary>
        public void SetNewTowersGroup()
        {
            if (this.groupIdJustChanged)
            {
                return;
            }

            List<Building_MobileProjector> projectorsInGroup = new List<Building_MobileProjector>();
            foreach (object obj in Find.Selector.SelectedObjectsListForReading)
            {
                if (obj is Building_MobileProjector)
                {
                    projectorsInGroup.Add(obj as Building_MobileProjector);
                }
            }
            if (projectorsInGroup.Count == 1)
            {
                projectorsInGroup.First().groupId = 0;
            }
            else
            {
                for (int projectorIndex = 0; projectorIndex < projectorsInGroup.Count; projectorIndex++)
                {
                    projectorsInGroup[projectorIndex].groupId = nextGroupId;
                    projectorsInGroup[projectorIndex].lightMode = LightMode.Conic;
                    projectorsInGroup[projectorIndex].groupIdJustChanged = true;
                }
                SynchronizeProjectorsInGroup();
                FindNextGroupId();
            }
        }

        public void FindNextGroupId()
        {
            const int maxIdValue = 1000;
            for (int id = 1; id <= 1000; id++)
            {
                if (id == maxIdValue)
                {
                    Log.Warning("MiningCo. projector tower: found no free group ID. Resetting to 1.");
                    nextGroupId = 1;
                    return;
                }
                bool idIsFree = true;
                foreach (Thing thing in this.Map.listerThings.ThingsOfDef(Util_Projector.ProjectorTowerDef))
                {
                    Building_MobileProjector tower = thing as Building_MobileProjector;
                    if ((tower != null)
                        && (tower.groupId == id))
                    {
                        idIsFree = false;
                        break;
                    }
                }
                foreach (Thing thing in this.Map.listerThings.ThingsOfDef(Util_Projector.ProjectorTurretDef))
                {
                    Building_MobileProjector turret = thing as Building_MobileProjector;
                    if ((turret != null)
                        && (turret.groupId == id))
                    {
                        idIsFree = false;
                        break;
                    }
                }
                if (idIsFree)
                {
                    nextGroupId = id;
                    break;
                }
            }
        }

        // ===================== Draw =====================
        /// <summary>
        /// Draw the projector and a line to the targeted pawn.
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            this.groupIdJustChanged = false; // This is done in the draw function so we can change the group of a tower several times even when the game is paused.
        }
    }
}
