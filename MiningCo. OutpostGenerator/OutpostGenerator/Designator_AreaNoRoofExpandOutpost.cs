using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Designator_AreaNoRoofExpandOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Designator_AreaNoRoofExpandOutpost : Designator_AreaNoRoofExpand
    {
        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (c.InBounds()
                && (c.Fogged() == false))
            {
                Area outpostArea = OG_Util.FindOutpostArea();
                if ((outpostArea != null)
                    && outpostArea.ActiveCells.Contains(c))
                {
                    return "You cannot manage MiningCo. Outpost roof. This area does not belong to your colony.";
                }
                if (Find.RoofGrid.RoofAt(c) == OG_Util.IronedRoofDef)
                {
                    return true;
                }
            }
            return base.CanDesignateCell(c);
        }
    }
}
