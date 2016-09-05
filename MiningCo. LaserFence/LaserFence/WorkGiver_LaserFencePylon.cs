using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace LaserFence
{
    /// <summary>
    /// WorkGiver_LaserFencePylon class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_LaserFencePylon : WorkGiver_Scanner
    {
		public override ThingRequest PotentialWorkThingRequest
		{
			get
            {
                return ThingRequest.ForDef(ThingDef.Named("LaserFencePylon"));
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{

            if ((t is Building_LaserFencePylon) == false)
            {
                return false;
            }
            if (pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Deadly) == false)
            {
                return false;
            }

            Building_LaserFencePylon pylon = t as Building_LaserFencePylon;
            
            if (pawn.Dead
                || pawn.Downed
                || pawn.IsBurning())
            {
                return false;
            }
            if (pylon.manualSwitchIsPending)
            {
                return true;
            }
            return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
            Building_LaserFencePylon pylon = t as Building_LaserFencePylon;
            Job job = new Job(DefDatabase<JobDef>.GetNamed("JobDef_SwitchLaserFence"), pylon);
            return job;
		}
    }
}
