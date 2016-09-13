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

        public static string JobDefName_TransferInjuredEmployeet = "JobDef_TransferInjuredEmployee";

        public static string JobDefName_BoardSupplyShip = "JobDef_BoardSupplyShip";

        public static string JobDefName_CleanOutpost = "JobDef_CleanOutpost";

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

        public static ThingDef OrbitalRelayDef
        {
            get
            {
                return ThingDef.Named("OrbitalRelay");
            }
        }

        public static ThingDef LandingPadBeaconDef
        {
            get
            {
                return ThingDef.Named("LandingPadBeacon");
            }
        }
        
        public static ThingDef FoodRackDef
        {
            get
            {
                return ThingDef.Named("FoodRack");
            }
        }
        
        public static ThingDef SupplyShipLandingOnDef
        {
            get
            {
                return ThingDef.Named("SupplyShipLandingOn");
            }
        }

        public static ThingDef SupplyShipDef
        {
            get
            {
                return ThingDef.Named("SupplyShip");
            }
        }

        public static ThingDef SupplyShipTakingOffDef
        {
            get
            {
                return ThingDef.Named("SupplyShipTakingOff");
            }
        }

        public static ThingDef SupplyShipCargoBayLeftDef
        {
            get
            {
                return ThingDef.Named("SupplyShipCargoBayLeft");
            }
        }

        public static ThingDef SupplyShipCargoBayRightDef
        {
            get
            {
                return ThingDef.Named("SupplyShipCargoBayRight");
            }
        }

        public static ThingDef SupplyShipCryptosleepBayLeftDef
        {
            get
            {
                return ThingDef.Named("SupplyShipCryptosleepBayLeft");
            }
        }
        
        public static ThingDef SupplyShipCryptosleepBayRightDef
        {
            get
            {
                return ThingDef.Named("SupplyShipCryptosleepBayRight");
            }
        }

        public static ThingDef LandingPadBeaconGlowerDef
        {
            get
            {
                return ThingDef.Named("LandingPadBeaconGlower");
            }
        }
        
        public static ThingDef SparePartsCabinetDef
        {
            get
            {
                return ThingDef.Named("SparePartsCabinet");
            }
        }
        
        public static ThingDef ForceFieldGeneratorDef
        {
            get
            {
                return ThingDef.Named("ForceFieldGenerator");
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
        public static FactionDef FactionDefOfMiningCo
        {
            get
            {
                return FactionDef.Named("MiningCo");
            }
        }
        public static Faction FactionOfMiningCo
        {
            get
            {
                return Find.FactionManager.FirstFactionOfDef(FactionDefOfMiningCo);
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

        // Thought def.
        public static ThoughtDef MiningCoEmployeeThoughtDef
        {
            get
            {
                return ThoughtDef.Named("MiningCoEmployee");
            }
        }

        // PawnKind def.
        public static PawnKindDef OutpostTechnicianDef
        {
            get
            {
                return PawnKindDef.Named("OutpostTechnician");
            }
        }

        public static PawnKindDef OutpostScoutDef
        {
            get
            {
                return PawnKindDef.Named("OutpostScout");
            }
        }

        public static PawnKindDef OutpostGuardDef
        {
            get
            {
                return PawnKindDef.Named("OutpostGuard");
            }
        }

        public static PawnKindDef OutpostHeavyGuardDef
        {
            get
            {
                return PawnKindDef.Named("OutpostHeavyGuard");
            }
        }

        public static PawnKindDef OutpostOfficerDef
        {
            get
            {
                return PawnKindDef.Named("OutpostOfficer");
            }
        }

        // Area.
        public static string OutpostAreaLabel
        {
            get
            {
                return "MiningCo. outpost";
            }
        }

        /// <summary>
        /// Find the outpost area if it exists.
        /// </summary>
        public static Area FindOutpostArea()
        {
            return Find.AreaManager.GetLabeled(OG_Util.OutpostAreaLabel);
        }

        /// <summary>
        /// Destroy the outpost area if it exists.
        /// </summary>
        public static void DestroyOutpostArea()
        {
            if (OG_Util.FindOutpostArea() != null)
            {
                OG_Util.FindOutpostArea().Delete();
            }
        }

        /// <summary>
        /// Find the outpost command console building if it exists.
        /// </summary>
        public static Building_OutpostCommandConsole FindOutpostCommandConsole(Faction faction)
        {
            List<Thing> commandConsoleList = Find.ListerThings.ThingsOfDef(OG_Util.OutpostCommandConsoleDef);
            foreach (Thing potentialCommandConsole in commandConsoleList)
            {
                Building_OutpostCommandConsole commandConsole = potentialCommandConsole as Building_OutpostCommandConsole;
                if ((commandConsole != null)
                    && (commandConsole.Faction != null)
                    && (commandConsole.Faction == faction))
                {
                    return commandConsole;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the orbital relay building if it exists.
        /// </summary>
        public static Building_OrbitalRelay FindOrbitalRelay(Faction faction)
        {
            List<Thing> orbitalRelayList = Find.ListerThings.ThingsOfDef(OG_Util.OrbitalRelayDef);
            foreach (Thing potentialOrbitalRelay in orbitalRelayList)
            {
                Building_OrbitalRelay orbitalRelay = potentialOrbitalRelay as Building_OrbitalRelay;
                if ((orbitalRelay != null)
                    && (orbitalRelay.Faction != null)
                    && (orbitalRelay.Faction == faction))
                {
                    return orbitalRelay;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the supply ship if it exists.
        /// </summary>
        public static Building_SupplyShip FindSupplyShip(Faction faction)
        {
            List<Thing> supplyShipList = Find.ListerThings.ThingsOfDef(OG_Util.SupplyShipDef);
            foreach (Thing potentialSupplyShip in supplyShipList)
            {
                Building_SupplyShip supplyShip = potentialSupplyShip as Building_SupplyShip;
                if ((supplyShip != null)
                    && (supplyShip.Faction != null)
                    && (supplyShip.Faction == faction))
                {
                    return supplyShip;
                }
            }
            return null;
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

        /// <summary>
        /// Check if modName is active.
        /// </summary>
        public static bool IsModActive(string modName)
        {
            foreach (ModMetaData mod in ModLister.AllInstalledMods)
            {
                if (mod.Active
                    && (mod.Name == modName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
