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
    /// SupplyShipTakingOff class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class SupplyShipTakingOff : Thing
    {
        private const int horizontalTrajectoryDurationInTicks = 480;
        private const int rotationDurationInTicks = 240;
        private const int verticalTrajectoryDurationInTicks = 240;
        private int ticksSinceTakeOff = 0;
        private IntVec3 landingPadPosition = Find.Map.Center;
        private Rot4 landingPadRotation = Rot4.North;
        private float relativeRotation = 0f; // Relative rotation of landing pad (supply ship always comes from the west side).

        // Texture.
        private Matrix4x4 supplyShipMatrix = default(Matrix4x4);
        private static Material supplyShipTexture = MaterialPool.MatFrom("Things/SupplyShip/SupplyShip");

        // Sound.
        private static readonly SoundDef takingOffSound = SoundDef.Named("SupplyShipTakingOff");
        
        private float supplyShipRotation
        {
            get
            {
                if (this.ticksSinceTakeOff < verticalTrajectoryDurationInTicks)
                {
                    return Rot4.East.AsAngle + this.relativeRotation;
                }
                else if (this.ticksSinceTakeOff < verticalTrajectoryDurationInTicks + rotationDurationInTicks)
                {
                    return Rot4.East.AsAngle + this.relativeRotation * ((1f - (float)(this.ticksSinceTakeOff - verticalTrajectoryDurationInTicks) / (float)rotationDurationInTicks));
                }
                else
                {
                    return Rot4.East.AsAngle;
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

                if (this.ticksSinceTakeOff < verticalTrajectoryDurationInTicks)
                {
                    result.z += 3f * ((float)this.ticksSinceTakeOff / verticalTrajectoryDurationInTicks);
                }
                else if (this.ticksSinceTakeOff < verticalTrajectoryDurationInTicks + rotationDurationInTicks)
                {
                    result.z += 3f;
                }
                else
                {
                    float coefficient = (float)(this.ticksSinceTakeOff - (verticalTrajectoryDurationInTicks + rotationDurationInTicks));
                    float num = coefficient * coefficient * 0.001f;
                    result.x += num * 0.8f;
                    result.z += 3f;
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
                if (this.ticksSinceTakeOff < verticalTrajectoryDurationInTicks)
                {
                    coefficient = 1f + 0.2f * ((float)this.ticksSinceTakeOff / verticalTrajectoryDurationInTicks);
                }
                else
                {
                    coefficient = 1.2f;
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
            Scribe_Values.LookValue<int>(ref this.ticksSinceTakeOff, "ticksSinceTakeOff");
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
            
            this.ticksSinceTakeOff++;
            if ((this.ticksSinceTakeOff == verticalTrajectoryDurationInTicks)
                && this.landingPadRotation == Rot4.East)
            {
                this.ticksSinceTakeOff += rotationDurationInTicks;
            }
            if (this.ticksSinceTakeOff >= verticalTrajectoryDurationInTicks + rotationDurationInTicks + horizontalTrajectoryDurationInTicks)
            {
                this.Destroy();
            }
            if (this.ticksSinceTakeOff == 1)
            {
                SupplyShipTakingOff.takingOffSound.PlayOneShot(base.Position);
            }
        }

        public override void Draw()
        {
            supplyShipMatrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, supplyShipRotation.ToQuat(), supplyShipScale);
            Graphics.DrawMesh(MeshPool.plane10, supplyShipMatrix, supplyShipTexture, 0);
        }
    }
}
