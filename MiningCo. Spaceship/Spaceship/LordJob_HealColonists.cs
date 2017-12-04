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
    public class LordJob_HealColonists : LordJob_MiningCoBase
    {
        public LordJob_HealColonists()
        {
        }

        public LordJob_HealColonists(IntVec3 targetDestination)
            : base(targetDestination)
		{
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_HealColonists healToil = new LordToil_HealColonists(this.targetDestination);
            stateGraph.StartingToil = healToil;
            LordToil boardSpaceshipToil = stateGraph.AttachSubgraph(new LordJob_BoardSpaceship(this.targetDestination).CreateGraph()).StartingToil;

            // Return to spaceship.
            stateGraph.AddTransition(new Transition(healToil, boardSpaceshipToil)
            {
                triggers =
                {
                    new Trigger_Memo("HealFinished"),
                    new Trigger_Memo("TakeOffImminent"), // Raised when take-off is requested by player.
                    new Trigger_MedicalSpaceshipTakeOffImminent() // Can happen if medic is busy for too long.
                },
                postActions =
                {
                    new TransitionAction_EndAllJobs()
                }
            });
            return stateGraph;
        }

        public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
        {
            base.Notify_PawnLost(p, condition);
            if (condition == PawnLostCondition.IncappedOrKilled)
            {
                if (p.kindDef == Util_PawnKindDefOf.Medic)
                {
                    Util_Faction.AffectFactionGoodwillWithOther(Util_Faction.MiningCoFaction, Faction.OfPlayer, -5);
                    string letterText = "A MiningCo. medic has been downed or killed while helping your colony.\n\n"
                        + "Medics are precious people, even more on rimworlds. Losing one will obviously anger MiningCo. and nobody wants to anger this company.\n"
                        + "Remember that you must ensure their safety in your area!";
                    Find.LetterStack.ReceiveLetter("Medic down", letterText, LetterDefOf.NegativeEvent);
                }
            }
        }
    }
}