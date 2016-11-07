using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace Projectile_Laser
{
    /// <summary>
    /// Laser projectile ThingDef custom variables class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class ThingDef_LaserProjectile : ThingDef
    {
        public float preFiringInitialIntensity = 0f;
        public float preFiringFinalIntensity = 0f;
        public float postFiringInitialIntensity = 0f;
        public float postFiringFinalIntensity = 0f;
        public string warmupGraphicPathSingle = null;
        public int preFiringDuration = 0;
        public int postFiringDuration = 0;
    }
}
