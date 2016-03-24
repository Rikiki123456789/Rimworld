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
        private const int horizontalTrajectoryDurationInTicks = 480;
        private const int rotationDurationInTicks = 240;
        private const int verticalTrajectoryDurationInTicks = 240;
        private int ticksToLanding = horizontalTrajectoryDurationInTicks + rotationDurationInTicks + verticalTrajectoryDurationInTicks;
        public Rot4 landingPadRotation = Rot4.North;
        private float relativeRotation = 0f;
        
        // Texture.
        private Matrix4x4 supplyShipMatrix = default(Matrix4x4);
        private static Material supplyShipTexture = MaterialPool.MatFrom("Things/SupplyShip/SupplyShip");
        private Vector3 supplyShipScale = new Vector3(20f, 1f, 11f);

        // Sound.
        private static readonly SoundDef preLandingSound = SoundDef.Named("DropPodFall");
        private static readonly SoundDef landingSound = SoundDef.Named("SupplyShipLanding");
        private const int soundAnticipationTicks = 60;
        
        private float supplyShipRotation
        {
            get
            {
                if (this.ticksToLanding > rotationDurationInTicks + verticalTrajectoryDurationInTicks)
                {
                    return 0f;
                }
                else if (this.ticksToLanding > verticalTrajectoryDurationInTicks)
                {
                    return this.relativeRotation * ((1f - (float)(this.ticksToLanding - verticalTrajectoryDurationInTicks) / (float)rotationDurationInTicks));
                }
                else
                {
                    return this.relativeRotation;
                }
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                Vector3 result = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem);
                if (this.ticksToLanding > rotationDurationInTicks + verticalTrajectoryDurationInTicks)
                {
                    float coefficient = (float)(this.ticksToLanding - (rotationDurationInTicks + verticalTrajectoryDurationInTicks));
                    float num = coefficient * coefficient * 0.001f;
                    result.x -= num * 0.8f;
                    result.z += 3f;
                }
                else if (this.ticksToLanding > verticalTrajectoryDurationInTicks)
                {
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
            if (this.landingPadRotation == Rot4.North)
            {
                relativeRotation = -90f;
            }
            else if (this.landingPadRotation == Rot4.East)
            {
                relativeRotation = 0f;
            }
            else if (this.landingPadRotation == Rot4.South)
            {
                relativeRotation = 90f;
            }
            else if (this.landingPadRotation == Rot4.West)
            {
                relativeRotation = 180f;
            }
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
            else if (this.ticksToLanding <= rotationDurationInTicks + verticalTrajectoryDurationInTicks)
            {
                for (int dustMoteIndex = 0; dustMoteIndex < 2; dustMoteIndex++)
                {
                    Vector3 dustMotePosition = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead) + Gen.RandomHorizontalVector(4f);
                    MoteThrower.ThrowDustPuff(dustMotePosition, 1.2f);
                }
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

        public override void Draw()
        {
            // TODO: draw over fog!
            supplyShipMatrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, supplyShipRotation.ToQuat(), supplyShipScale);
            Graphics.DrawMesh(MeshPool.plane10, supplyShipMatrix, supplyShipTexture, 0);
            // TODO: dust only at the end!
        }
    }
}
