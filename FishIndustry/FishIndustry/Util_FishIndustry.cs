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

        // Fishes lists.
        private static bool fishSpeciesListsAreInitialized = false;
        private static List<ThingDef> seaDayFishSpeciesList = null;
        private static List<ThingDef> seaNightFishSpeciesList = null;
        private static List<ThingDef> marshDayFishSpeciesList = null;
        private static List<ThingDef> marshNightFishSpeciesList = null;
        private static float seaDayFishSpeciesTotalCommonality = 0;
        private static float seaNightFishSpeciesTotalCommonality = 0;
        private static float marshDayFishSpeciesTotalCommonality = 0;
        private static float marshNightFishSpeciesTotalCommonality = 0;
        public static List<ThingDef> GetFishSpeciesList(ThingDef_FishSpeciesProperties.AquaticEnvironment aquaticEnvironment, ThingDef_FishSpeciesProperties.LivingTime livingTime)
        {
            if (fishSpeciesListsAreInitialized == false)
            {
                InitializeFishSpeciesListsAndTotalCommonality();
                fishSpeciesListsAreInitialized = true;
            }
            if ((aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.Sea)
                && (livingTime == ThingDef_FishSpeciesProperties.LivingTime.Day))
            {
                return seaDayFishSpeciesList;
            }
            else if ((aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.Sea)
                && (livingTime == ThingDef_FishSpeciesProperties.LivingTime.Night))
            {
                return seaNightFishSpeciesList;
            }
            else if ((aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.Marsh)
                && (livingTime == ThingDef_FishSpeciesProperties.LivingTime.Day))
            {
                return marshDayFishSpeciesList;
            }
            else if ((aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.Marsh)
                && (livingTime == ThingDef_FishSpeciesProperties.LivingTime.Night))
            {
                return marshNightFishSpeciesList;
            }
            else
            {
                Log.Warning("FishIndustry: this aquatic environment/living time combination is not implemented.");
                return null;
            }
        }

        private static void InitializeFishSpeciesListsAndTotalCommonality()
        {
            seaDayFishSpeciesTotalCommonality = 0;
            seaNightFishSpeciesTotalCommonality = 0;
            marshDayFishSpeciesTotalCommonality = 0;
            marshNightFishSpeciesTotalCommonality = 0;
            seaDayFishSpeciesList = new List<ThingDef>();
            seaNightFishSpeciesList = new List<ThingDef>();
            marshDayFishSpeciesList = new List<ThingDef>();
            marshNightFishSpeciesList = new List<ThingDef>();

            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (thingDef.defName.Contains("Fish_"))
                {
                    ThingDef_FishSpeciesProperties fishDef = thingDef as ThingDef_FishSpeciesProperties;
                    if (fishDef != null)
                    {
                        // Sea.
                        if ((fishDef.aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.Sea)
                            || (fishDef.aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.SeaAndMarch))
                        {
                            // Day.
                            if ((fishDef.livingTime == ThingDef_FishSpeciesProperties.LivingTime.Day)
                                || (fishDef.livingTime == ThingDef_FishSpeciesProperties.LivingTime.DayAndNight))
                            {
                                // Sea day.
                                seaDayFishSpeciesList.Add(thingDef);
                                seaDayFishSpeciesTotalCommonality += fishDef.commonality;
                            }
                            // Night.
                            if ((fishDef.livingTime == ThingDef_FishSpeciesProperties.LivingTime.Night)
                                || (fishDef.livingTime == ThingDef_FishSpeciesProperties.LivingTime.DayAndNight))
                            {
                                // Sea night.
                                seaNightFishSpeciesList.Add(thingDef);
                                seaNightFishSpeciesTotalCommonality += fishDef.commonality;
                            }
                        }
                        // Marsh.
                        if ((fishDef.aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.Marsh)
                            || (fishDef.aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.SeaAndMarch))
                        {
                            // Day.
                            if ((fishDef.livingTime == ThingDef_FishSpeciesProperties.LivingTime.Day)
                                || (fishDef.livingTime == ThingDef_FishSpeciesProperties.LivingTime.DayAndNight))
                            {
                                // Marsh day.
                                marshDayFishSpeciesList.Add(thingDef);
                                marshDayFishSpeciesTotalCommonality += fishDef.commonality;
                            }
                            // Night.
                            if ((fishDef.livingTime == ThingDef_FishSpeciesProperties.LivingTime.Night)
                                || (fishDef.livingTime == ThingDef_FishSpeciesProperties.LivingTime.DayAndNight))
                            {
                                // Marsh night.
                                marshNightFishSpeciesList.Add(thingDef);
                                marshNightFishSpeciesTotalCommonality += fishDef.commonality;
                            }
                        }
                    }
                }
            }
        }

        public static float GetFishSpeciesTotalCommonality(ThingDef_FishSpeciesProperties.AquaticEnvironment aquaticEnvironment, ThingDef_FishSpeciesProperties.LivingTime livingTime)
        {
            if ((aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.Sea)
                && (livingTime == ThingDef_FishSpeciesProperties.LivingTime.Day))
            {
                return seaDayFishSpeciesTotalCommonality;
            }
            else if ((aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.Sea)
                && (livingTime == ThingDef_FishSpeciesProperties.LivingTime.Night))
            {
                return seaNightFishSpeciesTotalCommonality;
            }
            else if ((aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.Marsh)
                && (livingTime == ThingDef_FishSpeciesProperties.LivingTime.Day))
            {
                return marshDayFishSpeciesTotalCommonality;
            }
            else if ((aquaticEnvironment == ThingDef_FishSpeciesProperties.AquaticEnvironment.Marsh)
                && (livingTime == ThingDef_FishSpeciesProperties.LivingTime.Night))
            {
                return marshNightFishSpeciesTotalCommonality;
            }
            else
            {
                Log.Warning("FishIndustry: this aquatic environment/living time combination is not implemented.");
                return 0f;
            }
        }
    }
}
