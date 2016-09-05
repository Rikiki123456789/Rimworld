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
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class ForceFieldGeneratorProperties
    {
        // Power consumption.
        public const int powerOutputDuringInitialization = -125;
        public const int powerOutputDuringCharge = -1000;
        public const int powerOutputDuringSustain = -250;
        public const int powerOutputDuringDischarge = 500;

        // Durations.
        public const int initializationDurationInTicks = 300;
        public const int chargeDurationInTicks = 3000;
        public const int dischargeDurationInTicks = 1500;

        // Force field charge.
        public const float forceFieldMaxCharge = 250f;
        public const float rocketAbsorbtionProportion = 0.5f; // Proportion of the force field max charge necessary to absorb a rocket.
        public const float explosiveRepelCharge = 40f; // Energy cost to repel an explosive (grenade).
    }
}
