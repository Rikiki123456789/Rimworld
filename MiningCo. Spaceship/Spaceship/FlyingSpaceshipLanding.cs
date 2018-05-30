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
        public const int horizontalTrajectoryDurationInTicks = 480;
        public const int verticalTrajectoryDurationInTicks = 240;
        public int ticksToLanding = horizontalTrajectoryDurationInTicks + verticalTrajectoryDurationInTicks;
        public IntVec3 landingPadPosition = IntVec3.Invalid;
        public Rot4 landingPadRotation = Rot4.North;
        public int landingDuration = 0;

        // Texture.
        public Vector3 defaultSpaceshipScale = new Vector3(11f, 1f, 20f);
        public Vector3 medicalSpaceshipScale = new Vector3(7f, 1f, 11f);
        public static Material defaultSpaceshipTexture = MaterialPool.MatFrom("Things/SupplySpaceship/SupplySpaceship");
        public static Material dispatcherTexture = MaterialPool.MatFrom("Things/Dispatcher/DispatcherFlying");
        public static Material medicalSpaceshipTexture = MaterialPool.MatFrom("Things/MedicalSpaceship/MedicalSpaceship");
        public Material spaceshipTexture = null;
        public Vector3 baseSpaceshipScale = new Vector3(1f, 1f, 1f);

        // Sound.
        public static readonly SoundDef preLandingSound = SoundDef.Named("DropPodFall");
        public static readonly SoundDef landingSound = SoundDef.Named("SpaceshipLanding");
        
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
                    Log.ErrorOnce("MiningCo. Spaceship: unhandled SpaceshipKind (" + this.spaceshipKind.ToString() + ") in FlyingSpaceshipLanding.ConfigureShipTexture.", 123456784);
                    break;
            }
        }

        public void InitializeLandingParameters(Building_LandingPad landingPad, int landingDuration, SpaceshipKind spaceshipKind)
        {
            landingPad.NotifyShipLanding();
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
                MoteMaker.ThrowDustPuff(GenAdj.CellsAdjacentCardinal(this.landingPadPosition, this.landingPadRotation, Util_ThingDefOf.LandingPad.Size).RandomElement(), this.Map, 3f * (1f - (float)this.ticksToLanding / (float)verticalTrajectoryDurationInTicks));
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
                        // Do not set faction to player. It will be done when repair materials are delivered.
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
                        Log.ErrorOnce("MiningCo. Spaceship: unhandled SpaceshipKind (" + this.spaceshipKind.ToString() + ") in SpaceshipLanding.Tick.", 123456783);
                        break;
                }
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
            if (this.ticksToLanding > verticalTrajectoryDurationInTicks)
            {
                // Horizontal trajectory.
                float coefficient = (float)(this.ticksToLanding - verticalTrajectoryDurationInTicks);
                float num = coefficient * coefficient * 0.001f * 0.8f;
                exactPosition -= new Vector3(0f, 0f, num).RotatedBy(this.spaceshipExactRotation);
                exactPosition.z += 3f;
            }
            else
            {
                // Descent.
                exactPosition.z += 3f * ((float)this.ticksToLanding / verticalTrajectoryDurationInTicks);
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
            if (this.ticksToLanding > verticalTrajectoryDurationInTicks)
            {
                // Horizontal trajectory and rotation.
                coefficient = 1.2f;
            }
            else
            {
                // Descent.
                coefficient = 1f + 0.2f * ((float)this.ticksToLanding / verticalTrajectoryDurationInTicks);
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
