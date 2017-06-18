using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace CaveworldFlora
{
    /// <summary>
    /// ClusterPlant_DevilTongue class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    [StaticConstructorOnStartup]
    public class ClusterPlant_DevilTongue : ClusterPlant
    {
        protected const float pawnDetectionRadius = 5f;
        protected const int pawnDetectionPeriodWhenOpenedInTicks = GenTicks.TicksPerRealSecond;
        protected const int pawnDetectionPeriodWhenClosedInTicks = 5 * GenTicks.TicksPerRealSecond;
        protected const int flowerClosingDurationInTicks = 1 * GenTicks.TicksPerRealSecond;
        protected const int flowerOpeningDurationInTicks = 5 * GenTicks.TicksPerRealSecond;

        public enum FlowerState
        {
            closed,
            opening,
            opened,
            closing
        }

        // Flower state.
        public FlowerState flowerState = FlowerState.closed;

        public int nextLongTick = GenTicks.TickLongInterval;
        public int nextNearbyPawnCheckTick = 0;

        public int flowerClosingRemainingTicks = 0;
        public int flowerOpeningTicks = 0;

        // Drawing.
        public static Material flowerTexture = MaterialPool.MatFrom("Things/Plant/DevilTongue/Flower/DevilTongueFlower", ShaderDatabase.Transparent);
        public Matrix4x4 flowerMatrix = default(Matrix4x4);
        public Vector3 flowerScale = new Vector3(0f, 1f, 0f);
        
        // ===================== Saving =====================
        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.nextLongTick, "nextLongTick");
            Scribe_Values.Look<int>(ref this.nextNearbyPawnCheckTick, "nextNearbyPawnCheckTick");
            Scribe_Values.Look<FlowerState>(ref this.flowerState, "flowerState");
            Scribe_Values.Look<int>(ref this.flowerClosingRemainingTicks, "flowerClosingRemainingTicks");
            Scribe_Values.Look<int>(ref this.flowerOpeningTicks, "flowerOpeningTicks");
            Scribe_Values.Look<Vector3>(ref this.flowerScale, "flowerScale");
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - look for nearby pawn: if any is found, close flower and glower.
        /// - when pawn is away, re-open flower after a delay.
        /// </summary>
        public override void Tick()
        {
            if (Find.TickManager.TicksGame >= this.nextLongTick)
            {
                this.nextLongTick = Find.TickManager.TicksGame + GenTicks.TickLongInterval;
                base.TickLong();
            }

            if (base.Destroyed)
            {
                return;
            }
            switch (flowerState)
            {
                case FlowerState.closed:
                    if (this.isInCryostasis)
                    {
                        break;
                    }
                    if (this.growthInt < 0.3f)
                    {
                        break;
                    }
                    LookForNearbyPawnWhenClosed();
                    break;
                case FlowerState.opening:
                    OpenFlower();
                    break;
                case FlowerState.opened:
                    if (this.isInCryostasis)
                    {
                        TransitionToClosing();
                        break;
                    }
                    LookForNearbyPawnWhenOpened();
                    break;
                case FlowerState.closing:
                    CloseFlower();
                    break;
            }
        }

        protected void LookForNearbyPawnWhenClosed()
        {
            if (Find.TickManager.TicksGame >= this.nextNearbyPawnCheckTick)
            {
                if (IsPawnNearby())
                {
                    this.nextNearbyPawnCheckTick = Find.TickManager.TicksGame + pawnDetectionPeriodWhenClosedInTicks;
                }
                else
                {
                    TransitionToOpening();
                }
            }
        }

        protected void TransitionToOpening()
        {
            this.flowerScale = new Vector3(0f, 1f, 0f);
            this.flowerOpeningTicks = 0;
            this.flowerState = FlowerState.opening;
        }

        protected void OpenFlower()
        {
            this.flowerOpeningTicks++;
            if (this.flowerOpeningTicks >= flowerOpeningDurationInTicks)
            {
                TransitionToOpened();
            }
            float scale = ((float)this.flowerOpeningTicks / (float)flowerOpeningDurationInTicks);
            float growthFactor = this.def.plant.visualSizeRange.min + this.growthInt * (this.def.plant.visualSizeRange.max - this.def.plant.visualSizeRange.min);
            scale *= growthFactor;
            this.flowerScale.x = scale;
            this.flowerScale.z = scale;
        }

        protected void TransitionToOpened()
        {
            this.glower = GenSpawn.Spawn(Util_CaveworldFlora.GetGlowerStaticDef(this.def), this.Position, this.Map);
            this.flowerState = FlowerState.opened;
        }

        protected void LookForNearbyPawnWhenOpened()
        {
            if (Find.TickManager.TicksGame >= this.nextNearbyPawnCheckTick)
            {
                if (IsPawnNearby())
                {
                    TransitionToClosing();
                }
                else
                {
                    this.nextNearbyPawnCheckTick = Find.TickManager.TicksGame + pawnDetectionPeriodWhenOpenedInTicks;
                }
            }
        }
        protected bool IsPawnNearby()
        {
            foreach (Pawn pawn in this.Map.mapPawns.AllPawns)
            {
                if (pawn.Position.InHorDistOf(this.Position, pawnDetectionRadius))
                {
                    return true;
                }
            }
            return false;
        }

        protected void TransitionToClosing()
        {
            TryToDestroyGlower();
            this.flowerClosingRemainingTicks = flowerClosingDurationInTicks;
            this.flowerState = FlowerState.closing;
        }

        protected void CloseFlower()
        {
            this.flowerClosingRemainingTicks--;
            if (this.flowerClosingRemainingTicks <= 0)
            {
                TransitionToClosed();
            }
            float scale = ((float)this.flowerClosingRemainingTicks / (float)flowerClosingDurationInTicks);
            this.flowerScale.x = scale;
            this.flowerScale.z = scale;
        }

        protected void TransitionToClosed()
        {
            this.nextNearbyPawnCheckTick = Find.TickManager.TicksGame + pawnDetectionPeriodWhenClosedInTicks;
            this.flowerState = FlowerState.closed;
        }
        
        public override void Draw()
        {
            if (this.flowerState != FlowerState.closed)
            {
                // Draw flower just under body texture.
                this.flowerMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect + new Vector3(0f, -0.1f, 0f), (0f).ToQuat(), this.flowerScale);
                Graphics.DrawMesh(MeshPool.plane10, this.flowerMatrix, flowerTexture, 0);
            }
        }
    }
}
