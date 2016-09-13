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
    /// WorkGiver_BuryCorpsesOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_BuryCorpsesOutpost : WorkGiver_BuryCorpses
    {
        // This workgiver is specific to MiningCo. employees.
        public override bool ShouldSkip(Pawn pawn)
        {
            if ((pawn.Faction != null)
                && (pawn.Faction == OG_Util.FactionOfMiningCo))
            {
                return base.ShouldSkip(pawn);
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            Area outpostArea = OG_Util.FindOutpostArea();
            if ((outpostArea != null)
                && (outpostArea.ActiveCells.Contains(t.Position)))
            {
                Building_Grave bestGrave = FindBestGrave(pawn, t as Corpse);
                if ((bestGrave != null)
                    && outpostArea.ActiveCells.Contains(bestGrave.Position))
                {
                    return base.JobOnThing(pawn, t);
                }
            }
            return null;
        }

        private Building_Grave FindBestGrave(Pawn p, Corpse corpse)
        {
            Predicate<Thing> predicate = (Thing m) => !m.IsForbidden(p) && p.CanReserve(m, 1) && ((Building_Grave)m).Accepts(corpse);
            if (corpse.innerPawn.ownership != null && corpse.innerPawn.ownership.AssignedGrave != null)
            {
                Building_Grave assignedGrave = corpse.innerPawn.ownership.AssignedGrave;
                if (predicate(assignedGrave) && corpse.Position.CanReach(assignedGrave, PathEndMode.ClosestTouch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false)))
                {
                    return assignedGrave;
                }
            }
            Func<Thing, float> priorityGetter = (Thing t) => (float)((IStoreSettingsParent)t).GetStoreSettings().Priority;
            Predicate<Thing> validator = predicate;
            return (Building_Grave)GenClosest.ClosestThing_Global_Reachable(corpse.Position, Find.ListerThings.ThingsInGroup(ThingRequestGroup.Grave), PathEndMode.ClosestTouch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, priorityGetter);
        }
    }
}
