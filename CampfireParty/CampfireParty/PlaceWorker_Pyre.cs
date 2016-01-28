using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace CampfireParty
{
    /// <summary>
    /// PlaceWorker_Pyre custom place worker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class PlaceWorker_Pyre : PlaceWorker_NotUnderRoof
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            AcceptanceReport acceptanceReport = base.AllowsPlacing(checkingDef, loc, rot);
            if (acceptanceReport.Accepted == false)
            {
                return acceptanceReport;
            }

            // Draw party and beer search area.
            CampfireParty.Building_Pyre.DrawPartyAndBeerSearchAreas(loc);

            return true;
        }
    }
}
