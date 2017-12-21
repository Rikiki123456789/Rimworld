using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship
{
    public class LordJob_BoardSpaceship : LordJob_MiningCoBase
    {
        public LordJob_BoardSpaceship()
        {
        }

        public LordJob_BoardSpaceship(IntVec3 targetDestination)
            : base(targetDestination)
        {
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_Travel travelToil = new LordToil_Travel(this.targetDestination);
            stateGraph.StartingToil = travelToil;
            LordToil_DefendPoint defendPointToil = new LordToil_DefendPoint(false);
            stateGraph.AddToil(defendPointToil);
            LordToil_BoardSpaceship boardSpaceshipToil = new LordToil_BoardSpaceship(this.targetDestination, LocomotionUrgency.Walk);
            stateGraph.AddToil(boardSpaceshipToil);
            LordToil_EscortDownedPawn escortDownedPawnPeacefullyToil = new LordToil_EscortDownedPawn(this.targetDestination, LocomotionUrgency.Walk);
            stateGraph.AddToil(escortDownedPawnPeacefullyToil);
            LordToil exitMap = stateGraph.AttachSubgraph(new LordJob_ExitMap(IntVec3.Invalid).CreateGraph()).StartingToil;
            
            // Defend.
            stateGraph.AddTransition(new Transition(travelToil, defendPointToil)
            {
                triggers =
                {
                    new Trigger_PawnHarmed(1f, false)
                },
                preActions =
                {
                    new TransitionAction_SetDefendLocalGroup()
                },
                postActions =
                {
                    new TransitionAction_EndAllJobs()
                }
            });
            // Back to travel after defending.
            stateGraph.AddTransition(new Transition(defendPointToil, travelToil)
            {
                triggers =
                {
                    new Trigger_TicksPassedWithoutHarm(1200)
                }
            });
            // Escort downed pawn peacefully.
            stateGraph.AddTransition(new Transition(travelToil, escortDownedPawnPeacefullyToil)
            {
                triggers =
                {
                    new Trigger_ReachableDownedPawn()
                },
                postActions =
                {
                    new TransitionAction_EndAllJobs()
                }
            });
            // Back to travel after escorting pawn peacefully.
            stateGraph.AddTransition(new Transition(escortDownedPawnPeacefullyToil, travelToil)
            {
                triggers =
                {
                    new Trigger_Memo("RescueEnded")
                }
            });
            // Board dispatcher.
            stateGraph.AddTransition(new Transition(travelToil, boardSpaceshipToil)
            {
                triggers =
                {
                    new Trigger_Memo("TravelArrived")
                },
                postActions =
                {
                    new TransitionAction_EndAllJobs()
                }
            });
            // Exit map.
            stateGraph.AddTransition(new Transition(travelToil, exitMap)
            {
                sources =
                {
                    escortDownedPawnPeacefullyToil,
                    boardSpaceshipToil
                },
                triggers =
                {
                    new Trigger_PawnCannotReachTargetDestination()
                },
                postActions =
                {
                    new TransitionAction_CancelDispatcherPick(),
                    new TransitionAction_EndAllJobs()
                }
            });
            // Exit map. This transition does not include travelToil as the dispatcher is not landed when the team begins travelling.
            stateGraph.AddTransition(new Transition(escortDownedPawnPeacefullyToil, exitMap)
            {
                sources =
                {
                    boardSpaceshipToil
                },
                triggers =
                {
                    new Trigger_SpaceshipNotFound()
                },
                postActions =
                {
                    new TransitionAction_CancelDispatcherPick(),
                    new TransitionAction_EndAllJobs()
                }
            });
            return stateGraph;
        }
    }
}
