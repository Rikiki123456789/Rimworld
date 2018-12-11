using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;    // Always needed
using RimWorld;       // RimWorld specific functions are found here
using Verse;          // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI
using Verse.AI.Group; // Needed when you do something with the AI
using Verse.Sound;    // Needed when you do something with the Sound

namespace Spaceship
{
    public class Trigger_ReachableDownedPawn : Trigger
    {
        public const int checkInterval = GenTicks.TicksPerRealSecond + 1;

        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            if ((signal.type == TriggerSignalType.Tick)
                && (Find.TickManager.TicksGame % checkInterval == 0))
            {
                IntVec3 targetDestination = (lord.LordJob as LordJob_MiningCoBase).targetDestination;
                // Look for a reachable unreserved downed pawn.
                if (lord.ownedPawns.NullOrEmpty())
                {
                    lord.Cleanup();
                    return false;
                }
                Pawn pawnToRescue = Util_DownedPawn.GetRandomReachableDownedPawn(lord.ownedPawns.RandomElement());
                if (pawnToRescue == null)
                {
                    return false;
                }
                // Check all lord pawns can reach downed pawn.
                foreach (Pawn pawn in lord.ownedPawns)
                {
                    if (pawn.CanReserveAndReach(pawnToRescue, PathEndMode.OnCell, Danger.Some) == false)
                    {
                        return false;
                    }
                }
                // Check all lord pawns can reach target destination.
                bool targetDestinationIsReachable = true;
                foreach (Pawn pawn in lord.ownedPawns)
                {
                    if (pawn.CanReach(targetDestination, PathEndMode.OnCell, Danger.Some) == false)
                    {
                        targetDestinationIsReachable = false;
                        break;
                    }
                }
                if (targetDestinationIsReachable)
                {
                    return true;
                }
                // Try to find a new exit spot.
                IntVec3 exitSpot = IntVec3.Invalid;
                bool exitSpotIsValid = Expedition.TryFindRandomExitSpot(lord.Map, lord.ownedPawns.RandomElement().Position, out exitSpot);
                if (exitSpotIsValid)
                {
                    targetDestinationIsReachable = true;
                    foreach (Pawn pawn in lord.ownedPawns)
                    {
                        if (pawn.CanReach(exitSpot, PathEndMode.OnCell, Danger.Some) == false)
                        {
                            targetDestinationIsReachable = false;
                            break;
                        }
                    }
                }
                return targetDestinationIsReachable;
            }
            return false;
        }
    }
}
