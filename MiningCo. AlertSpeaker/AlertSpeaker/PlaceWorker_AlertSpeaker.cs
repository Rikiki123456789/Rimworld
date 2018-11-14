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

namespace AlertSpeaker
{
    /// <summary>
    /// AlertSpeaker custom place worker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class PlaceWorker_AlertSpeaker : PlaceWorker
    {
        /// <summary>
        /// Checks if a new alert speaker can be built at this location (must be near a wall) and draw effect area.
        /// </summary>
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            // Check it is built near a wall.
            if (Building_AlertSpeaker.IsSupportAlive(map, loc, rot) == false)
            {
                return new AcceptanceReport("Alert speaker must be built near a wall or tall edifice.");
            }

            // Display effect zone.
            if (loc.GetEdifice(map) == null)
            {
                List<IntVec3> cellsInAoe = Building_AlertSpeaker.GetAreaOfEffectCells(map, loc);
                GenDraw.DrawFieldEdges(cellsInAoe);
            }

            return true;
        }
    }
}
