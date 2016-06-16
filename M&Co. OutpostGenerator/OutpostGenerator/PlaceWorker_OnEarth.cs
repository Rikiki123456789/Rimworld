using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Dirt floor place worker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class PlaceWorker_OnEarth : PlaceWorker
    {
        /// <summary>
        /// Restrict placement over non stony floors.
        /// </summary>
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            TerrainDef terrain = loc.GetTerrain();
            if ((terrain == TerrainDef.Named("Soil"))
                || (terrain == TerrainDef.Named("SoilRich"))
                || (terrain == TerrainDef.Named("Gravel"))
                || (terrain == TerrainDef.Named("MossyTerrain"))
                || (terrain == TerrainDef.Named("MarshyTerrain")))
            {
                return true;
            }
            return new AcceptanceReport("Dirt floor must be placed on soft soil.");
        }
    }
}
