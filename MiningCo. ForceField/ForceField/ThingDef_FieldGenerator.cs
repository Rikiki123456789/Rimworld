using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound;   // Needed when you do something with the Sound

namespace ForceField
{
    /// <summary>
    /// Force field ThingDef custom variables class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class ThingDef_FieldGenerator : ThingDef
    {
        // Power consumption.
        public int powerOutputDuringInitialization = -250;
        public int powerOutputDuringCharge = -2000;
        public int powerOutputDuringSustain = -250;

        // Durations.
        public int initializationDurationInTicks = 300;
        public int chargeDurationInTicks = 3000;
        public int dischargeDurationInTicks = 12000;

        // Force field charge.
        public float forceFieldMaxCharge = 250f;
        public float rocketAbsorbtionProportion = 0.5f; // Proportion of the force field max charge necessary to absorb a rocket.
        public float explosiveRepelCharge = 40f; // Energy cost to repel an explosive (grenade).
    }
}
