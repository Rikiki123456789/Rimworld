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

namespace ForceField
{
    /// <summary>
    /// ForceFieldGenerator custom place worker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class PlaceWorker_ForceFieldGenerator : PlaceWorker
    {
        public const int minDistanceBetweenTwoForceFieldGenerators = 4;

        /// <summary>
        /// Checks if a new force field generator can be built at this location.
        /// - must not be too near from another force field generator (or it would perturb other fields).
        /// </summary>
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            // Check if another force field generator is not too close.
            List<Thing> forceFieldGeneratorList = new List<Thing>();
            IEnumerable<Thing> list = Find.ListerThings.ThingsOfDef(ThingDef.Named("ForceFieldGenerator"));
            foreach (Thing generator in list)
            {
                forceFieldGeneratorList.Add(generator);
            }
            list = Find.ListerThings.ThingsOfDef(ThingDef.Named("ForceFieldGenerator").blueprintDef);
            foreach (Thing generator in list)
            {
                forceFieldGeneratorList.Add(generator);
            }
            list = Find.ListerThings.ThingsOfDef(ThingDef.Named("ForceFieldGenerator").frameDef);
            foreach (Thing generator in list)
            {
                forceFieldGeneratorList.Add(generator);
            }

            foreach (Thing generator in forceFieldGeneratorList)
            {
                if (generator.Position.InHorDistOf(loc, minDistanceBetweenTwoForceFieldGenerators))
                {
                    return new AcceptanceReport("An other force field generator is too close (would generate perturbations).");
                }
            }

            // Display effect zone.
            List<IntVec3> coveredCells = Building_ForceFieldGenerator.GetCoveredCells(loc, rot);
            GenDraw.DrawFieldEdges(coveredCells);

            return true;
        }
    }
}
