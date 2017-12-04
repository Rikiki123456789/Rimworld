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
    public class JobGiver_BoardSpaceship : ThinkNode_JobGiver
	{
		protected LocomotionUrgency defaultLocomotion;
		protected int jobMaxDuration = 999999;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
            JobGiver_BoardSpaceship jobGiver_BoardSpaceship = (JobGiver_BoardSpaceship)base.DeepCopy(resolve);
			jobGiver_BoardSpaceship.defaultLocomotion = this.defaultLocomotion;
			jobGiver_BoardSpaceship.jobMaxDuration = this.jobMaxDuration;
			return jobGiver_BoardSpaceship;
		}

		protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Position != pawn.DutyLocation())
            {
                return new Job(JobDefOf.Goto, pawn.DutyLocation())
                {
                    locomotionUrgency = pawn.mindState.duty.locomotion,
                    expiryInterval = this.jobMaxDuration
                };
            }
            else
            {
                Building_Spaceship spaceship = null;
                foreach (Thing thing in pawn.Position.GetThingList(pawn.Map))
                {
                    if (thing is Building_Spaceship)
                    {
                        spaceship = thing as Building_Spaceship;
                        break;
                    }
                }
                if (spaceship != null)
                {
                    return new Job(Util_JobDefOf.BoardSpaceship, spaceship);
                }
            }
            return null;
		}
	}
}
