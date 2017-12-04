using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;


namespace Spaceship
{
    /// <summary>
    /// Order a pawn to trade with a cargo spaceship.
    /// </summary>
    public class JobDriver_TradeWithCargoSpaceship : JobDriver
    {
        public TargetIndex cargoSpaceshipIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.TargetThingA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_SpaceshipCargo cargoSpaceship = this.TargetThingA as Building_SpaceshipCargo;
            
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOn(delegate ()
            {
                return (cargoSpaceship.DestroyedOrNull()
                || (cargoSpaceship.CanTradeNow == false));
            });

            Toil faceSpaceshipToil = new Toil()
            {
                initAction = () =>
                {
                    this.GetActor().rotationTracker.FaceCell(cargoSpaceship.Position);
                }
            };
            yield return faceSpaceshipToil;

            Toil tradeWithSpacehipToil = new Toil()
            {
                initAction = () =>
                {
                    Find.WindowStack.Add(new Dialog_Trade(this.GetActor(), cargoSpaceship as ITrader));
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return tradeWithSpacehipToil;

            yield return Toils_Reserve.Release(cargoSpaceshipIndex);
        }
    }
}
