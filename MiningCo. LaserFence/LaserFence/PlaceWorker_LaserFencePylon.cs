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
        public override AcceptanceReport  AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            string reason = "";
            bool canBePlacedHere = Building_LaserFencePylon.CanPlaceNewPylonHere(loc, out reason);
            if (canBePlacedHere == false)
            {
                return new AcceptanceReport(reason);
            }
            
            // Display potential placing positions.
            foreach (Thing pylon in Find.ListerThings.ThingsOfDef(ThingDef.Named("LaserFencePylon").blueprintDef))
            {
                if (pylon.Position.InHorDistOf(loc, 6f))
                {
                    Building_LaserFencePylon.DrawPotentialPlacePositions(pylon.Position);
                }
            }
            foreach (Thing pylon in Find.ListerThings.ThingsOfDef(ThingDef.Named("LaserFencePylon").frameDef))
            {
                if (pylon.Position.InHorDistOf(loc, 6f))
                {
                    Building_LaserFencePylon.DrawPotentialPlacePositions(pylon.Position);
                }
            }
            foreach (Thing pylon in Find.ListerThings.ThingsOfDef(ThingDef.Named("LaserFencePylon")))
            {
                if (pylon.Position.InHorDistOf(loc, 6f))
                {
                    Building_LaserFencePylon.DrawPotentialPlacePositions(pylon.Position);
                }
            }

            return true;
        }
    }
}
