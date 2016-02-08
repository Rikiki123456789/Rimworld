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
    /// SamMissile class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Projectile_SamMissile : Thing
    {
        private DropPodIncoming target = null;
        private Vector3 origin;
        private Vector3 predictedImpactPosition;
        private int predictedTicksToImpact = 0;
        private int ticksToImpact = 0;
        private float missileRotation = 0f;
        
        public static Material texture = null;

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
            Projectile_SamMissile.texture = MaterialPool.MatFrom(this.def.graphicData.texPath);
        }

        public void InitializeMissileData(DropPodIncoming dropPod, Vector3 missileOrigin, Vector3 predictedImpactPosition, int predictedTicksToImpact)
        {
            this.target = dropPod;
            this.origin = missileOrigin;
            this.predictedImpactPosition = predictedImpactPosition;
            this.predictedTicksToImpact = predictedTicksToImpact;
            this.ticksToImpact = predictedTicksToImpact;
            this.missileRotation = (this.predictedImpactPosition - this.origin).AngleFlat();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference<DropPodIncoming>(ref this.target, "target");
            Scribe_Values.LookValue<Vector3>(ref this.origin, "origin");
            Scribe_Values.LookValue<Vector3>(ref this.origin, "predictedImpactPosition");
            Scribe_Values.LookValue<int>(ref this.predictedTicksToImpact, "predictedTicksToImpact");
            Scribe_Values.LookValue<int>(ref this.ticksToImpact, "ticksToImpact");
            Scribe_Values.LookValue<float>(ref this.missileRotation, "missileRotation");
        }

        public override void Tick()
        {
            this.Position = this.DrawPos.ToIntVec3();
            this.ticksToImpact--;
            if (this.ticksToImpact == 0)
            {
                this.Destroy(DestroyMode.Vanish);
                // TODO: destroy drop pod or not?
            }
            MoteThrower.ThrowSmoke(this.DrawPos, 1f);
        }

        public override void Draw()
        {
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, this.missileRotation.ToQuat(), new Vector3(1.5f, 0, 1.5f)); // new Vector3(3f, 0, 3f)
            Graphics.DrawMesh(MeshPool.plane10, matrix, texture, 0);
        }
    }
}
