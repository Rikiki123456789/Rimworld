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
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
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
        public static ThingDef AquacultureBasinDef
        {
            get
            {
                return (ThingDef.Named("AquacultureBasin"));
            }
        }

        // Terrain.
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
        
        public static ThingDef MoteFishMashgonEastDef
        {
            get
            {
                return (ThingDef.Named("Mote_FishMashgonEast"));
            }
        }
        public static ThingDef MoteFishMashgonWestDef
        {
            get
            {
                return (ThingDef.Named("Mote_FishMashgonWest"));
            }
        }
        public static ThingDef MoteFishBluebladeEastDef
        {
            get
            {
                return (ThingDef.Named("Mote_FishBluebladeEast"));
            }
        }
        public static ThingDef MoteFishBluebladeWestDef
        {
            get
            {
                return (ThingDef.Named("Mote_FishBluebladeWest"));
            }
        }
        public static ThingDef MoteFishTailteethEastDef
        {
            get
            {
                return (ThingDef.Named("Mote_FishTailteethEast"));
            }
        }
        public static ThingDef MoteFishTailteethWestDef
        {
            get
            {
                return (ThingDef.Named("Mote_FishTailteethWest"));
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
        
        public static ThingDef RawRiceDef
        {
            get
            {
                return (ThingDef.Named("RawRice"));
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

        public static JobDef AquacultureBasinHarvestJobDef
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_AquacultureBasinHarvest");
            }
        }

        public static JobDef AquacultureBasinChangeSpeciesJobDef
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_AquacultureBasinChangeSpecies");
            }
        }

        public static JobDef AquacultureBasinMaintainJobDef
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_AquacultureBasinMaintain");
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

        public static string MashgonTexturePathWithChangeIcon
        {
            get
            {
                return "Ui/Gizmos/MashgonWithChangeIcon";
            }
        }

        public static string BluebladeTexturePath
        {
            get
            {
                return PawnKindDef.Named("PawnKindDefFishBlueblade").lifeStages.First().bodyGraphicData.texPath;
            }
        }

        public static string BluebladeTexturePathWithChangeIcon
        {
            get
            {
                return "Ui/Gizmos/BluebladeWithChangeIcon";
            }
        }

        public static string TailteethTexturePath
        {
            get
            {
                return PawnKindDef.Named("PawnKindDefFishTailteeth").lifeStages.First().bodyGraphicData.texPath;
            }
        }

        public static string TailteethTexturePathWithChangeIcon
        {
            get
            {
                return "Ui/Gizmos/TailteethWithChangeIcon";
            }
        }

        // Fishes lists.
        public static Dictionary<BiomeDef, List<PawnKindDef_FishSpecies>> fishSpeciesListDico = new Dictionary<BiomeDef, List<PawnKindDef_FishSpecies>>();
        public static List<PawnKindDef_FishSpecies> GetFishSpeciesList(BiomeDef biome)
        {
            if (fishSpeciesListDico.ContainsKey(biome) == false)
            {
                fishSpeciesListDico.Add(biome, BuildFishSpeciesListForMap(biome));
            }
            return fishSpeciesListDico.TryGetValue(biome);
        }

        public static List<PawnKindDef_FishSpecies> BuildFishSpeciesListForMap(BiomeDef biome)
        {
            List<PawnKindDef_FishSpecies> fishSpeciesList = new List<PawnKindDef_FishSpecies>();
            
            foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (def is PawnKindDef_FishSpecies)
                {
                    PawnKindDef_FishSpecies fishDef = def as PawnKindDef_FishSpecies;
                    if ((Settings.biomeRestrictionsIsEnabled == false)
                        || fishDef.naturalBiomes.Contains(biome))
                    {
                        fishSpeciesList.Add(fishDef);
                    }
                }
            }
            return fishSpeciesList;
        }
    }
}
