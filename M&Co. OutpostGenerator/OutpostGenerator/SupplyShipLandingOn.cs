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
    /// SupplyShipLandingOn class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class SupplyShipLandingOn : Thing
    {
        private const int horizontalTrajectoryDurationInTicks = 480;
        private const int rotationDurationInTicks = 240;
        private const int verticalTrajectoryDurationInTicks = 240;
        private int ticksToLanding = horizontalTrajectoryDurationInTicks + rotationDurationInTicks + verticalTrajectoryDurationInTicks;
        private IntVec3 landingPadPosition = Find.Map.Center;
        private Rot4 landingPadRotation = Rot4.North;
        private float relativeRotation = 0f; // Relative rotation of landing pad (supply ship always comes from the west side).
        
        // Texture.
        private Matrix4x4 supplyShipMatrix = default(Matrix4x4);
        private static Material supplyShipTexture = MaterialPool.MatFrom("Things/SupplyShip/SupplyShip");

        // Sound.
        private static readonly SoundDef preLandingSound = SoundDef.Named("DropPodFall");
        private static readonly SoundDef landingOnSound = SoundDef.Named("SupplyShipLandingOn");
        
        private float supplyShipRotation
        {
            get
            {
                if (this.ticksToLanding > rotationDurationInTicks + verticalTrajectoryDurationInTicks)
                {
                    return Rot4.East.AsAngle;
                }
                else if (this.ticksToLanding > verticalTrajectoryDurationInTicks)
                {
                    return Rot4.East.AsAngle + this.relativeRotation * ((1f - (float)(this.ticksToLanding - verticalTrajectoryDurationInTicks) / (float)rotationDurationInTicks));
                }
                else
                {
                    return Rot4.East.AsAngle + this.relativeRotation;
                }
            }
        }
        
        public override Vector3 DrawPos
        {
            get
            {
                Vector3 result = this.landingPadPosition.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem);
                // The 10f offset on Y axis is mandatory to be over the fog of war.
                result += new Vector3(0f, 10f, 0f);
                if (this.landingPadRotation == Rot4.North)
                {
                    result += new Vector3(0f, 0, 0.5f);
                }
                else if (this.landingPadRotation == Rot4.East)
                {
                    result += new Vector3(0.5f, 0, 0f);
                }
                else if (this.landingPadRotation == Rot4.South)
                {
                    result += new Vector3(0f, 0, -0.5f);
                }
                else if (this.landingPadRotation == Rot4.West)
                {
                    result += new Vector3(-0.5f, 0, 0f);
                }

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
                    result.z += 3f * ((float)this.ticksToLanding / verticalTrajectoryDurationInTicks);
                }
                return result;
            }
        }
        
        private Vector3 supplyShipScale
        {
            get
            {
                Vector3 result = new Vector3(11f, 1f, 20f);
                float coefficient = 1f;
                if (this.ticksToLanding > verticalTrajectoryDurationInTicks)
                {
                    coefficient = 1.2f;
                }
                else
                {
                    coefficient = 1f + 0.2f * ((float)this.ticksToLanding / verticalTrajectoryDurationInTicks);
                }
                result *= coefficient;
                return result;
            }
        }

        public void InitializeLandingData(IntVec3 position, Rot4 rotation)
        {
            this.landingPadPosition = position;
            this.landingPadRotation = rotation;
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
            Scribe_Values.LookValue<IntVec3>(ref this.landingPadPosition, "landingPadPosition");
            Scribe_Values.LookValue<Rot4>(ref this.landingPadRotation, "landingPadRotation");
        }

        public bool SupplyShipIsInBoundsAndVisible()
        {
            bool supplyShipIsInBounds = this.DrawPos.ToIntVec3().InBounds()
                && (Find.FogGrid.IsFogged(this.DrawPos.ToIntVec3()) == false)
                && this.DrawPos.ToIntVec3().x >= 10
                && this.DrawPos.ToIntVec3().z >= 10
                && this.DrawPos.ToIntVec3().x < Find.Map.Size.x - 10
                && this.DrawPos.ToIntVec3().z < Find.Map.Size.z - 10;
            return supplyShipIsInBounds;
        }

        public override void Tick()
        {
            // Set supply ship position so it is visible above fog of war.
            if (SupplyShipIsInBoundsAndVisible())
            {
                this.Position = this.DrawPos.ToIntVec3();
            }
            else
            {
                this.Position = this.landingPadPosition;
            }

            if (this.ticksToLanding == horizontalTrajectoryDurationInTicks + rotationDurationInTicks + verticalTrajectoryDurationInTicks)
            {
                // TODO: remove this sound?
                SupplyShipLandingOn.preLandingSound.PlayOneShot(base.Position);
            }
            this.ticksToLanding--;
            if ((this.ticksToLanding == rotationDurationInTicks + verticalTrajectoryDurationInTicks)
                && this.landingPadRotation == Rot4.East)
            {
                this.ticksToLanding -= rotationDurationInTicks;
            }
            if (this.ticksToLanding <= 0)
            {
                for (int dustMoteIndex = 0; dustMoteIndex < 40; dustMoteIndex++)
                {
                    Vector3 dustMotePosition = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead) + Gen.RandomHorizontalVector(6f);
                    MoteThrower.ThrowDustPuff(dustMotePosition, 1.6f);
                }
                Thing supplyShip = ThingMaker.MakeThing(OG_Util.SupplyShipDef);
                supplyShip.SetFactionDirect(OG_Util.FactionOfMAndCo);
                GenSpawn.Spawn(supplyShip, this.landingPadPosition, this.landingPadRotation);
                this.Destroy();
            }
            if (this.ticksToLanding == verticalTrajectoryDurationInTicks)
            {
                SupplyShipLandingOn.landingOnSound.PlayOneShot(base.Position);
            }
        }

        public override void Draw()
        {
            supplyShipMatrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, supplyShipRotation.ToQuat(), supplyShipScale);
            Graphics.DrawMesh(MeshPool.plane10, supplyShipMatrix, supplyShipTexture, 0);
        }
    }
}
