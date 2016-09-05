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
    /// ThingDef_FishSpecies custom variables class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class ThingDef_FishSpecies : ThingDef
    {
        // Note: the following false are only default values which can be overriden by the one read in the XML definition.
        public bool livesInSea = false;
        public bool livesInMarsh = false;
        public bool catchableDuringDay = false;
        public bool catchableDuringNight = false;
        public float commonality = 0.20f;
        public int catchQuantity = 1;
        public int breedQuantity = 0;
        public float breedingDurationInDays = 1f;
    }
}
