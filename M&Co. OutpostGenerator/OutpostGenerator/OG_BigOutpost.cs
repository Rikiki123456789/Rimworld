using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    // TODO: add tale: drop-pod destroyed in-flight.
    // TODO: create M&Co. backgrounds.
    // TODO: add cleaning job inside outpost area.
    // TODO: if pawn has no apparel or weapon, leave with next ship.
    // TODO: add animals? Might be tricky, especially for wounded animals.
    // TODO: improve AI with 2 states (standard defense + assault ennemy in outpost perimeter).
    // TODO: technician tuque not when in hot conditions.
    // TODO: check firefighters do their job!

    /// <summary>
    /// OG_BigOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_BigOutpost
    {
        public const int horizontalZonesNumber = 7;
        public const int verticalZonesNumber = 7;
        public const int areaSideLength = 7 * Genstep_GenerateOutpost.zoneSideSize;

        private static Rot4 mainEntranceDirection = Rot4.Random;
        private static ZoneProperties[,] zoneMap = new ZoneProperties[verticalZonesNumber, horizontalZonesNumber];
        private const int mainRoomsNumber = 5;

        private static OG_OutpostData outpostData = new OG_OutpostData();
        private static Building_OutpostCommandConsole commandConsole = null;
        private static Building_OrbitalRelay orbitalRelay = null;
        
        public static void GenerateOutpost(OG_OutpostData outpostDataParameter)
        {
            int mainRoomZoneAbs = 0;
            int mainRoomZoneOrd = 0;

            outpostData = outpostDataParameter;
            outpostData.triggerIntrusion = null;
            outpostData.outpostThingList = new List<Thing>();

            // Clear the whole area, remove any roof and water.
            ClearAnythingInOutpostArea();
            
            // Create the intrusion trigger.
            outpostData.triggerIntrusion = (TriggerIntrusion)ThingMaker.MakeThing(ThingDef.Named("TriggerIntrusion"));
            GetMainRoom4Zone(mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
            IntVec3 triggerINtrussionPosition = Zone.GetZoneOrigin(outpostData.areaSouthWestOrigin, mainRoomZoneAbs, mainRoomZoneOrd) + new IntVec3(5, 0, 5);
            GenSpawn.Spawn(outpostData.triggerIntrusion, triggerINtrussionPosition);

            GenerateOutpostLayout();
            // Display the generated layout.
            /*for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
            {
                for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
                {
                    ZoneProperties zone = zoneMap[zoneOrd, zoneAbs];
                    Log.Message("Layout: zoneMap[" + zoneOrd + "," + zoneAbs + "] => " + zone.zoneType.ToString() + "," + zone.rotation.ToString() + "," + zone.linkedZoneRelativeRotation.ToString());
                }
            }*/
            GenerateOutpostZones(outpostData.areaSouthWestOrigin);
            GenerateSasToLinkMediumAndMainRooms(outpostData.areaSouthWestOrigin);
            // Generate laser fences.
            OG_LaserFence.GenerateLaserFence(zoneMap, ref outpostData);
            // Generate battle remains.
            OG_WarfieldEffects.GenerateWarfieldEffects(zoneMap, horizontalZonesNumber, verticalZonesNumber, outpostData);
            // Damage outpost to reflect its history.
            OG_RuinEffects.GenerateRuinEffects(ref outpostData);
            // Generate some inhabitants.
            OG_Inhabitants.GenerateInhabitants(ref outpostData);
            // Spawn the no-roof area generator.
            SpawnNoRoofAreaGenerator(outpostData.areaSouthWestOrigin);

            // Initialize command console data.
            outpostData.outpostThingList = OG_Util.RefreshThingList(outpostData.outpostThingList);
            commandConsole.outpostThingList = outpostData.outpostThingList.ListFullCopy<Thing>();
            commandConsole.dropZoneCenter = outpostData.dropZoneCenter;
            // Initialize orbital relay data.
            orbitalRelay.InitializeLandingData(outpostData.landingPadCenter, outpostData.landingPadRotation);
            // Initialize intrusion trigger data.
            outpostData.triggerIntrusion.commandConsole = commandConsole; // TODO: revert dependency (commandConsole activates trigger intrusion tick)?
            
            SendWelcomeLetter(outpostData);
        }

        static void ClearAnythingInOutpostArea()
        {
            CellRect rect = new CellRect(outpostData.areaSouthWestOrigin.x - 1, outpostData.areaSouthWestOrigin.z - 1, areaSideLength + 2, areaSideLength + 2);
            foreach (IntVec3 cell in rect.Cells)
            {
                Find.RoofGrid.SetRoof(cell, null);
                List<Thing> thingList = cell.GetThingList();
                for (int j = thingList.Count - 1; j >= 0; j--)
                {
                    Thing thing = thingList[j];
                    if (thing.def.destroyable)
                    {
                        thing.Destroy(DestroyMode.Vanish);
                    }
                }
                Find.Map.terrainGrid.RemoveTopLayer(cell);
                TerrainDef terrain = Find.TerrainGrid.TerrainAt(cell);
                if ((terrain == TerrainDef.Named("Marsh"))
                    || (terrain == TerrainDef.Named("Mud"))
                    || (terrain == TerrainDef.Named("WaterShallow")))
                {
                    Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                }
            }
        }

        static void SendWelcomeLetter(OG_OutpostData outpostData)
        {
            string eventTitle = "";
            string eventText = "";
            LetterType letterType = LetterType.BadNonUrgent;
            int sectorCoordX = Find.Map.WorldCoords.x;
            int sectorCoordZ = Find.Map.WorldCoords.z;
            int rimLawNumber = Rand.RangeInclusive(25, 1350);
            if (outpostData.isMilitary)
            {
                eventTitle = "M&Co. military outpost";
                eventText = "   M&Co. warning message\n\n" +
                    "   Hello! I am Coralie, the AI in charge of outpost X" + sectorCoordX + ".Z" + sectorCoordZ + " Mil.\n\n" +
                    "My scanners detected the signature of your escape pods crashing nearby.\n\n" +
                    "WARNING! You are entering an M&Co. military sector. No activity is authorized here.\n\n" +
                    "Please notice that anyone found in the sector will be shot on sight with regards to the RimLaw RL-" + rimLawNumber + ".\n\n" +
                    "---- End of transmision ---";
                letterType = LetterType.BadUrgent;
            }
            else
            {
                eventTitle = "M&Co. civil outpost";
                eventText = "   M&Co. information message\n\n" +
                    "   Hello! I am Coralie, the AI in charge of outpost X" + sectorCoordX + ".Z" + sectorCoordZ + " Civ.\n\n" +
                    "My scanners detected the signature of your escape pods crashing nearby.\n" +
                    "My emotional analyzer tells me to be sorry for your ship and the people who most probably died within.\n\n" +
                    "I remind you this sector is the legal property of M&Co. However, the company generously grants you the right to pass through this sector.\n\n" +
                    "Please notice that any intruder found inside the outpost perimeter will be shot on sight with regards to the RimLaw RL-" + rimLawNumber + ".\n\n" +
                    "---- End of transmision ---";
                letterType = LetterType.Good;
            }
            Find.LetterStack.ReceiveLetter(eventTitle, eventText, letterType);
        }

        // ######## Layout generation functions ######## //

        static void GenerateOutpostLayout()
        {
            // Reset zoneMap.
            for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
            {
                for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
                {
                    zoneMap[zoneOrd, zoneAbs] = new ZoneProperties(ZoneType.NotYetGenerated, Rot4.North, Rot4.North);
                }
            }

            GenerateOutpostLayoutCentralRoom();
            GenerateOutpostLayoutNorthMainEntrance();
            GenerateOutpostLayoutPowerSupply();
            GenerateOutpostLayoutCommandRoom();
            GenerateOutpostLayoutBarracks();
            GenerateOutpostLayoutHydroponicRoom();
            GenerateOutpostLayoutBigSasAndSecondaryEntrances();

            GenerateOutpostLayoutAroundPlaza();
            GenerateOutpostLayoutEntranchedZones();
            GenerateOutpostLayoutSamSitesAndOrbitalRelay();

            GenerateOutpostLayoutSecondaryRooms();
            EnsureAtLeastOneMedibayInOutpost();
        }

        /// <summary>
        /// Generate the layout of the central room: main refectory.
        /// </summary>
        static void GenerateOutpostLayoutCentralRoom()
        {
            int mainRoom4ZoneAbs = 0;
            int mainRoom4ZoneOrd = 0;

            GetMainRoomZone(4, mainEntranceDirection, out mainRoom4ZoneAbs, out mainRoom4ZoneOrd);
            zoneMap[mainRoom4ZoneOrd, mainRoom4ZoneAbs] = new ZoneProperties(ZoneType.BigRoomRefectory, Rot4.Random, Rot4.Invalid);
        }

        static void GenerateOutpostLayoutNorthMainEntrance()
        {
            int mainRoom4ZoneAbs = 0;
            int mainRoom4ZoneOrd = 0;
            int straightAlleyZoneAbs = 0;
            int straightAlleyZoneOrd = 0;
            int northMainEntranceZoneAbs = 0;
            int northMainEntranceZoneOrd = 0;

            GetMainRoomZone(4, mainEntranceDirection, out mainRoom4ZoneAbs, out mainRoom4ZoneOrd);
            Zone.GetAdjacentZone(mainRoom4ZoneAbs, mainRoom4ZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), out straightAlleyZoneAbs, out straightAlleyZoneOrd);
            zoneMap[straightAlleyZoneOrd, straightAlleyZoneAbs] = new ZoneProperties(ZoneType.StraightAlley, mainEntranceDirection, Rot4.Invalid);
            Zone.GetAdjacentZone(straightAlleyZoneAbs, straightAlleyZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), out northMainEntranceZoneAbs, out northMainEntranceZoneOrd);
            zoneMap[northMainEntranceZoneOrd, northMainEntranceZoneAbs] = new ZoneProperties(ZoneType.MainEntrance, mainEntranceDirection, Rot4.Invalid);
        }

        static void GenerateOutpostLayoutPowerSupply()
        {
            int batteryRoomZoneAbs = 0;
            int batteryRoomZoneOrd = 0;
            int solarPanelZoneAbs = 0;
            int solarPanelZoneOrd = 0;
            int mainRoomIndex = 0;
            
            GetFreeMainRoomZone(out batteryRoomZoneAbs, out batteryRoomZoneOrd, out mainRoomIndex);
            
            // Set battery room zone.
            Rot4 batteryRoomRotation = new Rot4(Rot4.West.AsInt + mainRoomIndex); // Battery room is facing west for main room 0, north for main room 1, ...
            zoneMap[batteryRoomZoneOrd, batteryRoomZoneAbs] = new ZoneProperties(ZoneType.BigRoomBatteryRoom, batteryRoomRotation, Rot4.Invalid);

            // Set solar panel zones around relative south-west corner.
            Zone.GetAdjacentZone(batteryRoomZoneAbs, batteryRoomZoneOrd, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out solarPanelZoneAbs, out solarPanelZoneOrd);
            zoneMap[solarPanelZoneOrd, solarPanelZoneAbs] = new ZoneProperties(ZoneType.SolarPanelZone, Rot4.North, Rot4.Invalid);
            Zone.GetAdjacentZone(solarPanelZoneAbs, solarPanelZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out solarPanelZoneAbs, out solarPanelZoneOrd);
            zoneMap[solarPanelZoneOrd, solarPanelZoneAbs] = new ZoneProperties(ZoneType.SolarPanelZone, Rot4.North, Rot4.Invalid);
            Zone.GetAdjacentZone(solarPanelZoneAbs, solarPanelZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out solarPanelZoneAbs, out solarPanelZoneOrd);
            zoneMap[solarPanelZoneOrd, solarPanelZoneAbs] = new ZoneProperties(ZoneType.SolarPanelZone, Rot4.North, Rot4.Invalid);
        }

        static void GenerateOutpostLayoutCommandRoom()
        {
            int commandRoomZoneAbs = 0;
            int commandRoomZoneOrd = 0;
            int dropZoneZoneAbs = 0;
            int dropZoneZoneOrd = 0;
            int landingPadTopZoneAbs = 0;
            int landingPadTopZoneOrd = 0;
            int landingPadBottomZoneAbs = 0;
            int landingPadBottomZoneOrd = 0;
            int mainRoomIndex = 0;

            GetFreeMainRoomZone(out commandRoomZoneAbs, out commandRoomZoneOrd, out mainRoomIndex);
            zoneMap[commandRoomZoneOrd, commandRoomZoneAbs] = new ZoneProperties(ZoneType.BigRoomCommandRoom, Rot4.Random, Rot4.Invalid);

            // Set drop-zone and landing pad zones around relative south-west corner.
            if (Rand.Value < 0.5f)
            {
                // Drop-zone first.
                Zone.GetAdjacentZone(commandRoomZoneAbs, commandRoomZoneOrd, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out dropZoneZoneAbs, out dropZoneZoneOrd);
                zoneMap[dropZoneZoneOrd, dropZoneZoneAbs] = new ZoneProperties(ZoneType.DropZone, Rot4.North, Rot4.Invalid);
                if (Rand.Value < 0.5f)
                {
                    // Landing pad top first.
                    Zone.GetAdjacentZone(dropZoneZoneAbs, dropZoneZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadTopZoneAbs, out landingPadTopZoneOrd);
                    zoneMap[landingPadTopZoneOrd, landingPadTopZoneAbs] = new ZoneProperties(ZoneType.LandingPadTop, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.South);
                    Zone.GetAdjacentZone(landingPadTopZoneAbs, landingPadTopZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadBottomZoneAbs, out landingPadBottomZoneOrd);
                    zoneMap[landingPadBottomZoneOrd, landingPadBottomZoneAbs] = new ZoneProperties(ZoneType.LandingPadBottom, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.North);
                }
                else
                {
                    // Landing pad bottom first.
                    Zone.GetAdjacentZone(dropZoneZoneAbs, dropZoneZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadBottomZoneAbs, out landingPadBottomZoneOrd);
                    zoneMap[landingPadBottomZoneOrd, landingPadBottomZoneAbs] = new ZoneProperties(ZoneType.LandingPadBottom, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.North);
                    Zone.GetAdjacentZone(landingPadBottomZoneAbs, landingPadBottomZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadTopZoneAbs, out landingPadTopZoneOrd);
                    zoneMap[landingPadTopZoneOrd, landingPadTopZoneAbs] = new ZoneProperties(ZoneType.LandingPadTop, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.South);
                }
            }
            else
            {
                // Landing pad first.
                if (Rand.Value < 0.5f)
                {
                    // Landing pad top first.
                    Zone.GetAdjacentZone(commandRoomZoneAbs, commandRoomZoneOrd, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadTopZoneAbs, out landingPadTopZoneOrd);
                    zoneMap[landingPadTopZoneOrd, landingPadTopZoneAbs] = new ZoneProperties(ZoneType.LandingPadTop, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.South);
                    Zone.GetAdjacentZone(landingPadTopZoneAbs, landingPadTopZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadBottomZoneAbs, out landingPadBottomZoneOrd);
                    zoneMap[landingPadBottomZoneOrd, landingPadBottomZoneAbs] = new ZoneProperties(ZoneType.LandingPadBottom, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.North);
                    Zone.GetAdjacentZone(landingPadBottomZoneAbs, landingPadBottomZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out dropZoneZoneAbs, out dropZoneZoneOrd);
                    zoneMap[dropZoneZoneOrd, dropZoneZoneAbs] = new ZoneProperties(ZoneType.DropZone, Rot4.North, Rot4.Invalid);
                }
                else
                {
                    // Landing pad bottom first.
                    Zone.GetAdjacentZone(commandRoomZoneAbs, commandRoomZoneOrd, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadBottomZoneAbs, out landingPadBottomZoneOrd);
                    zoneMap[landingPadBottomZoneOrd, landingPadBottomZoneAbs] = new ZoneProperties(ZoneType.LandingPadBottom, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.North);
                    Zone.GetAdjacentZone(landingPadBottomZoneAbs, landingPadBottomZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadTopZoneAbs, out landingPadTopZoneOrd);
                    zoneMap[landingPadTopZoneOrd, landingPadTopZoneAbs] = new ZoneProperties(ZoneType.LandingPadTop, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.South);
                    Zone.GetAdjacentZone(landingPadTopZoneAbs, landingPadTopZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out dropZoneZoneAbs, out dropZoneZoneOrd);
                    zoneMap[dropZoneZoneOrd, dropZoneZoneAbs] = new ZoneProperties(ZoneType.DropZone, Rot4.North, Rot4.Invalid);
                }
            }
        }

        static void GenerateOutpostLayoutBarracks()
        {
            int barracksZoneAbs = 0;
            int barracksZoneOrd = 0;
            int mainRoomIndex = 0;

            GetFreeMainRoomZone(out barracksZoneAbs, out barracksZoneOrd, out mainRoomIndex);
            zoneMap[barracksZoneOrd, barracksZoneAbs] = new ZoneProperties(ZoneType.BigRoomBarracks, Rot4.Random, Rot4.Invalid);
        }

        static void GenerateOutpostLayoutHydroponicRoom()
        {
            int hydroponicRoomZoneAbs = 0;
            int hydroponicRoomZoneOrd = 0;
            int mainRoomIndex = 0;

            GetFreeMainRoomZone(out hydroponicRoomZoneAbs, out hydroponicRoomZoneOrd, out mainRoomIndex);
            zoneMap[hydroponicRoomZoneOrd, hydroponicRoomZoneAbs] = new ZoneProperties(ZoneType.BigRoomHydroponics, Rot4.Random, Rot4.Invalid);
        }

        /// <summary>
        /// Get the coordinates of one free main room, central room excluded.
        /// </summary>
        static void GetFreeMainRoomZone(out int mainRoomZoneAbs, out int mainRoomZoneOrd, out int mainRoomIndex)
        {
            do
            {
                mainRoomIndex = Rand.Range(0, mainRoomsNumber - 1);
                GetMainRoomZone(mainRoomIndex, mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
            }
            while (zoneMap[mainRoomZoneOrd, mainRoomZoneAbs].zoneType != ZoneType.NotYetGenerated);
        }

        static void GenerateOutpostLayoutBigSasAndSecondaryEntrances()
        {
            int mainRoomZoneAbs = 0;
            int mainRoomZoneOrd = 0;
            int bigSasZoneAbs = 0;
            int bigSasZoneOrd = 0;
            int secondaryEntranceZoneAbs = 0;
            int secondaryEntranceZoneOrd = 0;

            // Sas north from main room 0.
            GetMainRoomZone(0, mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
            Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), out bigSasZoneAbs, out bigSasZoneOrd);
            zoneMap[bigSasZoneOrd, bigSasZoneAbs] = new ZoneProperties(ZoneType.BigSas, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), Rot4.West);
            Zone.GetAdjacentZone(bigSasZoneAbs, bigSasZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), out secondaryEntranceZoneAbs, out secondaryEntranceZoneOrd);
            zoneMap[secondaryEntranceZoneOrd, secondaryEntranceZoneAbs] = new ZoneProperties(ZoneType.SecondaryEntrance, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            // Sas north from main room 3.
            GetMainRoomZone(3, mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
            Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), out bigSasZoneAbs, out bigSasZoneOrd);
            zoneMap[bigSasZoneOrd, bigSasZoneAbs] = new ZoneProperties(ZoneType.BigSas, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt), Rot4.East);
            Zone.GetAdjacentZone(bigSasZoneAbs, bigSasZoneOrd, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), out secondaryEntranceZoneAbs, out secondaryEntranceZoneOrd);
            zoneMap[secondaryEntranceZoneOrd, secondaryEntranceZoneAbs] = new ZoneProperties(ZoneType.SecondaryEntrance, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            // Sas west and east from main room 4.
            GetMainRoomZone(4, mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
            Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), out bigSasZoneAbs, out bigSasZoneOrd);
            zoneMap[bigSasZoneOrd, bigSasZoneAbs] = new ZoneProperties(ZoneType.BigSas, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), Rot4.West);
            Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), out bigSasZoneAbs, out bigSasZoneOrd);
            zoneMap[bigSasZoneOrd, bigSasZoneAbs] = new ZoneProperties(ZoneType.BigSas, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), Rot4.East);
        }

        /// <summary>
        /// Generate the layout of the central plaza: plaza and 4 straight alleys in cardinal directions.
        /// </summary>
        static void GenerateOutpostLayoutAroundPlaza()
        {
            int mainRoom4ZoneAbs = 0;
            int mainRoom4ZoneOrd = 0;
            int straightAlleyZoneAbs = 0;
            int straightAlleyZoneOrd = 0;
            int plazaZoneAbs = 0;
            int plazaZoneOrd = 0;
            int mainEntranceZoneAbs = 0;
            int mainEntranceZoneOrd = 0;

            // Generate plaza, straight alleys in cardinal direction and main entrance in south.
            GetMainRoom4Zone(mainEntranceDirection, out mainRoom4ZoneAbs, out mainRoom4ZoneOrd);
            // North alley.
            Zone.GetAdjacentZone(mainRoom4ZoneAbs, mainRoom4ZoneOrd, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt), out straightAlleyZoneAbs, out straightAlleyZoneOrd);
            zoneMap[straightAlleyZoneOrd, straightAlleyZoneAbs] = new ZoneProperties(ZoneType.StraightAlley, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            // Plaza.
            Zone.GetAdjacentZone(straightAlleyZoneAbs, straightAlleyZoneOrd, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt), out plazaZoneAbs, out plazaZoneOrd);
            zoneMap[plazaZoneOrd, plazaZoneAbs] = new ZoneProperties(ZoneType.Plaza, Rot4.North, Rot4.Invalid); // TODO: debug. ZoneType.Plaza
            // West alley.
            Zone.GetAdjacentZone(plazaZoneAbs, plazaZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), out straightAlleyZoneAbs, out straightAlleyZoneOrd);
            zoneMap[straightAlleyZoneOrd, straightAlleyZoneAbs] = new ZoneProperties(ZoneType.StraightAlley, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            // East alley.
            Zone.GetAdjacentZone(plazaZoneAbs, plazaZoneOrd, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), out straightAlleyZoneAbs, out straightAlleyZoneOrd);
            zoneMap[straightAlleyZoneOrd, straightAlleyZoneAbs] = new ZoneProperties(ZoneType.StraightAlley, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            // South alley.
            Zone.GetAdjacentZone(plazaZoneAbs, plazaZoneOrd, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt), out straightAlleyZoneAbs, out straightAlleyZoneOrd);
            zoneMap[straightAlleyZoneOrd, straightAlleyZoneAbs] = new ZoneProperties(ZoneType.StraightAlley, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            // Main entrance.
            Zone.GetAdjacentZone(straightAlleyZoneAbs, straightAlleyZoneOrd, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt), out mainEntranceZoneAbs, out mainEntranceZoneOrd);
            zoneMap[mainEntranceZoneOrd, mainEntranceZoneAbs] = new ZoneProperties(ZoneType.MainEntrance, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);

            // Generate exterior zones in diagonal directions.
            for (int xOffset = -1; xOffset <= 1; xOffset += 2)
            {
                for (int zOffset = -1; zOffset <= 1; zOffset += 2)
                {
                    ZoneType exteriorZoneType = OG_Common.GetRandomZoneTypeExteriorZone(outpostData);
                    zoneMap[plazaZoneOrd + zOffset, plazaZoneAbs + xOffset] = new ZoneProperties(exteriorZoneType, Rot4.Random, Rot4.Invalid);
                }
            }
        }

        static void GenerateOutpostLayoutEntranchedZones()
        {
            if ((mainEntranceDirection == Rot4.North)
                || (mainEntranceDirection == Rot4.South))
            {
                zoneMap[0, 1] = new ZoneProperties(ZoneType.EntranchedZone, Rot4.South, Rot4.Invalid);
                zoneMap[0, horizontalZonesNumber - 2] = new ZoneProperties(ZoneType.EntranchedZone, Rot4.South, Rot4.Invalid);
                zoneMap[verticalZonesNumber - 1, 1] = new ZoneProperties(ZoneType.EntranchedZone, Rot4.North, Rot4.Invalid);
                zoneMap[verticalZonesNumber - 1, horizontalZonesNumber - 2] = new ZoneProperties(ZoneType.EntranchedZone, Rot4.North, Rot4.Invalid);
            }
            else
            {
                zoneMap[1, 0] = new ZoneProperties(ZoneType.EntranchedZone, Rot4.West, Rot4.Invalid);
                zoneMap[verticalZonesNumber - 2, 0] = new ZoneProperties(ZoneType.EntranchedZone, Rot4.West, Rot4.Invalid);
                zoneMap[1, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.EntranchedZone, Rot4.East, Rot4.Invalid);
                zoneMap[verticalZonesNumber - 2, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.EntranchedZone, Rot4.East, Rot4.Invalid);
            }
        }

        static void GenerateOutpostLayoutSamSitesAndOrbitalRelay()
        {
            if (Rand.Value < 0.5f)
            {
                zoneMap[0, 0] = new ZoneProperties(ZoneType.SamSite, Rot4.North, Rot4.Invalid);
                zoneMap[verticalZonesNumber - 1, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.SamSite, Rot4.North, Rot4.Invalid);
                if (Rand.Value < 0.5f)
                {
                    zoneMap[0, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.OrbitalRelay, Rot4.North, Rot4.Invalid);
                    zoneMap[verticalZonesNumber - 1, 0] = new ZoneProperties(ZoneType.Empty, Rot4.North, Rot4.Invalid);
                }
                else
                {
                    zoneMap[0, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.Empty, Rot4.North, Rot4.Invalid);
                    zoneMap[verticalZonesNumber - 1, 0] = new ZoneProperties(ZoneType.OrbitalRelay, Rot4.North, Rot4.Invalid);
                }
            }
            else
            {
                zoneMap[verticalZonesNumber - 1, 0] = new ZoneProperties(ZoneType.SamSite, Rot4.North, Rot4.Invalid);
                zoneMap[0, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.SamSite, Rot4.North, Rot4.Invalid);
                if (Rand.Value < 0.5f)
                {
                    zoneMap[0, 0] = new ZoneProperties(ZoneType.OrbitalRelay, Rot4.North, Rot4.Invalid);
                    zoneMap[verticalZonesNumber - 1, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.Empty, Rot4.North, Rot4.Invalid);
                }
                else
                {
                    zoneMap[0, 0] = new ZoneProperties(ZoneType.Empty, Rot4.North, Rot4.Invalid);
                    zoneMap[verticalZonesNumber - 1, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.OrbitalRelay, Rot4.North, Rot4.Invalid);
                }
            }
        }

        static void GenerateOutpostLayoutSecondaryRooms()
        {
            int mainRoomZoneAbs = 0;
            int mainRoomZoneOrd = 0;
            int adjacentZoneAbs = 0;
            int adjacentZoneOrd = 0;
            int mediumRoomZoneAbs = 0;
            int mediumRoomZoneOrd = 0;

            // Generate secondary rooms north west and east from central room.
            GetMainRoom4Zone(mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
            // North alley.
            Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), out adjacentZoneAbs, out adjacentZoneOrd);
            // West medium room.
            Zone.GetAdjacentZone(adjacentZoneAbs, adjacentZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), out mediumRoomZoneAbs, out mediumRoomZoneOrd);
            ZoneType zoneType = OG_Common.GetRandomZoneTypeMediumRoom(outpostData);
            Rot4 roomRotation = Rot4.Invalid;
            if (Rand.Value < 0.5f)
            {
                roomRotation = new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt);
            }
            else
            {
                roomRotation = new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt);
            }
            zoneMap[mediumRoomZoneOrd, mediumRoomZoneAbs] = new ZoneProperties(zoneType, roomRotation, Rot4.Invalid);
            // East medium room.
            Zone.GetAdjacentZone(adjacentZoneAbs, adjacentZoneOrd, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), out mediumRoomZoneAbs, out mediumRoomZoneOrd);
            zoneType = OG_Common.GetRandomZoneTypeMediumRoom(outpostData);
            if (Rand.Value < 0.5f)
            {
                roomRotation = new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt);
            }
            else
            {
                roomRotation = new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt);
            }
            zoneMap[mediumRoomZoneOrd, mediumRoomZoneAbs] = new ZoneProperties(zoneType, roomRotation, Rot4.Invalid);
            
            for (int mainRoomIndex = 0; mainRoomIndex < 4; mainRoomIndex++)
            {
                GetMainRoomZone(mainRoomIndex, mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
                for (int rotationAsInt = 0; rotationAsInt < 4; rotationAsInt++)
                {
                    Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, new Rot4(rotationAsInt + mainEntranceDirection.AsInt), out mediumRoomZoneAbs, out mediumRoomZoneOrd);
                    if (zoneMap[mediumRoomZoneOrd, mediumRoomZoneAbs].zoneType == ZoneType.NotYetGenerated)
                    {
                        zoneType = OG_Common.GetRandomZoneTypeMediumRoom(outpostData);
                        if (Rand.Value < 0.5f)
                        {
                            roomRotation = new Rot4(Rot4.North.AsInt + rotationAsInt + mainEntranceDirection.AsInt);
                        }
                        else
                        {
                            roomRotation = new Rot4(Rot4.South.AsInt + rotationAsInt + mainEntranceDirection.AsInt);
                        }
                        zoneMap[mediumRoomZoneOrd, mediumRoomZoneAbs] = new ZoneProperties(zoneType, roomRotation, Rot4.Invalid);
                    }
                }
            }
        }

        static void EnsureAtLeastOneMedibayInOutpost()
        {
            List<IntVec2> mediumRoomsList = new List<IntVec2>();

            for (int zoneAbs = 0; zoneAbs < OG_BigOutpost.horizontalZonesNumber; zoneAbs++)
            {
                for (int zoneOrd = 0; zoneOrd < OG_BigOutpost.verticalZonesNumber; zoneOrd++)
                {
                    ZoneType zoneType = zoneMap[zoneOrd, zoneAbs].zoneType;
                    if (zoneType == ZoneType.MediumRoomMedibay)
                    {
                        // Medibay is found.
                        // TODO: debug.
                        Log.Message("EnsureAtLeastOneMedibayInOutpost: Medibay is found at " + new IntVec2(zoneAbs, zoneOrd).ToString());
                        return;
                    }
                    if ((zoneType == ZoneType.MediumRoomBarn)
                        || (zoneType == ZoneType.MediumRoomKitchen)
                        || (zoneType == ZoneType.MediumRoomLaboratory)
                        || (zoneType == ZoneType.MediumRoomPrison)
                        || (zoneType == ZoneType.MediumRoomRecRoom)
                        || (zoneType == ZoneType.MediumRoomWarehouse)
                        || (zoneType == ZoneType.MediumRoomWeaponRoom))
                    {
                        mediumRoomsList.Add(new IntVec2(zoneAbs, zoneOrd));
                    }
                }
            }
            if (mediumRoomsList.NullOrEmpty())
            {
                Log.Warning("M&Co. Outpost generator: EnsureAtLeastOneMedibayInOutpost did not found any medium room.");
                return;
            }
            IntVec2 roomCoord = mediumRoomsList.RandomElement();
            // TODO: debug.
            Log.Message("EnsureAtLeastOneMedibayInOutpost: setting zoneMap at " + roomCoord.ToString());
            zoneMap[roomCoord.z, roomCoord.x].zoneType = ZoneType.MediumRoomMedibay;
        }

        /// <summary>
        /// Get the coordinates of one of the main rooms.
        /// </summary>
        static void GetMainRoomZone(int roomIndex, Rot4 mainEntranceDirection, out int mainRoomZoneAbs, out int mainRoomZoneOrd)
        {
            mainRoomZoneAbs = 0;
            mainRoomZoneOrd = 0;

            switch (roomIndex)
            {
                case 0:
                    // Relative south-west.
                    GetMainRoom0Zone(mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
                    break;
                case 1:
                    // Relative north-west.
                    GetMainRoom1Zone(mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
                    break;
                case 2:
                    // Relative north-east.
                    GetMainRoom2Zone(mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
                    break;
                case 3:
                    // Relative south-east.
                    GetMainRoom3Zone(mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
                    break;
                case 4:
                    // Central main room.
                    GetMainRoom4Zone(mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
                    break;
            }
        }

        static void GetMainRoom0Zone(Rot4 mainEntranceDirection, out int mainRoom0ZoneAbs, out int mainRoom0ZoneOrd)
        {
            mainRoom0ZoneAbs = 0;
            mainRoom0ZoneOrd = 0;

            if (mainEntranceDirection == Rot4.North)
            {
                mainRoom0ZoneAbs = 1;
                mainRoom0ZoneOrd = 2;
            }
            else if (mainEntranceDirection == Rot4.East)
            {
                mainRoom0ZoneAbs = 2;
                mainRoom0ZoneOrd = 5;
            }
            else if (mainEntranceDirection == Rot4.South)
            {
                mainRoom0ZoneAbs = 5;
                mainRoom0ZoneOrd = 4;
            }
            else if (mainEntranceDirection == Rot4.West)
            {
                mainRoom0ZoneAbs = 4;
                mainRoom0ZoneOrd = 1;
            }
        }

        static void GetMainRoom1Zone(Rot4 mainEntranceDirection, out int mainRoom1ZoneAbs, out int mainRoom1ZoneOrd)
        {
            mainRoom1ZoneAbs = 0;
            mainRoom1ZoneOrd = 0;

            if (mainEntranceDirection == Rot4.North)
            {
                mainRoom1ZoneAbs = 1;
                mainRoom1ZoneOrd = 4;
            }
            else if (mainEntranceDirection == Rot4.East)
            {
                mainRoom1ZoneAbs = 4;
                mainRoom1ZoneOrd = 5;
            }
            else if (mainEntranceDirection == Rot4.South)
            {
                mainRoom1ZoneAbs = 5;
                mainRoom1ZoneOrd = 2;
            }
            else if (mainEntranceDirection == Rot4.West)
            {
                mainRoom1ZoneAbs = 2;
                mainRoom1ZoneOrd = 1;
            }
        }

        static void GetMainRoom2Zone(Rot4 mainEntranceDirection, out int mainRoom2ZoneAbs, out int mainRoom2ZoneOrd)
        {
            mainRoom2ZoneAbs = 0;
            mainRoom2ZoneOrd = 0;

            if (mainEntranceDirection == Rot4.North)
            {
                mainRoom2ZoneAbs = 5;
                mainRoom2ZoneOrd = 4;
            }
            else if (mainEntranceDirection == Rot4.East)
            {
                mainRoom2ZoneAbs = 4;
                mainRoom2ZoneOrd = 1;
            }
            else if (mainEntranceDirection == Rot4.South)
            {
                mainRoom2ZoneAbs = 1;
                mainRoom2ZoneOrd = 2;
            }
            else if (mainEntranceDirection == Rot4.West)
            {
                mainRoom2ZoneAbs = 2;
                mainRoom2ZoneOrd = 5;
            }
        }

        static void GetMainRoom3Zone(Rot4 mainEntranceDirection, out int mainRoom3ZoneAbs, out int mainRoom3ZoneOrd)
        {
            mainRoom3ZoneAbs = 0;
            mainRoom3ZoneOrd = 0;

            if (mainEntranceDirection == Rot4.North)
            {
                mainRoom3ZoneAbs = 5;
                mainRoom3ZoneOrd = 2;
            }
            else if (mainEntranceDirection == Rot4.East)
            {
                mainRoom3ZoneAbs = 2;
                mainRoom3ZoneOrd = 1;
            }
            else if (mainEntranceDirection == Rot4.South)
            {
                mainRoom3ZoneAbs = 1;
                mainRoom3ZoneOrd = 4;
            }
            else if (mainEntranceDirection == Rot4.West)
            {
                mainRoom3ZoneAbs = 4;
                mainRoom3ZoneOrd = 5;
            }
        }

        static void GetMainRoom4Zone(Rot4 mainEntranceDirection, out int mainRoom4ZoneAbs, out int mainRoom4ZoneOrd)
        {
            mainRoom4ZoneAbs = 0;
            mainRoom4ZoneOrd = 0;

            if (mainEntranceDirection == Rot4.North)
            {
                mainRoom4ZoneAbs = 3;
                mainRoom4ZoneOrd = 4;
            }
            else if (mainEntranceDirection == Rot4.East)
            {
                mainRoom4ZoneAbs = 4;
                mainRoom4ZoneOrd = 3;
            }
            else if (mainEntranceDirection == Rot4.South)
            {
                mainRoom4ZoneAbs = 3;
                mainRoom4ZoneOrd = 2;
            }
            else if (mainEntranceDirection == Rot4.West)
            {
                mainRoom4ZoneAbs = 2;
                mainRoom4ZoneOrd = 3;
            }
        }
        
        // ######## Zone generation functions ######## //

        static void GenerateOutpostZones(IntVec3 areaSouthWestOrigin)
        {
            for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
            {
                for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
                {
                    ZoneProperties zone = zoneMap[zoneOrd, zoneAbs];
                    switch (zone.zoneType)
                    {
                        // Special big rooms.
                        case ZoneType.BigRoomRefectory:
                            OG_ZoneBigRoomSpecial.GenerateBigRoomRefectory(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.BigRoomBatteryRoom:
                            OG_ZoneBigRoomSpecial.GenerateBigRoomBatteryRoom(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, zoneMap, ref outpostData);
                            break;
                        case ZoneType.BigRoomCommandRoom:
                            commandConsole = OG_ZoneBigRoomSpecial.GenerateBigRoomCommandRoom(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.BigRoomBarracks:
                            OG_ZoneBigRoomSpecial.GenerateBigRoomBarracks(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.BigRoomHydroponics:
                            OG_ZoneBigRoomSpecial.GenerateBigRoomHydroponics(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;

                        // Standard medium rooms.
                        case ZoneType.MediumRoomMedibay:
                            OG_ZoneMediumRoom.GenerateMediumRoomMedibay(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.MediumRoomPrison:
                            OG_ZoneMediumRoom.GenerateMediumRoomPrison(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.MediumRoomKitchen:
                            OG_ZoneMediumRoom.GenerateMediumRoomKitchen(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.MediumRoomWarehouse:
                            OG_ZoneMediumRoom.GenerateMediumRoomWarehouse(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.MediumRoomWeaponRoom:
                            OG_ZoneMediumRoom.GenerateMediumRoomWeaponRoom(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.MediumRoomBarn:
                            OG_ZoneMediumRoom.GenerateMediumRoomBarn(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.MediumRoomLaboratory:
                            OG_ZoneMediumRoom.GenerateMediumRoomLaboratory(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.MediumRoomRecRoom:
                            OG_ZoneMediumRoom.GenerateMediumRoomRecRoom(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;

                        // Standard small rooms.
                        case ZoneType.SmallRoomBarracks:
                            OG_ZoneSmallRoom.GenerateSmallRoomBarracks(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.SmallRoomMedibay:
                            OG_ZoneSmallRoom.GenerateSmallRoomMedibay(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.SmallRoomWeaponRoom:
                            OG_ZoneSmallRoom.GenerateSmallRoomWeaponRoom(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                            
                        // Special zones.
                        case ZoneType.SolarPanelZone:
                            OG_ZoneSpecial.GenerateSolarPanelZone(areaSouthWestOrigin, zoneAbs, zoneOrd, ref outpostData);
                            break;
                        case ZoneType.DropZone:
                            OG_ZoneSpecial.GenerateDropZone(areaSouthWestOrigin, zoneAbs, zoneOrd, ref outpostData);
                            break;
                        case ZoneType.LandingPadBottom:
                            OG_ZoneSpecial.GenerateLandingPadBottom(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.LandingPadTop:
                            OG_ZoneSpecial.GenerateLandingPadTop(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.OrbitalRelay:
                            orbitalRelay = OG_ZoneSpecial.GenerateOrbitalRelayZone(areaSouthWestOrigin, zoneAbs, zoneOrd, ref outpostData);
                            break;

                        // Exterior zones.
                        case ZoneType.Farm:
                            OG_ZoneExterior.GenerateFarmZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.Cemetery:
                            OG_ZoneExterior.GenerateCemeteryZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.ExteriorRecRoom:
                            OG_ZoneExterior.GenerateExteriorRecZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.WaterPool:
                            OG_ZoneExterior.GenerateWaterPoolZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.ShootingRange:
                            OG_ZoneExterior.GenerateShootingRangeZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.MortarBay:
                            OG_ZoneExterior.GenerateMortarZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;

                        // Other zones.
                        case ZoneType.Empty:
                            // Nothing to do;
                            break;
                        case ZoneType.MainEntrance:
                            OG_ZoneOther.GenerateMainEntranceZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.SecondaryEntrance:
                            OG_ZoneOther.GenerateSecondaryEntranceZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.EntranchedZone:
                            OG_ZoneOther.GenerateEntranchedZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.SamSite:
                            OG_ZoneOther.GenerateSamSiteZone(areaSouthWestOrigin, zoneAbs, zoneOrd, ref outpostData);
                            break;
                        case ZoneType.BigSas:
                            OG_ZoneOther.GenerateBigSasZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, zone.linkedZoneRelativeRotation != Rot4.Invalid, ref outpostData);
                            break;
                        case ZoneType.Plaza:
                            OG_ZoneOther.GeneratePlazaZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.StraightAlley:
                            OG_ZoneOther.GenerateStraightAlleyZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                    }
                }
            }
        }
        
        private static void GenerateSasToLinkMediumAndMainRooms(IntVec3 areaSouthWestOrigin)
        {
            int mainRoomZoneAbs = 0;
            int mainRoomZoneOrd = 0;
            int adjacentZoneAbs = 0;
            int adjacentZoneOrd = 0;

            for (int mainRoomIndex = 0; mainRoomIndex < 4; mainRoomIndex++)
            {
                GetMainRoomZone(mainRoomIndex, mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);

                for (int rotationAsInt = 0; rotationAsInt < 4; rotationAsInt++)
                {
                    Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, new Rot4(rotationAsInt), out adjacentZoneAbs, out adjacentZoneOrd);
                    ZoneType zoneType = zoneMap[adjacentZoneOrd, adjacentZoneAbs].zoneType;
                    if ((zoneType == ZoneType.MediumRoomMedibay)
                        || (zoneType == ZoneType.MediumRoomPrison)
                        || (zoneType == ZoneType.MediumRoomKitchen)
                        || (zoneType == ZoneType.MediumRoomWarehouse)
                        || (zoneType == ZoneType.MediumRoomWeaponRoom)
                        || (zoneType == ZoneType.MediumRoomBarn)
                        || (zoneType == ZoneType.MediumRoomLaboratory)
                        || (zoneType == ZoneType.MediumRoomRecRoom))
                    {
                        IntVec3 sasOrigin = Zone.GetZoneOrigin(areaSouthWestOrigin, mainRoomZoneAbs, mainRoomZoneOrd);
                        Rot4 rotation = new Rot4(rotationAsInt);
                        IntVec3 sasOriginOffset = new IntVec3(0, 0, 0);
                        if (rotation == Rot4.North)
                        {
                            sasOriginOffset = new IntVec3(5, 0, 10);
                        }
                        else if (rotation == Rot4.East)
                        {
                            sasOriginOffset = new IntVec3(10, 0, 5);
                        }
                        else if (rotation == Rot4.South)
                        {
                            sasOriginOffset = new IntVec3(5, 0, 0);
                        }
                        else if (rotation == Rot4.West)
                        {
                            sasOriginOffset = new IntVec3(0, 0, 5);
                        }
                        OG_Common.GenerateSas(sasOrigin + sasOriginOffset, rotation, 3, 3, ref outpostData);
                    }
                }
            }
        }

        private static void SpawnNoRoofAreaGenerator(IntVec3 areaSouthWestOrigin)
        {
            Building_NoRoofAreaGenerator noRoofAreaGenerator = ThingMaker.MakeThing(ThingDef.Named("NoRoofAreaGenerator")) as Building_NoRoofAreaGenerator;
            noRoofAreaGenerator.areaSouthWestOrigin = areaSouthWestOrigin;
            GenSpawn.Spawn(noRoofAreaGenerator, areaSouthWestOrigin + new IntVec3(-1, 0, -1));
        }
    }
}
