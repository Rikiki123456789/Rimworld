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

        // Texture.
        public Vector3 defaultSpaceshipScale = new Vector3(11f, 1f, 20f);
        public Vector3 medicalSpaceshipScale = new Vector3(7f, 1f, 11f);
        public static Material defaultSpaceshipTexture = MaterialPool.MatFrom("Things/SupplySpaceship/SupplySpaceship");
        public static Material dispatcherTexture = MaterialPool.MatFrom("Things/Dispatcher/DispatcherFlying");
        public static Material medicalSpaceshipTexture = MaterialPool.MatFrom("Things/MedicalSpaceship/MedicalSpaceship");
        public Material spaceshipTexture = null;
        public Vector3 baseSpaceshipScale = new Vector3(1f, 1f, 1f);

        // Sound.
        public static readonly SoundDef takingOffSound = SoundDef.Named("SpaceshipTakingOff");

        // ===================== Setup work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            ConfigureShipTexture(this.spaceshipKind);
        }

        public void ConfigureShipTexture(SpaceshipKind spaceshipKind)
        {
            switch (spaceshipKind)
            {
                case SpaceshipKind.CargoPeriodic:
                case SpaceshipKind.CargoRequested:
                case SpaceshipKind.Damaged:
                    this.spaceshipTexture = defaultSpaceshipTexture;
                    this.baseSpaceshipScale = defaultSpaceshipScale;
                    break;
                case SpaceshipKind.DispatcherDrop:
                case SpaceshipKind.DispatcherPick:
                    this.spaceshipTexture = dispatcherTexture;
                    this.baseSpaceshipScale = defaultSpaceshipScale;
                    break;
                case SpaceshipKind.Medical:
                    this.spaceshipTexture = medicalSpaceshipTexture;
                    this.baseSpaceshipScale = medicalSpaceshipScale;
                    break;
                default:
                    Log.ErrorOnce("MiningCo. Spaceship: unhandled SpaceshipKind (" + this.spaceshipKind.ToString() + ") in FlyingSpaceshipTakingOff.ConfigureShipTexture.", 123456785);
                    break;
            }
        }

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
                MoteMaker.ThrowDustPuff(GenAdj.CellsAdjacentCardinal(this.landingPadPosition, this.landingPadRotation, Util_ThingDefOf.LandingPad.Size).RandomElement(), this.Map, 3f * (1f - (float)this.ticksSinceTakeOff / (float)verticalTrajectoryDurationInTicks));
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
            Vector3 exactPosition = this.landingPadPosition.ToVector3ShiftedWithAltitude(AltitudeLayer.Skyfaller);
            // The 5f offset on Y axis is mandatory to be over the fog of war.
            exactPosition += new Vector3(0f, 5f, 0f);
            if (this.spaceshipKind != SpaceshipKind.Medical)
            {
                // Texture is not aligned. Need a small offset.
                exactPosition += new Vector3(0f, 0, 0.5f).RotatedBy(this.landingPadRotation.AsAngle);
            }
            if (this.ticksSinceTakeOff < verticalTrajectoryDurationInTicks)
            {
                // Ascending.
                exactPosition.z += 3f * ((float)this.ticksSinceTakeOff / verticalTrajectoryDurationInTicks);
            }
            else
            {
                // Horizontal trajectory.
                float coefficient = (float)(this.ticksSinceTakeOff - verticalTrajectoryDurationInTicks);
                float num = coefficient * coefficient * 0.001f * 0.8f;
                exactPosition += new Vector3(0f, 0f, num).RotatedBy(this.spaceshipExactRotation);
                exactPosition.z += 3f;
            }
            this.spaceshipExactPosition = exactPosition;
        }

        public override void ComputeShipExactRotation()
        {
            // Always equal to the landing pad rotation.
        }

        public override void ComputeShipScale()
        {
            float coefficient = 1f;
            if (this.ticksSinceTakeOff < verticalTrajectoryDurationInTicks)
            {
                // Ascending.
                coefficient = 1f + 0.2f * ((float)this.ticksSinceTakeOff / verticalTrajectoryDurationInTicks);
            }
            else
            {
                // Horizontal trajectory.
                coefficient = 1.2f;
            }
            this.spaceshipScale = this.baseSpaceshipScale * coefficient;
        }

        public override void SetShipVisibleAboveFog()
        {
            if (IsInBoundsAndVisible())
            {
                this.Position = this.spaceshipExactPosition.ToIntVec3();
            }
            else
            {
                this.Position = this.landingPadPosition;
            }
        }

        // ===================== Draw =====================
        public override void Draw()
        {
            this.spaceshipMatrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, this.spaceshipExactRotation.ToQuat(), this.spaceshipScale);
            Graphics.DrawMesh(MeshPool.plane10, this.spaceshipMatrix, this.spaceshipTexture, 0);
        }
    }
}
