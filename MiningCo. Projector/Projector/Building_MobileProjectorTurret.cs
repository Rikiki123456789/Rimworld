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
    /// Building_MobileProjectorTurret class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    [StaticConstructorOnStartup]
    public class Building_MobileProjectorTurret : Building_MobileProjector
    {
        // ===================== Main Work Function =====================
        /// <summary>
        /// No additional conditions.
        /// </summary>
        public override bool CheckAdditionalConditions()
        {
            return true;
        }

        // ===================== Utility Function =====================
        /// <summary>
        /// Try light the target position.
        /// </summary>
        public override void TryLightTarget(IntVec3 targetPosition)
        {
            if (targetPosition.CanBeSeenOver(this.Map)
                && GenSight.LineOfSight(this.Position, targetPosition, this.Map))
            {
                SwitchOnLight(targetPosition);
            }
            else
            {
                IntVec3 farthestPosition = TryGetFarthestPositionInSight(targetPosition);
                if (farthestPosition.IsValid)
                {
                    SwitchOnLight(farthestPosition);
                }
                else
                {
                    SwitchOffLight();
                }
            }
        }

        /// <summary>
        /// Check if a pawn is a valid target.
        /// </summary>
        public override bool IsPawnValidTarget(Pawn pawn)
        {
            if (pawn.HostileTo(this.Faction)
                && (pawn.Downed == false)
                && pawn.Position.InHorDistOf(this.Position, this.def.specialDisplayRadius)
                && GenSight.LineOfSight(this.Position, pawn.Position, this.Map))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the farthest position from the projector in direction of target.
        /// </summary>
        public IntVec3 TryGetFarthestPositionInSight(IntVec3 target)
        {
            IntVec3 farthestPosition = this.Position;

            Mathf.Clamp(target.x, 0, this.Map.Size.x);
            Mathf.Clamp(target.z, 0, this.Map.Size.z);

            IEnumerable<IntVec3> lineOfSightPoints = GenSight.PointsOnLineOfSight(this.Position, target);
            foreach (IntVec3 point in lineOfSightPoints)
            {
                if (point.InBounds(this.Map) == false)
                {
                    farthestPosition = IntVec3.Invalid;
                    break;
                }
                if (point.CanBeSeenOver(this.Map) == false)
                {
                    // Return last non-blocked position.
                    break;
                }
                farthestPosition = point; // Store last valid point in sight.
            }
            return farthestPosition;
        }
        
        // ===================== Draw =====================
        /// <summary>
        /// Draw the projector and a line to the targeted pawn.
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            projectorMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect, this.projectorRotation.ToQuat(), projectorScale);
            if (this.powerComp.PowerOn)
            {
                Graphics.DrawMesh(MeshPool.plane10, projectorMatrix, projectorOnTexture, 0);
                projectorLightEffectMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect, this.projectorRotation.ToQuat(), projectorLightEffectScale);
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
