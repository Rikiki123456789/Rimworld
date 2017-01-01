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

namespace FishIndustry
{
    /// <summary>
    /// AquacultureHopper custom PlaceWorker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class PlaceWorker_AquacultureHopper : PlaceWorker
    {
        /// <summary>
        /// Check the aquaculture hopper is placed next to an aquaculture basin.
        /// </summary>
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                IntVec3 cell = loc + GenAdj.CardinalDirections[directionAsInt];
                if (cell.InBounds(this.Map))
                {
                    Building building = cell.GetEdifice(this.Map);
                    if ((building != null)
                        && (building.def == Util_FishIndustry.AquacultureBasinDef))
                    {
                        return true;
                    }
                }
            }
            return new AcceptanceReport("Aquaculture hopper must be placed next to an aquaculture basin.");
        }
    }
}
