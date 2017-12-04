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
    public class LordToilData_EscortDownedPawn : LordToilData
	{
        public IntVec3 targetDestination;
		public LocomotionUrgency locomotion;
		public Pawn carrier = null;
		public override void ExposeData()
		{
            Scribe_Values.Look<IntVec3>(ref this.targetDestination, "targetDestination");
			Scribe_Values.Look<LocomotionUrgency>(ref this.locomotion, "locomotion", LocomotionUrgency.None, false);
            Scribe_References.Look<Pawn>(ref this.carrier, "carrier");
		}
	}
}
