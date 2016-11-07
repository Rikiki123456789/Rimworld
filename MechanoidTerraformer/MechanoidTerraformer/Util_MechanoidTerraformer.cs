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

namespace MechanoidTerraformer
{
    /// <summary>
    /// MechanoidTerraformer utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class Util_MechanoidTerraformer
    {
        public static ThingDef MechanoidTerraformerDef
        {
            get
            {
                return ThingDef.Named("MechanoidTerraformer");
            }
        }

        public static ThingDef MechanoidTerraformerIncomingDef
        {
            get
            {
                return ThingDef.Named("MechanoidTerraformerIncoming");
            }
        }

        public static ThingDef MechanoidPylonDef
        {
            get
            {
                return ThingDef.Named("MechanoidPylon");
            }
        }

        public static ThingDef MechanoidPylonDestructibleDef
        {
            get
            {
                return ThingDef.Named("MechanoidPylonDestructible");
            }
        }

        public static WeatherDef TerraformingThunderstormDef
        {
            get
            {
                return WeatherDef.Named("TerraformingThunderstorm");
            }
        }

        public static ThingDef WeatherControllerCoreDef
        {
            get
            {
                return ThingDef.Named("WeatherControllerCore");
            }
        }

        public static string JobDefName_ScoutStrangeArtifact = "JobDef_ScoutStrangeArtifact";
        public static string JobDefName_SecureStrangeArtifact = "JobDef_SecureStrangeArtifact";
        public static string JobDefName_StudyStrangeArtifact = "JobDef_StudyStrangeArtifact";
        public static string JobDefName_ReroutePower = "JobDef_ReroutePower";
        public static string JobDefName_ExtractWeatherController = "JobDef_ExtractWeatherController";
        public static string JobDefName_DisableBeacon = "JobDef_DisableBeacon";
    }
}
