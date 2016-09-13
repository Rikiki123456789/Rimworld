using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// WorkGiver_TransferInjuredEmployee class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_TransferInjuredEmployee : WorkGiver_Scanner
    {
        // This workgiver is specific to MiningCo. employees. When a supply ship is landed (so there is a cryptosleep bay),
        // it will order a pawn to open a used medibay cryptosleep casket so its content (a downed MiningCo. employee)
        // will be carried to the supply ship's cryptosleep bay.
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(ThingDefOf.CryptosleepCasket);
            }
        }
        
        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            if ((pawn.Faction != null)
                && (pawn.Faction != OG_Util.FactionOfMiningCo))
            {
                return false;
            }
            // Check if a supply ship is landed.
            if (IsSupplyShipCryptosleepBayAvailable(pawn) == false)
            {
                return false;
            }

            Building_CryptosleepCasket casket = t as Building_CryptosleepCasket;
            if ((casket.GetContainer().Count > 0)
                && pawn.CanReserveAndReach(casket, PathEndMode.InteractionCell, Danger.Deadly))
            {
                return true;
            }
            return false;
        }

        public static bool IsSupplyShipCryptosleepBayAvailable(Pawn opener)
        {
            // We check if pawn can reach left or right supply ship cryptosleep bay because one of them can be blocked by a laser fence.
            List<Thing> leftBaysList = Find.ListerThings.ThingsOfDef(OG_Util.SupplyShipCryptosleepBayLeftDef);
            for (int bayIndex = 0; bayIndex < leftBaysList.Count; bayIndex++)
            {
                Building_SupplyShipCryptosleepBay bay = leftBaysList[bayIndex] as Building_SupplyShipCryptosleepBay;
                if ((bay.Faction != null)
                    && (bay.Faction == opener.Faction))
                {
                    if (bay.ContainedThing == null)
                    {
                        if (opener.CanReserveAndReach(bay, PathEndMode.InteractionCell, Danger.Deadly))
                        {
                            return true;
                        }
                    }
                }
            }
            List<Thing> rightBaysList = Find.ListerThings.ThingsOfDef(OG_Util.SupplyShipCryptosleepBayRightDef);
            for (int bayIndex = 0; bayIndex < rightBaysList.Count; bayIndex++)
            {
                Building_SupplyShipCryptosleepBay bay = rightBaysList[bayIndex] as Building_SupplyShipCryptosleepBay;
                if ((bay.Faction != null)
                    && (bay.Faction == opener.Faction))
                {
                    if (bay.ContainedThing == null)
                    {
                        if (opener.CanReserveAndReach(bay, PathEndMode.InteractionCell, Danger.Deadly))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            return new Job(DefDatabase<JobDef>.GetNamed(OG_Util.JobDefName_TransferInjuredEmployeet), t);
        }
    }
}
