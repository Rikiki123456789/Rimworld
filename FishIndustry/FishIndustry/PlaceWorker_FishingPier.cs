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

namespace FishIndustry
{
    /// <summary>
    /// PlaceWorker_FishingPier class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class PlaceWorker_FishingPier : PlaceWorker
    {
        public static int lastMoteUpdateSecond = 0;
        public static int lastMoteUpdateTick = 0;
        public static IntVec3 lastMotePosition = IntVec3.Invalid;

        /// <summary>
        /// Check if a new fishing pier can be built at this location.
        /// - the fishing pier bank cell must be on a bank.
        /// - the rest of the fishing pier and the fishing spot must be on water.
        /// - must not be too near another fishing pier.
        /// </summary>
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            // Remove old fish stock respawn rate text mote.
            if (lastMotePosition.IsValid
                && (loc != lastMotePosition))
            {
                RemoveLastRespawnRateMote(map);
            }

            // Check this biome contains some fishes.
            if (Util_FishIndustry.GetFishSpeciesList(map.Biome).NullOrEmpty())
            {
                return new AcceptanceReport("FishIndustry.FishingPier_InvalidBiome".Translate());
            }

            // Check if another fishing pier is not too close.
            if (Util_PlaceWorker.IsNearFishingPier(map, loc, Util_PlaceWorker.minDistanceBetweenTwoFishingSpots))
            {
                return new AcceptanceReport("FishIndustry.TooCloseFishingPier".Translate());
            }

            // Check if a fishing zone is not too close.
            if (Util_PlaceWorker.IsNearFishingZone(map, loc, Util_PlaceWorker.minDistanceBetweenTwoFishingSpots))
            {
                return new AcceptanceReport("FishIndustry.TooCloseFishingZone".Translate());
            }

            // Check fishing pier is on water.
            if ((Util_Zone_Fishing.IsAquaticTerrain(map, loc + new IntVec3(0, 0, -1).RotatedBy(rot)) == false)
                || (Util_Zone_Fishing.IsAquaticTerrain(map, loc + new IntVec3(0, 0, 0).RotatedBy(rot)) == false)
                || (Util_Zone_Fishing.IsAquaticTerrain(map, loc + new IntVec3(0, 0, 1).RotatedBy(rot)) == false))
            {
                return new AcceptanceReport("FishIndustry.FishingPier_PierMustBeOnWater".Translate());
            }

            // Check fishing zone is on water.
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int yOffset = 2; yOffset <= 4; yOffset++)
                {
                    if (Util_Zone_Fishing.IsAquaticTerrain(map, loc + new IntVec3(xOffset, 0, yOffset).RotatedBy(rot)) == false)
                    {
                        return new AcceptanceReport("FishIndustry.FishingPier_ZoneMustBeOnWater".Translate());
                    }
                }
            }

            // Display fish stock respawn rate.
            if ((lastMotePosition.IsValid == false)
                || (Find.TickManager.Paused
                    && (DateTime.Now.Second != lastMoteUpdateSecond))
                || ((Find.TickManager.Paused == false)
                    && (Find.TickManager.TicksGame >= lastMoteUpdateTick + GenTicks.TicksPerRealSecond)))
            {
                lastMoteUpdateSecond = DateTime.Now.Second;
                lastMoteUpdateTick = Find.TickManager.TicksGame;
                RemoveLastRespawnRateMote(map);
                DisplayMaxFishStockMoteAt(map, loc, rot);
            }

            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
        {
            base.DrawGhost(def, center, rot, ghostCol);
            SimpleColor circleColor = SimpleColor.Red;
            if (ghostCol == Designator_Place.CanPlaceColor)
            {
                circleColor = SimpleColor.Green;
            }
            GenDraw.DrawCircleOutline(center.ToVector3Shifted() + new Vector3(0, AltitudeLayer.MetaOverlays.AltitudeFor(), 3).RotatedBy(rot.AsAngle), 1.5f, circleColor);
        }

        public void RemoveLastRespawnRateMote(Map map)
        {
            if (lastMotePosition.IsValid
                && lastMotePosition.InBounds(map))
            {
                foreach (Thing thing in lastMotePosition.GetThingList(map))
                {
                    if (thing is MoteText)
                    {
                        thing.Destroy();
                        break;
                    }
                }
            }
            lastMotePosition = IntVec3.Invalid;
        }

        public void DisplayMaxFishStockMoteAt(Map map, IntVec3 position, Rot4 rotation)
        {
            if (position.InBounds(map) == false)
            {
                return;
            }
            // Get aquatic cells around the fishing spot.
            int aquaticCellsAround = Util_PlaceWorker.GetAquaticCellsInRadius(map, position + new IntVec3(0, 0, 2).RotatedBy(rotation), Building_FishingPier.aquaticAreaRadius) - 3; // 3 cells will actually be occupied by the pier.
            int aquaticCellsProportionInPercent = Mathf.RoundToInt(((float)aquaticCellsAround / (float)(GenRadial.NumCellsInRadius(Building_FishingPier.aquaticAreaRadius) - 3)) * 100f);

            Color textColor = Color.red;
            string aquaticCellsProportionAsText = "";
            if (aquaticCellsAround < Util_Zone_Fishing.minCellsToSpawnFish)
            {
                aquaticCellsProportionAsText = "FishIndustry.FishingPier_TooFewWater".Translate();
            }
            else
            {
                aquaticCellsProportionAsText = aquaticCellsProportionInPercent + "%";
                if (aquaticCellsProportionInPercent >= 75)
                {
                    textColor = Color.green;
                }
                else if (aquaticCellsProportionInPercent >= 50)
                {
                    textColor = Color.yellow;
                }
                else if (aquaticCellsProportionInPercent >= 25)
                {
                    textColor = new Color(255, 165, 0); // Orange color.
                }
            }

            MoteText moteText = (MoteText)ThingMaker.MakeThing(ThingDefOf.Mote_Text, null);
            moteText.exactPosition = position.ToVector3Shifted();
            moteText.text = aquaticCellsProportionAsText;
            moteText.textColor = textColor;
            moteText.overrideTimeBeforeStartFadeout = 1f;
            GenSpawn.Spawn(moteText, position, map);
            lastMotePosition = position;
        }
    }
}
