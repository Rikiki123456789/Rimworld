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
    public class FlyingSpaceshipAirStrike : FlyingSpaceship
    {
        public int ticksBeforeOverflight = 0;
        public int ticksAfterOverflight = 0;
        public AirStrikeDef airStrikeDef = null;
        public IntVec3 targetPosition = IntVec3.Invalid;
        public float shipToTargetDistance = 0f;

        // Weapons.
        public List<int> weaponRemainingRounds = new List<int>(AirStrikeDef.maxWeapons) { -1, -1, -1 }; // -1 means weapon has not started shooting.
        public List<int> weaponNextShotTick = new List<int>(AirStrikeDef.maxWeapons) { 0, 0, 0 };
        public List<bool> weaponShootRight = new List<bool>(AirStrikeDef.maxWeapons) { true, true, true };
        
        // Sound.
        public static readonly SoundDef airStrikeSound = SoundDef.Named("AirStrike");
                
        // ===================== Setup work =====================
        public void InitializeAirStrikeData(IntVec3 targetPosition, AirStrikeDef airStrikeDef)
        {
            this.targetPosition = targetPosition;
            this.airStrikeDef = airStrikeDef;

            this.ticksBeforeOverflight = this.airStrikeDef.ticksBeforeOverflightInitialValue;
            this.ticksAfterOverflight = 0;

            this.spaceshipKind = SpaceshipKind.Airstrike;
            ComputeAirStrikeRotation(this.targetPosition);
            ConfigureShipTexture(this.spaceshipKind);
            base.Tick();
        }

        public void ComputeAirStrikeRotation(IntVec3 targetPosition)
        {
            int mapHalfSizeX = this.Map.Size.x / 2;
            int mapHalfSizeZ = this.Map.Size.z / 2;

            // Restrict rotation if target is near map edge.
            if ((targetPosition.x <= 50)
                || (this.Map.Size.x - targetPosition.x <= 50)
                || (targetPosition.z <= 50)
                || (this.Map.Size.z - targetPosition.z <= 50))
            {
                // N-W quadrant.
                if ((targetPosition.x <= mapHalfSizeX)
                    && (targetPosition.z >= mapHalfSizeZ))
                {
                    this.spaceshipExactRotation = Rand.RangeInclusive(280, 350);
                }
                // N-E quadrant.
                else if ((targetPosition.x >= mapHalfSizeX)
                    && (targetPosition.z >= mapHalfSizeZ))
                {
                    this.spaceshipExactRotation = Rand.RangeInclusive(10, 80);
                }
                // S-E quadrant.
                else if ((targetPosition.x >= mapHalfSizeX)
                    && (targetPosition.z <= mapHalfSizeZ))
                {
                    this.spaceshipExactRotation = Rand.RangeInclusive(100, 170);
                }
                // S-W quadrant.
                else
                {
                    this.spaceshipExactRotation = Rand.RangeInclusive(190, 260);
                }
            }
            else
            {
                this.spaceshipExactRotation = Rand.Range(0f, 360f);
            }
            this.Rotation = new Rot4(Mathf.RoundToInt(this.spaceshipExactRotation) / 90);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksBeforeOverflight, "ticksBeforeOverflight");
            Scribe_Values.Look<int>(ref this.ticksAfterOverflight, "ticksAfterOverflight");
            Scribe_Values.Look<IntVec3>(ref this.targetPosition, "targetPosition");
            Scribe_Defs.Look<AirStrikeDef>(ref this.airStrikeDef, "airStrikeDef");
            Scribe_Values.Look<float>(ref this.spaceshipExactRotation, "shipRotation");

            Scribe_Collections.Look<int>(ref this.weaponRemainingRounds, "weaponRemainingRounds");
            Scribe_Collections.Look<int>(ref this.weaponNextShotTick, "weaponNextShotTick");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();

            if (this.ticksBeforeOverflight == this.airStrikeDef.ticksBeforeOverflightPlaySound)
            {
                // Atmosphere entry sound.
                FlyingSpaceshipAirStrike.airStrikeSound.PlayOneShot(new TargetInfo(targetPosition, this.Map));
            }

            for (int weaponIndex = 0; weaponIndex < this.airStrikeDef.weapons.Count; weaponIndex++)
            {
                WeaponTick(weaponIndex, this.airStrikeDef.weapons[weaponIndex]);
            }
            
            if (this.ticksBeforeOverflight > 0)
            {
                this.ticksBeforeOverflight--;
            }
            else
            {
                this.ticksAfterOverflight++;
                if ((this.ticksAfterOverflight >= this.airStrikeDef.ticksAfterOverflightFinalValue)
                    || (this.spaceshipExactPosition.InBounds(this.Map) == false))
                {
                    this.Destroy();
                }
            }
        }

        public override void ComputeShipExactPosition()
        {
            Vector3 unitVector = new Vector3(0f, 0f, 1f).RotatedBy(this.spaceshipExactRotation);

            this.spaceshipExactPosition = this.targetPosition.ToVector3ShiftedWithAltitude(AltitudeLayer.Skyfaller);
            
            if (this.ticksBeforeOverflight > 0)
            {
                if (this.ticksBeforeOverflight > this.airStrikeDef.ticksBeforeOverflightReducedSpeed)
                {
                    float coefficient = (float)(this.ticksBeforeOverflight - this.airStrikeDef.ticksBeforeOverflightReducedSpeed);
                    float num = coefficient * coefficient * 0.01f;
                    this.shipToTargetDistance = (num + (float)this.ticksBeforeOverflight) * this.airStrikeDef.cellsTravelledPerTick;
                }
                else
                {
                    this.shipToTargetDistance = (float)this.ticksBeforeOverflight * this.airStrikeDef.cellsTravelledPerTick;
                }
                this.spaceshipExactPosition -= unitVector * this.shipToTargetDistance;
            }
            else
            {
                if (this.ticksAfterOverflight > this.airStrikeDef.ticksAfterOverflightReducedSpeed)
                {
                    float coefficient = (float)(this.ticksAfterOverflight - this.airStrikeDef.ticksAfterOverflightReducedSpeed);
                    float num = coefficient * coefficient * 0.01f;
                    this.shipToTargetDistance = (num + (float)this.ticksAfterOverflight) * this.airStrikeDef.cellsTravelledPerTick;
                }
                else
                {
                    this.shipToTargetDistance = (float)this.ticksAfterOverflight * this.airStrikeDef.cellsTravelledPerTick;
                }
                this.spaceshipExactPosition += unitVector * this.shipToTargetDistance;
            }
            // The 5f offset on Y axis is mandatory to be over the fog of war. The 0.1f is to ensure spaceship texture is above its shadow.
            this.spaceshipExactPosition += new Vector3(0f, 5.1f, 0f);
        }

        public override void ComputeShipShadowExactPosition()
        {
            GenCelestial.LightInfo lightInfo = GenCelestial.GetLightSourceInfo(this.Map, GenCelestial.LightType.Shadow);
            this.spaceshipShadowExactPosition = this.spaceshipExactPosition + 2f * new Vector3(lightInfo.vector.x, -0.1f, lightInfo.vector.y);
        }

        public override void ComputeShipExactRotation()
        {
            // Only modified at parameters initialization.
        }

        public override void ComputeShipScale()
        {
            // Equal to default parent value.
        }
        
        public override void SetShipVisibleAboveFog()
        {
            if (IsInBoundsAndVisible())
            {
                this.Position = this.spaceshipExactPosition.ToIntVec3();
            }
            else
            {
                this.Position = this.targetPosition;
            }
        }

        public void WeaponTick(int weaponIndex, WeaponDef weaponDef)
        {
            if ((weaponDef.ammoDef != null)
                && (this.weaponRemainingRounds[weaponIndex] == -1)
                && (this.shipToTargetDistance <= weaponDef.startShootingDistance))
            {
                // Start shooting.
                this.weaponRemainingRounds[weaponIndex] = weaponDef.ammoQuantity;
                this.weaponNextShotTick[weaponIndex] = Find.TickManager.TicksGame;
                int firstShotSideAsInt = Rand.RangeInclusive(0, 1);
                if (firstShotSideAsInt == 1)
                {
                    this.weaponShootRight[weaponIndex] = true;
                }
                else
                {
                    this.weaponShootRight[weaponIndex] = false;
                }
                if (weaponDef.disableRainDurationInTicks > 0)
                {
                    this.Map.weatherDecider.DisableRainFor(weaponDef.disableRainDurationInTicks);
                }
            }

            if ((this.weaponRemainingRounds[weaponIndex] > 0)
                && (Find.TickManager.TicksGame >= this.weaponNextShotTick[weaponIndex]))
            {
                // Shoot 1 round.
                float sign = 0;
                if ((weaponDef.isTwinGun == false)
                    || this.weaponShootRight[weaponIndex])
                {
                    sign = 1f;
                }
                else
                {
                    sign = -1f;
                }
                Vector3 roundOrigin = this.spaceshipExactPosition + new Vector3(sign * weaponDef.horizontalPositionOffset, 0f, weaponDef.verticalPositionOffset).RotatedBy(this.spaceshipExactRotation);
                Vector3 roundDestination = roundOrigin + new Vector3(0f, 0f, weaponDef.ammoTravelDistance).RotatedBy(this.spaceshipExactRotation);
                if (roundOrigin.InBounds(this.Map)
                    && roundDestination.InBounds(this.Map))
                {
                    Projectile projectile = GenSpawn.Spawn(weaponDef.ammoDef, roundOrigin.ToIntVec3(), this.Map) as Projectile;
                    if (weaponDef.soundCastDef != null)
                    {
                        weaponDef.soundCastDef.PlayOneShot(new TargetInfo(roundOrigin.ToIntVec3(), this.Map));
                    }
                    MoteMaker.MakeStaticMote(roundOrigin, this.Map, ThingDefOf.Mote_ShotFlash, 10f);
                    // Look for hostile pawn if weapon has a target acquire range.
                    Pawn pawn = null;
                    if (weaponDef.targetAcquireRange > 0f)
                    {
                        pawn = GetRandomeHostilePawnAround(roundDestination, weaponDef.targetAcquireRange);
                    }
                    if (pawn != null)
                    {
                        projectile.Launch(this, roundOrigin, pawn, pawn, ProjectileHitFlags.IntendedTarget);
                    }
                    else
                    {
                        roundDestination += new Vector3(Rand.Range(-weaponDef.targetAcquireRange, weaponDef.targetAcquireRange), 0f, 0f).RotatedBy(this.spaceshipExactRotation);
                        projectile.Launch(this, roundOrigin, roundDestination.ToIntVec3(), roundDestination.ToIntVec3(), ProjectileHitFlags.None);
                    }
                }
                this.weaponRemainingRounds[weaponIndex]--;
                this.weaponNextShotTick[weaponIndex] = Find.TickManager.TicksGame + weaponDef.ticksBetweenShots;
                this.weaponShootRight[weaponIndex] = !this.weaponShootRight[weaponIndex];
            }
        }

        public Pawn GetRandomeHostilePawnAround(Vector3 center, float radius)
        {
            List<Pawn> hostilePawnsAround = new List<Pawn>();
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center.ToIntVec3(), radius, true))
            {
                if (cell.InBounds(this.Map) == false)
                {
                    continue;
                }
                Pawn pawn = cell.GetFirstPawn(this.Map);
                if ((pawn != null)
                    && pawn.HostileTo(Faction.OfPlayer)
                    && (pawn.health.Downed == false))
                {
                    hostilePawnsAround.Add(pawn);
                }
            }
            if (hostilePawnsAround.Count > 0)
            {
                return hostilePawnsAround.RandomElement();
            }
            return null;
        }
    }
}
