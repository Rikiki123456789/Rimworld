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
    public class LordJob_ExitMap : LordJob_MiningCoBase
    {
        public bool impactMessageIsSent = false;

        public LordJob_ExitMap()
        {
        }

        public LordJob_ExitMap(IntVec3 targetDestination)
            : base(targetDestination)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.impactMessageIsSent, "impactMessageIsSent");
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_DefendPoint dropToil = new LordToil_DefendPoint(true);
            stateGraph.StartingToil = dropToil; // Just used to verify the exit spot is valid.
            LordToil_Travel travelToil = new LordToil_Travel(this.targetDestination);            
            stateGraph.AddToil(travelToil);
            LordToil_DefendPoint defendPointToil = new LordToil_DefendPoint(false);
            stateGraph.AddToil(defendPointToil);
            LordToil_ExitMap exitMapPeacefullyToil = new LordToil_ExitMap(LocomotionUrgency.Walk, false);
            stateGraph.AddToil(exitMapPeacefullyToil);
            LordToil_EscortDownedPawn escortDownedPawnPeacefullyToil = new LordToil_EscortDownedPawn(this.targetDestination, LocomotionUrgency.Walk);
            stateGraph.AddToil(escortDownedPawnPeacefullyToil);
            LordToil_ExitMap exitMapHurryToil = new LordToil_ExitMap(LocomotionUrgency.Jog, true);
            stateGraph.AddToil(exitMapHurryToil);
            LordToil_DefendPoint defendPointHurryToil = new LordToil_DefendPoint(false);
            stateGraph.AddToil(defendPointHurryToil);
            LordToil_EscortDownedPawn escortDownedPawnHurryToil = new LordToil_EscortDownedPawn(this.targetDestination, LocomotionUrgency.Jog);
            stateGraph.AddToil(escortDownedPawnHurryToil);

            // Begin travel.
            stateGraph.AddTransition(new Transition(dropToil, travelToil)
            {
                triggers =
                {
                    new Trigger_TicksPassedWithoutHarm(1)
                },
                preActions =
                {
                    new TransitionAction_CheckExitSpotIsValid()
                },
                postActions =
                {
                    new TransitionAction_EndAllJobs()
                }
            });
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
                },
                preActions =
                {
                    new TransitionAction_EnsureHaveExitDestination()
                }
            });
            // Escort downed pawn peacefully.
            stateGraph.AddTransition(new Transition(travelToil, escortDownedPawnPeacefullyToil)
            {
                sources =
                {
                    exitMapPeacefullyToil
                },
                triggers =
                {
                    new Trigger_ReachableDownedPawn()
                },
                preActions =
                {
                    new TransitionAction_CheckExitSpotIsValid()
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
            // Exit peacefully.
            stateGraph.AddTransition(new Transition(travelToil, exitMapPeacefullyToil)
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
            // Exit in hurry.
            stateGraph.AddTransition(new Transition(travelToil, exitMapHurryToil)
            {
                sources =
                {
                    exitMapPeacefullyToil,
                    escortDownedPawnPeacefullyToil
                },
                triggers =
                {
                    new Trigger_PawnCannotReachTargetDestination(),
                    new Trigger_HostileToColony()
                },
                postActions =
                {
                    new TransitionAction_EndAllJobs()
                }
            });
            // Defend in hurry.
            stateGraph.AddTransition(new Transition(exitMapHurryToil, defendPointHurryToil)
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
            // Back to exit in hurry after defending.
            stateGraph.AddTransition(new Transition(defendPointHurryToil, exitMapHurryToil)
            {
                triggers =
                {
                    new Trigger_TicksPassedWithoutHarm(1200)
                }
            });
            // Escort downed pawn in hurry.
            stateGraph.AddTransition(new Transition(exitMapHurryToil, escortDownedPawnHurryToil)
            {
                triggers =
                {
                    new Trigger_ReachableDownedPawn()
                },
                preActions =
                {
                    new TransitionAction_CheckExitSpotIsValid()
                },
                postActions =
                {
                    new TransitionAction_EndAllJobs()
                }
            });
            // Back to exit in hurry after escorting pawn in hurry.
            stateGraph.AddTransition(new Transition(escortDownedPawnHurryToil, exitMapHurryToil)
            {
                triggers =
                {
                    new Trigger_Memo("RescueEnded")
                }
            });
            return stateGraph;
        }
    }
}
