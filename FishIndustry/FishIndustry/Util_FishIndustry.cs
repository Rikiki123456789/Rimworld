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
    /// FishIndustry utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class Util_FishIndustry
    {
        // TODO: damage equipment when catching big fish?
        // TODO: add aquariums for sduiggles

        // Building.
        public static ThingDef FishingPierDef
        {
            get
            {
                return (ThingDef.Named("FishingPier"));
            }
        }
        public static ThingDef FishingPierSpawnerDef
        {
            get
            {
                return (ThingDef.Named("FishingPierSpawner"));
            }
        }
        public static ThingDef AquacultureBasinDef
        {
            get
            {
                return (ThingDef.Named("AquacultureBasin"));
            }
        }
        public static ThingDef AquacultureHopperDef
        {
            get
            {
                return (ThingDef.Named("AquacultureHopper"));
            }
        }

        // Recipe.
        public static RecipeDef SupplyMashgonEggsRecipeDef
        {
            get
            {
                return (DefDatabase<RecipeDef>.GetNamed("SupplyMashgonEggs"));
            }
        }
        public static RecipeDef SupplyBluebladeEggsRecipeDef
        {
            get
            {
                return (DefDatabase<RecipeDef>.GetNamed("SupplyBluebladeEggs"));
            }
        }
        public static RecipeDef SupplyTailteethEggsRecipeDef
        {
            get
            {
                return (DefDatabase<RecipeDef>.GetNamed("SupplyTailteethEggs"));
            }
        }

        // Apparel.
        public static ThingDef FishingRodDef
        {
            get
            {
                return (ThingDef.Named("FishingRod"));
            }
        }

        // Equipment.
        public static ThingDef HarpoonDef
        {
            get
            {
                return (ThingDef.Named("Harpoon"));
            }
        }

        // Terrain.
        public static TerrainDef FishingPierFloorDeepWaterDef
        {
            get
            {
                return (TerrainDef.Named("FishingPierFloorDeepWater"));
            }
        }
        public static TerrainDef FishingPierFloorShallowWaterDef
        {
            get
            {
                return (TerrainDef.Named("FishingPierFloorShallowWater"));
            }
        }
        public static TerrainDef FishingPierFloorMarshDef
        {
            get
            {
                return (TerrainDef.Named("FishingPierFloorMarsh"));
            }
        }

        // Mote.
        public static ThingDef MoteFishingRodNorthDef
        {
            get
            {
                return (ThingDef.Named("Mote_FishingRodNorth"));
            }
        }
        public static ThingDef MoteFishingRodEastDef
        {
            get
            {
                return (ThingDef.Named("Mote_FishingRodEast"));
            }
        }
        public static ThingDef MoteFishingRodSouthDef
        {
            get
            {
                return (ThingDef.Named("Mote_FishingRodSouth"));
            }
        }
        public static ThingDef MoteFishingRodWestDef
        {
            get
            {
                return (ThingDef.Named("Mote_FishingRodWest"));
            }
        }

        public static ThingDef MoteBubbleDef
        {
            get
            {
                return (ThingDef.Named("Mote_Bubble"));
            }
        }

        // Sea products.
        public static ThingDef OysterDef
        {
            get
            {
                return (ThingDef.Named("Oyster"));
            }
        }
        
        public static ThingDef PearlDef
        {
            get
            {
                return (ThingDef.Named("Pearl"));
            }
        }

        // Stat.
        public static StatDef FishingSpeedDef
        {
            get
            {
                return (StatDef.Named("FishingSpeed"));
            }
        }

        // Job.
        public static string JobDefName_FishAtFishingPier = "JobDef_FishAtFishingPier";

        public static string JobDefName_HarvestAquacultureBasinProduction = "JobDef_HarvestAquacultureBasinProduction";

        // Breedable fishes def.
        public static ThingDef MashgonDef
        {
            get
            {
                return (ThingDef.Named("Fish_Mashgon"));
            }
        }

        public static ThingDef BluebladeDef
        {
            get
            {
                return (ThingDef.Named("Fish_Blueblade"));
            }
        }

        public static ThingDef TailteethDef
        {
            get
            {
                return (ThingDef.Named("Fish_Tailteeth"));
            }
        }

        // Texture path.
        public static string MashgonTexturePath
        {
            get
            {
                return ThingDef.Named("Fish_Mashgon").graphicData.texPath;
            }
        }

        public static string BluebladeTexturePath
        {
            get
            {
                return ThingDef.Named("Fish_Blueblade").graphicData.texPath;
            }
        }

        public static string TailteethTexturePath
        {
            get
            {
                return ThingDef.Named("Fish_Tailteeth").graphicData.texPath;
            }
        }

        // Fishes lists.
        private static List<ThingDef_FishSpecies> fishSpeciesList = null;
        public static List<ThingDef_FishSpecies> GetFishSpeciesList()
        {
            if (fishSpeciesList.NullOrEmpty())
            {
                fishSpeciesList = new List<ThingDef_FishSpecies>();

                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (def is ThingDef_FishSpecies)
                    {
                        fishSpeciesList.Add(def as ThingDef_FishSpecies);
                    }
                }
                if (fishSpeciesList.NullOrEmpty())
                {
                    Log.Warning("FishIndustry: did not found any fish species.");
                }
                //TODO: debug.
                Log.Message("Number of species = " + fishSpeciesList.Count);
            }

            return fishSpeciesList;
        }
    }
}
