using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;      // Always needed
//using VerseBase;      // Material/Graphics handling functions are found here
using RimWorld;         // RimWorld specific functions are found here
using Verse;            // RimWorld universal objects are here
using Verse.AI;         // Needed when you do something with the AI
using Verse.AI.Group;   // Needed when you do something with group AI
//using Verse.Sound;    // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// LordToil_DefendOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class LordToil_DefendOutpost : LordToil
    {
        private bool allowSatisfyLongNeeds = true;

        protected LordToilData_DefendPoint Data
        {
            get
            {
                return (LordToilData_DefendPoint)this.data;
            }
        }

        public override IntVec3 FlagLoc
        {
            get
            {
                return this.Data.defendPoint;
            }
        }
        public override bool AllowSatisfyLongNeeds
        {
            get
            {
                return this.allowSatisfyLongNeeds;
            }
        }

        public LordToil_DefendOutpost(bool canSatisfyLongNeeds = true)
        {
            this.allowSatisfyLongNeeds = canSatisfyLongNeeds;
            this.data = new LordToilData_DefendPoint();
        }

        public LordToil_DefendOutpost(IntVec3 defendPoint, float defendRadius = 28f) : this(true)
		{
            this.Data.defendPoint = defendPoint;
            this.Data.defendRadius = defendRadius;
        }

        public override void UpdateAllDuties()
        {
            LordToilData_DefendPoint data = this.Data;
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(OG_Util.DefendOutpostDutyDef, this.Data.defendPoint, -1f);
                this.lord.ownedPawns[i].mindState.duty.focusSecond = this.Data.defendPoint;
                this.lord.ownedPawns[i].mindState.duty.radius = this.Data.defendRadius;
            }
        }

        public void SetDefendPoint(IntVec3 defendPoint)
        {
            this.Data.defendPoint = defendPoint;
        }
    }
}
