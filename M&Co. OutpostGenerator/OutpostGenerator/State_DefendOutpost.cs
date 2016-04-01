using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;      // Always needed
//using VerseBase;      // Material/Graphics handling functions are found here
using RimWorld;         // RimWorld specific functions are found here
using Verse;            // RimWorld universal objects are here
using Verse.AI;         // Needed when you do something with the AI
using RimWorld.SquadAI; // Needed when you do something with the squad AI
//using Verse.Sound;    // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// OG_Inhabitants class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class State_DefendOutpost : State
    {
        public IntVec3 defendPoint;
        public float defendRadius = 50f;

        public override IntVec3 FlagLoc
        {
            get
            {
                return this.defendPoint;
            }
        }

        public State_DefendOutpost()
        {
        }

        public State_DefendOutpost(IntVec3 defendPoint, float defendRadius)
        {
            this.defendPoint = defendPoint;
            this.defendRadius = defendRadius;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<IntVec3>(ref this.defendPoint, "defendPoint", default(IntVec3), false);
            Scribe_Values.LookValue<float>(ref this.defendRadius, "defendRadius", 50f, false);
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.brain.ownedPawns.Count; i++)
            {
                this.brain.ownedPawns[i].mindState.duty = new PawnDuty(OG_Util.DefendOutpostDutyDef, this.defendPoint, -1f);
                this.brain.ownedPawns[i].mindState.duty.focusSecond = this.defendPoint;
                this.brain.ownedPawns[i].mindState.duty.radius = this.defendRadius;
            }
        }

        public override void StateTick()
        {
            base.StateTick();
        }
    }
}
