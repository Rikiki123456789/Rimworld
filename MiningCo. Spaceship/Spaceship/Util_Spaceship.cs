using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;

namespace Spaceship
{
    public static class Util_Spaceship
    {
        public const int cargoSupplyCostInSilver = 1500;
        public const int medicalSupplyCostInSilver = 800;
        public const int orbitalHealingCost = 500;
        public const int feePerPawnInSilver = 40;
        public const int medicsRecallBeforeTakeOffMarginInTicks = 6 * GenDate.TicksPerHour;
        
        public const int cargoPeriodicSupplyLandingDuration = 12 * GenDate.TicksPerHour;
        public const int cargoRequestedSupplyLandingDuration = GenDate.TicksPerDay;
        public static IntRange damagedSpaceshipLandingDuration = new IntRange(8 * GenDate.TicksPerDay, 12 * GenDate.TicksPerDay);
        public const int dispatcherDropDurationInTicks = 2 * GenDate.TicksPerHour;
        public const int dispatcherPickDurationInTicks = 2 * GenDate.TicksPerDay;
        public const int medicalSupplyLandingDuration = GenDate.TicksPerDay;

        // Landing and taking off.
        public static ThingDef SpaceshipLanding
        {
            get
            {
                return ThingDef.Named("FlyingSpaceshipLanding");
            }
        }

        public static ThingDef SpaceshipTakingOff
        {
            get
            {
                return ThingDef.Named("FlyingSpaceshipTakingOff");
            }
        }

        // Landed "building" spaceships.
        public static ThingDef SpaceshipCargo
        {
            get
            {
                return ThingDef.Named("SpaceshipCargo");
            }
        }

        public static ThingDef SpaceshipDamaged
        {
            get
            {
                return ThingDef.Named("SpaceshipDamaged");
            }
        }

        public static ThingDef SpaceshipDispatcherDrop
        {
            get
            {
                return ThingDef.Named("SpaceshipDispatcherDrop");
            }
        }

        public static ThingDef SpaceshipDispatcherPick
        {
            get
            {
                return ThingDef.Named("SpaceshipDispatcherPick");
            }
        }

        public static ThingDef SpaceshipMedical
        {
            get
            {
                return ThingDef.Named("SpaceshipMedical");
            }
        }

        // Air strike.
        public static ThingDef SpaceshipAirStrike
        {
            get
            {
                return ThingDef.Named("FlyingSpaceshipAirStrike");
            }
        }
        
        public static FlyingSpaceshipLanding SpawnSpaceship(Building_LandingPad landingPad, SpaceshipKind spaceshipKind)
        {
            Building_OrbitalRelay orbitalRelay = Util_OrbitalRelay.GetOrbitalRelay(landingPad.Map);
            int landingDuration = 0; 
            switch (spaceshipKind)
            {
                case SpaceshipKind.CargoPeriodic:
                    landingDuration = cargoPeriodicSupplyLandingDuration;
                    if (orbitalRelay != null)
                    {
                        orbitalRelay.Notify_CargoSpaceshipPeriodicLanding();
                    }
                    Util_Misc.Partnership.nextPeriodicSupplyTick[landingPad.Map] = Find.TickManager.TicksGame + WorldComponent_Partnership.cargoSpaceshipPeriodicSupplyPeriodInTicks;
                    Messages.Message("A MiningCo. cargo spaceship is landing.", new TargetInfo(landingPad.Position, landingPad.Map), MessageTypeDefOf.NeutralEvent);
                    break;
                case SpaceshipKind.CargoRequested:
                    landingDuration = cargoRequestedSupplyLandingDuration;
                    if (orbitalRelay != null)
                    {
                        orbitalRelay.Notify_CargoSpaceshipRequestedLanding();
                    }
                    Util_Misc.Partnership.nextRequestedSupplyMinTick[landingPad.Map] = Find.TickManager.TicksGame + WorldComponent_Partnership.cargoSpaceshipRequestedSupplyPeriodInTicks;
                    Messages.Message("A MiningCo. cargo spaceship is landing.", new TargetInfo(landingPad.Position, landingPad.Map), MessageTypeDefOf.NeutralEvent);
                    break;
                case SpaceshipKind.Damaged:
                    landingDuration = damagedSpaceshipLandingDuration.RandomInRange;
                    // Letter is sent by incident worker.
                    break;
                case SpaceshipKind.DispatcherDrop:
                    landingDuration = dispatcherDropDurationInTicks;
                    Messages.Message("A MiningCo. dispatcher is dropping an expedition team.", new TargetInfo(landingPad.Position, landingPad.Map), MessageTypeDefOf.NeutralEvent);
                    break;
                case SpaceshipKind.DispatcherPick:
                    landingDuration = dispatcherPickDurationInTicks;
                    Messages.Message("A MiningCo. dispatcher is picking an expedition team.", new TargetInfo(landingPad.Position, landingPad.Map), MessageTypeDefOf.NeutralEvent);
                    break;
                case SpaceshipKind.Medical:
                    landingDuration = medicalSupplyLandingDuration;
                    if (orbitalRelay != null)
                    {
                        orbitalRelay.Notify_MedicalSpaceshipLanding();
                    }
                    Util_Misc.Partnership.nextMedicalSupplyMinTick[landingPad.Map] = Find.TickManager.TicksGame + WorldComponent_Partnership.medicalSpaceshipRequestedSupplyPeriodInTicks;
                    Messages.Message("A MiningCo. medical spaceship is landing.", new TargetInfo(landingPad.Position, landingPad.Map), MessageTypeDefOf.NeutralEvent);
                    break;
            }

            FlyingSpaceshipLanding flyingSpaceship = ThingMaker.MakeThing(Util_Spaceship.SpaceshipLanding) as FlyingSpaceshipLanding;
            GenSpawn.Spawn(flyingSpaceship, landingPad.Position, landingPad.Map, landingPad.Rotation);
            flyingSpaceship.InitializeLandingParameters(landingPad, landingDuration, spaceshipKind);
            return flyingSpaceship;
        }
    }
}
