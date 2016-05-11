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
            List<Thing> commandConsoleList = Find.ListerThings.ThingsOfDef(OG_Util.OutpostCommandConsoleDef);
            if (commandConsoleList.Count == 1)
            {
                // Command console exists. Check which faction it belongs to.
                Thing commandConsole = commandConsoleList[0];
                if ((commandConsole.Faction != null)
                    && (commandConsole.Faction == OG_Util.FactionOfMAndCo))
                {
                    if (!c.InBounds())
                    {
                        return false;
                    }
                    if (c.Fogged())
                    {
                        return false;
                    }
                    RoofDef roofDef = Find.RoofGrid.RoofAt(c);
                    if (roofDef == OG_Util.IronedRoofDef)
                    {
                        return "You cannot remove M&Co. Outpost roof. This area does not belong to your colony.";
                    }
                }
            }
            return base.CanDesignateCell(c);
        }
    }
}
