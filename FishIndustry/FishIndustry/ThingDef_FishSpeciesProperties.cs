using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// ThingDef_FishSpeciesProperties custom variables class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class ThingDef_FishSpeciesProperties : ThingDef
    {
        // Environment.
        public enum AquaticEnvironment
        {
            Sea = 1,
            Marsh = 2,
            SeaAndMarch = 3
        }

        // Environment.
        public enum LivingTime
        {
            Day = 1,
            Night = 2,
            DayAndNight = 3
        }

        // Note: the following values are only default values which can be ovveriden by the one read in the XML definition.

        public AquaticEnvironment aquaticEnvironment = AquaticEnvironment.SeaAndMarch;
        public LivingTime livingTime = LivingTime.DayAndNight;
        public float commonality = 0.20f;
        public int catchQuantity = 1;
        public int breedQuantity = 0;
        public float breedingDurationInDays = 1f;
    }
}
