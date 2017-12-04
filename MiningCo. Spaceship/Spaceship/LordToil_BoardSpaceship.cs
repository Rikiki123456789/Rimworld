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
	public class LordToil_BoardSpaceship : LordToil
	{
		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}
		protected LordToilData_BoardSpaceship Data
		{
			get
			{
				return (LordToilData_BoardSpaceship)this.data;
			}
		}
        public LordToil_BoardSpaceship(IntVec3 boardCell, LocomotionUrgency locomotion = LocomotionUrgency.Walk)
		{
            this.data = new LordToilData_BoardSpaceship();
            this.Data.boardCell = boardCell;
			this.Data.locomotion = locomotion;
		}
		public override void UpdateAllDuties()
		{
            LordToilData_BoardSpaceship data = this.Data;
			for (int pawnIndex = 0; pawnIndex < this.lord.ownedPawns.Count; pawnIndex++)
			{
				PawnDuty pawnDuty = new PawnDuty(Util_DutyDefOf.DutyBoardSpaceship);
				pawnDuty.locomotion = data.locomotion;
                pawnDuty.focus = data.boardCell;
				this.lord.ownedPawns[pawnIndex].mindState.duty = pawnDuty;
			}
		}
	}
}
