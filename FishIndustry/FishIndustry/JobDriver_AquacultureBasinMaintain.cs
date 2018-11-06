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
//using RimWorld.SquadAI;

namespace FishIndustry
{
    /// <summary>
    /// Order a pawn to go and maintain an aquaculture basin.
    /// </summary>
    public class JobDriver_AquacultureBasinMaintain : JobDriver
    {
        public TargetIndex aquacultureBasinIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.TargetA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_AquacultureBasin aquacultureBasin = this.TargetThingA as Building_AquacultureBasin;
            yield return Toils_Goto.GotoThing(aquacultureBasinIndex, PathEndMode.InteractionCell).FailOn(() => aquacultureBasin.powerComp.PowerOn == false);

            yield return Toils_General.Wait(600).WithProgressBarToilDelay(aquacultureBasinIndex).FailOn(() => aquacultureBasin.powerComp.PowerOn == false);

            Toil maintainAquacultureBasin = new Toil()
            {
                initAction = () =>
                {
                    aquacultureBasin.Notify_MaintenanceDone();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return maintainAquacultureBasin;
        }
    }
}
