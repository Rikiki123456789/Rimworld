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
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
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

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
            if ((t is Building_FishingPier) == false)
            {
                return false;
            }
            Building_FishingPier fishingPier = t as Building_FishingPier;

            if (fishingPier.IsBurning()
                || (fishingPier.allowFishing == false))
            {
                return false;
            }
            if (Util_Zone_Fishing.IsAquaticTerrain(fishingPier.Map, fishingPier.fishingSpotCell) == false)
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

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
            Job job = null;
            Building_FishingPier fishingPier = t as Building_FishingPier;

            if ((fishingPier.allowUsingGrain)
                && (HasFoodToAttractFishes(pawn) == false))
            {
                Predicate <Thing> predicate = delegate(Thing grainStack)
                {
                    return (grainStack.IsForbidden(pawn.Faction) == false)
                        && (grainStack.stackCount >= 4 * JobDriver_FishAtFishingPier.grainCountToAttractFishes);
                };
                TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn, false);
                // Look for corn to pick.
                Thing corn = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(Util_FishIndustry.RawCornDef), Verse.AI.PathEndMode.ClosestTouch, traverseParams, 9999f, predicate);
                if (corn != null)
                {
                    job = JobMaker.MakeJob(JobDefOf.TakeInventory, corn);
                    job.count = 4 * JobDriver_FishAtFishingPier.grainCountToAttractFishes;
                    return job;
                }
                // Look for rice to pick.
                Thing rice = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(Util_FishIndustry.RawRiceDef), Verse.AI.PathEndMode.ClosestTouch, traverseParams, 9999f, predicate);
                if (rice != null)
                {
                    job = JobMaker.MakeJob(JobDefOf.TakeInventory, rice);
                    job.count = 4 * JobDriver_FishAtFishingPier.grainCountToAttractFishes;
                    return job;
                }
            }
            job = JobMaker.MakeJob(Util_FishIndustry.FishAtFishingPierJobDef, fishingPier, fishingPier.fishingSpotCell);

            return job;
		}

        public bool HasFoodToAttractFishes(Pawn fisher)
        {
            foreach (Thing thing in fisher.inventory.innerContainer)
            {
                if (((thing.def == Util_FishIndustry.RawCornDef)
                    || (thing.def == Util_FishIndustry.RawRiceDef))
                    && (thing.stackCount >= JobDriver_FishAtFishingPier.grainCountToAttractFishes))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
