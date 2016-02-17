using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using RimWorld.SquadAI;
using Verse.Sound;   // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Building_SamSite class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_SamSite : Building
    {
        // Patrol rotation (turret rotation when idle).
        private const float turnRate = 0.12f;
        private const int patrolRotationIntervalMin = 600;
        private const int patrolRotationIntervalMax = 900;
        private int patrolTicksUntilNextRotation = patrolRotationIntervalMin;
        private const int patrolRotationDurationMin = 240;
        private const int patrolRotationDurationMax = 360;
        private int patrolTicksUntilRotationEnd = 0;
        private bool patrolClockwiseRotation = true;

        // Missiles loading and launching.
        private const int missilesDelayBetweenLaunches = 20;
        private int missilesTicksUntilNextLaunch = 0;
        private const int missilesLoadedNumberMax = 5;
        private int missilesLoadedNumber = missilesLoadedNumberMax;
        private const int missilesReloadingDuration = 600;
        private int missilesReloadingProgress = 0;

        private List<DropPodIncoming> acquiredTargets = new List<DropPodIncoming>();

        private CompPowerTrader powerComp = null;

        // Sound.
        private Sustainer rotationSoundSustainer = null;

        // Texture.
        private float turretRotation = 0f;
        private static Vector3 turretScale = new Vector3(2.2f, 0, 2.2f);

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            powerComp = base.GetComp<CompPowerTrader>();
        }

        public override void Tick()
        {
            base.Tick();
            
            if (powerComp.PowerOn)
            {
                LookForIncomingDropPod();
                UpdateTurretRotation();
                UpdateMissileReloading();
            }
            else
            {
                StopRotationSound();
            }
        }

        /// <summary>
        /// Notify the SAM site that a target has been destroyed or missed.
        /// </summary>
        public void NotifyTargetIsDestroyedOrMissed(DropPodIncoming dropPod)
        {
            if (this.acquiredTargets.Contains(dropPod))
            {
                this.acquiredTargets.Remove(dropPod);
            }
        }

        private void LookForIncomingDropPod()
        {
            if (this.missilesTicksUntilNextLaunch > 0)
            {
                // Ensure a minimum delay between each missile launch.
                this.missilesTicksUntilNextLaunch--;
                return;
            }
            if (this.missilesLoadedNumber == 0)
            {
                return;
            }

            List<Thing> incomingDropPodsList = Find.ListerThings.ThingsOfDef(ThingDefOf.DropPodIncoming);
            foreach (Thing element in incomingDropPodsList)
            {
                DropPodIncoming dropPod = element as DropPodIncoming;
                int dropPodTicksToLanding = (int)(typeof(DropPodIncoming).GetField("ticksToImpact", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(dropPod));
                if (dropPodTicksToLanding < 80)
                {
                    continue;
                }
                if (dropPod.contents.containedThings.NullOrEmpty())
                {
                    continue;
                }
                foreach (Thing thing in dropPod.contents.containedThings)
                {
                    if (thing is Pawn)
                    {
                        Pawn pawn = thing as Pawn;
                        if ((pawn.Faction != null)
                            && pawn.Faction.HostileTo(this.Faction))
                        {
                            if (this.acquiredTargets.Contains(dropPod) == false)
                            {
                                this.acquiredTargets.Add(dropPod);
                                LaunchMissile(dropPod);
                                this.missilesTicksUntilNextLaunch = missilesDelayBetweenLaunches;
                                return;
                            }
                        }
                    }
                }
                
            }
        }

        private void LaunchMissile(DropPodIncoming dropPod)
        {
            const int ticksToImpactOffsetAverage = 40;

            bool targetWillBeHit = true;
            float landingDistanceCoefficient = 0f;
            Vector3 missPositionOffset;

            // Determine if drop pod will be hit.
            if (Rand.Value < 0.8f)
            {
                targetWillBeHit = true;
                missPositionOffset = new Vector3(0, 0, 0);
            }
            else
            {
                targetWillBeHit = false;
                missPositionOffset = new Vector3(5, 0, 5).RotatedBy(Rand.Range(0, 360));
            }

            // Compute predicted target impact position and time.
            int dropPodTicksToLanding = (int)(typeof(DropPodIncoming).GetField("ticksToImpact", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(dropPod));
            int ticksToImpactOffset = ticksToImpactOffsetAverage + Rand.RangeInclusive(-10, 10);
            int predictedTicksToImpact = dropPodTicksToLanding - ticksToImpactOffset;
            landingDistanceCoefficient = (float)(ticksToImpactOffset * ticksToImpactOffset) * 0.01f;
            Vector3 predictedImpactPosition = dropPod.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem) + new Vector3(-landingDistanceCoefficient * 0.4f, 0, +landingDistanceCoefficient * 0.6f) + missPositionOffset;
            Projectile_Sam sam = ThingMaker.MakeThing(ThingDef.Named("SamMissile")) as Projectile_Sam;
            sam.InitializeMissileData(this, dropPod, this.Position.ToVector3Shifted(), predictedImpactPosition, predictedTicksToImpact, targetWillBeHit);
            GenSpawn.Spawn(sam, this.Position);
            this.missilesLoadedNumber--;

            // Update turret rotation.
            this.patrolTicksUntilNextRotation = patrolRotationIntervalMin;
            this.turretRotation = (predictedImpactPosition - this.DrawPos).AngleFlat();
            StopRotationSound();

            // Throw smoke and play missile launch sound.
            MoteThrower.ThrowSmoke(this.DrawPos, 2f);
            SoundInfo infos = SoundInfo.InWorld(this);
            OG_Util.MissileLaunchSoundDef.PlayOneShot(infos);
        }

        private void UpdateTurretRotation()
        {
            if (this.patrolTicksUntilNextRotation > 0)
            {
                this.patrolTicksUntilNextRotation--;
                if (this.patrolTicksUntilNextRotation == 0)
                {
                    this.patrolTicksUntilRotationEnd = Rand.RangeInclusive(patrolRotationDurationMin, patrolRotationDurationMax);
                    if (Rand.Value < 0.5f)
                    {
                        this.patrolClockwiseRotation = true;
                    }
                    else
                    {
                        this.patrolClockwiseRotation = false;
                    }
                    StartRotationSound();
                }
            }
            else
            {
                if (this.patrolClockwiseRotation)
                {
                    this.turretRotation += turnRate;
                }
                else
                {
                    this.turretRotation -= turnRate;
                }
                this.patrolTicksUntilRotationEnd--;
                if (this.patrolTicksUntilRotationEnd == 0)
                {
                    this.patrolTicksUntilNextRotation = Rand.RangeInclusive(patrolRotationIntervalMin, patrolRotationIntervalMax);
                    StopRotationSound();
                }
            }
        }

        private void UpdateMissileReloading()
        {
            if (this.missilesLoadedNumber < missilesLoadedNumberMax)
            {
                this.missilesReloadingProgress++;
                if (this.missilesReloadingProgress >= missilesReloadingDuration)
                {
                    this.missilesLoadedNumber++;
                    this.missilesReloadingProgress = 0;
                }
            }
        }

        private void StartRotationSound()
        {
            StopRotationSound();
            SoundInfo info = SoundInfo.InWorld(this, MaintenanceType.None);
            this.rotationSoundSustainer = this.def.building.soundDispense.TrySpawnSustainer(info);
        }

        private void StopRotationSound()
        {
            if (this.rotationSoundSustainer != null)
            {
                this.rotationSoundSustainer.End();
                this.rotationSoundSustainer = null;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            StopRotationSound();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.patrolTicksUntilNextRotation, "ticksUntilPatrolRotation");
            Scribe_Values.LookValue<float>(ref this.turretRotation, "turretRotation");
            Scribe_Values.LookValue<bool>(ref this.patrolClockwiseRotation, "patrolRotationClockwise");
            Scribe_Values.LookValue<int>(ref this.patrolTicksUntilRotationEnd, "ticksUntilPatrolRotationEnd");
            Scribe_Values.LookValue<int>(ref this.missilesTicksUntilNextLaunch, "ticksSinceLastMissileLaunch");
            Scribe_Values.LookValue<int>(ref this.missilesLoadedNumber, "loadedMissiles");
            Scribe_Values.LookValue<int>(ref this.missilesReloadingProgress, "ticksUntilMissileReloading");
            Scribe_Collections.LookList<DropPodIncoming>(ref this.acquiredTargets, "acquiredTargets", LookMode.Deep);
    }

        public override void Draw()
        {
            base.Draw();
            DrawTurretTop();
        }

        private void DrawTurretTop()
        {
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, this.turretRotation.ToQuat(), turretScale);
            Graphics.DrawMesh(MeshPool.plane10, matrix, this.def.building.turretTopMat, 0);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(base.GetInspectString());
            stringBuilder.AppendLine("Loaded missiles: " + this.missilesLoadedNumber + "/" + missilesLoadedNumberMax);

            return stringBuilder.ToString();
        }
    }
}
