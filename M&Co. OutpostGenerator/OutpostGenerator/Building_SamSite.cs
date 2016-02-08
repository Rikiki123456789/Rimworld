using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using RimWorld.SquadAI;
//using Verse.Sound; // Needed when you do something with the Sound

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
        private const float turnRate = 0.12f;
        private const int patrolRotationIntervalMin = 150;
        private const int patrolRotationIntervalMax = 350;
        private const int patrolRotationDurationMin = 240;
        private const int patrolRotationDurationMax = 360;
        private const int delayBetweenEachMissile = 20;

        private float turretRotation = 0f;
        private int ticksUntilPatrolRotation = patrolRotationIntervalMin;
        private bool patrolRotationClockwise = true;
        private int ticksUntilPatrolRotationEnd = 0;
        private int ticksAfterMissileLaunch = 0;

        private int ticksSinceLastMissileLaunch = 0;
        private List<Thing> acquiredTargets = new List<Thing>(5);

        private CompPowerTrader powerComp = null;

        private static Vector3 turretScale = new Vector3(2f, 0, 2f);

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
            }
            // TODO: clear list!
        }

        private void LookForIncomingDropPod()
        {
            if (this.ticksSinceLastMissileLaunch > 0)
            {
                this.ticksSinceLastMissileLaunch--;
                return;
            }
            //if (this.acquiredTargets.Count < this.acquiredTargets.Capacity)
            {
                List<Thing> incomingDropPods = Find.ListerThings.ThingsOfDef(ThingDefOf.DropPodIncoming);
                foreach (Thing dropPod in incomingDropPods)
                {
                    // TODO: check content is enemy.
                    if (this.acquiredTargets.Contains(dropPod) == false)
                    {
                        LaunchMissile(dropPod as DropPodIncoming);
                        this.ticksSinceLastMissileLaunch = delayBetweenEachMissile;
                    }
                }
            }
        }

        private void LaunchMissile(DropPodIncoming dropPod)
        {
            const int ticksToImpactOffset = 40;

            //this.acquiredTargets.Add(dropPod);
            int dropPodTicksToLanding = (int)(typeof(DropPodIncoming).GetField("ticksToImpact", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(dropPod));
            int predictedTicksToImpact = dropPodTicksToLanding - ticksToImpactOffset;
            float landingDistanceCoefficient = (float)(ticksToImpactOffset * ticksToImpactOffset) * 0.01f;
            Vector3 predictedImpactPosition = dropPod.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem) + new Vector3(-landingDistanceCoefficient * 0.4f, 0, +landingDistanceCoefficient * 0.6f);
            Projectile_SamMissile samMissile = ThingMaker.MakeThing(ThingDef.Named("SamMissile")) as Projectile_SamMissile;
            samMissile.InitializeMissileData(dropPod, this.Position.ToVector3Shifted(), predictedImpactPosition, predictedTicksToImpact);
            GenSpawn.Spawn(samMissile, this.Position);

            // Update turret rotation.
            this.ticksAfterMissileLaunch = patrolRotationIntervalMin;
            this.turretRotation = (predictedImpactPosition - this.DrawPos).AngleFlat();
            
        }

        private void UpdateTurretRotation()
        {
            if (ticksAfterMissileLaunch > 0)
            {
                ticksAfterMissileLaunch--;
            }
            else if (this.ticksUntilPatrolRotation > 0)
            {
                this.ticksUntilPatrolRotation--;
                if (this.ticksUntilPatrolRotation == 0)
                {
                    this.ticksUntilPatrolRotationEnd = Rand.RangeInclusive(patrolRotationDurationMin, patrolRotationDurationMax);
                    if (Rand.Value < 0.5f)
                    {
                        this.patrolRotationClockwise = true;
                    }
                    else
                    {
                        this.patrolRotationClockwise = false;
                    }
                }
            }
            else
            {
                if (this.patrolRotationClockwise)
                {
                    this.turretRotation += turnRate;
                }
                else
                {
                    this.turretRotation -= turnRate;
                }
                this.ticksUntilPatrolRotationEnd--;
                if (this.ticksUntilPatrolRotationEnd == 0)
                {
                    this.ticksUntilPatrolRotation = Rand.RangeInclusive(patrolRotationIntervalMin, patrolRotationIntervalMax);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ticksUntilPatrolRotation, "ticksUntilPatrolRotation");
            Scribe_Values.LookValue<float>(ref this.turretRotation, "turretRotation");
            Scribe_Values.LookValue<bool>(ref this.patrolRotationClockwise, "patrolRotationClockwise");
            Scribe_Values.LookValue<int>(ref this.ticksUntilPatrolRotationEnd, "ticksUntilPatrolRotationEnd");
            Scribe_Values.LookValue<int>(ref this.ticksAfterMissileLaunch, "ticksAfterMissileLaunch");
            Scribe_Values.LookValue<int>(ref this.ticksSinceLastMissileLaunch, "ticksSinceLastMissileLaunch");
            Scribe_Collections.LookList<Thing>(ref this.acquiredTargets, "acquiredTargets", LookMode.Deep);
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
    }
}
