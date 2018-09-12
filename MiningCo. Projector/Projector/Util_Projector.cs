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
    /// Util_Projector utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public static class Util_Projector
    {
        // Projector tower.
        public static ThingDef ProjectorTowerDef
        {
            get
            {
                return ThingDef.Named("ProjectorTower");
            }
        }

        // Projector turret.
        public static ThingDef ProjectorTurretDef
        {
            get
            {
                return ThingDef.Named("ProjectorTurret");
            }
        }

        // Projector glower.
        public static ThingDef MobileProjectorLightDef
        {
            get
            {
                return ThingDef.Named("MobileProjectorLight");
            }
        }

        public static ThingDef FixedProjectorLightDef
        {
            get
            {
                return ThingDef.Named("FixedProjectorLight");
            }
        }

        // Blinded by Projector hediff.
        public static HediffDef BlindedByProjectorDef
        {
            get
            {
                return HediffDef.Named("BlindedByProjector");
            }
        }
    }
}