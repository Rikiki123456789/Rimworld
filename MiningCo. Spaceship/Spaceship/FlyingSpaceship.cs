using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{

    [StaticConstructorOnStartup]
    public abstract class FlyingSpaceship : Thing
    {
        public Vector3 spaceshipExactPosition = Vector3.zero;
        public float spaceshipExactRotation = 0f;
        public SpaceshipKind spaceshipKind = SpaceshipKind.CargoPeriodic;

        // Draw.
        public Matrix4x4 spaceshipMatrix = default(Matrix4x4);
        public Vector3 spaceshipScale = new Vector3(11f, 1f, 20f);
        public override Vector3 DrawPos
        {
            get
            {
                return this.spaceshipExactPosition;
            }
        }

        // ===================== Setup work =====================
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector3>(ref this.spaceshipExactPosition, "spaceshipExactPosition");
            Scribe_Values.Look<float>(ref this.spaceshipExactRotation, "spaceshipExactRotation");
            Scribe_Values.Look<SpaceshipKind>(ref this.spaceshipKind, "spaceshipKind");
            Scribe_Values.Look<Vector3>(ref this.spaceshipScale, "spaceshipScale");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            ComputeShipExactPosition();
            ComputeShipExactRotation();
            ComputeShipScale();
            SetShipVisibleAboveFog();
        }

        public abstract void ComputeShipExactPosition();
        public abstract void ComputeShipExactRotation();
        public abstract void ComputeShipScale();
        public abstract void SetShipVisibleAboveFog();

        public bool IsInBoundsAndVisible()
        {
            bool isInBoundsAndVisible = this.DrawPos.ToIntVec3().InBounds(this.Map)
                && (this.Map.fogGrid.IsFogged(this.DrawPos.ToIntVec3()) == false)
                && this.DrawPos.ToIntVec3().x >= 10
                && this.DrawPos.ToIntVec3().z >= 10
                && this.DrawPos.ToIntVec3().x < this.Map.Size.x - 10
                && this.DrawPos.ToIntVec3().z < this.Map.Size.z - 10;
            return isInBoundsAndVisible;
        }

    }
}
