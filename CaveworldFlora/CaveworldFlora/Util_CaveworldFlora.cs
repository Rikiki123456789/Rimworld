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
    /// Util_CaveworldFlora utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class Util_CaveworldFlora
    {
        // Fungiponics buiding.
        public static ThingDef fungiponicsBasinDef
        {
            get
            {
                return (ThingDef.Named("FungiponicsBasin"));
            }
        }
        // Glowers.
        public static ThingDef GetGlowerStaticDef(ThingDef plantDef)
        {
            string glowerDefName = plantDef.defName + "Glower";
            return (ThingDef.Named(glowerDefName));
        }
        public static ThingDef GetGlowerSmallDef(ThingDef plantDef)
        {
            string glowerDefName = plantDef.defName + "GlowerSmall";
            return (ThingDef.Named(glowerDefName));
        }
        public static ThingDef GetGlowerMediumDef(ThingDef plantDef)
        {
            string glowerDefName = plantDef.defName + "GlowerMedium";
            return (ThingDef.Named(glowerDefName));
        }
        public static ThingDef GetGlowerBigDef(ThingDef plantDef)
        {
            string glowerDefName = plantDef.defName + "GlowerBig";
            return (ThingDef.Named(glowerDefName));
        }

        // Spore spawner.
        public static ThingDef gleamcapSporeSpawnerDef
        {
            get
            {
                return (ThingDef.Named("GleamcapSporeSpawner"));
            }
        }

        // HediffDef.
        public static HediffDef gleamcapSmokeDef
        {
            get
            {
                return (HediffDef.Named("HediffGleamcapSmoke"));
            }
        }

        // Cluster.
        public static ThingDef ClusterDef
        {
            get
            {
                return ThingDef.Named("Cluster");
            }
        }
        
        // Mote.
        public static ThingDef MotePoisonSmokeDef
        {
            get
            {
                return ThingDef.Named("Mote_PoisonSmoke");
            }
        }
    }
}
