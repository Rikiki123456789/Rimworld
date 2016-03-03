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
    /// WorkGiver_GrowerOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_GrowerOutpost : WorkGiver_Grower
    {
        /*public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            if ((pawn.Faction != null)
                && (pawn.Faction == OG_Util.FactionOfMAndCo))
            {
                List<IntVec3> workCells = new List<IntVec3>();

                List<Thing> hydroponicsList = Find.ListerThings.ThingsOfDef(ThingDef.Named("HydroponicsBasin"));
                Log.Message("Found ")
                for (int hydroponicsIndex = 0; hydroponicsIndex < hydroponicsList.Count; hydroponicsIndex++)
                {
                    Thing hydroponics = hydroponicsList[hydroponicsIndex];
                    if ((hydroponics.Faction != null)
                        && (pawn.Faction == OG_Util.FactionOfMAndCo))
                    {

                    }
                }

                return workCells;
            }
            else
            {
                return base.PotentialWorkCellsGlobal(pawn);
            }
        }*/
    }
}
