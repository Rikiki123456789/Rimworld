using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace Weapons
{
    /// <summary>
    /// Projectile_Laser class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class Projectile_Laser : Bullet
    {
        // TODO: check Impact is called.
        // TODO: convert sounds to ogg.
        // TODO: check save/load.
        // TODO: check out of bounds.

        // Variables.
        public int tickCounter = 0;

        ThingDef_LaserProjectile additionalParameters
        {
            get
            {
                return def as ThingDef_LaserProjectile;
            }
        }

        // Draw variables.
        public const float preFiringInitialIntensity = 0f;
        public const float preFiringFinalIntensity = 1f;
        public const float postFiringInitialIntensity = 1f;
        public const float postFiringFinalIntensity = 0f;
        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        public Vector3 drawingScale;
        public Vector3 drawingPosition;
        public float drawingIntensity = 0f;
        public Material drawingTexture = null;

        // ===================== Setup Work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Log.Message("SpawnSetup");
            Log.Message("additionalParameters.preFiringDuration = " + additionalParameters.preFiringDuration);
            Log.Message("additionalParameters.postFiringDuration = " + additionalParameters.postFiringDuration);

        }

        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.tickCounter, "tickCounter", 0);

            /*if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                GetParametersFromXml();
            }*/
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Main projectile sequence.
        /// </summary>
        public override void Tick()
        {
            /*base.Tick();
            Log.Message("this.HitFlags = " + this.HitFlags);*/
            
            // TODO: check it is not called when loading a savegame with existing lasers.
            if (this.tickCounter == 0)
            {
                PerformPreFiringTreatment();
            }

            if (this.tickCounter == this.additionalParameters.preFiringDuration)
            {
                Log.Message("TODO: ApplyDamage");
            }
            ComputeDrawingParameters();

            if (this.tickCounter == (this.additionalParameters.preFiringDuration + this.additionalParameters.postFiringDuration))
            {
                this.Destroy(DestroyMode.Vanish);
            }

            /*if (this.launcher is Pawn)
            {
                Pawn launcherPawn = this.launcher as Pawn;
                if (((launcherPawn.stances.curStance is Stance_Warmup) == false)
                    && ((launcherPawn.stances.curStance is Stance_Cooldown) == false))
                {
                    this.Destroy(DestroyMode.Vanish);
                }
            }*/

            this.tickCounter++;
        }

        /// <summary>
        /// Determines impact position and initialize drawing parameters.
        /// </summary>
        public virtual void PerformPreFiringTreatment()
        {
            // We use grounded origin position to get rid of y value in magnitude calculation.
            this.origin.y = 0f;
            DetermineImpactExactPosition();
            Vector3 unitVector = (this.destination - this.origin).normalized;
            Vector3 laserRayVector = this.destination - this.origin;
            Vector3 cannonMouthOffset = laserRayVector.normalized * 0.9f;
            this.drawingScale = new Vector3(1f, 1f, laserRayVector.magnitude - cannonMouthOffset.magnitude);
            this.drawingPosition = this.origin + cannonMouthOffset + (laserRayVector - cannonMouthOffset) * 0.5f + Vector3.up * this.def.Altitude;
            this.drawingMatrix.SetTRS(this.drawingPosition, laserRayVector.AngleFlat().ToQuat(), this.drawingScale);
            this.Position = (this.origin + laserRayVector * 0.5f).ToIntVec3();
            // TODO: check out of bounds case.

            MoteMaker.ThrowMicroSparks(this.destination, this.Map);


            /*Log.Message("this.origin = " + this.origin);
            Log.Message("originGrounded = " + originGrounded);
            Log.Message("this.destination = " + this.destination);
            Log.Message("unitVector = " + unitVector);
            Log.Message("laserRayVector = " + laserRayVector);
            Log.Message("laserRayVector.magnitude = " + laserRayVector.magnitude);
            Log.Message("this.drawingScale = " + this.drawingScale);
            Log.Message("this.drawingPosition = " + this.drawingPosition);*/
        }


        /// <summary>
        /// Checks for free intercept targets (cover, neutral animal, pawn) along the trajectory.
        /// </summary>
        protected void DetermineImpactExactPosition()
        {
            Log.Message("DetermineImpactExactPosition");

            // We split the trajectory into small segments of approximatively 1 cell size.
            Vector3 trajectory = (this.destination - this.origin);
            int numberOfSegments = (int)trajectory.magnitude;
            Vector3 trajectorySegment = (trajectory / trajectory.magnitude);

            Vector3 lastValidPosition = this.origin; // Last valid tested position in case of an out of boundaries shot.
            Vector3 nextPosition = this.origin;
            IntVec3 testedPosition = nextPosition.ToIntVec3();

            for (int segmentIndex = 1; segmentIndex <= numberOfSegments; segmentIndex++)
            {

                nextPosition += trajectorySegment;
                testedPosition = nextPosition.ToIntVec3();

                if (!nextPosition.InBounds(this.Map))
                {
                    this.destination = lastValidPosition;
                    break;
                }

                if (CheckForFreeInterceptBetween(lastValidPosition, nextPosition))
                {
                    Log.Message("Impact between " + lastValidPosition + " and " + nextPosition);
                    this.destination = nextPosition;
                    return;
                }
                lastValidPosition = nextPosition;
            }
        }
        
        private static List<IntVec3> checkedCells = new List<IntVec3>();
        private bool CheckForFreeInterceptBetween(Vector3 lastExactPos, Vector3 newExactPos)
        {
            IntVec3 intVec = lastExactPos.ToIntVec3();
            IntVec3 intVec2 = newExactPos.ToIntVec3();
            if (intVec2 == intVec)
            {
                return false;
            }
            if (!intVec.InBounds(base.Map) || !intVec2.InBounds(base.Map))
            {
                return false;
            }
            if (intVec2.AdjacentToCardinal(intVec))
            {
                return CheckForFreeIntercept(intVec2);
            }
            if (VerbUtility.InterceptChanceFactorFromDistance(origin, intVec2) <= 0f)
            {
                return false;
            }
            Vector3 vect = lastExactPos;
            Vector3 v = newExactPos - lastExactPos;
            Vector3 vector = v.normalized * 0.2f;
            int num = (int)(v.MagnitudeHorizontal() / 0.2f);
            checkedCells.Clear();
            int num2 = 0;
            IntVec3 intVec3;
            do
            {
                vect += vector;
                intVec3 = vect.ToIntVec3();
                if (!checkedCells.Contains(intVec3))
                {
                    if (CheckForFreeIntercept(intVec3))
                    {
                        return true;
                    }
                    checkedCells.Add(intVec3);
                }
                num2++;
                if (num2 > num)
                {
                    return false;
                }
            }
            while (!(intVec3 == intVec2));
            return false;
        }

        private bool CheckForFreeIntercept(IntVec3 c)
        {
            if (destination.ToIntVec3() == c)
            {
                return false;
            }
            float num = VerbUtility.InterceptChanceFactorFromDistance(origin, c);
            if (num <= 0f)
            {
                return false;
            }
            bool flag = false;
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (!CanHit(thing))
                {
                    continue;
                }
                bool flag2 = false;
                if (thing.def.Fillage == FillCategory.Full)
                {
                    Building_Door building_Door = thing as Building_Door;
                    if (building_Door == null || !building_Door.Open)
                    {
                        ThrowDebugText("int-wall", c);
                        Impact(thing);
                        return true;
                    }
                    flag2 = true;
                }
                float num2 = 0f;
                Pawn pawn = thing as Pawn;
                if (pawn != null)
                {
                    num2 = 0.4f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
                    if (pawn.GetPosture() != 0)
                    {
                        num2 *= 0.1f;
                    }
                    if (launcher != null && pawn.Faction != null && launcher.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
                    {
                        num2 *= 0.4f;
                    }
                }
                else if (thing.def.fillPercent > 0.2f)
                {
                    num2 = (flag2 ? 0.05f : ((!DestinationCell.AdjacentTo8Way(c)) ? (thing.def.fillPercent * 0.15f) : (thing.def.fillPercent * 1f)));
                }
                num2 *= num;
                if (num2 > 1E-05f)
                {
                    if (Rand.Chance(num2))
                    {
                        ThrowDebugText("int-" + num2.ToStringPercent(), c);
                        Impact(thing);
                        return true;
                    }
                    flag = true;
                    ThrowDebugText(num2.ToStringPercent(), c);
                }
            }
            if (!flag)
            {
                ThrowDebugText("o", c);
            }
            return false;
        }

        private void ThrowDebugText(string text, IntVec3 c)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), base.Map, text);
            }
        }

        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(base.launcher, hitThing, intendedTarget.Thing, base.equipmentDef, def, targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                DamageDef damageDef = def.projectile.damageDef;
                float amount = base.DamageAmount;
                float armorPenetration = base.ArmorPenetration;
                Vector3 eulerAngles = ExactRotation.eulerAngles;
                float y = eulerAngles.y;
                Thing launcher = base.launcher;
                ThingDef equipmentDef = base.equipmentDef;
                DamageInfo dinfo = new DamageInfo(damageDef, amount, armorPenetration, y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                Pawn pawn = hitThing as Pawn;
                if (pawn != null && pawn.stances != null && pawn.BodySize <= def.projectile.StoppingPower + 0.001f)
                {
                    pawn.stances.StaggerFor(95);
                }
            }
        }
        
        // ===================== Draw =====================
        public void ComputeDrawingParameters()
        {
            if (this.tickCounter <= this.additionalParameters.preFiringDuration)
            {
                if (this.additionalParameters.preFiringDuration > 0)
                {
                    this.drawingIntensity = preFiringInitialIntensity + (preFiringFinalIntensity - preFiringInitialIntensity) * (float)this.tickCounter / (float)this.additionalParameters.preFiringDuration;
                }
            }
            else
            {
                if (this.additionalParameters.postFiringDuration > 0)
                {
                    this.drawingIntensity = postFiringInitialIntensity + (postFiringFinalIntensity - postFiringInitialIntensity) * (((float)this.tickCounter - (float)this.additionalParameters.preFiringDuration) / (float)this.additionalParameters.postFiringDuration);
                }
            }
        }
        
        public override void Draw()
        {
            Graphics.DrawMesh(MeshPool.plane10, this.drawingMatrix, FadedMaterialPool.FadedVersionOf(this.def.DrawMatSingle, this.drawingIntensity), 0);
            Comps_PostDraw();
        }
    }
}