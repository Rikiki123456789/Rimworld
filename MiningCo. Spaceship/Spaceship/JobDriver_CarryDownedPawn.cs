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
    /// Order a pawn to go and carry a downed one.
    /// </summary>
    public class JobDriver_CarryDownedPawn : JobDriver
    {
        public TargetIndex downedPawnIndex = TargetIndex.A;
        public TargetIndex travelDestCellIndex = TargetIndex.B;

        public override bool TryMakePreToilReservations()
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

            Toil gotoTravelDestToil = Toils_Haul.CarryHauledThingToCell(travelDestCellIndex).FailOn(delegate ()
            {
                return (this.pawn.carryTracker.CarriedThing.DestroyedOrNull()
                    || (this.pawn.CanReach(this.pawn.jobs.curJob.targetB.Cell, PathEndMode.OnCell, Danger.Some) == false));
            });
            yield return gotoTravelDestToil;

            Toil arrivedToil = new Toil();
            arrivedToil.initAction = delegate
            {
                Building_Spaceship spaceship = null;
                List<Thing> thingList = this.pawn.Position.GetThingList(this.pawn.Map);
                foreach (Thing thing in thingList)
                {
                    if (thing is Building_Spaceship)
                    {
                        spaceship = thing as Building_Spaceship;
                        break;
                    }
                }
                Thing carriedPawn = null;
                this.pawn.carryTracker.TryDropCarriedThing(this.pawn.Position, ThingPlaceMode.Near, out carriedPawn);
                if (spaceship != null)
                {
                    spaceship.Notify_PawnBoarding(carriedPawn as Pawn, false);
                }
                else if (this.pawn.Position.CloseToEdge(this.pawn.Map, 5))
                {
                    carriedPawn.Destroy();
                    Util_Faction.AffectFactionGoodwillWithOther(this.pawn.Faction, Faction.OfPlayer, LordJob_MiningCoBase.pawnExitedGoodwillImpact);
                }
            };
            arrivedToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return arrivedToil;
        }
    }
}
