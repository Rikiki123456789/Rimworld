using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{
    [StaticConstructorOnStartup]
    public class FlyingSpaceshipTakingOff : FlyingSpaceship
    {
        public const int horizontalTrajectoryDurationInTicks = 480;
        public const int verticalTrajectoryDurationInTicks = 240;
        public int ticksSinceTakeOff = 0;
        public IntVec3 landingPadPosition = IntVec3.Invalid;
        public Rot4 landingPadRotation = Rot4.North;
        
        // Sound.
        public static readonly SoundDef takingOffSound = SoundDef.Named("SpaceshipTakingOff");

        // ===================== Setup work =====================
        public void InitializeTakingOffParameters(IntVec3 position, Rot4 rotation, SpaceshipKind spaceshipKind)
        {
            this.landingPadPosition = position;
            this.landingPadRotation = rotation;
            this.spaceshipExactRotation = this.landingPadRotation.AsAngle;
            this.spaceshipKind = spaceshipKind;
            ConfigureShipTexture(this.spaceshipKind);
            base.Tick(); // To update exact position for drawing purpose.
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksSinceTakeOff, "ticksSinceTakeOff");
            Scribe_Values.Look<IntVec3>(ref this.landingPadPosition, "landingPadPosition");
            Scribe_Values.Look<Rot4>(ref this.landingPadRotation, "landingPadRotation");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();

            this.ticksSinceTakeOff++;
            if (this.ticksSinceTakeOff <= verticalTrajectoryDurationInTicks)
            {
                FleckMaker.ThrowDustPuff(GenAdj.CellsAdjacentCardinal(this.landingPadPosition, this.landingPadRotation, Util_ThingDefOf.LandingPad.Size).RandomElement(), this.Map, 3f * (1f - (float)this.ticksSinceTakeOff / (float)verticalTrajectoryDurationInTicks));
            }
            if (this.ticksSinceTakeOff == 1)
            {
                // Taking off sound.
                FlyingSpaceshipTakingOff.takingOffSound.PlayOneShot(new TargetInfo(this.Position, this.Map));
            }
            if (this.ticksSinceTakeOff >= verticalTrajectoryDurationInTicks + horizontalTrajectoryDurationInTicks)
            {
                this.Destroy();
            }
        }

        public override void ComputeShipExactPosition()
        {
            Vector3 exactPosition = this.landingPadPosition.ToVector3ShiftedWithAltitude(Altitudes.AltitudeFor(this.def.altitudeLayer));

            if (this.spaceshipKind != SpaceshipKind.Medical)
            {
                // Texture is not aligned. Need a small offset.
                exactPosition += new Vector3(0f, 0, 0.5f).RotatedBy(this.landingPadRotation.AsAngle);
            }
            // Horizontal position.
            if (this.ticksSinceTakeOff >= verticalTrajectoryDurationInTicks)
            {
                // Horizontal trajectory.
                float coefficient = (float)(this.ticksSinceTakeOff - verticalTrajectoryDurationInTicks);
                float num = coefficient * coefficient * 0.001f * 0.8f;
                exactPosition += new Vector3(0f, 0f, num).RotatedBy(this.spaceshipExactRotation);
            }
            this.spaceshipExactPosition = exactPosition;
        }

        public override void ComputeShipShadowExactPosition()
        {
            this.spaceshipShadowExactPosition = this.spaceshipExactPosition;
            float shadowDistanceCoefficient = 2f;
            if (this.ticksSinceTakeOff < verticalTrajectoryDurationInTicks)
            {
                // Taking off.
                shadowDistanceCoefficient *= ((float)this.ticksSinceTakeOff / verticalTrajectoryDurationInTicks);
            }
            GenCelestial.LightInfo lightInfo = GenCelestial.GetLightSourceInfo(this.Map, GenCelestial.LightType.Shadow);
            this.spaceshipShadowExactPosition += new Vector3(lightInfo.vector.x, -0.1f, lightInfo.vector.y) * shadowDistanceCoefficient; // The small 0.01f offset is to ensure spaceship shadow is above its texture.
        }

        public override void ComputeShipExactRotation()
        {
            // Always equal to the landing pad rotation.
        }

        public override void ComputeShipScale()
        {
            // Default value for horizontal trajectory and rotation.
            float coefficient = 1.2f;
            float shadowCoefficient = 0.8f;

            if (this.ticksSinceTakeOff < verticalTrajectoryDurationInTicks)
            {
                // Ascending.
                coefficient = 1f + 0.2f * ((float)this.ticksSinceTakeOff / verticalTrajectoryDurationInTicks);
                shadowCoefficient = 1f - 0.2f * ((float)this.ticksSinceTakeOff / verticalTrajectoryDurationInTicks);
            }
            this.spaceshipScale = this.baseSpaceshipScale * coefficient;
            this.spaceshipShadowScale = this.baseSpaceshipScale * shadowCoefficient;
        }

        // ===================== Draw =====================
        public override void SetShipPositionToBeSelectable()
        {
            if (IsInBounds())
            {
                this.Position = this.spaceshipExactPosition.ToIntVec3();
            }
            else
            {
                this.Position = this.landingPadPosition;
            }
        }
    }
}
