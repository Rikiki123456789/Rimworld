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
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class PlaceWorker_AlertSpeaker : PlaceWorker
    {
        public const int minDistanceBetweenTwoAlertSpeakers = 1;

        /// <summary>
        /// Checks if a new alert speaker can be built at this location.
        /// - must be near a wall,
        /// - must not be too near from another alert speaker.
        /// </summary>
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            IntVec3 potentialWallPosition = loc;

            // Check if another alert speaker is not too close.
            List<Thing> alertSpeakerList = Find.ListerThings.ThingsOfDef(ThingDef.Named("AlertSpeaker"));
            List<Thing> alertSpeakerBlueprintList = Find.ListerThings.ThingsOfDef(ThingDef.Named("AlertSpeaker").blueprintDef);
            List<Thing> alertSpeakerFrameList = Find.ListerThings.ThingsOfDef(ThingDef.Named("AlertSpeaker").frameDef);

            if (alertSpeakerList != null)
            {
                IEnumerable<Thing> alertSpeakerInTheArea = alertSpeakerList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenTwoAlertSpeakers));
                if (alertSpeakerInTheArea.Count() > 0)
                {
                    return new AcceptanceReport("An other alert speaker is too close.");
                }
            }
            if (alertSpeakerBlueprintList != null)
            {
                IEnumerable<Thing> alertSpeakerBlueprintInTheArea = alertSpeakerBlueprintList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenTwoAlertSpeakers));
                if (alertSpeakerBlueprintInTheArea.Count() > 0)
                {
                    return new AcceptanceReport("An other alert speaker blueprint is too close.");
                }
            }
            if (alertSpeakerFrameList != null)
            {
                IEnumerable<Thing> alertSpeakerFrameInTheArea = alertSpeakerFrameList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenTwoAlertSpeakers));
                if (alertSpeakerFrameInTheArea.Count() > 0)
                {
                    return new AcceptanceReport("An other alert speaker frame is too close.");
                }
            }

            // Check it is built near a wall.
            if (Building_AlertSpeaker.CheckIfSupportingWallIsAlive(loc, rot) == false)
            {
                return new AcceptanceReport("Alert speaker must be built near a wall.");
            }

            // Display effect zone.
            if (Find.ThingGrid.CellContains(loc, ThingCategory.Building) == false)
            {
                List<IntVec3> cellsInEffectZone = Building_AlertSpeaker.GetEffectZoneCells(loc);
                GenDraw.DrawFieldEdges(cellsInEffectZone);
            }

            return true;
        }
    }
}
