using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using RimWorld.SquadAI;
using Verse.Sound;   // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Projectile_Sam class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    [StaticConstructorOnStartup]
    public class Projectile_Sam : Thing
    {
        private Building_SamSite launcher = null;
        private DropPodIncoming target = null;
        private Vector3 origin;
        private Vector3 predictedImpactPosition;
        private int predictedTicksToImpact = 0;
        private int ticksToImpact = 0;
        private float missileRotation = 0f;
        private bool targetWillBeHit = true;
        
        public static Material texture = MaterialPool.MatFrom("Things/Projectile/Rocket_Big");

        public override Vector3 DrawPos
        {
            get
            {
                Vector3 b = (this.predictedImpactPosition - this.origin) * (1f - (float)this.ticksToImpact / (float)this.predictedTicksToImpact);
                return this.origin + b + Vector3.up * this.def.Altitude;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
        }

        public void InitializeMissileData(Building_SamSite launcher, DropPodIncoming dropPod, Vector3 missileOrigin, Vector3 predictedImpactPosition, int predictedTicksToImpact, bool targetWillBeHit = true)
        {
            this.launcher = launcher;
            this.target = dropPod;
            this.origin = missileOrigin;
            this.predictedImpactPosition = predictedImpactPosition;
            this.predictedTicksToImpact = predictedTicksToImpact;
            this.ticksToImpact = predictedTicksToImpact;
            this.missileRotation = (this.predictedImpactPosition - this.origin).AngleFlat();
            this.targetWillBeHit = targetWillBeHit;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference< Building_SamSite> (ref this.launcher, "launcher");
            Scribe_References.LookReference<DropPodIncoming>(ref this.target, "target");
            Scribe_Values.LookValue<Vector3>(ref this.origin, "origin");
            Scribe_Values.LookValue<Vector3>(ref this.origin, "predictedImpactPosition");
            Scribe_Values.LookValue<int>(ref this.predictedTicksToImpact, "predictedTicksToImpact");
            Scribe_Values.LookValue<int>(ref this.ticksToImpact, "ticksToImpact");
            Scribe_Values.LookValue<float>(ref this.missileRotation, "missileRotation");
            Scribe_Values.LookValue<bool>(ref this.targetWillBeHit, "targetWillBeHit");
        }

        public override void Tick()
        {
            if (this.target.Destroyed)
            {
                this.launcher.NotifyTargetIsDestroyedOrMissed(this.target);
                this.Destroy(DestroyMode.Kill);
                return;
            }

            if (this.DrawPos.ToIntVec3().InBounds())
            {
                // Mind the case where a pod is landing near the left map border so the missile will impact outside of the map.
                this.Position = this.DrawPos.ToIntVec3();
            }
            this.ticksToImpact--;
            if (this.ticksToImpact == 0)
            {
                this.Destroy(DestroyMode.Kill);
                DestroyTarget();
            }
        }

        private void DestroyTarget()
        {
            if (this.targetWillBeHit)
            {
                foreach (Thing thing in this.target.contents.containedThings)
                {
                    if (thing is Pawn)
                    {
                        // Spawn a dead copy of the pawn as the original one will be destroyed with the incoming drop pod.
                        Pawn pawn = thing as Pawn;
                        Pawn pawnCopy = PawnGenerator.GeneratePawn(pawn.kindDef, pawn.Faction);
                        GenPlace.TryPlaceThing(pawnCopy, this.target.Position, ThingPlaceMode.Near);
                        if (pawnCopy.equipment.Primary != null)
                        {
                            pawnCopy.equipment.Primary.HitPoints = (int)(Rand.Range(0.05f, 0.30f) * pawnCopy.equipment.Primary.MaxHitPoints);
                        }
                        HealthUtility.GiveInjuriesToKill(pawnCopy);
                        // TODO: add tale. Destroyed drop pod.
                        /*TaleRecorder.RecordTale(TaleDef.Named("LandedInPod"), new object[]
                        {
                            pawnCopy
                        });*/
                        break;
                    }
                }
                for (int slagIndex = 0; slagIndex < 3; slagIndex++)
                {
                    GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel), this.target.Position + new IntVec3(Rand.Range(-3, 3), 0, Rand.Range(-3, 3)), ThingPlaceMode.Near);
                }
                FireUtility.TryStartFireIn(this.target.Position + new IntVec3(Rand.Range(-3, 3), 0, Rand.Range(-3, 3)), 0.3f);
                this.target.Destroy(DestroyMode.Vanish);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);

            this.launcher.NotifyTargetIsDestroyedOrMissed(this.target);
            if (mode == DestroyMode.Kill)
            {
                MoteMaker.ThrowLightningGlow(this.DrawPos, 6f);
                for (int smokeIndex = 0; smokeIndex < 3; smokeIndex++)
                {
                    MoteMaker.ThrowSmoke(this.DrawPos + new Vector3(Rand.Range(0, 1), 0, Rand.Range(0, 1)), 3f);
                }
                SoundInfo infos = SoundInfo.InWorld(this);
                SoundDef.Named("Explosion_Bomb").PlayOneShot(infos);
            }
        }

        public override void Draw()
        {
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, this.missileRotation.ToQuat(), Vector3.one);
            Graphics.DrawMesh(MeshPool.plane10, matrix, texture, 0);
        }
    }
}
