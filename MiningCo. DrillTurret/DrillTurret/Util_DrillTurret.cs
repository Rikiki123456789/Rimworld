using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace MiningTurret
{
    /// <summary>
    /// DrillTurret utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class Util_DrillTurret
    {
        public static ThingDef drillTurretDef
        {
            get
            {
                return ThingDef.Named("MiningTurret");
            }
        }

        public static JobDef operateDrillTurretJobDef
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("OperateDrillTurret");
            }
        }

        public static ResearchProjectDef researchMiningTurretEfficientDrillingDef
        {
            get
            {
                return ResearchProjectDef.Named("ResearchMiningTurretEfficientDrilling");
            }
        }
    }
}

