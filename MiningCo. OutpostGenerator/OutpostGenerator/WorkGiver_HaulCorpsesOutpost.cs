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
    /// WorkGiver_HaulCorpsesOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_HaulCorpsesOutpost : WorkGiver_HaulCorpses
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
            if (!(t is Corpse))
            {
                return null;
            }
            Area outpostArea = OG_Util.FindOutpostArea();
            if ((outpostArea != null)
                && (outpostArea.ActiveCells.Contains(t.Position)))
            {
                // Get potential storage cell and check it is in the outpost area.
                StoragePriority currentPriority = HaulAIUtility.StoragePriorityAtFor(t.Position, t);
                IntVec3 storeCell;
                if (StoreUtility.TryFindBestBetterStoreCellFor(t, pawn, currentPriority, pawn.Faction, out storeCell, true))
                {
                    if (outpostArea.ActiveCells.Contains(storeCell))
                    {
                        return base.JobOnThing(pawn, t);
                    }
                }
            }
            return null;
        }
    }
}
