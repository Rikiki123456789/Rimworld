﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// FishingPier custom PlaceWorker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class PlaceWorker_FishingPierSpawner : PlaceWorker
    {
        public const int minDistanceBetweenTwoFishingPiers = 15;
        private static int lastTextThrowTick = 0;

        /// <summary>
        /// Check if a new fishing pier can be built at this location.
        /// - the fishing pier bank cell must be on a bank.
        /// - the rest of the fishing pier and the fishing spot must be on water.
        /// - must not be too near from another fishing pier.
        /// </summary>
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
        {
            // Check this biome contains some fishes.
            if (Util_FishIndustry.GetFishSpeciesList(this.Map.Biome).NullOrEmpty())
            {
                return new AcceptanceReport("FishIndustry.FishingPier_InvalidBiome".Translate());
            }

            // Check fishing pier bank cell is on a "solid" terrain.
            if (Util_FishIndustry.IsAquaticTerrain(this.Map, loc))
            {
                return new AcceptanceReport("FishIndustry.FishingPier_MustTouchBank".Translate());
            }
            // Check fishing pier middle and river cells are on water.
            if ((Util_FishIndustry.IsAquaticTerrain(this.Map, loc + new IntVec3(0, 0, 1).RotatedBy(rot)) == false)
                || (Util_FishIndustry.IsAquaticTerrain(this.Map, loc + new IntVec3(0, 0, 2).RotatedBy(rot)) == false))
            {
                return new AcceptanceReport("FishIndustry.FishingPier_PierMustOnWater".Translate());
            }
            // Check fishing zone is on water.
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int yOffset = 3; yOffset <= 5; yOffset++)
                {
                    if (Util_FishIndustry.IsAquaticTerrain(this.Map, loc + new IntVec3(xOffset, 0, yOffset).RotatedBy(rot)) == false)
                    {
                        return new AcceptanceReport("FishIndustry.FishingPier_ZoneMustOnWater".Translate());
                    }
                }
            }

            // Check if another fishing pier is not too close (mind the test on "fishing pier" def and "fishing pier spawner" blueprint and frame defs.
            List<Thing> fishingPierList = this.Map.listerThings.ThingsOfDef(Util_FishIndustry.FishingPierDef);
            List<Thing> fishingPierSpawnerBlueprintList = this.Map.listerThings.ThingsOfDef(Util_FishIndustry.FishingPierSpawnerDef.blueprintDef);
            List<Thing> fishingPierSpawnerFrameList = this.Map.listerThings.ThingsOfDef(Util_FishIndustry.FishingPierSpawnerDef.frameDef);
            List<Thing> fishingPierSpawnerOnMudBlueprintList = this.Map.listerThings.ThingsOfDef(Util_FishIndustry.FishingPierSpawnerOnMudDef.blueprintDef);
            List<Thing> fishingPierSpawnerOnMudFrameList = this.Map.listerThings.ThingsOfDef(Util_FishIndustry.FishingPierSpawnerOnMudDef.frameDef);

            if (fishingPierList != null)
            {
                IEnumerable<Thing> fishingPierInTheArea = fishingPierList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenTwoFishingPiers));
                if (fishingPierInTheArea.Count() > 0)
                {
                    return new AcceptanceReport("FishIndustry.FishingPier_ToClose".Translate());
                }
            }
            if (fishingPierSpawnerBlueprintList != null)
            {
                IEnumerable<Thing> fishingPierBlueprintInTheArea = fishingPierSpawnerBlueprintList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenTwoFishingPiers));
                if (fishingPierBlueprintInTheArea.Count() > 0)
                {
                    return new AcceptanceReport("FishIndustry.FishingPier_ToCloseBlueprint".Translate());
                }
            }
            if (fishingPierSpawnerFrameList != null)
            {
                IEnumerable<Thing> fishingPierFrameInTheArea = fishingPierSpawnerFrameList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenTwoFishingPiers));
                if (fishingPierFrameInTheArea.Count() > 0)
                {
                    return new AcceptanceReport("FishIndustry.FishingPier_ToCloseFrame".Translate());
                }
            }
            if (fishingPierSpawnerOnMudBlueprintList != null)
            {
                IEnumerable<Thing> fishingPierOnMudBlueprintInTheArea = fishingPierSpawnerOnMudBlueprintList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenTwoFishingPiers));
                if (fishingPierOnMudBlueprintInTheArea.Count() > 0)
                {
                    return new AcceptanceReport("FishIndustry.FishingPier_ToCloseBlueprint".Translate());
                }
            }
            if (fishingPierSpawnerOnMudFrameList != null)
            {
                IEnumerable<Thing> fishingPierOnMudFrameInTheArea = fishingPierSpawnerOnMudFrameList.Where(building => loc.InHorDistOf(building.Position, minDistanceBetweenTwoFishingPiers));
                if (fishingPierOnMudFrameInTheArea.Count() > 0)
                {
                    return new AcceptanceReport("FishIndustry.FishingPier_ToCloseFrame".Translate());
                }
            }

            // Display fish stock respawn rate.
            if ((Find.TickManager.Paused == false)
                && (Find.TickManager.TicksGame > lastTextThrowTick + Find.TickManager.TickRateMultiplier * Verse.GenTicks.TicksPerRealSecond))
            {
                lastTextThrowTick = Find.TickManager.TicksGame;
                float fishStockRespawnRateAsFloat = Util_FishIndustry.GetAquaticCellsProportionInRadius(loc + new IntVec3(0, 0, 1).RotatedBy(rot), this.Map, Building_FishingPier.optimalAquaticAreaRadius) / Building_FishingPier.optimalAquaticCellsProportion;
                int fishStockRespawnRateAsInt = Mathf.RoundToInt(fishStockRespawnRateAsFloat * 100f);
                if (fishStockRespawnRateAsInt > 100)
                {
                    fishStockRespawnRateAsInt = 100;
                }
                Color textColor = Color.red;
                if (fishStockRespawnRateAsInt >= 75)
                {
                    textColor = Color.green;
                }
                else if (fishStockRespawnRateAsInt >= 25)
                {
                    textColor = Color.yellow;
                }
                string fishStockRespawnRateAsText = fishStockRespawnRateAsInt + "%";
                MoteMaker.ThrowText(loc.ToVector3Shifted(), this.Map, fishStockRespawnRateAsText, textColor);
            }

            return true;
        }
    }
}
