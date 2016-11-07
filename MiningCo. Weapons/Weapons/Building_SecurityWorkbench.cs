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

namespace Weapons
{
    /// <summary>
    /// SecurityWorkbench class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class Building_SecurityWorkbench : Building_WorkTable
    {
        public static void TryAddRecipesMakeWeaponPartsAndMakeLaserRifle()
        {
            ThingDef securityWorkbench = DefDatabase<ThingDef>.GetNamed("SecurityWorkbench");
            if ((Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchLaserRifle")) == true)
                && (securityWorkbench.recipes.Contains(DefDatabase<RecipeDef>.GetNamed("MakeLaserRifle")) == false))
            {
                securityWorkbench.recipes.Add(DefDatabase<RecipeDef>.GetNamed("MakeWeaponParts"));
                securityWorkbench.recipes.Add(DefDatabase<RecipeDef>.GetNamed("MakeLaserRifle"));
                typeof(ThingDef).GetField("allRecipesCached", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(securityWorkbench, null);
            }
        }

        public static void TryAddRecipesExtractCrysteelAndMakePrismRifle()
        {
            ThingDef securityWorkbench = DefDatabase<ThingDef>.GetNamed("SecurityWorkbench");
            if ((Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchPrismRifle")) == true)
                && (securityWorkbench.recipes.Contains(DefDatabase<RecipeDef>.GetNamed("MakePrismRifle")) == false))
            {
                securityWorkbench.recipes.Add(DefDatabase<RecipeDef>.GetNamed("ExtractCrysteel"));
                securityWorkbench.recipes.Add(DefDatabase<RecipeDef>.GetNamed("MakePrismRifle"));
                typeof(ThingDef).GetField("allRecipesCached", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(securityWorkbench, null);
            }
        }

        public static void TryAddRecipeMakeLaserGatling()
        {
            ThingDef securityWorkbench = DefDatabase<ThingDef>.GetNamed("SecurityWorkbench");
            if ((Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchLaserGatling")) == true)
                && (securityWorkbench.recipes.Contains(DefDatabase<RecipeDef>.GetNamed("MakeLaserGatling")) == false))
            {
                securityWorkbench.recipes.Add(DefDatabase<RecipeDef>.GetNamed("MakeLaserGatling"));
                typeof(ThingDef).GetField("allRecipesCached", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(securityWorkbench, null);
            }
        }

        public static void TryAddRecipeMakeHighPrecisionLasgun()
        {
            ThingDef securityWorkbench = DefDatabase<ThingDef>.GetNamed("SecurityWorkbench");
            if ((Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchHighPrecisionLasgun")) == true)
                && (securityWorkbench.recipes.Contains(DefDatabase<RecipeDef>.GetNamed("MakeHighPrecisionLasgun")) == false))
            {
                securityWorkbench.recipes.Add(DefDatabase<RecipeDef>.GetNamed("MakeHighPrecisionLasgun"));
                typeof(ThingDef).GetField("allRecipesCached", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(securityWorkbench, null);
            }
        }

        /// <summary>
        /// Spawns the worktable and add the recipes if available.
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            TryAddRecipesMakeWeaponPartsAndMakeLaserRifle();
            TryAddRecipesExtractCrysteelAndMakePrismRifle();
            TryAddRecipeMakeLaserGatling();
            TryAddRecipeMakeHighPrecisionLasgun();
        }
    }
}
