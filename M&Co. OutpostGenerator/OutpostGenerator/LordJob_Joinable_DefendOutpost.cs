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

        private Trigger_TicksPassed timeoutTrigger; // TODO: remove it?

        public LordJob_Joinable_DefendOutpost()
        {
        }

        public LordJob_Joinable_DefendOutpost(IntVec3 rallyPoint)
        {
            this.rallyPoint = rallyPoint;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            //LordToil_Party lordToil_Defend = new LordToil_Party(this.rallyPoint);
            LordToil_DefendOutpost lordToil_Defend = new LordToil_DefendOutpost(this.rallyPoint);
            stateGraph.AddToil(lordToil_Defend);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);

            Transition transition = new Transition(lordToil_Defend, lordToil_End);
            transition.AddTrigger(new Trigger_PawnLostViolently());
            transition.AddAction(new TransitionAction_Message("MessagePartyCalledOff".Translate(), MessageSound.Negative, this.rallyPoint));
            stateGraph.AddTransition(transition);

            this.timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(5000, 5000));
            Transition transition2 = new Transition(lordToil_Defend, lordToil_End);
            transition2.AddTrigger(this.timeoutTrigger);
            transition2.AddAction(new TransitionAction_Message("MessagePartyFinished".Translate(), MessageSound.Negative, this.rallyPoint));
            /*transition2.AddAction(new TransitionAction_Custom(delegate
            {
                this.Finished();
            }));*/
            stateGraph.AddTransition(transition2);

            return stateGraph;
        }
        
        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            if ((p.Faction != null)
                && (p.Faction == OG_Util.FactionOfMAndCo)
                && (p.kindDef != OG_Util.OutpostTechnicianDef))
            {
                Log.Message("VoluntaryJoinPriorityFor: joining " + p.Name.ToStringShort);
                return 20f;
            }
            return 0f;
        }

        public override void ExposeData()
        {
            Scribe_Values.LookValue<IntVec3>(ref this.rallyPoint, "rallyPoint", default(IntVec3), false);
        }
    }
}
