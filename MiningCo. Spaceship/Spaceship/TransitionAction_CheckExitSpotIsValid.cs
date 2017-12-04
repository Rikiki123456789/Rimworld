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
    public class TransitionAction_CheckExitSpotIsValid : TransitionAction
    {
        public override void DoAction(Transition trans)
        {
            Lord lord = trans.target.lord;
            IntVec3 targetDestination = (lord.LordJob as LordJob_MiningCoBase).targetDestination;
            bool needNewExitSpot = false;
            bool isEdgeCell = targetDestination.InBounds(lord.Map)
                && ((targetDestination.x == 0)
                    || (targetDestination.x == lord.Map.Size.x - 1)
                    || (targetDestination.z == 0)
                    || (targetDestination.z == lord.Map.Size.z - 1));
            if (isEdgeCell == false)
            {
                needNewExitSpot = true;
            }
            else
            {
                foreach (Pawn pawn in lord.ownedPawns)
                {
                    if (pawn.CanReach(targetDestination, PathEndMode.OnCell, Danger.Some) == false)
                    {
                        needNewExitSpot = true;
                        break;
                    }
                }
            }

            IntVec3 newTargetDestination = targetDestination;
            if (needNewExitSpot)
            {
                if (Expedition.TryFindRandomExitSpot(lord.Map, lord.ownedPawns.RandomElement().Position, out newTargetDestination) == false)
                {
                    newTargetDestination = CellFinder.RandomEdgeCell(lord.Map);
                }
            }
            (lord.LordJob as LordJob_MiningCoBase).targetDestination = newTargetDestination;
            // Refresh lord toil destination anyway as it may have been initialized with an invalid vector (case of a fallback).
            LordToil_Travel travelToil = trans.target as LordToil_Travel;
            if (travelToil != null)
            {
                travelToil.SetDestination(newTargetDestination);
            }
            LordToil_EscortDownedPawn escortToil = trans.target as LordToil_EscortDownedPawn;
            if (escortToil != null)
            {
                escortToil.SetDestination(newTargetDestination);
            }
        }
    }
}
