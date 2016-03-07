using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Outpost generator utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_Util
    {
        // Job def.
        public static string JobDefName_TryToCaptureOutpost = "JobDef_TryToCaptureOutpost";

        // Thing defs.
        public static ThingDef FireproofPowerConduitDef
        {
            get
            {
                return ThingDef.Named("FireproofPowerConduit");
            }
        }

        public static ThingDef FireproofAutodoorDef
        {
            get
            {
                return ThingDef.Named("FireproofAutodoor");
            }
        }

        public static ThingDef SamSiteDef
        {
            get
            {
                return ThingDef.Named("SamSite");
            }
        }
        
        public static ThingDef SamDef
        {
            get
            {
                return ThingDef.Named("Sam");
            }
        }

        public static ThingDef VulcanTurretDef
        {
            get
            {
                return ThingDef.Named("VulcanTurret");
            }
        }

        public static ThingDef CompactAutonomousGeneratorDef
        {
            get
            {
                return ThingDef.Named("CompactAutonomousGenerator");
            }
        }

        public static ThingDef OutpostCommandConsoleDef
        {
            get
            {
                return ThingDef.Named("OutpostCommandConsole");
            }
        }

        public static ThingDef LandingPadBeaconDef
        {
            get
            {
                return ThingDef.Named("LandingPadBeacon");
            }
        }

        // Terrain def.
        public static TerrainDef DirtFloorDef
        {
            get
            {
                return TerrainDef.Named("DirtFloor");
            }
        }


        // Roof def.
        public static RoofDef IronedRoofDef
        {
            get
            {
                return DefDatabase<RoofDef>.GetNamed("IronedRoof");
            }
        }

        // Faction def.
        public static FactionDef FactionDefOfMAndCo
        {
            get
            {
                return FactionDef.Named("MAndCo");
            }
        }
        public static Faction FactionOfMAndCo
        {
            get
            {
                return Find.FactionManager.FirstFactionOfDef(FactionDefOfMAndCo);
            }
        }

        public static SoundDef MissileLaunchSoundDef
        {
            get
            {
                return DefDatabase<SoundDef>.GetNamed("MissileLaunch");
            }
        }

        // Duty def.
        public static DutyDef DefendOutpostDutyDef
        {
            get
            {
                return DefDatabase<DutyDef>.GetNamed("DefendOutpost");
            }
        }

        // PawnKind def.
        public static PawnKindDef OutpostGuardDef
        {
            get
            {
                return PawnKindDef.Named("OutpostGuard");
            }
        }

        public static PawnKindDef OutpostOfficerDef
        {
            get
            {
                return PawnKindDef.Named("OutpostOfficer");
            }
        }

        // Label.
        public static string OutpostAreaLabel
        {
            get
            {
                return "M&Co. outpost";
            }
        }

        /// <summary>
        /// Return a copy of the listToRefresh but remove any destroyed item.
        /// </summary>
        public static List<Thing> RefreshThingList(List<Thing> listToRefresh)
        {
            List<Thing> refreshedList = new List<Thing>();

            if (listToRefresh.NullOrEmpty() == false)
            {
                foreach (Thing thing in listToRefresh)
                {
                    if (thing.Destroyed == false)
                    {
                        refreshedList.Add(thing);
                    }
                }
                return refreshedList;
            }
            else
            {
                return null;
            }
        }
    }
}
