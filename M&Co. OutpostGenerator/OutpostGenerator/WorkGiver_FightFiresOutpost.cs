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
    /// WorkGiver_FightFiresOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_FightFiresOutpost : WorkGiver_Scanner
    {
        private const int NearbyPawnRadius = 15;
        private const int MaxReservationCheckDistance = 15;
        private const float HandledDistance = 5f;

        // This workgiver is specific to M&Co. employees.
        public override bool ShouldSkip(Pawn pawn)
        {
            if ((pawn.Faction != null)
                && (pawn.Faction == OG_Util.FactionOfMAndCo))
            {
                return base.ShouldSkip(pawn);
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            return new Job(JobDefOf.BeatFire, t);
        }

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(ThingDefOf.Fire);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            Fire fire = t as Fire;
            if (fire == null)
            {
                return false;
            }
            Pawn pawn2 = fire.parent as Pawn;
            if (pawn2 != null)
            {
                if (pawn2 == pawn)
                {
                    return false;
                }
                if ((pawn2.Faction == pawn.Faction || pawn2.HostFaction == pawn.Faction || pawn2.HostFaction == pawn.HostFaction)
                    && ((OG_Util.OutpostArea == null) || (OG_Util.OutpostArea.ActiveCells.Contains(t.Position) == false))
                    && Gen.ManhattanDistanceFlat(pawn.Position, pawn2.Position) > 15)
                {
                    return false;
                }
            }
            else if ((OG_Util.OutpostArea == null)
                || (OG_Util.OutpostArea.ActiveCells.Contains(t.Position) == false))
            {
                return false;
            }
            return ((pawn.Position - fire.Position).LengthHorizontalSquared <= 225f || pawn.CanReserve(fire, 1)) && !WorkGiver_FightFiresOutpost.FireIsBeingHandled(fire, pawn);
        }


        public static bool FireIsBeingHandled(Fire f, Pawn potentialHandler)
        {
            Pawn pawn = Find.Reservations.FirstReserverOf(f, potentialHandler.Faction);
            return pawn != null && pawn.Position.InHorDistOf(f.Position, 5f);
        }
    }
}
