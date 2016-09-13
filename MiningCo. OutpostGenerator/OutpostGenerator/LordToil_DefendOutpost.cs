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
    public class LordToil_DefendOutpost : LordToil_DefendPoint
    {
        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
        {
            return ThinkTreeDutyHook.MediumPriority;
        }
        
        public LordToil_DefendOutpost(IntVec3 defendPoint, float defendRadius = 28f)
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

        public override void LordToilTick()
        {
            base.LordToilTick();
            if (this.lord.ticksInToil > GenTicks.TickRareInterval)
            {
                Building_OrbitalRelay orbitalRelay = OG_Util.FindOrbitalRelay(OG_Util.FactionOfMiningCo);
                if (orbitalRelay != null)
                {
                    IntVec3 hostilePosition = orbitalRelay.FindHostileInPerimeter();
                    if ((hostilePosition == IntVec3.Invalid)
                        || (IntVec3Utility.ManhattanDistanceFlat(hostilePosition, this.FlagLoc) > 60)) // If an hostile is still in the perimeter, a new lord will be generated.
                    {
                        this.lord.ReceiveMemo("ThreatIsFinished");
                    }
                }
            }
        }
    }
}
