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
    public class LordToilData_HealColonists : LordToilData
	{
        public IntVec3 spaceshipPosition;

		public override void ExposeData()
		{
            Scribe_Values.Look<IntVec3>(ref this.spaceshipPosition, "spaceshipPosition");
		}
	}
}
