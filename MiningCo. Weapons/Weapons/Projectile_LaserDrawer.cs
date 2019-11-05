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
    /// Projectile_LaserDrawer class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class Projectile_LaserDrawer : Thing
    {
        // Variables.
        public int tickCounter = 0;
        ThingDef projectileDef = null;
        Vector3 origin;
        Vector3 destination;

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
        public float exactRotation = 0f;

        ThingDef_LaserProjectile additionalParameters
        {
            get
            {
                return this.projectileDef as ThingDef_LaserProjectile;
            }
        }
        
        // ===================== Setup Work =====================
        public void Initialize(ThingDef projectileDef, Vector3 origin, Vector3 destination)
        {
            this.projectileDef = projectileDef;
            this.origin = origin;
            this.destination = destination;

            InitializeDrawingParameters();
        }

        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.tickCounter, "tickCounter");
            Scribe_Defs.Look<ThingDef>(ref this.projectileDef, "projectileDef");
            Scribe_Values.Look<Vector3>(ref this.origin, "origin");
            Scribe_Values.Look<Vector3>(ref this.destination, "destination");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                InitializeDrawingParameters();
            }
        }
        
        public void InitializeDrawingParameters()
        {
            Vector3 laserRayVector = this.destination - this.origin;
            Vector3 cannonMouthPositionOffset = laserRayVector.normalized * 0.52f;
            this.drawingScale = new Vector3(1f, 1f, laserRayVector.magnitude - cannonMouthPositionOffset.magnitude);
            this.drawingPosition = this.origin + cannonMouthPositionOffset + laserRayVector / 2f + Vector3.up * Altitudes.AltitudeFor(AltitudeLayer.Projectile);
            this.exactRotation = laserRayVector.AngleFlat();
            this.drawingMatrix.SetTRS(this.drawingPosition, laserRayVector.AngleFlat().ToQuat(), this.drawingScale);
            this.Position = this.drawingPosition.ToIntVec3();
        }

        public override void PostMapInit()
        {
            base.PostMapInit();
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Main projectile sequence.
        /// </summary>
        public override void Tick()
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
            this.tickCounter++;
            if (this.tickCounter >= this.additionalParameters.preFiringDuration + this.additionalParameters.postFiringDuration)
            {
                this.Destroy();
            }
        }
        
        public override void Draw()
        {
            if (this.tickCounter == 0)
            {
                // Avoid drawing laser ray before it is initialized (this can happen when Draw is called before Tick).
                return;
            }
            Log.Message("this.tickCounter = " + this.tickCounter + ", this.drawingScale = " + this.drawingScale + ", this.drawingPosition = " + this.drawingPosition);
            Graphics.DrawMesh(MeshPool.plane10, this.drawingMatrix, FadedMaterialPool.FadedVersionOf(this.projectileDef.DrawMatSingle, this.drawingIntensity), 0);
        }
    }
}