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
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class PlaceWorker_LaserFencePylon : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
        {
            Map currentMap = Find.CurrentMap;

            // Display potential build cells.
            foreach (Thing pylon in currentMap.listerThings.ThingsOfDef(Util_LaserFence.LaserFencePylonDef.blueprintDef))
            {
                if (pylon.Position.InHorDistOf(center, Settings.laserFenceMaxRange + 2f))
                {
                    Building_LaserFencePylon.DrawPotentialBuildCells(currentMap, pylon.Position);
                }
            }
            foreach (Thing pylon in currentMap.listerThings.ThingsOfDef(Util_LaserFence.LaserFencePylonDef.frameDef))
            {
                if (pylon.Position.InHorDistOf(center, Settings.laserFenceMaxRange + 2f))
                {
                    Building_LaserFencePylon.DrawPotentialBuildCells(currentMap, pylon.Position);
                }
            }
            foreach (Thing pylon in currentMap.listerThings.ThingsOfDef(Util_LaserFence.LaserFencePylonDef))
            {
                if (pylon.Position.InHorDistOf(center, Settings.laserFenceMaxRange + 2f))
                {
                    Building_LaserFencePylon.DrawPotentialBuildCells(currentMap, pylon.Position);
                }
            }
        }
    }
}
