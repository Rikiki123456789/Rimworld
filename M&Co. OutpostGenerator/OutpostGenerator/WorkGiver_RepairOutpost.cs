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
    /// WorkGiver_RepairOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_RepairOutpost : WorkGiver_Repair
    {
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
            if ((OG_Util.OutpostArea != null)
                && (OG_Util.OutpostArea.ActiveCells.Contains(t.Position)))
            {
                return base.JobOnThing(pawn, t);
            }
            return null;
        }
    }
}
