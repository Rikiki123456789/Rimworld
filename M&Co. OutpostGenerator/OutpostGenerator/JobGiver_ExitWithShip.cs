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
    public class JobGiver_ExitWithShip : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            Building_SupplyShip supplyShip = OG_Util.FindSupplyShip(pawn.Faction);
            if (supplyShip == null)
            {
                return null;
            }
            
            // Outpost has been captured or pawn has no weapon or an apparel in bad conditions.
            if (IsOutpostCaptured()
                || IsLackingWeapon(pawn)
                || IsWearingDamagedApparel(pawn)
                || IsLackingPant(pawn))
            {
                if (pawn.CanReserveAndReach(supplyShip, PathEndMode.OnCell, Danger.Deadly))
                {
                    return new Job(DefDatabase<JobDef>.GetNamed(OG_Util.JobDefName_BoardSupplyShip), supplyShip);
                }
            }
            return null;
        }

        private bool IsOutpostCaptured()
        {
            Building_OutpostCommandConsole commandConsole = OG_Util.FindOutpostCommandConsole(OG_Util.FactionOfMAndCo);
            if ((commandConsole == null)
                || (commandConsole.Faction != OG_Util.FactionOfMAndCo))
            {
                return true;
            }
            return false;
        }

        private bool IsLackingWeapon(Pawn pawn)
        {
            return (pawn.equipment.Primary == null);
        }

        private bool IsWearingDamagedApparel(Pawn pawn)
        {
            for (int apparelIndex = 0; apparelIndex < pawn.apparel.WornApparelCount; apparelIndex++)
            {
                if ((float)pawn.apparel.WornApparel[apparelIndex].HitPoints < (0.5f * (float)pawn.apparel.WornApparel[apparelIndex].MaxHitPoints))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsLackingPant(Pawn pawn)
        {
            for (int apparelIndex = 0; apparelIndex < pawn.apparel.WornApparelCount; apparelIndex++)
            {
                if (pawn.apparel.WornApparel[apparelIndex].def == ThingDef.Named("Apparel_Pants"))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
