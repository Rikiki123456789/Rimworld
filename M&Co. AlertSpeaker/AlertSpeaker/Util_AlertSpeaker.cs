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
        // Stat bonus.
        public static ThingDef SmallAdrenalineBoostStatBonusDef
        {
            get
            {
                return (ThingDef.Named("SmallAdrenalineBoostStatBonus"));
            }
        }
        public static ThingDef MediumAdrenalineBoostStatBonusDef
        {
            get
            {
                return (ThingDef.Named("MediumAdrenalineBoostStatBonus"));
            }
        }
        public static ThingDef SmallStressStatMalusDef
        {
            get
            {
                return (ThingDef.Named("SmallStressStatMalus"));
            }
        }

        // Thought.
        public static ThoughtDef SmallAdrenalineBoostThoughtDef
        {
            get
            {
                return (ThoughtDef.Named("SmallAdrenalineBoostThought"));
            }
        }
        public static ThoughtDef MediumAdrenalineBoostThoughtDef
        {
            get
            {
                return (ThoughtDef.Named("MediumAdrenalineBoostThought"));
            }
        }
        public static ThoughtDef ColonyIsThreatenedThoughtDef
        {
            get
            {
                return (ThoughtDef.Named("ColonyIsThreatenedThought"));
            }
        }
        public static ThoughtDef ColonyIsUnderPressureThoughtDef
        {
            get
            {
                return (ThoughtDef.Named("ColonyIsUnderPressureThought"));
            }
        }
        public static ThoughtDef ThreatIsFinishedThoughtDef
        {
            get
            {
                return (ThoughtDef.Named("ThreatIsFinishedThought"));
            }
        }

        // Motes.
        public static ThingDef SmallAdrenalineBoostMoteDef
        {
            get
            {
                return (ThingDef.Named("Mote_SmallAdrenalineBoost"));
            }
        }
        public static ThingDef MediumAdrenalineBoostMoteDef
        {
            get
            {
                return (ThingDef.Named("Mote_MediumAdrenalineBoost"));
            }
        }
        public static ThingDef SmallStressMoteDef
        {
            get
            {
                return (ThingDef.Named("Mote_SmallStress"));
            }
        }
    }
}
