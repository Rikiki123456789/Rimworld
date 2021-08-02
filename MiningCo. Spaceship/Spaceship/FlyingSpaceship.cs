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
        public Vector3 spaceshipShadowExactPosition = Vector3.zero;
        public float spaceshipExactRotation = 0f;
        public SpaceshipKind spaceshipKind = SpaceshipKind.CargoPeriodic;

        // Draw.
        public static Vector3 supplySpaceshipScale = new Vector3(11f, 1f, 20f);
        public static Vector3 medicalSpaceshipScale = new Vector3(7f, 1f, 11f);
        public static Material supplySpaceshipTexture = MaterialPool.MatFrom("Things/SupplySpaceship/SupplySpaceship");
        public static Material dispatcherTexture = MaterialPool.MatFrom("Things/Dispatcher/DispatcherFlying");
        public static Material medicalSpaceshipTexture = MaterialPool.MatFrom("Things/MedicalSpaceship/MedicalSpaceship");
        public static Material strikeshipTexture = MaterialPool.MatFrom("Things/StrikeShip/StrikeShip");
        public static Material supplySpaceshipShadowTexture = MaterialPool.MatFrom("Things/SupplySpaceship/SupplySpaceshipShadow", ShaderDatabase.Transparent);
        public static Material medicalSpaceshipShadowTexture = MaterialPool.MatFrom("Things/MedicalSpaceship/MedicalSpaceshipShadow", ShaderDatabase.Transparent);

        public Material spaceshipTexture = null;
        public Material spaceshipShadowTexture = null;
        public Matrix4x4 spaceshipMatrix = default(Matrix4x4);
        public Matrix4x4 spaceshipShadowMatrix = default(Matrix4x4);
        public Vector3 baseSpaceshipScale = new Vector3(1f, 1f, 1f);
        public Vector3 spaceshipScale = new Vector3(11f, 1f, 20f);
        public Vector3 spaceshipShadowScale = new Vector3(11f, 1f, 20f);

        public override Vector3 DrawPos
        {
            get
            {
                return this.spaceshipExactPosition;
            }
        }

        public Vector3 ShadowDrawPos
        {
            get
            {
                return this.spaceshipShadowExactPosition;
            }
        }

        // ===================== Setup work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad)
            {
                ConfigureShipTexture(this.spaceshipKind);
            }
        }

        public void ConfigureShipTexture(SpaceshipKind spaceshipKind)
        {
            switch (spaceshipKind)
            {
                case SpaceshipKind.CargoPeriodic:
                case SpaceshipKind.CargoRequested:
                case SpaceshipKind.Damaged:
                    this.spaceshipTexture = supplySpaceshipTexture;
                    this.spaceshipShadowTexture = supplySpaceshipShadowTexture;
                    this.baseSpaceshipScale = supplySpaceshipScale;
                    break;
                case SpaceshipKind.DispatcherDrop:
                case SpaceshipKind.DispatcherPick:
                    this.spaceshipTexture = dispatcherTexture;
                    this.spaceshipShadowTexture = supplySpaceshipShadowTexture;
                    this.baseSpaceshipScale = supplySpaceshipScale;
                    break;
                case SpaceshipKind.Medical:
                    this.spaceshipTexture = medicalSpaceshipTexture;
                    this.spaceshipShadowTexture = medicalSpaceshipShadowTexture;
                    this.baseSpaceshipScale = medicalSpaceshipScale;
                    break;
                case SpaceshipKind.Airstrike:
                    this.spaceshipTexture = strikeshipTexture;
                    this.spaceshipShadowTexture = supplySpaceshipShadowTexture;
                    this.baseSpaceshipScale = supplySpaceshipScale;
                    break;
                default:
                    Log.ErrorOnce("MiningCo. Spaceship: unhandled SpaceshipKind (" + this.spaceshipKind.ToString() + ").", 123456784);
                    break;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector3>(ref this.spaceshipExactPosition, "spaceshipExactPosition");
            Scribe_Values.Look<Vector3>(ref this.spaceshipShadowExactPosition, "spaceshipShadowExactPosition");
            Scribe_Values.Look<float>(ref this.spaceshipExactRotation, "spaceshipExactRotation");
            Scribe_Values.Look<SpaceshipKind>(ref this.spaceshipKind, "spaceshipKind");
            Scribe_Values.Look<Vector3>(ref this.spaceshipScale, "spaceshipScale");
            Scribe_Values.Look<Vector3>(ref this.spaceshipShadowScale, "spaceshipShadowScale");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            ComputeShipExactPosition();
            ComputeShipShadowExactPosition();
            ComputeShipExactRotation();
            ComputeShipScale();
            SetShipPositionToBeSelectable();
        }

        public abstract void ComputeShipExactPosition();
        public abstract void ComputeShipShadowExactPosition();
        public abstract void ComputeShipExactRotation();
        public abstract void ComputeShipScale();
        public abstract void SetShipPositionToBeSelectable();

        public bool IsInBounds()
        {
            bool isInBounds = this.DrawPos.ToIntVec3().InBounds(this.Map)
                && this.DrawPos.ToIntVec3().x >= 10 && this.DrawPos.ToIntVec3().x < this.Map.Size.x - 10
                && this.DrawPos.ToIntVec3().z >= 10 && this.DrawPos.ToIntVec3().z < this.Map.Size.z - 10;
            return isInBounds;
        }

        // ===================== Draw =====================
        public override void Draw()
        {
            this.spaceshipMatrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, this.spaceshipExactRotation.ToQuat(), this.spaceshipScale);
            Graphics.DrawMesh(MeshPool.plane10, this.spaceshipMatrix, this.spaceshipTexture, 0);
            this.spaceshipShadowMatrix.SetTRS(this.ShadowDrawPos + Altitudes.AltIncVect, this.spaceshipExactRotation.ToQuat(), this.spaceshipShadowScale);
            Graphics.DrawMesh(MeshPool.plane10, this.spaceshipShadowMatrix, FadedMaterialPool.FadedVersionOf(this.spaceshipShadowTexture, 0.4f * GenCelestial.CurShadowStrength(this.Map)), 0);
        }
    }
}
