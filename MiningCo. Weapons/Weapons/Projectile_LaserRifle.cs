using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace Projectile_Laser
{
    /// <summary>
    /// Common laser type projectile class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Projectile_LaserRifle : Projectile
    {
        // Variables.
        public int tickCounter = 0;
        public Thing hitThing = null;

        // Miscellaneous.
        public const float stunChance = 0.1f;
        
        // Draw variables.
        public Material preFiringTexture;
        public Material postFiringTexture;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Vector3 drawingScale;
        public Vector3 drawingPosition;
        public float drawingIntensity = 0f;
        public Material drawingTexture = null;

        // Custom XML variables.
        public float preFiringInitialIntensity = 0f;
        public float preFiringFinalIntensity = 0f;
        public float postFiringInitialIntensity = 0f;
        public float postFiringFinalIntensity = 0f;
        public int preFiringDuration = 0;
        public int postFiringDuration = 0;

        /// <summary>
        /// Get parameters from XML.
        /// </summary>
        public void GetParametersFromXml()
        {
            Projectile_Laser.ThingDef_LaserProjectile additionalParameters = def as Projectile_Laser.ThingDef_LaserProjectile;

            preFiringDuration = additionalParameters.preFiringDuration;
            postFiringDuration = additionalParameters.postFiringDuration;

            // Draw.
            preFiringInitialIntensity = additionalParameters.preFiringInitialIntensity;
            preFiringFinalIntensity = additionalParameters.preFiringFinalIntensity;
            postFiringInitialIntensity = additionalParameters.postFiringInitialIntensity;
            postFiringFinalIntensity = additionalParameters.postFiringFinalIntensity;
        }

        /// <summary>
        /// Save/load data from a savegame file (apparently not used for projectile for now).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref tickCounter, "tickCounter", 0);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                GetParametersFromXml();
            }
        }

        /// <summary>
        /// Main projectile sequence.
        /// </summary>
        public override void Tick()
        {
            if (tickCounter == 0)
            {
                GetParametersFromXml();
                PerformPreFiringTreatment();
            }

            // Pre firing.
            if (tickCounter < preFiringDuration)
            {
                GetPreFiringDrawingParameters();
            }
            // Firing.
            else if (tickCounter == this.preFiringDuration)
            {
                Fire();
                GetPostFiringDrawingParameters();
            }
            // Post firing.
            else
            {
                GetPostFiringDrawingParameters();
            }

            if (tickCounter == (this.preFiringDuration + this.postFiringDuration))
            {
                this.Destroy(DestroyMode.Vanish);
            }
            if (this.launcher is Pawn)
            {
                Pawn launcherPawn = this.launcher as Pawn;
                if (((launcherPawn.stances.curStance is Stance_Warmup) == false)
                    && ((launcherPawn.stances.curStance is Stance_Cooldown) == false))
                {
                    this.Destroy(DestroyMode.Vanish);
                }
            }

            tickCounter++;
        }
        
        /// <summary>
        /// Performs prefiring treatment: data initalization.
        /// </summary>
        public virtual void PerformPreFiringTreatment()
        {
            drawingTexture = this.def.DrawMatSingle;
            DetermineImpactExactPosition();
            Vector3 cannonMouthOffset = ((this.destination - this.origin).normalized * 0.9f);
            drawingScale = new Vector3(1f, 1f, (this.destination - this.origin).magnitude - cannonMouthOffset.magnitude);
            drawingPosition = this.origin + (cannonMouthOffset / 2) + ((this.destination - this.origin) / 2) + Vector3.up * this.def.Altitude;
            drawingMatrix.SetTRS(drawingPosition, this.ExactRotation, drawingScale);
        }
        
        /// <summary>
        /// Gets the prefiring drawing parameters.
        /// </summary>
        public virtual void GetPreFiringDrawingParameters()
        {
            if (preFiringDuration != 0)
            {
                drawingIntensity = preFiringInitialIntensity + (preFiringFinalIntensity - preFiringInitialIntensity) * (float)tickCounter / (float)preFiringDuration;
            }
        }

        /// <summary>
        /// Gets the postfiring drawing parameters.
        /// </summary>
        public virtual void GetPostFiringDrawingParameters()
        {
            if (postFiringDuration != 0)
            {
                drawingIntensity = postFiringInitialIntensity + (postFiringFinalIntensity - postFiringInitialIntensity) * (((float)tickCounter - (float)preFiringDuration) / (float)postFiringDuration);
            }
        }

        /// <summary>
        /// Checks for colateral targets (cover, neutral animal, pawn) along the trajectory.
        /// </summary>
        protected void DetermineImpactExactPosition()
        {
            // We split the trajectory into small segments of approximatively 1 cell size.
            Vector3 trajectory = (this.destination - this.origin);
            int numberOfSegments = (int)trajectory.magnitude;
            Vector3 trajectorySegment = (trajectory / trajectory.magnitude);

            Vector3 temporaryDestination = this.origin; // Last valid tested position in case of an out of boundaries shot.
            Vector3 exactTestedPosition = this.origin;
            IntVec3 testedPosition = exactTestedPosition.ToIntVec3();

            for (int segmentIndex = 1; segmentIndex <= numberOfSegments; segmentIndex++)
            {
                exactTestedPosition += trajectorySegment;
                testedPosition = exactTestedPosition.ToIntVec3();

                if (!exactTestedPosition.InBounds())
                {
                    this.destination = temporaryDestination;
                    break;
                }

                if (!this.def.projectile.flyOverhead && this.canFreeIntercept && segmentIndex >= 5)
                {
                    List<Thing> list = Find.ThingGrid.ThingsListAt(base.Position);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Thing current = list[i];

                        // Check impact on a wall.
                        if (current.def.Fillage == FillCategory.Full)
                        {
                            this.destination = testedPosition.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
                            this.hitThing = current;
                            break;
                        }

                        // Check impact on a pawn.
                        if (current.def.category == EntityCategory.Pawn)
                        {
                            Pawn pawn = current as Pawn;
                            float chanceToHitCollateralTarget = 0.45f;
                            if (pawn.Downed)
                            {
                                chanceToHitCollateralTarget *= 0.1f;
                            }
                            float targetDistanceFromShooter = (this.ExactPosition - this.origin).MagnitudeHorizontal();
                            if (targetDistanceFromShooter < 4f)
                            {
                                chanceToHitCollateralTarget *= 0f;
                            }
                            else
                            {
                                if (targetDistanceFromShooter < 7f)
                                {
                                    chanceToHitCollateralTarget *= 0.5f;
                                }
                                else
                                {
                                    if (targetDistanceFromShooter < 10f)
                                    {
                                        chanceToHitCollateralTarget *= 0.75f;
                                    }
                                }
                            }
                            chanceToHitCollateralTarget *= pawn.RaceProps.bodySize;

                            if (Rand.Value < chanceToHitCollateralTarget)
                            {
                                this.destination = testedPosition.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
                                this.hitThing = (Thing)pawn;
                                break;
                            }
                        }
                    }
                }

                temporaryDestination = exactTestedPosition;
            }
        }

        /// <summary>
        /// Manages the projectile damage application.
        /// </summary>
        public virtual void Fire()
        {
            ApplyDamage(this.hitThing);
        }

        /// <summary>
        /// Applies damage on a collateral pawn or an object.
        /// </summary>
        protected void ApplyDamage(Thing hitThing)
        {
            if (hitThing != null)
            {
                // Impact collateral target.
                this.Impact(hitThing);
            }
            else
            {
                this.ImpactSomething();
            }
        }

        /// <summary>
        /// Computes what should be impacted in the DestinationCell.
        /// </summary>
        protected void ImpactSomething()
        {
            // Check impact on a thick mountain.
            if (this.def.projectile.flyOverhead)
            {
                RoofDef roofDef = Find.RoofGrid.RoofAt(this.DestinationCell);
                if (roofDef != null && roofDef.isThickRoof)
                {
                    this.def.projectile.soundHitThickRoof.PlayOneShot(this.DestinationCell);
                    return;
                }
            }

            // Impact the initial targeted pawn.
            if (this.assignedTarget != null)
            {
                Pawn pawn = this.assignedTarget as Pawn;
                if (pawn != null && pawn.Downed && (this.origin - this.destination).magnitude > 5f && Rand.Value < 0.2f)
                {
                    this.Impact(null);
                    return;
                }
                this.Impact(this.assignedTarget);
                return;
            }
            else
            {
                // Impact a pawn in the destination cell if present.
                Thing thing = Find.ThingGrid.ThingAt(this.DestinationCell, EntityCategory.Pawn);
                if (thing != null)
                {
                    this.Impact(thing);
                    return;
                }
                // Impact any cover object.
                foreach (Thing current in Find.ThingGrid.ThingsAt(this.DestinationCell))
                {
                    if (current.def.fillPercent > 0f || current.def.passability != Traversability.Standable)
                    {
                        this.Impact(current);
                        return;
                    }
                }
                this.Impact(null);
                return;
            }
        }

        /// <summary>
        /// Impacts a pawn/object or the ground.
        /// </summary>
        protected override void Impact(Thing hitThing)
        {
            if (hitThing != null)
            {
                int damageAmountBase = this.def.projectile.damageAmountBase;
                BodyPartDamageInfo value = new BodyPartDamageInfo(null, null);
                DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, this.launcher, this.ExactRotation.eulerAngles.y, new BodyPartDamageInfo?(value), this.equipmentDef);
                hitThing.TakeDamage(dinfo);
                hitThing.def.soundImpactDefault.PlayOneShot(this.DestinationCell);
                Pawn pawn = hitThing as Pawn;
                if (pawn != null && !pawn.Downed && Rand.Value < Projectile_LaserRifle.stunChance)
                {
                    hitThing.TakeDamage(new DamageInfo(DamageDefOf.Stun, 10, this.launcher, null, null));
                }
            }
            else
            {
                MoteThrower.ThrowMicroSparks(this.destination);
            }
        }
        
        /// <summary>
        /// Draws the laser ray.
        /// </summary>
        public override void Draw()
        {
            this.Comps_PostDraw();
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, drawingMatrix, FadedMaterialPool.FadedVersionOf(drawingTexture, drawingIntensity), 0);
        }
    }
}
