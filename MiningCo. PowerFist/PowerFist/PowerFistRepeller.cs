using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;

namespace PowerFist
{
    public class PowerFistRepeller : Thing
    {
        public Pawn targetPawn;
        public IntVec3 repelVector = IntVec3.Zero;
        public float repelDistance = 0;
        public float repelDurationInTicks = 0;
        public float repelTicks = 0;
        public IntVec3 initialRepelPosition = IntVec3.Zero;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.LookReference<Pawn>(ref this.targetPawn, "targetpawn");
            Scribe_Values.LookValue<IntVec3>(ref this.repelVector, "repelVector");
            Scribe_Values.LookValue<float>(ref this.repelDistance, "repelDistance");
            Scribe_Values.LookValue<float>(ref this.repelDurationInTicks, "repelDurationInTicks");
            Scribe_Values.LookValue<float>(ref this.repelTicks, "repelTicks");
            Scribe_Values.LookValue<IntVec3>(ref this.initialRepelPosition, "initialRepelPosition");
        }

        public override void Tick()
        {
            this.repelTicks++;
            if ((this.repelTicks >= this.repelDurationInTicks)
                || (this.targetPawn.Dead))
            {
                if (this.targetPawn.Dead == false)
                {
                    // Avoid teleportation when the stun ends.
                    this.targetPawn.pather.ResetToCurrentPosition();
                }
                this.Destroy();
            }
            else
            {
                // Repel the pawn.
                float repelProgress = this.repelTicks / this.repelDurationInTicks;
                IntVec3 offset = new IntVec3(Mathf.RoundToInt((float)(this.repelVector.x) * this.repelDistance * repelProgress), 0, Mathf.RoundToInt((float)(this.repelVector.z) * this.repelDistance * repelProgress));
                this.targetPawn.Position = this.initialRepelPosition + offset;

                // Stagger pawns on the path.
                Pawn pawn = this.targetPawn.Position.GetFirstPawn(this.Map);
                if (pawn != null)
                {
                    pawn.stances.StaggerFor(2 * GenTicks.TicksPerRealSecond);
                }

                // Generate some dust.
                MoteMaker.ThrowDustPuff(this.targetPawn.DrawPos, this.Map, 1f);
            }
        }

        public void Notify_BeginRepel(Pawn targetPawn, IntVec3 baseRepelVector, float maxRepelDistance, int baseRepelDurationInTicks, out float outRepelDistance, out ThingDef obstacleDef)
        {
            this.targetPawn = targetPawn;
            this.initialRepelPosition = targetPawn.Position;
            this.repelVector = baseRepelVector;
            // Compute repel distance.
            obstacleDef = GetObstacleDefAndComputeRepelDistance(targetPawn.Position, this.repelVector, maxRepelDistance, out this.repelDistance);
            this.repelDurationInTicks = (int)((float)baseRepelDurationInTicks * (this.repelDistance / maxRepelDistance));
            outRepelDistance = this.repelDistance;
            // At least stun the target during repel.
            targetPawn.stances.stunner.StunFor((int)this.repelDurationInTicks);
        }

        // Look for blocking obstacles and terrain on the repel path (walls, high buildings, big pawns, big plants and impassable terrain like deep water).
        protected ThingDef GetObstacleDefAndComputeRepelDistance(IntVec3 startPosition, IntVec3 repelVector, float maxRepelDistance, out float repelDistance)
        {
            IntVec3 checkedPosition = startPosition;
            ThingDef obstacleDef = null;

            repelDistance = 0;
            for (int distance = 1; distance <= maxRepelDistance; distance++)
            {
                checkedPosition = startPosition + repelVector * distance;
                if (checkedPosition.InBounds(this.Map) == false)
                {
                    return null;
                }
                if (IsObstacleBlockingNear(checkedPosition, repelVector, out obstacleDef))
                {
                    return obstacleDef;
                }
                if (IsTerrainBlockingNear(checkedPosition, repelVector))
                {
                    return null;
                }
                // No obstacle, store this temporary valid distance.
                repelDistance = distance;
            }
            return null;
        }

        protected bool IsObstacleBlockingNear(IntVec3 position, IntVec3 repelVector, out ThingDef obstacleDef)
        {
            bool obstacleIsBlockingDiagonalLeft = false;
            bool obstacleIsBlockingDiagonalRight = false;

            obstacleDef = null;
            bool obstacleIsBlocking = IsObstacleBlockingAt(position, out obstacleDef);
            if (obstacleIsBlocking)
            {
                return true;
            }
            if (IsDiagonalVector(repelVector))
            {
                obstacleIsBlockingDiagonalLeft = IsObstacleBlockingAt(GetPositionDiagonalLeft(position, repelVector), out obstacleDef);
                if (obstacleIsBlockingDiagonalLeft)
                {
                    return true;
                }
                obstacleIsBlockingDiagonalRight = IsObstacleBlockingAt(GetPositionDiagonalRight(position, repelVector), out obstacleDef);
                if (obstacleIsBlockingDiagonalRight)
                {
                    return true;
                }
            }
            return false;
        }

        protected bool IsObstacleBlockingAt(IntVec3 position, out ThingDef obstacleDef)
        {
            obstacleDef = null;
            // Look for blocking building.
            Building building = position.GetEdifice(this.Map);
            if (building != null)
            {
                bool doorIsOpen = ((building is Building_Door)
                    && (building as Building_Door).Open);
                bool canPassThroughBuilding = ((building.def.fillPercent < 0.7f)
                    || doorIsOpen);
                if (!canPassThroughBuilding)
                {
                    obstacleDef = building.def;
                    return true;
                }
            }
            // Look for blocking plant.
            Plant plant = position.GetPlant(this.Map);
            bool plantIsBlocking = ((plant != null)
                && (plant.def.fillPercent >= 0.4f)
                && (plant.Growth >= 0.5f));
            if (plantIsBlocking)
            {
                obstacleDef = plant.def;
                return true;
            }
            // Look for blocking pawn.
            Pawn pawn = position.GetFirstPawn(this.Map);
            bool pawnIsBlocking = ((pawn != null)
                && (pawn.BodySize >= 1.5f));
            if (pawnIsBlocking)
            {
                obstacleDef = pawn.def;
                return true;
            }
            return false;
        }

        protected bool IsTerrainBlockingNear(IntVec3 position, IntVec3 repelVector)
        {
            bool terrainIsBlockingDiagonalLeft = false;
            bool terrainIsBlockingDiagonalRight = false;

            bool terrainIsBlocking = IsTerrainBlockingAt(position);
            if (IsDiagonalVector(repelVector))
            {
                terrainIsBlockingDiagonalLeft = IsTerrainBlockingAt(GetPositionDiagonalLeft(position, repelVector));
                terrainIsBlockingDiagonalRight = IsTerrainBlockingAt(GetPositionDiagonalRight(position, repelVector));
            }
            return (terrainIsBlocking
                || terrainIsBlockingDiagonalLeft
                || terrainIsBlockingDiagonalRight);
        }

        protected bool IsTerrainBlockingAt(IntVec3 position)
        {
            bool terrainIsBlocking = (position.GetTerrain(this.Map).passability == Traversability.Impassable);
            return terrainIsBlocking;
        }
        
        protected bool IsDiagonalVector(IntVec3 repelVector)
        {
            if ((repelVector == (IntVec3.North + IntVec3.East))
                || (repelVector == (IntVec3.South + IntVec3.East))
                || (repelVector == (IntVec3.South + IntVec3.West))
                || (repelVector == (IntVec3.North + IntVec3.West)))
            {
                return true;
            }
            return false;
        }

        protected IntVec3 GetPositionDiagonalLeft(IntVec3 position, IntVec3 repelVector)
        {
            IntVec3 positionDiagonalLeft = position;
            if (repelVector == (IntVec3.North + IntVec3.East))
            {
                positionDiagonalLeft += new IntVec3(-1, 0, 0);
            }
            else if (repelVector == (IntVec3.South + IntVec3.East))
            {
                positionDiagonalLeft += new IntVec3(0, 0, 1);
            }
            else if (repelVector == (IntVec3.South + IntVec3.West))
            {
                positionDiagonalLeft += new IntVec3(1, 0, 0);
            }
            else if (repelVector == (IntVec3.North + IntVec3.West))
            {
                positionDiagonalLeft += new IntVec3(0, 0, -1);
            }
            return positionDiagonalLeft;
        }

        protected IntVec3 GetPositionDiagonalRight(IntVec3 position, IntVec3 repelVector)
        {
            IntVec3 positionDiagonalRight = position;
            if (repelVector == (IntVec3.North + IntVec3.East))
            {
                    positionDiagonalRight += new IntVec3(0, 0, -1);
            }
            else if (repelVector == (IntVec3.South + IntVec3.East))
            {
                    positionDiagonalRight += new IntVec3(-1, 0, 0);
            }
            else if (repelVector == (IntVec3.South + IntVec3.West))
            {
                    positionDiagonalRight += new IntVec3(0, 0, 1);
            }
            else if (repelVector == (IntVec3.North + IntVec3.West))
            {
                    positionDiagonalRight += new IntVec3(1, 0, 0);
            }
            return positionDiagonalRight;
        }
    }
}
