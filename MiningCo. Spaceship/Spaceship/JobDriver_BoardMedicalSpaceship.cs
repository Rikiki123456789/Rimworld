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
    /// Order a pawn to board a medical spaceship.
    /// </summary>
    public class JobDriver_BoardMedicalSpaceship : JobDriver
    {
        public TargetIndex medicalSpaceshipIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_SpaceshipMedical medicalSpaceship = this.TargetThingA as Building_SpaceshipMedical;

            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOn(delegate ()
            {
                return medicalSpaceship.DestroyedOrNull();
            });

            yield return Toils_General.Wait(5 * GenTicks.TicksPerRealSecond).WithProgressBarToilDelay(medicalSpaceshipIndex).FailOn(delegate() 
            {
                return medicalSpaceship.DestroyedOrNull();
            });

            Toil boardToil = new Toil()
            {
                initAction = () =>
                {
                    if (medicalSpaceship.orbitalHealingPawnsAboardCount >= Building_SpaceshipMedical.orbitalHealingPawnsAboardMaxCount)
                    {
                        Messages.Message(this.pawn.NameStringShort + " cannot board MiningCo. medical spaceship.. There is no more any free slot.", this.pawn, MessageTypeDefOf.RejectInput);
                    }
                    else if (TradeUtility.ColonyHasEnoughSilver(this.pawn.Map, Util_Spaceship.orbitalHealingCost))
                    {
                        TradeUtility.LaunchSilver(this.Map, Util_Spaceship.orbitalHealingCost);
                        medicalSpaceship.Notify_PawnBoarding(pawn, false);
                    }
                    else
                    {
                        Messages.Message(this.pawn.NameStringShort + " cannot board MiningCo. medical spaceship.. You have not enough silver to pay for its orbital healing.", this.pawn, MessageTypeDefOf.RejectInput);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return boardToil;
        }
    }
}
