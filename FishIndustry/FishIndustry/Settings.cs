using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace FishIndustry
{
    public class Settings : ModSettings
    {
        public static float fishRespawnRateFactor = 1f;
        public static float fishBreedQuantityFactor = 1f;
        public static bool biomeRestrictionsIsEnabled = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref fishRespawnRateFactor, "fishRespawnRateFactor", 1f);
            Scribe_Values.Look<float>(ref fishBreedQuantityFactor, "fishBreedQuantityFactor", 1f);
            Scribe_Values.Look<bool>(ref biomeRestrictionsIsEnabled, "biomeRestrictionsIsEnabled", true);
        }

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.ColumnWidth = inRect.width / 2f;
            list.Begin(inRect);
            list.Label("FishIndustry.Settings_FishRespawnRateFactorLabel".Translate(fishRespawnRateFactor.ToString("0.0")), -1f, "FishIndustry.Settings_FishRespawnRateFactorDesc".Translate());
            fishRespawnRateFactor = list.Slider(fishBreedQuantityFactor, 0.1f, 10f);
            list.Label("FishIndustry.Settings_FishBreedQuantityFactorLabel".Translate(fishBreedQuantityFactor.ToString("0.0")), -1f, "FishIndustry.Settings_FishBreedQuantityFactorDesc".Translate());
            fishRespawnRateFactor = list.Slider(fishBreedQuantityFactor, 0.1f, 10f);
            list.Gap(12f);
            list.CheckboxLabeled("FishIndustry.Settings_BiomeRestrictionLabel".Translate(), ref biomeRestrictionsIsEnabled, "FishIndustry.Settings_BiomeRestrictionDesc".Translate());
            list.End();
        }
    }
}
