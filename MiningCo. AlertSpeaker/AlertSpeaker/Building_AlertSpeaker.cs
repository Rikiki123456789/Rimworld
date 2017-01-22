using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace AlertSpeaker
{
    /// <summary>
    /// AlertSpeaker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    [StaticConstructorOnStartup]
    class Building_AlertSpeaker : Building
    {
        // Constants.
        public const float alertSpeakerMaxRange = 6.9f;
        public const int earlyPhaseTicksThreshold = GenDate.TicksPerDay / 4; // Maximum duration the adrenaline boost can last: 1/4 day.
        public const int latePhaseTicksThreshold = 2 * GenDate.TicksPerDay; // A very prolonged alert may generate stress: 2 days.

        // Danger phase.
        public static int lastUpdateTick = 0;
        public static int alertStartTick = 0;
        public static StoryDanger previousDangerRate = StoryDanger.None;
        public static StoryDanger currentDangerRate = StoryDanger.None;

        // Drawing parameters.
        public CompGlower glowerComp = null;
        public static float glowRadius = 0f;
        public static ColorInt glowColor = new ColorInt(0, 0, 0, 255);
        public static float redAlertLightAngle = 0f;
        public static float redAlertLightIntensity = 0.25f;
        public static Material redAlertLight = MaterialPool.MatFrom("Effects/RedAlertLight", ShaderDatabase.Transparent);
        public static Matrix4x4 redAlertLightMatrix = default(Matrix4x4);
        public static Vector3 redAlertLightScale = new Vector3(5f, 1f, 5f);

        // Sound parameters.
        public static bool soundIsActivated = true;
        public static int nextAlarmSoundTick = 0;
        public const int alarmSoundPeriod = 20 * GenTicks.TicksPerRealSecond; // 20 s.
        public static SoundDef lowDangerAlarmSound = SoundDef.Named("LowDangerAlarm");
        public static SoundDef highDangerAlarmSound = SoundDef.Named("HighDangerAlarm");

        // Other variables.
        public CompPowerTrader powerComp = null;
        
        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        /// 
        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            
            glowerComp = this.GetComp<CompGlower>();
            powerComp = this.GetComp<CompPowerTrader>();
        }

        /// <summary>
        /// Save and load variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref lastUpdateTick, "lastUpdateTick", 0);
            Scribe_Values.LookValue<int>(ref alertStartTick, "alertStartTick", 0);
            Scribe_Values.LookValue<StoryDanger>(ref previousDangerRate, "previousDangerRate", StoryDanger.None);
            Scribe_Values.LookValue<StoryDanger>(ref currentDangerRate, "currentDangerRate", StoryDanger.None);
            Scribe_Values.LookValue<int>(ref nextAlarmSoundTick, "nextAlarmSoundTick", 0);
        }
        
        /// <summary>
        /// Checks if the wall supporting the alert speaker is still alive.
        /// </summary>
        public static bool CheckIfSupportingWallIsAlive(Map map, IntVec3 position, Rot4 rotation)
        {
            IntVec3 wallPosition = position + new IntVec3(0, 0, -1).RotatedBy(rotation);
            
            // Built wall.
            if (map.thingGrid.ThingAt(wallPosition, ThingDefOf.Wall) != null)
            {
                return true;
            }

            // Natural block.
            Thing potentialWall = map.thingGrid.ThingAt(wallPosition, ThingCategory.Building);
            if (potentialWall != null)
            {
                if ((potentialWall as Building).def.building.isNaturalRock)
                {
                    return true;
                }
            }
            // No wall.
            return false;
        }
        
        /// <summary>
        /// Get the effect zone cells.
        /// </summary>
        public static List<IntVec3> GetEffectZoneCells(Map map, IntVec3 position)
        {
            IEnumerable<IntVec3> cellsInRange = GenRadial.RadialCellsAround(position, Building_AlertSpeaker.alertSpeakerMaxRange, true);
            List<IntVec3> effectZoneCells = new List<IntVec3>();
            foreach (IntVec3 cell in cellsInRange)
            {
                if (cell.GetRoom(map) == position.GetRoom(map))
                {
                    effectZoneCells.Add(cell);
                }
            }
            return effectZoneCells;
        }

        // ===================== Main work function =====================
        /// <summary>
        /// - Check if the supporting wall is still alive.
        /// - Check current threat level.
        /// - Perform adequate treatment when a danger level transition occurs.
        /// - Apply an adrenaline bonus to nearby colonists according to current danger rate.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (this.Map == null)
            {
                // This case can occur when the alert speaker has just been uninstalled.
                return;
            }
            if (CheckIfSupportingWallIsAlive(this.Map, this.Position, this.Rotation) == false)
            {
                this.Destroy(DestroyMode.Deconstruct);
                return;
            }

            int tickCounter = Find.TickManager.TicksGame;
            if (tickCounter > lastUpdateTick)
            {
                // The following treatment is performed only once per tick for all speakers.
                lastUpdateTick = tickCounter;
                if ((tickCounter % GenTicks.TicksPerRealSecond) == 0)
                {
                    previousDangerRate = currentDangerRate;
                    currentDangerRate = this.Map.dangerWatcher.DangerRating;
                    if (currentDangerRate != previousDangerRate)
                    {
                        PerformTreatmentOnDangerRateTransition();
                    }
                }
                PerformSoundTreatment();
                ComputeDrawingParameters();
            }

            // Update for each speaker.
            if ((tickCounter % GenTicks.TicksPerRealSecond) == 0)
            {
                if (powerComp.PowerOn)
                {
                    PerformTreatmentDuringAlert();
                }
            }
            PerformDrawingTreatment();
        }

        /// <summary>
        /// Check if a danger rate transition is occuring and perform the corresponding treatment.
        /// Transitions management:
        /// Transition 1: beginning alert.
        ///   => set the alert start tick.
        /// Transition 2: finished alert.
        ///   => remove any stat bonus/malus and malus thought.
        ///   => add a bonus thought.
        /// Transition 3: increased danger rating.
        ///   => convert the small adrenaline boost into medium adrenaline boost.
        /// Transition 4: decreased danger rating.
        ///   => keep the medium adrenaline boost (nothing to do).
        /// </summary>
        public void PerformTreatmentOnDangerRateTransition()
        {
            // Transition 1: beginning alert.
            if ((previousDangerRate == StoryDanger.None)
                && ((currentDangerRate == StoryDanger.Low)
                || (currentDangerRate == StoryDanger.High)))
            {
                // Set the alert start tick.
                alertStartTick = Find.TickManager.TicksGame;
                nextAlarmSoundTick = 0;
            }
            // Transition 2: finished alert.
            else if (((previousDangerRate == StoryDanger.Low)
                || (previousDangerRate == StoryDanger.High))
                && (currentDangerRate == StoryDanger.None))
            {
                RemoveAnyAdrenalineHediffFromAllColonists();
                nextAlarmSoundTick = 0;
            }
            // Transition 3: increased danger rating.
            else if ((previousDangerRate == StoryDanger.Low)
                && (currentDangerRate == StoryDanger.High))
            {
                nextAlarmSoundTick = 0;

                // Convert the small adrenaline hediff into medium adrenaline hediff.
                foreach (Pawn colonist in this.Map.mapPawns.FreeColonists)
                {
                    if (HasHediffAdrenalineSmall(colonist))
                    {
                        RemoveHediffAdrenalineSmall(colonist);
                        ApplyHediffAdrenalineMedium(colonist);
                    }
                }
            }
            // Transition 4: decreased danger rating.
            else if ((previousDangerRate == StoryDanger.High)
                && (currentDangerRate == StoryDanger.Low))
            {
                nextAlarmSoundTick = 0;
            }
        }
        
        /// <summary>
        /// Performs the treatments during an alert according to the current danger rate.
        ///   o danger rate == None => no bonus.
        ///   o danger rate == Low  => small adrenaline.
        ///   o danger rate == High => medium adrenaline.
        ///   
        /// No bonus is applied when alert is lasting for too long.
        /// </summary>
        public void PerformTreatmentDuringAlert()
        {
            if (currentDangerRate == StoryDanger.None)
            {
                return;
            }

            if (Find.TickManager.TicksGame <= alertStartTick + earlyPhaseTicksThreshold)
            {
                foreach (Pawn colonist in this.Map.mapPawns.FreeColonists)
                {
                    if ((colonist.Downed == false)
                        && (colonist.GetRoom() == this.GetRoom())
                        && colonist.Position.InHorDistOf(this.Position, alertSpeakerMaxRange))
                    {
                        if (currentDangerRate == StoryDanger.Low)
                        {
                            if (HasHediffAdrenalineMedium(colonist) == false)
                            {
                                ApplyHediffAdrenalineSmall(colonist);
                            }
                        }
                        else
                        {
                            RemoveHediffAdrenalineSmall(colonist);
                            ApplyHediffAdrenalineMedium(colonist);
                        }
                    }
                }
            }
        }

        // ===================== Hediff functions =====================

        public static bool HasHediffAdrenalineSmall(Pawn colonist)
        {
            Hediff adrenalineHediff = colonist.health.hediffSet.GetFirstHediffOfDef(Util_AlertSpeaker.HediffAdrenalineSmallDef);
            if (adrenalineHediff != null)
            {
                return true;
            }
            return false;
        }

        public static bool HasHediffAdrenalineMedium(Pawn colonist)
        {
            Hediff adrenalineHediff = colonist.health.hediffSet.GetFirstHediffOfDef(Util_AlertSpeaker.HediffAdrenalineMediumDef);
            if (adrenalineHediff != null)
            {
                return true;
            }
            return false;
        }

        public static void ApplyHediffAdrenalineSmall(Pawn colonist)
        {
            if (HasHediffAdrenalineSmall(colonist) == false)
            {
                MoteBubble mote = (MoteBubble)ThingMaker.MakeThing(ThingDefOf.Mote_ThoughtGood, null);
                mote.SetupMoteBubble(ContentFinder<Texture2D>.Get("Things/Mote/IncapIcon"), null);
                mote.Attach(colonist);
                GenSpawn.Spawn(mote, colonist.Position, colonist.Map);
            }
            colonist.health.AddHediff(Util_AlertSpeaker.HediffAdrenalineSmallDef);
        }

        public static void ApplyHediffAdrenalineMedium(Pawn colonist)
        {
            if (HasHediffAdrenalineMedium(colonist) == false)
            {
                MoteBubble mote = (MoteBubble)ThingMaker.MakeThing(ThingDefOf.Mote_ThoughtBad, null);
                mote.SetupMoteBubble(ContentFinder<Texture2D>.Get("Things/Mote/IncapIcon"), null);
                mote.Attach(colonist);
                GenSpawn.Spawn(mote, colonist.Position, colonist.Map);
            }
            colonist.health.AddHediff(Util_AlertSpeaker.HediffAdrenalineMediumDef);
        }

        public static void RemoveHediffAdrenalineSmall(Pawn colonist)
        {
            Hediff adrenalineHediff = colonist.health.hediffSet.GetFirstHediffOfDef(Util_AlertSpeaker.HediffAdrenalineSmallDef);
            if (adrenalineHediff != null)
            {
                colonist.health.RemoveHediff(adrenalineHediff);
            }
        }
        
        public void RemoveAnyAdrenalineHediffFromAllColonists()
        {
            IEnumerable<Pawn> colonistList = this.Map.mapPawns.FreeColonists;
            foreach (Pawn colonist in colonistList)
            {
                Hediff adrenalineHediff = colonist.health.hediffSet.GetFirstHediffOfDef(Util_AlertSpeaker.HediffAdrenalineSmallDef);
                if (adrenalineHediff != null)
                {
                    colonist.health.RemoveHediff(adrenalineHediff);
                }
                adrenalineHediff = colonist.health.hediffSet.GetFirstHediffOfDef(Util_AlertSpeaker.HediffAdrenalineMediumDef);
                if (adrenalineHediff != null)
                {
                    colonist.health.RemoveHediff(adrenalineHediff);
                }
            }
        }
        
        // ===================== Sound functions =====================

        /// <summary>
        /// Performs the sound treatment.
        ///   o danger rate == None => no sound.
        ///   o danger rate == Low  => plays low danger alarm sound
        ///   o danger rate == High => plays high danger alarm sound
        /// </summary>
        public void PerformSoundTreatment()
        {
            if (currentDangerRate == StoryDanger.None)
            {
                return;
            }
            if (Find.TickManager.TicksGame >= nextAlarmSoundTick)
            {
                nextAlarmSoundTick = Find.TickManager.TicksGame + alarmSoundPeriod * (int)Find.TickManager.CurTimeSpeed;
                if (currentDangerRate == StoryDanger.Low)
                {
                    PlayOneLowDangerAlarmSound();
                }
                else
                {
                    PlayOneHighDangerAlarmSound();
                }
            }
        }

        /// <summary>
        /// Play one low danger alarm sound.
        /// </summary>
        public void PlayOneLowDangerAlarmSound()
        {
            if (soundIsActivated)
            {
                lowDangerAlarmSound.PlayOneShotOnCamera();
            }
        }

        /// <summary>
        /// Play one high danger alarm sound.
        /// </summary>
        public void PlayOneHighDangerAlarmSound()
        {
            if (soundIsActivated)
            {
                highDangerAlarmSound.PlayOneShotOnCamera();
            }
        }

        // ===================== Drawing functions =====================

        /// <summary>
        /// Compute the global drawing parameters.
        ///   o danger rate == None => small green light.
        ///   o danger rate == Low  => medium yellow light ramping up and down.
        ///   o danger rate == High => big flashing red light.
        /// </summary>
        public void ComputeDrawingParameters()
        {
            const float rotationPeriod = 2f * GenTicks.TicksPerRealSecond;
            const float rotationAngleStep = 360f / rotationPeriod;
            const float lowDangerGlowRadiusOffset = 4f;

            switch (currentDangerRate)
            {
                case StoryDanger.None:
                    glowRadius = 0;
                    glowColor.r = 0;
                    glowColor.g = 220;
                    glowColor.b = 0;
                    break;
                case StoryDanger.Low:
                    glowRadius = lowDangerGlowRadiusOffset;
                    glowColor.r = 242;
                    glowColor.g = 185;
                    glowColor.b = 0;
                    break;
                case StoryDanger.High:
                    glowRadius = 0;
                    glowColor.r = 220;
                    glowColor.g = 0;
                    glowColor.b = 0;

                    // TickRateMultiplier should always be > 0 when unpaused.
                    redAlertLightAngle = (redAlertLightAngle + (rotationAngleStep / Find.TickManager.TickRateMultiplier)) % 360f;
                    if (redAlertLightAngle < 90f)
                    {
                        redAlertLightIntensity = (redAlertLightAngle / 90f);
                    }
                    else if (redAlertLightAngle < 180f)
                    {
                        redAlertLightIntensity = 1f - ((redAlertLightAngle - 90f) / 90f);
                    }
                    else
                    {
                        redAlertLightIntensity = 0;
                    }
                    break;
            }
        }

        /// <summary>
        /// Performs the drawing treatment. Applies the drawing parameters.
        /// </summary>
        public void PerformDrawingTreatment()
        {
            if ((this.glowerComp.Props.glowRadius != glowRadius)
                || (this.glowerComp.Props.glowColor != glowColor))
            {
                this.glowerComp.Props.glowRadius = glowRadius;
                this.glowerComp.Props.glowColor = glowColor;
                this.Map.mapDrawer.MapMeshDirty(this.Position, MapMeshFlag.Things);
                this.Map.glowGrid.MarkGlowGridDirty(this.Position);
            }
        }

        /// <summary>
        /// Performs the drawing treatment. Applies the drawing parameters.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            if ((currentDangerRate == StoryDanger.High)
                && (this.powerComp.PowerOn))
            {
                redAlertLightMatrix.SetTRS(this.Position.ToVector3Shifted() + new Vector3(0f, 10f, -0.25f).RotatedBy(this.Rotation.AsAngle) + Altitudes.AltIncVect, redAlertLightAngle.ToQuat(), redAlertLightScale);
                Graphics.DrawMesh(MeshPool.plane10, redAlertLightMatrix, FadedMaterialPool.FadedVersionOf(redAlertLight, redAlertLightIntensity), 0);
            }

            if (Find.Selector.IsSelected(this))
            {
                List<IntVec3> cellsInEffectZone = Building_AlertSpeaker.GetEffectZoneCells(this.Map, this.Position);
                GenDraw.DrawFieldEdges(cellsInEffectZone);
            }
        }

        ///<summary>
        ///This creates the command buttons to control the alert speaker sound activation.
        ///</summary>
        ///<returns>The list of command buttons to display.</returns>
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000100;

            Command_Action soundActivationButton = new Command_Action();
            if (soundIsActivated)
            {
                soundActivationButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_SirenSoundEnabled");
                soundActivationButton.defaultDesc = "Deactivate siren sound.";
                soundActivationButton.defaultLabel = "Siren sound activated.";
            }
            else
            {
                soundActivationButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_SirenSoundDisabled");
                soundActivationButton.defaultDesc = "Activate siren sound.";
                soundActivationButton.defaultLabel = "Siren sound deactivated.";
            }
            soundActivationButton.activateSound = SoundDef.Named("Click");
            soundActivationButton.action = new Action(PerformSirenSoundAction);
            soundActivationButton.groupKey = groupKeyBase + 1;
            buttonList.Add(soundActivationButton);

            IEnumerable<Gizmo> resultButtonList;
            IEnumerable<Gizmo> basebuttonList = base.GetGizmos();
            if (basebuttonList != null)
            {
                resultButtonList = buttonList.AsEnumerable<Gizmo>().Concat(basebuttonList);
            }
            else
            {
                resultButtonList = buttonList.AsEnumerable<Gizmo>();
            }
            return (resultButtonList);
        }

        /// <summary>
        /// Activates/deactivates siren sound.
        /// </summary>
        public void PerformSirenSoundAction()
        {
            soundIsActivated = !soundIsActivated;
        }

    }
}
