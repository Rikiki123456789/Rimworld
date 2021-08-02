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
    /// Order a pawn to go and operate a laser fence console.
    /// </summary>
    public class JobDriver_OperateLaserFenceConsole : JobDriver
    {
        public TargetIndex consoleIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.TargetA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnBurningImmobile(consoleIndex);
            this.FailOnDespawnedNullOrForbidden(consoleIndex);

            yield return Toils_Goto.GotoCell(consoleIndex, PathEndMode.InteractionCell);

            yield return Toils_General.Wait(240).WithProgressBarToilDelay(consoleIndex);

            Toil operateLaserFenceConsoleToil = new Toil()
            {
                initAction = () =>
                {
                    (this.TargetThingA as Building_LaserFenceConsole).Notify_ApplyCachedConfiguration();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return operateLaserFenceConsoleToil;

            yield return Toils_Reserve.Release(consoleIndex);
        }
    }
}
