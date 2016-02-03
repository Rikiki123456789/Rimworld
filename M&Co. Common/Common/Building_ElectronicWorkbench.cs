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

namespace Common
{
    /// <summary>
    /// Building_ElectronicWorkbench class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class Building_ElectronicWorkbench : Building_WorkTable_HeatPush
    {
        public static void TryAddRecipeMakeMineralSonarModule()
        {
            if (ModsConfig.IsActive("M&Co. MMS"))
            {
                ThingDef electronicWorkbench = DefDatabase<ThingDef>.GetNamed("ElectronicWorkbench");
                if ((Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchMobileMineralSonar")) == true)
                    && (electronicWorkbench.recipes.Contains(DefDatabase<RecipeDef>.GetNamed("MakeMineralSonarModule")) == false))
                {
                    electronicWorkbench.recipes.Add(DefDatabase<RecipeDef>.GetNamed("MakeMineralSonarModule"));
                    typeof(ThingDef).GetField("allRecipesCached", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(electronicWorkbench, null);
                }
            }
        }
        
        /// <summary>
        /// Spawn the worktable and add the recipes if available.
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            TryAddRecipeMakeMineralSonarModule();
        }
    }
}
