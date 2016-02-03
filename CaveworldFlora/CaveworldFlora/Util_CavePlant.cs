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

namespace CaveworldFlora
{
    /// <summary>
    /// CavePlant utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class Util_CavePlant
    {
        // Fungiponics buiding.
        public static ThingDef fungiponicsBasinDef
        {
            get
            {
                return (ThingDef.Named("FungiponicsBasin"));
            }
        }
        // Glower buidings.
        public static ThingDef GetGlowerSmallDef(ThingDef cavePlantDef)
        {
            string glowerDefName = cavePlantDef.defName + "GlowerSmall";
            return (ThingDef.Named(glowerDefName));
        }
        public static ThingDef GetGlowerMediumDef(ThingDef cavePlantDef)
        {
            string glowerDefName = cavePlantDef.defName + "GlowerMedium";
            return (ThingDef.Named(glowerDefName));
        }
        public static ThingDef GetGlowerBigDef(ThingDef cavePlantDef)
        {
            string glowerDefName = cavePlantDef.defName + "GlowerBig";
            return (ThingDef.Named(glowerDefName));
        }

        // Spore spawner buiding.
        public static ThingDef gleamcapSporeSpawnerDef
        {
            get
            {
                return (ThingDef.Named("GleamcapSporeSpawner"));
            }
        }

        // Thoughts.
        public static ThoughtDef breathedGleamcapSmokeDef
        {
            get
            {
                return (ThoughtDef.Named("BreathedGleamcapSmoke"));
            }
        }
    }
}
