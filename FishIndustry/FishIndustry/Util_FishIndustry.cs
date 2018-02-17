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

        // ThingDef.
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
        
        public static ThingDef RawCornDef
        {
            get
            {
                return (ThingDef.Named("RawCorn"));
            }
        }
        
        // Job.
        public static JobDef FishAtFishingPierJobDef
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_FishAtFishingPier");
            }
        }

        public static JobDef FishAtFishingZoneJobDef
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_FishAtFishingZone");
            }
        }

        public static JobDef HarvestAquacultureBasinProductionJobDef
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_HarvestAquacultureBasinProduction");
            }
        }

        // Fishes def.
        public static ThingDef MashgonDef
        {
            get
            {
                return (ThingDef.Named("Mashgon"));
            }
        }

        public static ThingDef BluebladeDef
        {
            get
            {
                return (ThingDef.Named("Blueblade"));
            }
        }

        public static ThingDef TailteethDef
        {
            get
            {
                return (ThingDef.Named("Tailteeth"));
            }
        }

        // Fishes meat def.
        public static ThingDef MashgonMeatDef
        {
            get
            {
                return (ThingDef.Named(MashgonDef.defName + "_Meat"));
            }
        }

        public static ThingDef BluebladeMeatDef
        {
            get
            {
                return (ThingDef.Named(BluebladeDef.defName + "_Meat"));
            }
        }

        public static ThingDef TailteethMeatDef
        {
            get
            {
                return (ThingDef.Named(TailteethDef.defName + "_Meat"));
            }
        }

        // Breedable fishes PawnKindDef.
        public static PawnKindDef MashgonPawnKindDef
        {
            get
            {
                return (PawnKindDef.Named("PawnKindDefFishMashgon"));
            }
        }

        public static PawnKindDef BluebladePawnKindDef
        {
            get
            {
                return (PawnKindDef.Named("PawnKindDefFishBlueblade"));
            }
        }

        public static PawnKindDef TailteethPawnKindDef
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
        
        // Fishes lists.
        public static List<PawnKindDef_FishSpecies> GetFishSpeciesList(BiomeDef biome)
        {
            List<PawnKindDef_FishSpecies> fishSpeciesList = new List<PawnKindDef_FishSpecies>();
            
            foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (def is PawnKindDef_FishSpecies)
                {
                    PawnKindDef_FishSpecies fishDef = def as PawnKindDef_FishSpecies;
                    if (fishDef.naturalBiomes.Contains(biome))
                    {
                        fishSpeciesList.Add(fishDef);
                    }
                }
            }
            return fishSpeciesList;
        }
    }
}
