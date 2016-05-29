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
//using RimWorld.Planet;


namespace OutpostGenerator
{
    public class LordJob_Joinable_DefendOutpost : LordJob_VoluntarilyJoinable
    {
        private IntVec3 rallyPoint;
        private Trigger_TicksPassed timeoutTrigger;

        public LordJob_Joinable_DefendOutpost()
        {
        }

        public LordJob_Joinable_DefendOutpost(IntVec3 rallyPoint)
        {
            this.rallyPoint = rallyPoint;
        }

        public override StateGraph CreateGraph()
        {
            Log.Message("CreateGraph");
            StateGraph stateGraph = new StateGraph();
            LordToil_DefendOutpost lordToil_Defend = new LordToil_DefendOutpost(this.rallyPoint);
            stateGraph.AddToil(lordToil_Defend);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);

            this.timeoutTrigger = new Trigger_TicksPassed(8000);
            Transition transition = new Transition(lordToil_Defend, lordToil_End);
            transition.AddTrigger(this.timeoutTrigger);
            transition.AddAction(new TransitionAction_Message("Defend lord timeout", MessageSound.Standard, this.rallyPoint));
            stateGraph.AddTransition(transition);

            return stateGraph;
        }
        
        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            if ((p.Faction != null)
                && (p.Faction == OG_Util.FactionOfMAndCo)
                && (p.kindDef != OG_Util.OutpostTechnicianDef))
            {
                return 25f;
            }
            return 0f;
        }

        public override void ExposeData()
        {
            Scribe_Values.LookValue<IntVec3>(ref this.rallyPoint, "rallyPoint", default(IntVec3), false);
        }
    }
}
