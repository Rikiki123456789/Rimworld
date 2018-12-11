using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;


namespace Spaceship
{
    /// <summary>
    /// Order a pawn to go and carry a downed pawn to a medical spaceship.
    /// </summary>
    public class JobDriver_TransferToMedibay : JobDriver
    {
        public TargetIndex downedPawnIndex = TargetIndex.A;
        public TargetIndex medicalSPaceshipCellIndex = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.TargetThingA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Pawn downedPawn = this.TargetThingA as Pawn;

            Toil gotoDownedPawnToil = Toils_Goto.GotoCell(downedPawnIndex, PathEndMode.OnCell).FailOn(delegate ()
            {
                return (downedPawn.DestroyedOrNull()
                    || (downedPawn.Downed == false));
            });
            yield return gotoDownedPawnToil;

            yield return Toils_Haul.StartCarryThing(downedPawnIndex);

            Toil gotoTravelDestToil = Toils_Haul.CarryHauledThingToCell(medicalSPaceshipCellIndex).FailOn(delegate ()
            {
                return (this.pawn.carryTracker.CarriedThing.DestroyedOrNull()
                    || (this.pawn.CanReach(this.pawn.jobs.curJob.targetB.Cell, PathEndMode.OnCell, Danger.Some) == false));
            });
            yield return gotoTravelDestToil;

            Toil arrivedToil = new Toil()
            {
                initAction = () =>
                {
                    Thing carriedPawn = null;
                    this.pawn.carryTracker.TryDropCarriedThing(this.pawn.Position, ThingPlaceMode.Near, out carriedPawn);
                    Building_SpaceshipMedical medicalSpaceship = this.pawn.Position.GetFirstThing(this.pawn.Map, Util_Spaceship.SpaceshipMedical) as Building_SpaceshipMedical;
                    if (medicalSpaceship != null)
                    {
                        if (medicalSpaceship.orbitalHealingPawnsAboardCount >= Building_SpaceshipMedical.orbitalHealingPawnsAboardMaxCount)
                        {
                            Messages.Message((carriedPawn as Pawn).Name.ToStringShort + " cannot board MiningCo. medical spaceship.. There is no more any free slot.", carriedPawn, MessageTypeDefOf.RejectInput);
                        }
                        else if (TradeUtility.ColonyHasEnoughSilver(this.pawn.Map, Util_Spaceship.orbitalHealingCost))
                        {
                            TradeUtility.LaunchSilver(this.pawn.Map, Util_Spaceship.orbitalHealingCost);
                            medicalSpaceship.Notify_PawnBoarding(carriedPawn as Pawn, false);
                        }
                        else
                        {
                            Messages.Message((carriedPawn as Pawn).Name.ToStringShort + " cannot board MiningCo. medical spaceship.. You have not enough silver to pay for " + this.pawn.gender.GetPossessive() + " orbital healing.", carriedPawn, MessageTypeDefOf.RejectInput);
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return arrivedToil;
        }
    }
}
