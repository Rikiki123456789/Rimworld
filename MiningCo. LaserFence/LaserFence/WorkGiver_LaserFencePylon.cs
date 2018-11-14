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
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class WorkGiver_LaserFencePylon : WorkGiver_Scanner
    {
		public override ThingRequest PotentialWorkThingRequest
		{
			get
            {
                return ThingRequest.ForDef(Util_LaserFence.LaserFencePylonDef);
			}
		}

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
            Building_LaserFencePylon pylon = t as Building_LaserFencePylon;
            if (pylon == null)
            {
                return false;
            }
            if (pawn.Dead
                || pawn.Downed
                || pawn.IsBurning())
            {
                return false;
            }
            if (pawn.CanReserveAndReach(pylon, PathEndMode.InteractionCell, Danger.Deadly) == false)
            {
                return false;
            }
            if (pylon.manualSwitchIsPending)
            {
                return true;
            }
            return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
            Building_LaserFencePylon pylon = t as Building_LaserFencePylon;
            return new Job(Util_LaserFence.SwitchLaserFenceDef, pylon);
		}
    }
}
