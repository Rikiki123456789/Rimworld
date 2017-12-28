using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace SpotlightTurret
{
    // TODO: do not blind target if wearing bionic eye or power armor helmet?

    /// <summary>
    /// SpotlightTurret class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    [StaticConstructorOnStartup]
    class Building_SpotlightTurret : Building
    {
        public enum LightMode
        {
            Conic,
            Automatic,
            Fixed
        }
        
        // ===================== Variables =====================
        public const int targetSearchPeriodInTicks = 30;
        public int nextTargetSearchTick = 0;

        // Components references.
        public CompPowerTrader powerComp;

        // Target and light references.
        public Pawn target = null;
        public Thing light = null;

        // Light mode, spotlight range and rotation.
        public LightMode lightMode = LightMode.Conic;
        public const float spotlightMinRange = 5f;
        public const int spotLightRangeRateInTicksIdle = 16;      // Ticks necessary to change range by 1 when idle.
        public const int spotLightRangeRateInTicksTargetting = 8; // Ticks necessary to change range by 1 when targetting.
        public int spotLightRangeRateInTicks = spotLightRangeRateInTicksIdle;
        public float spotLightRangeBaseOffset = 15f;
        public float spotLightRange = 15f;
        public float spotLightRangeTarget = 15f;

        public const int spotLightRotationRateInTicksIdle = 4;       // Ticks between 1° rotation when idle.
        public const int spotLightRotationRateInTicksTargetting = 1; // Ticks between 1° rotation when targetting.
        public int spotLightRotationRateInTicks = spotLightRotationRateInTicksIdle;
        public float spotLightRotationBaseOffset = 0f;
        public float spotLightRotation = 0f;
        public float spotLightRotationTarget = 0f;
        public bool spotLightRotationClockwise = true;

        public const int idlePauseDurationInTicks = 3 * GenTicks.TicksPerRealSecond;
        public int idlePauseTicks = 1;

        // Synchronization.
        public static int nextGroupId = 1;
        public int groupId = 0;
        public bool groupIdJustChanged = false;

        // Textures.
        public static Material spotlightOnTexture = MaterialPool.MatFrom("Things/Building/SpotlightTurret_SpotlightOn");
        public Matrix4x4 spotlightMatrix = default(Matrix4x4);
        public Vector3 spotlightScale = new Vector3(5f, 1f, 5f);
        public static Material spotlightOffTexture = MaterialPool.MatFrom("Things/Building/SpotlightTurret_SpotlightOff");
        public Matrix4x4 spotlightLightEffectMatrix = default(Matrix4x4);
        public Vector3 spotlightLightEffectScale = new Vector3(5f, 1f, 5f);
        public static Material spotlightLightEffectTexture = MaterialPool.MatFrom("Things/Building/SpotlightTurret_LightEffect", ShaderDatabase.Transparent);
        public static Material targetLineTexture = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 1f, 1f));

        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.powerComp = base.GetComp<CompPowerTrader>();
            this.nextTargetSearchTick = Rand.Range(0, targetSearchPeriodInTicks);

            FindNextGroupId();

            this.spotLightRotation = this.Rotation.AsAngle;
            spotlightMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect, this.spotLightRotation.ToQuat(), spotlightScale);
            this.powerComp.powerStartedAction = OnPoweredOn;
            this.powerComp.powerStoppedAction = OnPoweredOff;
        }

        /// <summary>
        /// Initialize instance variables.
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
            Scribe_Values.Look<LightMode>(ref this.lightMode, "lightMode");
            Scribe_Values.Look<int>(ref this.spotLightRangeRateInTicks, "spotLightRangeRateInTicks");
            Scribe_Values.Look<int>(ref this.spotLightRotationRateInTicks, "spotLightRotationRateInTicks");
            Scribe_Values.Look<float>(ref this.spotLightRotationBaseOffset, "spotLightRotationBaseOffset");
            Scribe_Values.Look<float>(ref this.spotLightRotation, "spotLightRotation");
            Scribe_Values.Look<float>(ref this.spotLightRotationTarget, "spotLightRotationTarget");
            Scribe_Values.Look<bool>(ref this.spotLightRotationClockwise, "spotLightRotationClockwise");
            Scribe_Values.Look<float>(ref this.spotLightRangeBaseOffset, "spotLightRangeBaseOffset");
            Scribe_Values.Look<float>(ref this.spotLightRange, "spotLightRange");
            Scribe_Values.Look<float>(ref this.spotLightRangeTarget, "spotLightRangeTarget");
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

            // Check if turret is powered.
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
                }
            }

            if (Find.TickManager.TicksGame >= this.nextTargetSearchTick)
            {
                this.nextTargetSearchTick = Find.TickManager.TicksGame + targetSearchPeriodInTicks;

                // Target is invalid. Periodically look for a valid one.
                if ((this.target == null)
                    && (this.lightMode != LightMode.Fixed)
                    && (this.Faction != null))
                {
                    // Look for a new target.
                    Pawn newTarget = LookForNewTarget();
                    if (newTarget != null)
                    {
                        StartTargetting(newTarget);
                    }
                }

                // Update target rotation and range.
                if (this.target != null)
                {
                    // Target is valid.
                    this.spotLightRotationTarget = Mathf.Round((this.target.Position - this.Position).AngleFlat);
                    ComputeRotationDirection();
                    this.spotLightRangeTarget = (this.target.Position - this.Position).ToVector3().magnitude;
                }

                // Blind target if it is facing the spotlight.
                if ((this.target != null)
                    && (this.spotLightRotation == this.spotLightRotationTarget)
                    && (this.spotLightRange == this.spotLightRangeTarget))
                {
                    float deltaAngle = ComputeAbsoluteAngleDelta(this.spotLightRotation, this.target.Rotation.AsAngle);
                    if ((deltaAngle >= 90f)
                        && (deltaAngle <= 270f))
                    {
                        this.target.health.AddHediff(Util_SpotlightTurret.BlindedBySpotlightDef);
                    }
                }
            }

            // Idle turn.
            if (this.target == null)
            {
                IdleTurnTick();
            }

            // Update the spotlight rotation and range.
            SpotlightMotionTick();
        }

        // ===================== Utility Function =====================
        /// <summary>
        /// Action when powered on.
        /// </summary>
        public void OnPoweredOn()
        {
            SynchronizeTurretsInGroup();
        }

        /// <summary>
        /// Action when powered off.
        /// </summary>
        public void OnPoweredOff()
        {
            StopTargetting();
            SwitchOffLight();
            StartNewIdleMotion();
            this.target = null;
        }

        /// <summary>
        /// Reset rotation and range rates.
        /// </summary>
        public void StopTargetting()
        {
            this.target = null;
            this.spotLightRotationRateInTicks = spotLightRotationRateInTicksIdle;
            this.spotLightRangeRateInTicks = spotLightRangeRateInTicksIdle;
            SynchronizeTurretsInGroup();
        }

        /// <summary>
        /// Set rotation and range rates.
        /// </summary>
        public void StartTargetting(Pawn newTarget)
        {
            this.target = newTarget;
            this.spotLightRotationRateInTicks = spotLightRotationRateInTicksTargetting;
            this.spotLightRangeRateInTicks = spotLightRangeRateInTicksTargetting;
            SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(this.Position, this.Map));
        }

        /// <summary>
        /// Look for a valid target to light: an hostile pawn within direct line of sight.
        /// </summary>
        public Pawn LookForNewTarget()
        {
            List<Pawn> hostilesInSight = new List<Pawn>();
            
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
            return null;
        }

        /// <summary>
        /// Check if a pawn is a valid target.
        /// </summary>
        public bool IsPawnValidTarget(Pawn pawn)
        {
            if (pawn.HostileTo(this.Faction)
                && (pawn.Downed == false)
                && pawn.Position.InHorDistOf(this.Position, this.def.specialDisplayRadius)
                && GenSight.LineOfSight(this.Position, pawn.Position, this.Map))
            {
                return true;
            }
            return false;
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
            // Remove old light if target has moved.
            if ((this.light.DestroyedOrNull() == false)
                && (position != this.light.Position))
            {
                SwitchOffLight();
            }
            // Spawn a new light.
            if (this.light.DestroyedOrNull())
            {
                this.light = GenSpawn.Spawn(Util_SpotlightTurret.SpotlightLightDef, position, this.Map);
            }
        }
        
        /// <summary>
        /// Synchronize a group of turrets (instantaneously reset their light rotation).
        /// </summary>
        public void SynchronizeTurretsInGroup()
        {
            if (this.groupId == 0)
            {
                return;
            }
            foreach (Building building in this.Map.listerBuildings.AllBuildingsColonistOfDef(Util_SpotlightTurret.SpotlightTurretDef))
            {
                Building_SpotlightTurret turret = building as Building_SpotlightTurret;
                if ((turret != null)
                    && (turret.groupId == this.groupId)
                    && (turret.target == null)
                    && (turret.Faction == this.Faction)
                    && turret.powerComp.PowerOn)
                {
                    turret.spotLightRotation = turret.Rotation.AsAngle;
                    turret.spotLightRange = spotlightMinRange;
                    turret.StartNewIdleMotion();
                }
            }
        }

        /// <summary>
        /// Start a new idle motion when turret is paused for a moment.
        /// </summary>
        public void IdleTurnTick()
        {
            if (this.idlePauseTicks > 0)
            {
                this.idlePauseTicks--;
                if (this.idlePauseTicks == 0)
                {
                    // Start a new idle motion.
                    StartNewIdleMotion();
                }
            }
        }

        /// <summary>
        /// Compute spotlight target rotation, target range and rotation direction.
        /// </summary>
        public void StartNewIdleMotion()
        {
            this.idlePauseTicks = 0;
            switch (this.lightMode)
            {
                case LightMode.Automatic:
                    this.spotLightRotationTarget = (float)Rand.Range(0, 360);
                    this.spotLightRangeTarget = Rand.Range(spotlightMinRange, this.def.specialDisplayRadius);
                    break;
                case LightMode.Conic:
                    if (this.spotLightRotation == Mathf.Repeat(this.Rotation.AsAngle + this.spotLightRotationBaseOffset - 45f, 360f))
                    {
                        // Spotlight is targeting the left. Now, target the right.
                        this.spotLightRotationTarget = Mathf.Repeat(this.Rotation.AsAngle + this.spotLightRotationBaseOffset + 45f, 360f);
                    }
                    else
                    {
                        // Spotlight is targeting the right. Now, target the left.
                        this.spotLightRotationTarget = Mathf.Repeat(this.Rotation.AsAngle + this.spotLightRotationBaseOffset - 45f, 360f);
                    }
                    this.spotLightRangeTarget = this.spotLightRangeBaseOffset;
                    break;
                case LightMode.Fixed:
                    // Fixed range and rotation.
                    this.spotLightRotationTarget = Mathf.Repeat(this.Rotation.AsAngle + this.spotLightRotationBaseOffset, 360f);
                    this.spotLightRangeTarget = this.spotLightRangeBaseOffset;
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
            if (this.spotLightRotationTarget >= this.spotLightRotation)
            {
                float dif = this.spotLightRotationTarget - this.spotLightRotation;
                if (dif <= 180f)
                {
                    this.spotLightRotationClockwise = true;
                }
                else
                {
                    this.spotLightRotationClockwise = false;
                }
            }
            else
            {
                float dif = this.spotLightRotation - this.spotLightRotationTarget;
                if (dif <= 180f)
                {
                    this.spotLightRotationClockwise = false;
                }
                else
                {
                    this.spotLightRotationClockwise = true;
                }
            }
        }

        /// <summary>
        /// Update spotlight to face the target.
        /// </summary>
        public void SpotlightMotionTick()
        {
            // Update spotlight rotation.
            if (this.spotLightRotation != this.spotLightRotationTarget)
            {
                int rotationRate = this.spotLightRotationRateInTicks;
                float deltaAngle = ComputeAbsoluteAngleDelta(this.spotLightRotation, this.spotLightRotationTarget);
                if (deltaAngle < 20f)
                {
                    rotationRate *= 2; // Slow down rotation when reaching target rotation.
                }
                if ((Find.TickManager.TicksGame % rotationRate) == 0)
                {
                    if (this.spotLightRotationClockwise)
                    {
                        this.spotLightRotation = Mathf.Repeat(this.spotLightRotation + 1f, 360f);
                    }
                    else
                    {
                        this.spotLightRotation = Mathf.Repeat(this.spotLightRotation - 1f, 360f);
                    }
                }
            }

            // Update spotlight range.
            if (this.spotLightRange != this.spotLightRangeTarget)
            {
                if ((Find.TickManager.TicksGame % this.spotLightRangeRateInTicks) == 0)
                {
                    if (Mathf.Abs(this.spotLightRangeTarget - this.spotLightRange) < 1f)
                    {
                        this.spotLightRange = this.spotLightRangeTarget;
                    }
                    else if (this.spotLightRange < this.spotLightRangeTarget)
                    {
                        this.spotLightRange++;
                    }
                    else
                    {
                        this.spotLightRange--;
                    }
                }
            }
            
            if ((this.target == null)
                && (this.idlePauseTicks == 0)
                && (this.spotLightRotation == this.spotLightRotationTarget)
                && (this.spotLightRange == this.spotLightRangeTarget))
            {
                // Motion is finished, start pause.
                this.idlePauseTicks = idlePauseDurationInTicks;
            }
            
            // Light the area in front of the spotlight: can be blocked by wall/building.
            Vector3 lightVector3 = new Vector3(0, 0, this.spotLightRange).RotatedBy(this.spotLightRotation);
            IntVec3 lightIntVec3 = new IntVec3(Mathf.RoundToInt(lightVector3.x), 0, Mathf.RoundToInt(lightVector3.z));
            IntVec3 spotlightTarget = this.Position + lightIntVec3;
            IntVec3 farthestPosition = GetFarthestPositionInSight(spotlightTarget);
            SwitchOnLight(farthestPosition);
        }

        /// <summary>
        /// Compute the absolute delta angle between two angles.
        /// </summary>
        public float ComputeAbsoluteAngleDelta(float angle1, float angle2)
        {
            float absoluteDeltaAngle = Mathf.Abs(angle2 - angle1) % 360f;
            if (absoluteDeltaAngle > 180f)
            {
                absoluteDeltaAngle -= 180f;
            }
            return absoluteDeltaAngle;
        }

        /// <summary>
        /// Get the farthest position from the turret in direction of spotlightTarget.
        /// </summary>
        public IntVec3 GetFarthestPositionInSight(IntVec3 spotlightTarget)
        {
            IntVec3 farthestPosition = this.Position;

            Mathf.Clamp(spotlightTarget.x, 0, this.Map.Size.x);
            Mathf.Clamp(spotlightTarget.z, 0, this.Map.Size.z);

            IEnumerable<IntVec3> lineOfSightPoints = GenSight.PointsOnLineOfSight(this.Position, spotlightTarget);
            foreach (IntVec3 point in lineOfSightPoints)
            {
                if (point.CanBeSeenOverFast(this.Map) == false)
                {
                    // Return last non-blocked position.
                    return farthestPosition;
                }
                farthestPosition = point; // Store last valid point in sight.
            }
            if (spotlightTarget.CanBeSeenOverFast(this.Map))
            {
                // Nothing is blocking.
                return spotlightTarget;
            }
            else
            {
                // Target position is blocked. Return last non-blocked position.
                return farthestPosition;
            }
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
                    lightModeButton.defaultLabel = "Ligth mode: conic.";
                    lightModeButton.defaultDesc = "In this mode, the spotlight turret patrols in a conic area in front of it. Automatically lock on hostiles.";
                    break;
                case (LightMode.Automatic):
                    lightModeButton.defaultLabel = "Ligth mode: automatic.";
                    lightModeButton.defaultDesc = "In this mode, the spotlight turret randomly lights the surroundings. Automatically lock on hostiles.";
                    break;
                case (LightMode.Fixed):
                    lightModeButton.defaultLabel = "Ligth mode: fixed.";
                    lightModeButton.defaultDesc = "In this mode, the spotlight turret only light a fixed area. Does NOT automatically lock on hostiles.";
                    break;
            }
            lightModeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_SwitchMode");
            lightModeButton.activateSound = SoundDef.Named("Click");
            lightModeButton.action = new Action(SwitchLigthMode);
            lightModeButton.groupKey = groupKeyBase + 1;
            buttonList.Add(lightModeButton);

            if ((this.lightMode == LightMode.Conic)
                || (this.lightMode == LightMode.Fixed))
            {
                Command_Action decreaseRangeButton = new Command_Action();
                decreaseRangeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_DecreaseRange");
                decreaseRangeButton.defaultLabel = "Range: " + this.spotLightRangeBaseOffset;
                decreaseRangeButton.defaultDesc = "Decrease range.";
                decreaseRangeButton.activateSound = SoundDef.Named("Click");
                decreaseRangeButton.action = new Action(DecreaseSpotlightRange);
                decreaseRangeButton.groupKey = groupKeyBase + 2;
                buttonList.Add(decreaseRangeButton);

                Command_Action increaseRangeButton = new Command_Action();
                increaseRangeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_IncreaseRange");
                increaseRangeButton.defaultLabel = "";
                increaseRangeButton.defaultDesc = "Increase range.";
                increaseRangeButton.activateSound = SoundDef.Named("Click");
                increaseRangeButton.action = new Action(IncreaseSpotlightRange);
                increaseRangeButton.groupKey = groupKeyBase + 3;
                buttonList.Add(increaseRangeButton);

                float rotation = Mathf.Repeat(this.Rotation.AsAngle + this.spotLightRotationBaseOffset, 360f);
                Command_Action turnLeftButton = new Command_Action();
                turnLeftButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_TurnLeft");
                turnLeftButton.defaultLabel = "Rotation: " + rotation + "°";
                turnLeftButton.defaultDesc = "Turn left.";
                turnLeftButton.activateSound = SoundDef.Named("Click");
                turnLeftButton.action = new Action(AddSpotlightBaseRotationLeftOffset);
                turnLeftButton.groupKey = groupKeyBase + 4;
                buttonList.Add(turnLeftButton);

                Command_Action turnRightButton = new Command_Action();
                turnRightButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_TurnRight");
                turnRightButton.defaultLabel = "";
                turnRightButton.defaultDesc = "Turn right.";
                turnRightButton.activateSound = SoundDef.Named("Click");
                turnRightButton.action = new Action(AddSpotlightBaseRotationRightOffset);
                turnRightButton.groupKey = groupKeyBase + 5;
                buttonList.Add(turnRightButton);
            }

            Command_Action setTargetButton = new Command_Action();
            setTargetButton.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack");
            setTargetButton.defaultLabel = "Set target";
            setTargetButton.defaultDesc = "Order the turret to light a specific target. Can only target hostiles in range with line of sight.";
            setTargetButton.activateSound = SoundDef.Named("Click");
            setTargetButton.action = new Action(SelectTarget);
            setTargetButton.groupKey = groupKeyBase + 6;
            buttonList.Add(setTargetButton);

            Command_Action synchronizeButton = new Command_Action();
            synchronizeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_Synchronize");
            synchronizeButton.defaultLabel = "Group: " + this.groupId;
            synchronizeButton.defaultDesc = "Synchronize the selected turrets and select conic mode.";
            synchronizeButton.activateSound = SoundDef.Named("Click");
            synchronizeButton.action = new Action(SetNewTurretsGroup);
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
        public void SwitchLigthMode()
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
        /// Add an offset to the spotlight base rotation.
        /// </summary>
        public void AddSpotlightBaseRotationLeftOffset()
        {
            this.spotLightRotationBaseOffset = Mathf.Repeat(this.spotLightRotationBaseOffset - 10f, 360f);
            if (this.groupId > 0)
            {
                SynchronizeTurretsInGroup();
            }
            else
            {
                StartNewIdleMotion();
            }
        }

        /// <summary>
        /// Add an offset to the spotlight base rotation.
        /// </summary>
        public void AddSpotlightBaseRotationRightOffset()
        {
            this.spotLightRotationBaseOffset = Mathf.Repeat(this.spotLightRotationBaseOffset + 10f, 360f);
            if (this.groupId > 0)
            {
                SynchronizeTurretsInGroup();
            }
            else
            {
                StartNewIdleMotion();
            }
        }

        /// <summary>
        /// Decrease the spotlight range.
        /// </summary>
        public void DecreaseSpotlightRange()
        {
            if (this.spotLightRangeBaseOffset > spotlightMinRange)
            {
                this.spotLightRangeBaseOffset -= 1f;
                this.spotLightRangeTarget = this.spotLightRangeBaseOffset;
            }
            if (this.groupId > 0)
            {
                SynchronizeTurretsInGroup();
            }
        }

        /// <summary>
        /// Increase the spotlight range.
        /// </summary>
        public void IncreaseSpotlightRange()
        {
            if (this.spotLightRangeBaseOffset < Mathf.Round(this.def.specialDisplayRadius))
            {
                this.spotLightRangeBaseOffset += 1f;
                this.spotLightRangeTarget = this.spotLightRangeBaseOffset;
            }
            if (this.groupId > 0)
            {
                SynchronizeTurretsInGroup();
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
        /// Group the selected turrets so they are synchronized when idle.
        /// </summary>
        public void SetNewTurretsGroup()
        {
            if (this.groupIdJustChanged)
            {
                return;
            }

            List<Building_SpotlightTurret> turretsInGroup = new List<Building_SpotlightTurret>();
            foreach (object obj in Find.Selector.SelectedObjectsListForReading)
            {
                if (obj is Building_SpotlightTurret)
                {
                    turretsInGroup.Add(obj as Building_SpotlightTurret);
                }
            }
            if (turretsInGroup.Count == 1)
            {
                turretsInGroup.First().groupId = 0;
            }
            else
            {
                for (int turretIndex = 0; turretIndex < turretsInGroup.Count; turretIndex++)
                {
                    turretsInGroup[turretIndex].groupId = nextGroupId;
                    turretsInGroup[turretIndex].lightMode = LightMode.Conic;
                    turretsInGroup[turretIndex].groupIdJustChanged = true;
                }
                SynchronizeTurretsInGroup();
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
                    Log.Warning("MiningCo. spotlight turret: found no free group ID. Resetting to 1.");
                    nextGroupId = 1;
                    return;
                }
                bool idIsFree = true;
                foreach (Thing thing in this.Map.listerThings.ThingsOfDef(Util_SpotlightTurret.SpotlightTurretDef))
                {
                    Building_SpotlightTurret turret = thing as Building_SpotlightTurret;
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
        /// Draw the spotlight and a line to the targeted pawn.
        /// </summary>
        public override void Draw()
        {
            this.groupIdJustChanged = false; // This is done in the draw function so we can change the group of a turret several times even when the game is paused.
            base.Draw();
            spotlightMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect, this.spotLightRotation.ToQuat(), spotlightScale);
            if (this.powerComp.PowerOn)
            {
                Graphics.DrawMesh(MeshPool.plane10, spotlightMatrix, spotlightOnTexture, 0);
                spotlightLightEffectMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect, this.spotLightRotation.ToQuat(), spotlightLightEffectScale);
                Graphics.DrawMesh(MeshPool.plane10, spotlightLightEffectMatrix, spotlightLightEffectTexture, 0);
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, spotlightMatrix, spotlightOffTexture, 0);
            }

            if (Find.Selector.IsSelected(this)
                && (this.target != null))
            {
                Vector3 lineOrigin = this.TrueCenter();
                Vector3 lineTarget = this.target.Position.ToVector3Shifted();
                lineTarget.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
                lineOrigin.y = lineTarget.y;
                GenDraw.DrawLineBetween(lineOrigin, lineTarget, targetLineTexture);
            }
        }
    }
}
