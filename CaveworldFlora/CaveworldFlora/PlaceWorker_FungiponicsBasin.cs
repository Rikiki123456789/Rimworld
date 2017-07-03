﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace CaveworldFlora
{
    /// <summary>
    /// PlaceWorker_FungiponicsBasin custom PlaceWorker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class PlaceWorker_FungiponicsBasin : PlaceWorker
    {
        public const float minDistanceBetweenFungiponicsBasins = 5.9f;

        /// <summary>
        /// Check if a new fungiponics basin can be built at this location.
        /// - the fungiponics basin must be roofed.
        /// - must not be too near from another fungiponics basin.
        /// </summary>
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
        {
            foreach (IntVec3 cell in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size))
            {
                if (ClusterPlant.IsNaturalRoughRockAt(base.Map, cell) == false)
                {
                    return new AcceptanceReport("CaveworldFlora.MustOnRoughRock".Translate());
                }
                if (base.Map.roofGrid.Roofed(loc) == false)
                {
                    return new AcceptanceReport("CaveworldFlora.MustBeRoofed".Translate());
                }
            }

            List<Thing> fungiponicsBasinsList = new List<Thing>();
            IEnumerable<Thing> list = base.Map.listerThings.ThingsOfDef(ThingDef.Named("FungiponicsBasin"));
            foreach (Thing basin in list)
            {
                fungiponicsBasinsList.Add(basin);
            }
            list = base.Map.listerThings.ThingsOfDef(ThingDef.Named("FungiponicsBasin").blueprintDef);
            foreach (Thing basin in list)
            {
                fungiponicsBasinsList.Add(basin);
            }
            list = base.Map.listerThings.ThingsOfDef(ThingDef.Named("FungiponicsBasin").frameDef);
            foreach (Thing basin in list)
            {
                fungiponicsBasinsList.Add(basin);
            }
            foreach (Thing basin in fungiponicsBasinsList)
            {
                if (basin.Position.InHorDistOf(loc, minDistanceBetweenFungiponicsBasins))
                {
                    return new AcceptanceReport("CaveworldFlora.TooClose".Translate());
                }
            }

            return true;
        }
    }
}
