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

namespace Projector
{
    /// <summary>
    /// PlaceWorker_DrawLightedCells class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class PlaceWorker_DrawLightedCells : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol)
        {
            List<IntVec3> lightedCellsList = Building_FixedProjector.GetLightedCells(Find.CurrentMap, loc, rot);
            GenDraw.DrawFieldEdges(lightedCellsList);
        }
    }
}