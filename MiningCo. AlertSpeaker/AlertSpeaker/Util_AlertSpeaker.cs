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

namespace AlertSpeaker
{
    /// <summary>
    /// AlertSpeaker utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class Util_AlertSpeaker
    {
        // HediffDefs.
        public static HediffDef HediffAdrenalineSmallDef
        {
            get
            {
                return HediffDef.Named("HediffAdrenalineSmall");
            }
        }

        public static HediffDef HediffAdrenalineMediumDef
        {
            get
            {
                return HediffDef.Named("HediffAdrenalineMedium");
            }
        }
    }
}
