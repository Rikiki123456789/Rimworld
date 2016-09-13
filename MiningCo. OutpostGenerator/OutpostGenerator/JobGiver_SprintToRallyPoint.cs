using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
//using RimWorld.Planet;


namespace OutpostGenerator
{
    public class JobGiver_SprintToRallyPoint : JobGiver_WanderNearDutyLocation
    {
        public JobGiver_SprintToRallyPoint()
        {
            this.ticksBetweenWandersRange = new IntRange(125, 200);
            this.locomotionUrgency = LocomotionUrgency.Sprint;
        }
    }
}
