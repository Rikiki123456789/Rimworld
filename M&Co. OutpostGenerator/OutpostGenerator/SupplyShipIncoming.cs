using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// SupplyShipIncoming class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class SupplyShipIncoming : Thing
    {
        public const int diagonalTrajectoryDurationInTicks = 240;
        public const int verticalTrajectoryDurationInTicks = 240;
        public int ticksToLanding = diagonalTrajectoryDurationInTicks + verticalTrajectoryDurationInTicks;
        public Rot4 landingPadRotation = Rot4.North;

        // Thrust effect.
        public static readonly Material shadowTexture = MaterialPool.MatFrom("Things/Special/DropPodShadow", ShaderDatabase.Transparent);

        // Sound.
        public static readonly SoundDef preLandingSound = SoundDef.Named("DropPodFall");
        public static readonly SoundDef landingSound = SoundDef.Named("SupplyShipLanding");
        public const int soundAnticipationTicks = 60;

        public override Vector3 DrawPos
        {
            get
            {
                Vector3 result = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem);
                if (this.ticksToLanding > verticalTrajectoryDurationInTicks)
                {
                    float coefficient = (float)(this.ticksToLanding - verticalTrajectoryDurationInTicks);
                    float num = coefficient * coefficient * 0.001f;
                    result.x -= num * 0.4f;
                    result.z += 3f;
                }
                else
                {
                    result.z += ((float)this.ticksToLanding / verticalTrajectoryDurationInTicks) * 3f;
                }
                return result;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ticksToLanding, "ticksToLanding");
            Scribe_Values.LookValue<Rot4>(ref this.landingPadRotation, "landingPadRotation");
        }

        public override void Tick()
        {
            this.ticksToLanding--;
            if (this.ticksToLanding <= 0)
            {
                /*Thing mechanoidTerraformer = ThingMaker.MakeThing(Util_MechanoidTerraformer.MechanoidTerraformerDef);
                mechanoidTerraformer.SetFactionDirect(Faction.OfMechanoids);
                GenSpawn.Spawn(mechanoidTerraformer, this.Position);*/
                this.Destroy();
            }
            else if (this.ticksToLanding <= verticalTrajectoryDurationInTicks)
            {
                for (int dustMoteIndex = 0; dustMoteIndex < 2; dustMoteIndex++)
                {
                    Vector3 dustMotePosition = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead) + Gen.RandomHorizontalVector(4f);
                    MoteThrower.ThrowDustPuff(dustMotePosition, 1.2f);
                }
            }
            if (this.ticksToLanding == soundAnticipationTicks + verticalTrajectoryDurationInTicks)
            {
                SupplyShipIncoming.preLandingSound.PlayOneShot(base.Position);
            }
            if (this.ticksToLanding == verticalTrajectoryDurationInTicks)
            {
                SupplyShipIncoming.landingSound.PlayOneShot(base.Position);
            }
        }

        public override void DrawAt(Vector3 drawLoc)
        {
            base.DrawAt(drawLoc);
            float num = 5f + (float)this.ticksToLanding / 100f;
            Vector3 scale = new Vector3(num, 1f, num);
            Matrix4x4 matrix = default(Matrix4x4);
            drawLoc.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
            matrix.SetTRS(this.TrueCenter(), base.Rotation.AsQuat, scale);
            Graphics.DrawMesh(MeshPool.plane10Back, matrix, SupplyShipIncoming.shadowTexture, 0);
        }
    }
}
