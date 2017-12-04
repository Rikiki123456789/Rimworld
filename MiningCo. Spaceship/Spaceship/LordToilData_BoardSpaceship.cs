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
	public class LordToilData_BoardSpaceship : LordToilData
	{
        public IntVec3 boardCell;
		public LocomotionUrgency locomotion;
		public override void ExposeData()
		{
            Scribe_Values.Look<IntVec3>(ref this.boardCell, "boardCell");
			Scribe_Values.Look<LocomotionUrgency>(ref this.locomotion, "locomotion", LocomotionUrgency.None, false);
		}
	}
}
