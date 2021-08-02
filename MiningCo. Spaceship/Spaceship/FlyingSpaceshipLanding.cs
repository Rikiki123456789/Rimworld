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
    public class FlyingSpaceshipLanding : FlyingSpaceship
    {
        public const int horizontalTrajectoryDurationInTicks = 8 * GenTicks.TicksPerRealSecond;
        public const int verticalTrajectoryDurationInTicks = 4 * GenTicks.TicksPerRealSecond;
        public int ticksToLanding = horizontalTrajectoryDurationInTicks + verticalTrajectoryDurationInTicks;
        public IntVec3 landingPadPosition = IntVec3.Invalid;
        public Rot4 landingPadRotation = Rot4.North;
        public int landingDuration = 0;
        
        // Sound.
        public static readonly SoundDef preLandingSound = SoundDef.Named("DropPodFall");
        public static readonly SoundDef landingSound = SoundDef.Named("SpaceshipLanding");
        
        // ===================== Setup work =====================
        /// <summary>
        /// Must be called just after spawning the ship to set the correct texture.
        /// </summary>
        public void InitializeLandingParameters(Building_LandingPad landingPad, int landingDuration, SpaceshipKind spaceshipKind)
        {
            landingPad.Notify_ShipLanding();
            this.landingPadPosition = landingPad.Position;
            this.landingPadRotation = landingPad.Rotation;
            this.spaceshipExactRotation = this.landingPadRotation.AsAngle;
            this.landingDuration = landingDuration;
            this.spaceshipKind = spaceshipKind;
            ConfigureShipTexture(this.spaceshipKind);
            this.Tick(); // To update exact position for drawing purpose.
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksToLanding, "ticksToLanding");
            Scribe_Values.Look<IntVec3>(ref this.landingPadPosition, "landingPadPosition");
            Scribe_Values.Look<Rot4>(ref this.landingPadRotation, "landingPadRotation");
            Scribe_Values.Look<int>(ref this.landingDuration, "landingDuration");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();

            if (this.ticksToLanding == horizontalTrajectoryDurationInTicks + verticalTrajectoryDurationInTicks)
            {
                // Atmosphere entry sound.
                FlyingSpaceshipLanding.preLandingSound.PlayOneShot(new TargetInfo(this.Position, this.Map));
            }
            this.ticksToLanding--;
            if (this.ticksToLanding == verticalTrajectoryDurationInTicks)
            {
                // Landing on sound.
                FlyingSpaceshipLanding.landingSound.PlayOneShot(new TargetInfo(this.Position, this.Map));
            }
            if (this.ticksToLanding <= verticalTrajectoryDurationInTicks)
            {
                // Throw dust during descent.
                FleckMaker.ThrowDustPuff(GenAdj.CellsAdjacentCardinal(this.landingPadPosition, this.landingPadRotation, Util_ThingDefOf.LandingPad.Size).RandomElement(), this.Map, 3f * (1f - (float)this.ticksToLanding / (float)verticalTrajectoryDurationInTicks));
            }
            if (this.ticksToLanding == 0)
            {
                Building_Spaceship spaceship = null;
                switch (this.spaceshipKind)
                {
                    case SpaceshipKind.CargoPeriodic:
                    case SpaceshipKind.CargoRequested:
                        // Spawn cargo spaceship.
                        Building_SpaceshipCargo cargoSpaceship = ThingMaker.MakeThing(Util_Spaceship.SpaceshipCargo) as Building_SpaceshipCargo;
                        cargoSpaceship.InitializeData_Cargo(Util_Faction.MiningCoFaction, this.HitPoints, this.landingDuration, this.spaceshipKind);
                        spaceship = GenSpawn.Spawn(cargoSpaceship, this.landingPadPosition, this.Map, this.landingPadRotation) as Building_Spaceship;
                        break;
                    case SpaceshipKind.Damaged:
                        // Spawn damaged spaceship.
                        Building_SpaceshipDamaged damagedSpaceship = ThingMaker.MakeThing(Util_Spaceship.SpaceshipDamaged) as Building_SpaceshipDamaged;
                        damagedSpaceship.InitializeData_Damaged(Util_Faction.MiningCoFaction, this.HitPoints, this.landingDuration, this.spaceshipKind, this.HitPoints);
                        // Faction will be set to player when repair materials are delivered.
                        spaceship = GenSpawn.Spawn(damagedSpaceship, this.landingPadPosition, this.Map, this.landingPadRotation) as Building_Spaceship;
                        break;
                    case SpaceshipKind.DispatcherDrop:
                        // Spawn dispatcher spaceship.
                        Building_SpaceshipDispatcherDrop dispatcherSpaceshipDrop = ThingMaker.MakeThing(Util_Spaceship.SpaceshipDispatcherDrop) as Building_SpaceshipDispatcherDrop;
                        dispatcherSpaceshipDrop.InitializeData_DispatcherDrop(Util_Faction.MiningCoFaction, this.HitPoints, this.landingDuration, this.spaceshipKind);
                        spaceship = GenSpawn.Spawn(dispatcherSpaceshipDrop, this.landingPadPosition, this.Map, this.landingPadRotation) as Building_Spaceship;
                        break;
                    case SpaceshipKind.DispatcherPick:
                        // Spawn dispatcher spaceship.
                        Building_SpaceshipDispatcherPick dispatcherSpaceshipPick = ThingMaker.MakeThing(Util_Spaceship.SpaceshipDispatcherPick) as Building_SpaceshipDispatcherPick;
                        dispatcherSpaceshipPick.InitializeData_DispatcherPick(Util_Faction.MiningCoFaction, this.HitPoints, this.landingDuration, this.spaceshipKind);
                        spaceship = GenSpawn.Spawn(dispatcherSpaceshipPick, this.landingPadPosition, this.Map, this.landingPadRotation) as Building_Spaceship;
                        break;
                    case SpaceshipKind.Medical:
                        // Spawn medical spaceship.
                        Building_SpaceshipMedical medicalSpaceship = ThingMaker.MakeThing(Util_Spaceship.SpaceshipMedical) as Building_SpaceshipMedical;
                        medicalSpaceship.InitializeData_Medical(Util_Faction.MiningCoFaction, this.HitPoints, this.landingDuration, this.spaceshipKind);
                        spaceship = GenSpawn.Spawn(medicalSpaceship, this.landingPadPosition, this.Map, this.landingPadRotation) as Building_Spaceship;
                        break;
                    default:
                        Log.ErrorOnce("MiningCo. Spaceship: unhandled SpaceshipKind (" + this.spaceshipKind.ToString() + ").", 123456783);
                        break;
                }
                this.Destroy();
            }
        }

        public override void ComputeShipExactPosition()
        {
            Vector3 exactPosition = this.landingPadPosition.ToVector3ShiftedWithAltitude(Altitudes.AltitudeFor(this.def.altitudeLayer));

            if (this.spaceshipKind != SpaceshipKind.Medical)
            {
                // Texture is not aligned. Need a small offset.
                exactPosition += new Vector3(0f, 0f, 0.5f).RotatedBy(this.landingPadRotation.AsAngle);
            }
            // Horizontal position.
            if (this.ticksToLanding > verticalTrajectoryDurationInTicks)
            {
                // Horizontal trajectory.
                float coefficient = (float)(this.ticksToLanding - verticalTrajectoryDurationInTicks);
                float num = coefficient * coefficient * 0.001f * 0.8f;
                exactPosition -= new Vector3(0f, 0f, num).RotatedBy(this.spaceshipExactRotation);
            }
            this.spaceshipExactPosition = exactPosition;
        }

        public override void ComputeShipShadowExactPosition()
        {
            this.spaceshipShadowExactPosition = this.spaceshipExactPosition;
            float shadowDistanceCoefficient = 2f;
            if (this.ticksToLanding < verticalTrajectoryDurationInTicks)
            {
                // Landing.
                shadowDistanceCoefficient *= ((float)this.ticksToLanding / verticalTrajectoryDurationInTicks);
            }
            GenCelestial.LightInfo lightInfo = GenCelestial.GetLightSourceInfo(this.Map, GenCelestial.LightType.Shadow);
            this.spaceshipShadowExactPosition += new Vector3(lightInfo.vector.x, -0.01f, lightInfo.vector.y) * shadowDistanceCoefficient; // The small 0.01f offset is to ensure spaceship shadow is above its texture.
        }

        public override void ComputeShipExactRotation()
        {
            // Always equal to the landing pad rotation.
        }

        public override void ComputeShipScale()
        {
            // Default value for horizontal trajectory and rotation.
            float coefficient = 1.2f;
            float shadowCoefficient = 0.9f;

            if (this.ticksToLanding <= verticalTrajectoryDurationInTicks)
            {
                // Descent.
                coefficient = 1f + 0.2f * ((float)this.ticksToLanding / verticalTrajectoryDurationInTicks);
                shadowCoefficient = 1f - 0.1f * ((float)this.ticksToLanding / verticalTrajectoryDurationInTicks);
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
