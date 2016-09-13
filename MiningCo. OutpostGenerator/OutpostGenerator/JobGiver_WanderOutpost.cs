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


namespace OutpostGenerator
{
    public class JobGiver_WanderOutpost : JobGiver_Wander
    {
        public JobGiver_WanderOutpost()
        {
            Log.Message("JobGiver_WanderOutpost");
            this.wanderRadius = 10f;
            this.ticksBetweenWandersRange = new IntRange(125, 200);
            this.locomotionUrgency = LocomotionUrgency.Walk;
            this.wanderDestValidator = delegate (Pawn pawn, IntVec3 loc)
            {
                Area outpostArea = OG_Util.FindOutpostArea();
                return ((outpostArea != null)
                    && outpostArea.ActiveCells.Contains(loc));
            };
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Area outpostArea = OG_Util.FindOutpostArea();
            if (outpostArea == null)
            {
                return null;
            }
            if (outpostArea.ActiveCells.Count() == 0)
            {
                return null;
            }
            return base.TryGiveJob(pawn);
        }

        protected override IntVec3 GetExactWanderDest(Pawn pawn)
        {
            Area outpostArea = OG_Util.FindOutpostArea();
            if ((outpostArea != null)
                && outpostArea.ActiveCells.Contains(pawn.Position))
            {
                return RCellFinder.RandomWanderDestFor(pawn, pawn.Position, this.wanderRadius, this.wanderDestValidator, PawnUtility.ResolveMaxDanger(pawn, this.maxDanger));
            }
            else
            {
                Building_OutpostCommandConsole console = OG_Util.FindOutpostCommandConsole(OG_Util.FactionOfMiningCo);
                if (console != null)
                {
                    IntVec3 cell1 = WanderUtility.BestCloseWanderRoot(console.Position, pawn);
                    return cell1;
                }
                else
                {
                    for (int cellIndex = 0; cellIndex < 50; cellIndex++)
                    {
                        IntVec3 cell2 = outpostArea.ActiveCells.RandomElement();
                        if (pawn.CanReserveAndReach(cell2, PathEndMode.Touch, Danger.Some))
                        {
                            return cell2;
                        }
                    }
                    IntVec3 cell3 = WanderUtility.BestCloseWanderRoot(pawn.Position, pawn);
                    return cell3;
                }
            }
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            return pawn.Position;
        }
    }
}
