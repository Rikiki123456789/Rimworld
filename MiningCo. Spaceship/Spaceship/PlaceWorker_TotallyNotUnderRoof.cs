using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;    // Always needed
using RimWorld;       // RimWorld specific functions are found here
using Verse;          // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI
using Verse.AI.Group; // Needed when you do something with the AI
using Verse.Sound;    // Needed when you do something with the Sound

namespace Spaceship
{
    public class PlaceWorker_TotallyNotUnderRoof : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
            foreach (IntVec3 cell in GenAdj.OccupiedRect(loc, rot, checkingDef.Size).Cells)
			{
                if (cell.Roofed(map))
                {
                    return new AcceptanceReport("MustPlaceUnroofed".Translate());
                }
            }
			return true;
		}
    }
}
