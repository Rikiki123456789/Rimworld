using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// WorkGiver_FishingPier class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_FishingPier : WorkGiver_Scanner
    {
		public override ThingRequest PotentialWorkThingRequest
		{
			get
            {
                return ThingRequest.ForDef(Util_FishIndustry.FishingPierDef);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
            if ((t is Building_FishingPier) == false)
            {
                return false;
            }
            Building_FishingPier fishingPier = t as Building_FishingPier;

            if (fishingPier.IsBurning())
            {
                return false;
            }
            if (Util_FishIndustry.IsAquaticTerrain(fishingPier.Map, fishingPier.fishingSpotCell) == false)
            {
                return false;
            }
            if (pawn.Dead
                || pawn.Downed
                || pawn.IsBurning())
            {
                return false;
            }
            if (pawn.CanReserveAndReach(fishingPier, this.PathEndMode, Danger.Some) == false)
            {
                return false;
            }
            if (fishingPier.fishStock <= 0)
            {
                return false;
            }
            return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
            Job job = new Job();
            Building_FishingPier fishingPier = t as Building_FishingPier;

            job = new Job(DefDatabase<JobDef>.GetNamed(Util_FishIndustry.JobDefName_FishAtFishingPier), fishingPier, fishingPier.fishingSpotCell);

            return job;
		}
    }
}
