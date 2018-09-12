using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;   
using Verse;      
using Verse.Sound;

namespace Projector
{
    /// <summary>
    /// Building_MobileProjectorTower class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    [StaticConstructorOnStartup]
    public class Building_MobileProjectorTower : Building_MobileProjector
    {
        // ===================== Variables =====================
        public bool isRoofed = false;

        // ===================== Setup Work =====================
        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.Look<bool>(ref this.isRoofed, "isRoofed");
        }

        // ===================== Utility Function =====================
        /// <summary>
        /// Update roof state and perform action on state change.
        /// </summary>
        public override bool CheckAdditionalConditions()
        {
            if (this.isRoofed
                && (this.Position.Roofed(this.Map) == false))
            {
                // Becomes unroofed.
                this.isRoofed = false;
                OnPoweredOn();
            }
            else if ((this.isRoofed == false)
                && this.Position.Roofed(this.Map))
            {
                // Becomes roofed.
                this.isRoofed = true;
                OnPoweredOff();
            }
            return (this.isRoofed == false);
        }

        /// <summary>
        /// Try light the target position.
        /// </summary>
        public override void TryLightTarget(IntVec3 targetPosition)
        {
            if (targetPosition.InBounds(this.Map) == false)
            {
                SwitchOffLight();
                return;
            }
            Building building = targetPosition.GetEdifice(this.Map);
            if (targetPosition.Roofed(this.Map)
                || ((building != null)
                    && building.def.Fillage == FillCategory.Full))
            {
                SwitchOffLight();
                return;
            }
            SwitchOnLight(targetPosition);
        }
        
        /// <summary>
        /// Check if a pawn is a valid target.
        /// </summary>
        public override bool IsPawnValidTarget(Pawn pawn)
        {
            if (pawn.Spawned
                && pawn.HostileTo(this.Faction)
                && (pawn.Downed == false)
                && pawn.Position.InHorDistOf(this.Position, this.def.specialDisplayRadius)
                && (pawn.Position.Roofed(pawn.Map) == false))
            {
                return true;
            }
            return false;
        }
        
        // ===================== Draw =====================
        /// <summary>
        /// Draw the projector and a line to the targeted pawn.
        /// The small draw offset (AltitudeLayer.Blueprint) is sused to draw the projector and light above the pawns.
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            projectorMatrix.SetTRS(base.DrawPos + new Vector3(0f, Altitudes.AltitudeFor(AltitudeLayer.Blueprint), 0f) + Altitudes.AltIncVect, this.projectorRotation.ToQuat(), projectorScale);
            if ((this.powerComp.PowerOn)
                && (this.isRoofed == false))
            {
                Graphics.DrawMesh(MeshPool.plane10, projectorMatrix, projectorOnTexture, 0);
                projectorLightEffectMatrix.SetTRS(base.DrawPos + new Vector3(0f, Altitudes.AltitudeFor(AltitudeLayer.Blueprint), 0f) + Altitudes.AltIncVect, this.projectorRotation.ToQuat(), projectorLightEffectScale);
                Graphics.DrawMesh(MeshPool.plane10, projectorLightEffectMatrix, projectorLightEffectTexture, 0);
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, projectorMatrix, projectorOffTexture, 0);
            }

            if (Find.Selector.IsSelected(this)
                && (this.target != null))
            {
                Vector3 lineOrigin = this.TrueCenter();
                Vector3 lineTarget = this.target.Position.ToVector3Shifted();
                lineTarget.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
                lineOrigin.y = lineTarget.y;
                GenDraw.DrawLineBetween(lineOrigin, lineTarget, targetLineTexture);
            }
        }
    }
}
