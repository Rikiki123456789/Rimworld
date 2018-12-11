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
    public class LordToil_EscortDownedPawn : LordToil
    {
        public LordToilData_EscortDownedPawn Data
        {
            get
            {
                return (LordToilData_EscortDownedPawn)this.data;
            }
        }
		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

        public LordToil_EscortDownedPawn(IntVec3 travelDest, LocomotionUrgency locomotion)
        {
            this.data = new LordToilData_EscortDownedPawn();
            this.Data.targetDestination = travelDest;
            this.Data.locomotion = locomotion;
		}

        public void SetDestination(IntVec3 newTargetDestination)
        {
            this.Data.targetDestination = newTargetDestination;
        }

		public override void UpdateAllDuties()
		{
            if (this.Data.carrier == null)
            {
                this.Data.carrier = this.lord.ownedPawns.RandomElement();
            }
			for (int pawnIndex = 0; pawnIndex < this.lord.ownedPawns.Count; pawnIndex++)
			{
                Pawn pawn = this.lord.ownedPawns[pawnIndex];
                if (pawn == this.Data.carrier)
                {
                    PawnDuty pawnDuty = new PawnDuty(Util_DutyDefOf.CarryDownedPawn, this.Data.targetDestination);
                    pawn.mindState.duty = pawnDuty;
                }
                else
                {
                    PawnDuty pawnDuty = new PawnDuty(Util_DutyDefOf.EscortCarrier, this.Data.carrier, 5f);
                    pawn.mindState.duty = pawnDuty;
                }
			}
		}

        public override void LordToilTick()
        {
            base.LordToilTick();
            if ((Find.TickManager.TicksGame % (2 * GenTicks.TicksPerRealSecond)) == 0)
            {
                // Check if carrier is still doing his job.
                Pawn carrier = this.Data.carrier;
                if (carrier.DestroyedOrNull()
                    || carrier.Dead
                    || carrier.Downed)
                {
                    Notify_RescueEnded();
                }
            }
        }

        public void Notify_RescueEnded()
        {
            this.Data.carrier = null;
            this.lord.ReceiveMemo("RescueEnded");
        }
	}
}
