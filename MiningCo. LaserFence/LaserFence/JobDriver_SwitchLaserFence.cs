using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
//using RimWorld.Planet;


namespace LaserFence
{
    /// <summary>
    /// Order a pawn to go and switch the laser fences of a pylon.
    /// </summary>
    public class JobDriver_SwitchLaserFence : JobDriver
    {
        public TargetIndex pylonIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.TargetA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnBurningImmobile(pylonIndex);
            this.FailOnDespawnedNullOrForbidden(pylonIndex);

            yield return Toils_Goto.GotoThing(pylonIndex, PathEndMode.InteractionCell);

            yield return Toils_General.Wait(60).WithProgressBarToilDelay(pylonIndex);

            Toil switchLaserFenceToil = new Toil()
            {
                initAction = () =>
                {
                    (this.TargetThingA as Building_LaserFencePylon).Notify_ApplyCachedConfiguration();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return switchLaserFenceToil;

            yield return Toils_Reserve.Release(pylonIndex);
        }
    }
}
