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
    public class LordToil_HealColonists : LordToil
    {
        public LordToilData_HealColonists Data
        {
            get
            {
                return (LordToilData_HealColonists)this.data;
            }
        }
		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

        public LordToil_HealColonists(IntVec3 spaceshipPosition)
        {
            this.data = new LordToilData_HealColonists();
            this.Data.spaceshipPosition = spaceshipPosition;
		}

		public override void UpdateAllDuties()
		{
            foreach (Pawn pawn in this.lord.ownedPawns)
            {
                PawnDuty pawnDuty = new PawnDuty(Util_DutyDefOf.HealColonists);
                pawn.mindState.duty = pawnDuty;
                pawn.mindState.duty.focus = this.Data.spaceshipPosition;
            }
		}
	}
}
