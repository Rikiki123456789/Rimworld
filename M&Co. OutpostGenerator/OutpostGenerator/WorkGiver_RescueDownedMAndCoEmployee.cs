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
    /// WorkGiver_RescueDownedMAndCoEmployee class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_RescueDownedMAndCoEmployee : WorkGiver_RescueDowned
    {
        // This workgiver is specific to M&Co. employees. It allows them to rescue fallen comrades even when M&Co. is hostile to colony (thus the redefinition of EnemyIsNear function).
        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            if ((pawn.Faction != null)
                && (pawn.Faction != OG_Util.FactionOfMAndCo))
            {
                return false;
            }

            Pawn downedPawn = t as Pawn;
            if (downedPawn == null || !downedPawn.Downed || downedPawn.Faction != pawn.Faction || downedPawn.InBed() || !pawn.CanReserve(downedPawn, 1) || EnemyIsNear(downedPawn, 40f))
            {
                return false;
            }
            Thing cryptosleepBay = FindFreeSupplyShipCryptosleepBay(pawn);
            Thing cryptosleepCasket = FindFreeCryptosleepCasket(pawn);
            return (((cryptosleepBay != null) && downedPawn.CanReserve(cryptosleepBay, 1))
                || ((cryptosleepCasket != null) && downedPawn.CanReserve(cryptosleepCasket, 1)));
        }

        public static bool EnemyIsNear(Pawn p, float radius)
        {
            foreach (Pawn current in Find.MapPawns.AllPawns)
            {
                if ((current.HostileTo(p.Faction)) && !current.Downed && (current.Position - p.Position).LengthHorizontalSquared < radius * radius)
                {
                    return true;
                }
            }
            return false;
        }

        public static Thing FindFreeSupplyShipCryptosleepBay(Pawn rescuer)
        {
            List<Thing> leftBaysList = Find.ListerThings.ThingsOfDef(OG_Util.SupplyShipCryptosleepBayLeftDef);
            for (int bayIndex = 0; bayIndex < leftBaysList.Count; bayIndex++)
            {
                Thing potentialBay = leftBaysList[bayIndex];
                if ((potentialBay.Faction != null)
                    && (potentialBay.Faction == rescuer.Faction))
                {
                    Building_SupplyShipCryptosleepBay bay = potentialBay as Building_SupplyShipCryptosleepBay;
                    if ((bay.GetContainer().Count == 0)
                        && (rescuer.CanReserveAndReach(bay, PathEndMode.InteractionCell, Danger.Deadly)))
                    {
                        return bay;
                    }
                }
            }
            List<Thing> rightBaysList = Find.ListerThings.ThingsOfDef(OG_Util.SupplyShipCryptosleepBayRightDef);
            for (int bayIndex = 0; bayIndex < rightBaysList.Count; bayIndex++)
            {
                Thing potentialBay = rightBaysList[bayIndex];
                if ((potentialBay.Faction != null)
                    && (potentialBay.Faction == rescuer.Faction))
                {
                    Building_SupplyShipCryptosleepBay bay = potentialBay as Building_SupplyShipCryptosleepBay;
                    if ((bay.GetContainer().Count == 0)
                        && (rescuer.CanReserveAndReach(bay, PathEndMode.InteractionCell, Danger.Deadly)))
                    {
                        return bay;
                    }
                }
            }
            return null;
        }

        public static Thing FindFreeCryptosleepCasket(Pawn rescuer)
        {
            List<Thing> casketsList = Find.ListerThings.ThingsOfDef(ThingDefOf.CryptosleepCasket);
            for (int casketIndex = 0; casketIndex < casketsList.Count; casketIndex++)
            {
                Thing potentialCasket = casketsList[casketIndex];
                if ((potentialCasket.Faction != null)
                    && (potentialCasket.Faction == rescuer.Faction))
                {
                    Building_CryptosleepCasket casket = potentialCasket as Building_CryptosleepCasket;
                    if ((casket.GetContainer().Count == 0)
                        && (rescuer.CanReserveAndReach(casket, PathEndMode.InteractionCell, Danger.Deadly)))
                    {
                        return casket;
                    }
                }
            }
            return null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            Pawn downedPawn = t as Pawn;
            Thing bay = FindFreeSupplyShipCryptosleepBay(pawn);
            if (bay != null)
            {
                return new Job(JobDefOf.CarryToCryptosleepCasket, downedPawn, bay)
                {
                    maxNumToCarry = 1
                };
            }
            Thing casket = FindFreeCryptosleepCasket(pawn);
            return new Job(JobDefOf.CarryToCryptosleepCasket, downedPawn, casket)
            {
                maxNumToCarry = 1
            };
        }
    }
}
