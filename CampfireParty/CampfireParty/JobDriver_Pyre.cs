using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound
using RimWorld;      // RimWorld specific functions are found here
//using RimWorld.SquadAI;

namespace CampfireParty
{
    /// <summary>
    /// Abstract JobDriver only used to centralize common toils.
    /// </summary>
    public class JobDriver_Pyre : JobDriver
    {
        public TargetIndex pyreIndex = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Log.Error("CampfireParty: cannot instantiate a JobDriver_Pyre. This is an 'abstract' JobDriver. You must inherit from it!");
            throw new NotImplementedException();
        }

        protected Toil ToilGetWanderCell(IntVec3 pyrePosition)
        {
            Toil toil = new Toil()
            {
                initAction = () =>
                {
                    IntVec3 cell;
                    bool validCellIsFound = CellFinder.TryFindRandomReachableCellNear(pyrePosition, Building_Pyre.partyAreaRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.None), new Predicate<IntVec3>(this.IsValidCellToWander), null, out cell);
                    if (validCellIsFound)
                    {
                        this.CurJob.targetB = cell;
                    }
                    else
                    {
                        this.CurJob.targetB = this.pawn.Position;
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            return toil;
        }

        protected bool IsValidCellToWander(IntVec3 cell)
        {
            if (cell.Standable() == false)
            {
                return false;
            }
            if (this.pawn.CanReach(new TargetInfo(cell), PathEndMode.OnCell, Danger.None) == false)
            {
                return false;
            }
            foreach (Thing thing in cell.GetThingList())
            {
                if (thing is Fire)
                {
                    return false;
                }
            }
            if (cell.GetRoom() != this.TargetLocA.GetRoom())
            {
                return false;
            }
            if (Find.PawnDestinationManager.DestinationIsReserved(cell))
            {
                return false;
            }
            return true;
        }

        protected Toil ToilReleaseCell()
        {
            Toil releaseCell = new Toil()
            {
                initAction = () =>
                {
                    Find.PawnDestinationManager.UnreserveAllFor(this.pawn);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            return releaseCell;
        }

    }
}
