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
        public static ThingDef FishingPierSpawnerOnMudDef
        {
            get
            {
                return (ThingDef.Named("FishingPierSpawnerOnMud"));
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
        
        // Job.
        public static string JobDefName_FishAtFishingPier = "JobDef_FishAtFishingPier";

        public static string JobDefName_HarvestAquacultureBasinProduction = "JobDef_HarvestAquacultureBasinProduction";

        // Breedable fishes def.
        public static PawnKindDef MashgonDef
        {
            get
            {
                return (PawnKindDef.Named("PawnKindDefFishMashgon"));
            }
        }

        public static PawnKindDef BluebladeDef
        {
            get
            {
                return (PawnKindDef.Named("PawnKindDefFishBlueblade"));
            }
        }

        public static PawnKindDef TailteethDef
        {
            get
            {
                return (PawnKindDef.Named("PawnKindDefFishTailteeth"));
            }
        }

        // Texture path.
        public static string MashgonTexturePath
        {
            get
            {
                return PawnKindDef.Named("PawnKindDefFishMashgon").lifeStages.First().bodyGraphicData.texPath;
            }
        }

        public static string BluebladeTexturePath
        {
            get
            {
                return PawnKindDef.Named("PawnKindDefFishBlueblade").lifeStages.First().bodyGraphicData.texPath;
            }
        }

        public static string TailteethTexturePath
        {
            get
            {
                return PawnKindDef.Named("PawnKindDefFishTailteeth").lifeStages.First().bodyGraphicData.texPath;
            }
        }

        // Util functions.
        public static bool IsAquaticTerrain(Map map, IntVec3 position)
        {
            TerrainDef terrainDef = map.terrainGrid.TerrainAt(position);
            if ((terrainDef == TerrainDef.Named("WaterShallow"))
                || (terrainDef == TerrainDef.Named("WaterDeep"))
                || (terrainDef == TerrainDef.Named("Marsh")))
            {
                return true;
            }
            return false;
        }

        // Fishes lists.
        private static List<PawnKindDef_FishSpecies> fishSpeciesList = null;
        public static List<PawnKindDef_FishSpecies> GetFishSpeciesList()
        {
            if (fishSpeciesList.NullOrEmpty())
            {
                fishSpeciesList = new List<PawnKindDef_FishSpecies>();

                foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading)
                {
                    if (def is PawnKindDef_FishSpecies)
                    {
                        fishSpeciesList.Add(def as PawnKindDef_FishSpecies);
                    }
                }
                if (fishSpeciesList.NullOrEmpty())
                {
                    Log.Warning("FishIndustry: did not found any fish species.");
                }
            }

            return fishSpeciesList;
        }
    }
}
