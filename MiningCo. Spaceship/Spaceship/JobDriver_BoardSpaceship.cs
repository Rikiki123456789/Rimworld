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
    /// Order a pawn to board a spaceship.
    /// </summary>
    public class JobDriver_BoardSpaceship : JobDriver
    {
        public TargetIndex spaceshipIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_Spaceship spaceship = this.TargetThingA as Building_Spaceship;

            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOn(delegate ()
            {
                return spaceship.DestroyedOrNull();
            });

            Toil boardToil = new Toil()
            {
                initAction = () =>
                {
                    bool isLastLordPawn = false;
                    Lord lord = pawn.GetLord();
                    if (lord != null)
                    {
                        if (lord.ownedPawns.Count == 1)
                        {
                            isLastLordPawn = true;
                        }
                        lord.Notify_PawnLost(pawn, PawnLostCondition.ExitedMap);
                    }
                    spaceship.Notify_PawnBoarding(pawn, isLastLordPawn);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return boardToil;
        }
    }
}
