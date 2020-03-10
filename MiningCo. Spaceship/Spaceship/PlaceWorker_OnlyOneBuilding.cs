using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{
    public class PlaceWorker_OnlyOneBuilding : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            List<Thing> blueprints = map.listerThings.ThingsOfDef(checkingDef.blueprintDef);
            List<Thing> frames = map.listerThings.ThingsOfDef(checkingDef.frameDef);
            if (((blueprints != null) && (blueprints.Count > 0))
                || ((frames != null) && (frames.Count > 0))
                || map.listerBuildings.ColonistsHaveBuilding(ThingDef.Named(checkingDef.defName)))
            {
                return "You can only build one " + checkingDef.defName + " per map.";
            }
            return true;
        }
    }
}
