using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace LaserFence
{
    /// <summary>
    /// PlaceWorker_LaserFence custom place worker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class PlaceWorker_LaserFencePylon : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            // Display potential build cells.
            foreach (Thing pylon in map.listerThings.ThingsOfDef(Util_LaserFence.LaserFencePylonDef.blueprintDef))
            {
                if (pylon.Position.InHorDistOf(loc, 6f))
                {
                    Building_LaserFencePylon.DrawPotentialBuildCells(map, pylon.Position);
                }
            }
            foreach (Thing pylon in map.listerThings.ThingsOfDef(Util_LaserFence.LaserFencePylonDef.frameDef))
            {
                if (pylon.Position.InHorDistOf(loc, 6f))
                {
                    Building_LaserFencePylon.DrawPotentialBuildCells(map, pylon.Position);
                }
            }
            foreach (Thing pylon in map.listerThings.ThingsOfDef(Util_LaserFence.LaserFencePylonDef))
            {
                if (pylon.Position.InHorDistOf(loc, 6f))
                {
                    Building_LaserFencePylon.DrawPotentialBuildCells(map, pylon.Position);
                }
            }
            return true;
        }
    }
}
