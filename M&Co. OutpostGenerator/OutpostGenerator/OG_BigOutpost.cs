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
    // TODO: add lazer/vulcan turret.
    // TODO: add SAM site.
    // TODO: add landing light (random tick offset to turn comp glower on and off).

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
        static int smallRoomWallOffset = 2; // Empty space in every direction of the room in the zone.

        private static Rot4 mainEntranceDirection;
        private static ZoneProperties[,] zoneMap = new ZoneProperties[verticalZonesNumber, horizontalZonesNumber];
        private const int mainRoomsNumber = 5;

        private static OG_OutpostData outpostData = new OG_OutpostData();
        private static Building_OutpostCommandConsole commandConsole = null;
        
        public static void GenerateOutpost(OG_OutpostData outpostDataParameter)
        {
            outpostData = outpostDataParameter;
            outpostData.structureStuffDef = ThingDef.Named("BlocksGranite");
            outpostData.furnitureStuffDef = ThingDefOf.Steel;
            outpostData.triggerIntrusion = null;
            outpostData.outpostThingList = new List<Thing>();

            // Clear the whole area, remove any roof and water.
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
                TerrainDef terrain = Find.TerrainGrid.TerrainAt(cell);
                if ((terrain == TerrainDef.Named("Marsh"))
                    || (terrain == TerrainDef.Named("Mud"))
                    || (terrain == TerrainDef.Named("WaterShallow")))
                {
                    Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                }
            }

            // Create the intrusion trigger.
            outpostData.triggerIntrusion = (TriggerIntrusion)ThingMaker.MakeThing(ThingDef.Named("TriggerIntrusion"));
            GenSpawn.Spawn(outpostData.triggerIntrusion, rect.Center);

            GenerateOutpostLayout();
            
            // TODO: debug. Display the generated layout.
            for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
            {
                for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
                {
                    ZoneProperties zone = zoneMap[zoneOrd, zoneAbs];
                    Log.Message("Layout: zoneMap[" + zoneOrd + "," + zoneAbs + "] => " + zone.zoneType.ToString() + "," + zone.rotation.ToString() + "," + zone.linkedZoneRelativeRotation.ToString());
                }
            }

            GenerateOutpostZones(outpostData.areaSouthWestOrigin);
            // TODO: improve sas generation (should be more generic).
            /*IntVec3 mainRoomZoneOrigin = Zone.GetZoneOrigin(outpostData.areaSouthWestOrigin, mainRoomZoneAbs, mainRoomZoneOrd);
            GenerateSas(mainRoomZoneOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, Genstep_GenerateOutpost.zoneSideSize - 1), Rot4.North);
            GenerateSas(mainRoomZoneOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideSize - 1, 0, Genstep_GenerateOutpost.zoneSideCenterOffset), Rot4.East);
            GenerateSas(mainRoomZoneOrigin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, 0), Rot4.South);
            GenerateSas(mainRoomZoneOrigin + new IntVec3(0, 0, Genstep_GenerateOutpost.zoneSideCenterOffset), Rot4.West);
            
            // Generate laser fences.
            GenerateLaserFence(ref outpostData);
            // Generate battle remains.
            GenerateWarfieldEffects(outpostData);
            // Damage outpost to reflect its history.
            GenerateRuinEffects(ref outpostData);
            // Generate some inhabitants.
            //GenerateInhabitants(outpostData); // TODO: will be hard!
            
            // Initialize command console data.
            outpostData.outpostThingList = OG_Util.RefreshThingList(outpostData.outpostThingList);
            commandConsole.outpostThingList = outpostData.outpostThingList.ListFullCopy<Thing>();
            commandConsole.dropZoneCenter = outpostData.dropZoneCenter;
            // Initialize intrusion trigger data.
            outpostData.triggerIntrusion.commandConsole = commandConsole;
            */
            SendWelcomeLetter(outpostData);
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

            mainEntranceDirection = Rot4.Random;
            Log.Message("mainEntranceDirection = " + mainEntranceDirection.ToString());

            GenerateOutpostLayoutCentralRoom();
            GenerateOutpostLayoutRadarDome();
            GenerateOutpostLayoutPowerSupply();
            GenerateOutpostLayoutCommandRoom();
            GenerateOutpostLayoutBarracks();
            GenerateOutpostLayoutHydroponicRoom();
            GenerateOutpostLayoutBigSasAndSecondaryEntrances();

            GenerateOutpostLayoutAroundPlaza();
            GenerateOutpostLayoutEntranchedZones();

            /*GenerateOutpostLayoutSecondaryRooms();*/
        }

        /// <summary>
        /// Generate the layout of the central room: main refectory.
        /// </summary>
        static void GenerateOutpostLayoutCentralRoom()
        {
            int mainRoom4ZoneAbs = 0;
            int mainRoom4ZoneOrd = 0;

            GetMainRoomZone(4, mainEntranceDirection, out mainRoom4ZoneAbs, out mainRoom4ZoneOrd);
            zoneMap[mainRoom4ZoneOrd, mainRoom4ZoneAbs] = new ZoneProperties(ZoneType.BigRoomWarehouse, Rot4.Random, Rot4.Invalid); // TODO: debug. ZoneType.BigRoomRefectory
        }

        static void GenerateOutpostLayoutRadarDome()
        {
            int mainRoom4ZoneAbs = 0;
            int mainRoom4ZoneOrd = 0;
            int radarDomeZoneAbs = 0;
            int radarDomeZoneOrd = 0;

            GetMainRoomZone(4, mainEntranceDirection, out mainRoom4ZoneAbs, out mainRoom4ZoneOrd);
            Zone.GetAdjacentZone(mainRoom4ZoneAbs, mainRoom4ZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), out radarDomeZoneAbs, out radarDomeZoneOrd);
            Zone.GetAdjacentZone(mainRoom4ZoneAbs, mainRoom4ZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), out radarDomeZoneAbs, out radarDomeZoneOrd);
            zoneMap[radarDomeZoneOrd, radarDomeZoneAbs] = new ZoneProperties(ZoneType.RadarDome, mainEntranceDirection, Rot4.Invalid);

            zoneMap[mainRoom4ZoneOrd, mainRoom4ZoneAbs] = new ZoneProperties(ZoneType.BigRoomWarehouse, Rot4.Random, Rot4.Invalid); // TODO: debug. ZoneType.BigRoomRefectory
        }

        static void GenerateOutpostLayoutPowerSupply()
        {
            int batteryRoomZoneAbs = 0;
            int batteryRoomZoneOrd = 0;
            int solarPanelZoneAbs = 0;
            int solarPanelZoneOrd = 0;
            int mainRoomIndex = 0;
            
            GetFreeMainRoomZone(out batteryRoomZoneAbs, out batteryRoomZoneOrd, out mainRoomIndex);

            Log.Message("batteryRoom mainRoomIndex = " + mainRoomIndex);
            // Set battery room zone.
            Rot4 batteryRoomRotation = new Rot4(Rot4.West.AsInt + mainRoomIndex); // Battery room is facing west for main room 0, north for main room 1, ...
            zoneMap[batteryRoomZoneOrd, batteryRoomZoneAbs] = new ZoneProperties(ZoneType.BigRoomWarehouse, batteryRoomRotation, Rot4.Invalid); // TODO: debug. ZoneType.BigRoomBatteryRoom

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

            Log.Message("commandRoom mainRoomIndex = " + mainRoomIndex);
            zoneMap[commandRoomZoneOrd, commandRoomZoneAbs] = new ZoneProperties(ZoneType.BigRoomWarehouse, Rot4.Random, Rot4.Invalid); // TODO: debug. ZoneType.BigRoomCommandRoom

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
                    zoneMap[landingPadTopZoneOrd, landingPadTopZoneAbs] = new ZoneProperties(ZoneType.SecondaryEntrance, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.South); // TODO: debug. ZoneType.LandingPadTop
                    Zone.GetAdjacentZone(landingPadTopZoneAbs, landingPadTopZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadBottomZoneAbs, out landingPadBottomZoneOrd);
                    zoneMap[landingPadBottomZoneOrd, landingPadBottomZoneAbs] = new ZoneProperties(ZoneType.SolarPanelZone, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.North); // TODO: debug. ZoneType.LandingPadTop
                }
                else
                {
                    // Landing pad bottom first.
                    Zone.GetAdjacentZone(dropZoneZoneAbs, dropZoneZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadBottomZoneAbs, out landingPadBottomZoneOrd);
                    zoneMap[landingPadBottomZoneOrd, landingPadBottomZoneAbs] = new ZoneProperties(ZoneType.SolarPanelZone, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.North); // TODO: debug. ZoneType.LandingPadTop
                    Zone.GetAdjacentZone(landingPadBottomZoneAbs, landingPadBottomZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadTopZoneAbs, out landingPadTopZoneOrd);
                    zoneMap[landingPadTopZoneOrd, landingPadTopZoneAbs] = new ZoneProperties(ZoneType.SecondaryEntrance, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.South); // TODO: debug. ZoneType.LandingPadTop
                }
            }
            else
            {
                // Landing pad first.
                if (Rand.Value < 0.5f)
                {
                    // Landing pad top first.
                    Zone.GetAdjacentZone(commandRoomZoneAbs, commandRoomZoneOrd, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadTopZoneAbs, out landingPadTopZoneOrd);
                    zoneMap[landingPadTopZoneOrd, landingPadTopZoneAbs] = new ZoneProperties(ZoneType.SecondaryEntrance, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.South); // TODO: debug. ZoneType.LandingPadTop
                    Zone.GetAdjacentZone(landingPadTopZoneAbs, landingPadTopZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadBottomZoneAbs, out landingPadBottomZoneOrd);
                    zoneMap[landingPadBottomZoneOrd, landingPadBottomZoneAbs] = new ZoneProperties(ZoneType.SolarPanelZone, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.North); // TODO: debug. ZoneType.LandingPadTop
                    Zone.GetAdjacentZone(landingPadBottomZoneAbs, landingPadBottomZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out dropZoneZoneAbs, out dropZoneZoneOrd);
                    zoneMap[dropZoneZoneOrd, dropZoneZoneAbs] = new ZoneProperties(ZoneType.DropZone, Rot4.North, Rot4.Invalid);
                }
                else
                {
                    // Landing pad bottom first.
                    Zone.GetAdjacentZone(commandRoomZoneAbs, commandRoomZoneOrd, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadBottomZoneAbs, out landingPadBottomZoneOrd);
                    zoneMap[landingPadBottomZoneOrd, landingPadBottomZoneAbs] = new ZoneProperties(ZoneType.SolarPanelZone, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.North); // TODO: debug. ZoneType.LandingPadTop
                    Zone.GetAdjacentZone(landingPadBottomZoneAbs, landingPadBottomZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), out landingPadTopZoneAbs, out landingPadTopZoneOrd);
                    zoneMap[landingPadTopZoneOrd, landingPadTopZoneAbs] = new ZoneProperties(ZoneType.SecondaryEntrance, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt + mainRoomIndex), Rot4.South); // TODO: debug. ZoneType.LandingPadTop
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

            Log.Message("barracks mainRoomIndex = " + mainRoomIndex);
            zoneMap[barracksZoneOrd, barracksZoneAbs] = new ZoneProperties(ZoneType.BigRoomWarehouse, Rot4.Random, Rot4.Invalid); // TODO: debug. ZoneType.BigRoomBarracks
        }

        static void GenerateOutpostLayoutHydroponicRoom()
        {
            int hydroponicRoomZoneAbs = 0;
            int hydroponicRoomZoneOrd = 0;
            int mainRoomIndex = 0;

            GetFreeMainRoomZone(out hydroponicRoomZoneAbs, out hydroponicRoomZoneOrd, out mainRoomIndex);

            Log.Message("hydroponic room mainRoomIndex = " + mainRoomIndex);
            zoneMap[hydroponicRoomZoneOrd, hydroponicRoomZoneAbs] = new ZoneProperties(ZoneType.BigRoomWarehouse, Rot4.Random, Rot4.Invalid); // TODO: debug. ZoneType.BigRoomHydroponics
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
            zoneMap[bigSasZoneOrd, bigSasZoneAbs] = new ZoneProperties(ZoneType.BigSas, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            Zone.GetAdjacentZone(bigSasZoneAbs, bigSasZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), out secondaryEntranceZoneAbs, out secondaryEntranceZoneOrd);
            zoneMap[secondaryEntranceZoneOrd, secondaryEntranceZoneAbs] = new ZoneProperties(ZoneType.SecondaryEntrance, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            // Sas north from main room 3.
            GetMainRoomZone(3, mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
            Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), out bigSasZoneAbs, out bigSasZoneOrd);
            zoneMap[bigSasZoneOrd, bigSasZoneAbs] = new ZoneProperties(ZoneType.BigSas, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            Zone.GetAdjacentZone(bigSasZoneAbs, bigSasZoneOrd, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), out secondaryEntranceZoneAbs, out secondaryEntranceZoneOrd);
            zoneMap[secondaryEntranceZoneOrd, secondaryEntranceZoneAbs] = new ZoneProperties(ZoneType.SecondaryEntrance, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            // Sas west and east from main room 4.
            GetMainRoomZone(4, mainEntranceDirection, out mainRoomZoneAbs, out mainRoomZoneOrd);
            Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), out bigSasZoneAbs, out bigSasZoneOrd);
            zoneMap[bigSasZoneOrd, bigSasZoneAbs] = new ZoneProperties(ZoneType.BigSas, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), out bigSasZoneAbs, out bigSasZoneOrd);
            zoneMap[bigSasZoneOrd, bigSasZoneAbs] = new ZoneProperties(ZoneType.BigSas, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
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
            zoneMap[plazaZoneOrd, plazaZoneAbs] = new ZoneProperties(ZoneType.SolarPanelZone, Rot4.North, Rot4.Invalid); // TODO: debug. ZoneType.Plaza
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
            zoneMap[mainEntranceZoneOrd, mainEntranceZoneAbs] = new ZoneProperties(ZoneType.SecondaryEntrance, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid); // TODO: debug. ZoneType.MainEntrance

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
                zoneMap[0, 1] = new ZoneProperties(ZoneType.EntranchedZone, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
                zoneMap[0, horizontalZonesNumber - 2] = new ZoneProperties(ZoneType.EntranchedZone, new Rot4(Rot4.South.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
                zoneMap[verticalZonesNumber - 1, 1] = new ZoneProperties(ZoneType.EntranchedZone, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
                zoneMap[verticalZonesNumber - 1, horizontalZonesNumber - 2] = new ZoneProperties(ZoneType.EntranchedZone, new Rot4(Rot4.North.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            }
            else
            {
                zoneMap[1, 0] = new ZoneProperties(ZoneType.EntranchedZone, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
                zoneMap[verticalZonesNumber - 2, 0] = new ZoneProperties(ZoneType.EntranchedZone, new Rot4(Rot4.West.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
                zoneMap[1, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.EntranchedZone, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
                zoneMap[verticalZonesNumber - 2, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.EntranchedZone, new Rot4(Rot4.East.AsInt + mainEntranceDirection.AsInt), Rot4.Invalid);
            }
        }

        static void GenerateOutpostLayoutSamSites()
        {
            if (Rand.Value < 0.5f)
            {
                zoneMap[0, 0] = new ZoneProperties(ZoneType.SamSite, Rot4.North, Rot4.Invalid);
                zoneMap[verticalZonesNumber - 1, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.SamSite, Rot4.North, Rot4.Invalid);
                zoneMap[0, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.Empty, Rot4.North, Rot4.Invalid);
                zoneMap[verticalZonesNumber - 1, 0] = new ZoneProperties(ZoneType.Empty, Rot4.North, Rot4.Invalid);
            }
            else
            {
                zoneMap[verticalZonesNumber - 1, 0] = new ZoneProperties(ZoneType.SamSite, Rot4.North, Rot4.Invalid);
                zoneMap[0, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.SamSite, Rot4.North, Rot4.Invalid);
                zoneMap[0, 0] = new ZoneProperties(ZoneType.Empty, Rot4.North, Rot4.Invalid);
                zoneMap[verticalZonesNumber - 1, horizontalZonesNumber - 1] = new ZoneProperties(ZoneType.Empty, Rot4.North, Rot4.Invalid);
            }
        }

        /*static void GenerateOutpostLayoutCommandRoom()
        {
            Rot4 commandRoomCardinal;
            bool freeZoneIsFound = Zone.GetRandomFreeCardinalZoneAdjacentTo(mainRoomZoneAbs, mainRoomZoneOrd, out commandRoomCardinal, zoneMap, horizontalZonesNumber, verticalZonesNumber);
            if (freeZoneIsFound == false)
            {
                Log.Warning("M&Co. OutpostGenerator: failed to find a free zone for the command room.");
                return;
            }
            int commandRoomZoneAbs = 0;
            int commandRoomZoneOrd = 0;
            Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, commandRoomCardinal, out commandRoomZoneAbs, out commandRoomZoneOrd);

            float dropZoneSideSelector = Rand.Value;
            Rot4 relativeRotation = Rot4.North;
            Rot4 absoluteRotation = Rot4.North;
            if (dropZoneSideSelector < 0.5f)
            {
                // Drop zone is "on the west" of command room.
                relativeRotation = Rot4.West;
                absoluteRotation = new Rot4((commandRoomCardinal.AsInt + relativeRotation.AsInt) % 4);
            }
            else
            {
                // Drop zone is "on the east" of command room.
                relativeRotation = Rot4.East;
                absoluteRotation = new Rot4((commandRoomCardinal.AsInt + relativeRotation.AsInt) % 4);
            }
            int dropZoneAbs = 0;
            int dropZoneOrd = 0;
            Zone.GetAdjacentZone(commandRoomZoneAbs, commandRoomZoneOrd, absoluteRotation, out dropZoneAbs, out dropZoneOrd);
            if ((Zone.ZoneIsInArea(dropZoneAbs, dropZoneOrd, horizontalZonesNumber, verticalZonesNumber) == false)
                || (zoneMap[dropZoneOrd, dropZoneAbs].zoneType != ZoneType.NotYetGenerated))
            {
                // Get the opposite side.
                relativeRotation = new Rot4((relativeRotation.AsInt + 2) % 4);
                absoluteRotation = new Rot4((absoluteRotation.AsInt + 2) % 4);
                Zone.GetAdjacentZone(commandRoomZoneAbs, commandRoomZoneOrd, absoluteRotation, out dropZoneAbs, out dropZoneOrd);
                if ((Zone.ZoneIsInArea(dropZoneAbs, dropZoneOrd, horizontalZonesNumber, verticalZonesNumber) == false)
                    || (zoneMap[dropZoneOrd, dropZoneAbs].zoneType != ZoneType.NotYetGenerated))
                {
                    Log.Warning("M&Co. OutpostGenerator: failed to find a free zone for the drop zone.");
                    return;
                }
            }
            zoneMap[commandRoomZoneOrd, commandRoomZoneAbs] = new ZoneProperties(ZoneType.SmallRoomCommandRoom, commandRoomCardinal, relativeRotation);
            relativeRotation.Rotate(RotationDirection.Clockwise); // Rotate 2 times to get the opposite direction.
            relativeRotation.Rotate(RotationDirection.Clockwise);
            zoneMap[dropZoneOrd, dropZoneAbs] = new ZoneProperties(ZoneType.DropZone, commandRoomCardinal, relativeRotation);
        }*/

        /*static void GenerateOutpostLayoutSecondaryRooms()
        {
            for (int rotationAsInt = 0; rotationAsInt < 4; rotationAsInt++)
            {
                int zoneAbs = 0;
                int zoneOrd = 0;
                Rot4 rotation = new Rot4(rotationAsInt);
                Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, rotation, out zoneAbs, out zoneOrd);
                if (zoneMap[zoneOrd, zoneAbs].zoneType == ZoneType.NotYetGenerated)
                {
                    ZoneType secondaryRoomType = GetRandomZoneTypeSmallRoom();
                    zoneMap[zoneOrd, zoneAbs] = new ZoneProperties(secondaryRoomType, rotation, Rot4.North);
                }
            }
        }*/

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

        static ZoneType GetRandomZoneTypeBigRoom()
        {
            List<ZoneTypeWithWeight> bigRoomsList = new List<ZoneTypeWithWeight>();
            if (outpostData.isMilitary)
            {
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomLivingRoom, 2f));
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomWarehouse, 2f));
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomPrison, 6f));
            }
            else
            {
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomLivingRoom, 4f));
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomWarehouse, 4f));
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomPrison, 2f));
            }

            ZoneType bigRoomType = GetRandomZoneTypeByWeight(bigRoomsList);
            return bigRoomType;
        }

        static ZoneType GetRandomZoneTypeSmallRoom()
        {
            List<ZoneTypeWithWeight> smallRoomsList = new List<ZoneTypeWithWeight>();
            if (outpostData.isMilitary)
            {
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomBarracks, 2f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomMedibay, 1f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomWeaponRoom, 5f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SecondaryEntrance, 6f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.Empty, 1f));
            }
            else
            {
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomBarracks, 4f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomMedibay, 3f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomWeaponRoom, 1f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SecondaryEntrance, 2f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.Empty, 1f));
            }

            ZoneType smallRoomType = GetRandomZoneTypeByWeight(smallRoomsList);
            return smallRoomType;
        }

        static ZoneType GetRandomZoneTypeByWeight(List<ZoneTypeWithWeight> list)
        {
            float weightTotalSum = 0;
            foreach (ZoneTypeWithWeight element in list)
            {
                weightTotalSum += element.weight;
            }
            float elementSelector = Rand.Range(0f, weightTotalSum);

            float weightSum = 0;
            foreach (ZoneTypeWithWeight element in list)
            {
                weightSum += element.weight;
                if (elementSelector <= weightSum)
                {
                    return element.zoneType;
                }
            }

            return ZoneType.Empty;
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
                        // Standard big rooms.
                        case ZoneType.BigRoomLivingRoom:
                            OG_ZoneBigRoom.GenerateBigRoomLivingRoom(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.BigRoomWarehouse:
                            OG_ZoneBigRoom.GenerateBigRoomWherehouse(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.BigRoomPrison:
                            OG_ZoneBigRoom.GenerateBigRoomPrison(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
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

                        // Special small rooms.
                        case ZoneType.SmallRoomBatteryRoom:
                            OG_ZoneSmallRoomSpecial.GenerateBatteryRoomZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, zone.linkedZoneRelativeRotation, ref outpostData);
                            break;
                        case ZoneType.SmallRoomCommandRoom:
                            commandConsole = OG_ZoneSmallRoomSpecial.GenerateCommandRoomZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, zone.linkedZoneRelativeRotation, ref outpostData);
                            break;

                        // Special zones.
                        case ZoneType.SolarPanelZone:
                            OG_ZoneSpecial.GenerateSolarPanelZone(areaSouthWestOrigin, zoneAbs, zoneOrd, ref outpostData);
                            break;
                        case ZoneType.DropZone:
                            OG_ZoneSpecial.GenerateDropZone(areaSouthWestOrigin, zoneAbs, zoneOrd, ref outpostData);
                            break;

                        // Other zones.
                        case ZoneType.Empty:
                            OG_ZoneOther.GenerateEmptyZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation);
                            break;
                        case ZoneType.SecondaryEntrance:
                            OG_ZoneOther.GenerateSecondaryEntranceZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                    }
                }
            }
        }

        // ######## Sas functions ######## //

        static void GenerateSas(IntVec3 origin, Rot4 rotation)
        {
            int sasWidth = 1; // Distance of the wall from the central alley.
            int sasHeight = smallRoomWallOffset * 2;

            OG_Common.GenerateEmptyRoomAt(origin + new IntVec3(-sasWidth, 0, 0).RotatedBy(rotation), 1 + 2 * sasWidth, sasHeight, rotation, null, null, ref outpostData);
            OG_Common.SpawnDoorAt(origin, ref outpostData);
            OG_Common.SpawnDoorAt(origin + new IntVec3(0, 0, sasHeight - 1).RotatedBy(rotation), ref outpostData);
            // Generate floor.
            for (int xOffset = -sasWidth - 1; xOffset <= sasWidth + 1; xOffset++)
            {
                for (int zOffset = 0; zOffset < sasHeight; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(origin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            for (int zOffset = 0; zOffset < sasHeight; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(origin + new IntVec3(0, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
        }

        // ######## Warfield effects functions ######## //

        static void GenerateWarfieldEffects(OG_OutpostData outpostData)
        {
            bool battleZoneIsFound = false;
            int battleZoneAbs = 0;
            int battleZoneOrd = 0;

            if (outpostData.battleOccured == false)
            {
                return;
            }
            battleZoneIsFound = GetBattleZoneAbsAndOrd(out battleZoneAbs, out battleZoneOrd);
            if (battleZoneIsFound == false)
            {
                Log.Warning("M&Co. OutpostGenerator: failed to find an appropriate zone to generate warfield.");
                return;
            }

            Building_WarfieldGenerator warfieldGenerator = ThingMaker.MakeThing(ThingDef.Named("WarfieldGenerator")) as Building_WarfieldGenerator;
            warfieldGenerator.battleZoneAbs = battleZoneAbs;
            warfieldGenerator.battleZoneOrd = battleZoneOrd;
            warfieldGenerator.outpostData = outpostData;
            IntVec3 warfieldCenter = Zone.GetZoneOrigin(outpostData.areaSouthWestOrigin, battleZoneAbs, battleZoneOrd) + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset);
            GenSpawn.Spawn(warfieldGenerator, warfieldCenter);
        }

        static bool GetBattleZoneAbsAndOrd(out int battleZoneAbs, out int battleZoneOrd)
        {
            battleZoneAbs = 0;
            battleZoneOrd = 0;

            // Look for an entranched zone.
            for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
            {
                for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
                {
                    if (zoneMap[zoneOrd, zoneAbs].zoneType == ZoneType.SecondaryEntrance)
                    {
                        battleZoneAbs = zoneAbs;
                        battleZoneOrd = zoneOrd;
                        return true;
                    }
                }
            }
            // Else, look for an empty zone.
            for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
            {
                for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
                {
                    if ((zoneMap[zoneOrd, zoneAbs].zoneType == ZoneType.NotYetGenerated)
                        || (zoneMap[zoneOrd, zoneAbs].zoneType == ZoneType.Empty))
                    {
                        battleZoneAbs = zoneAbs;
                        battleZoneOrd = zoneOrd;
                        return true;
                    }
                }
            }
            return false;
        }

        // ######## Ruin effects functions ######## //

        static void GenerateRuinEffects(ref OG_OutpostData outpostData)
        {
            GenerateRuinDamage(ref outpostData);
            GenerateRuinFilth(ref outpostData);
        }

        static void GenerateRuinDamage(ref OG_OutpostData outpostData)
        {
            float minHitPointsFactor = 0.10f;
            float maxHitPointsFactor = 1.0f;
            float damageDensity = 0.5f;
            if (outpostData.isRuined)
            {
                // Ruined outpost.
                minHitPointsFactor = 0.2f;
                maxHitPointsFactor = 0.6f;
                damageDensity = 0.3f;
            }
            else
            {
                // Only rusty outpost.
                minHitPointsFactor = 0.8f;
                maxHitPointsFactor = 1f;
                damageDensity = 0.05f;
            }
            foreach (Thing thing in outpostData.outpostThingList)
            {
                if (Rand.Value < damageDensity)
                {
                    thing.HitPoints = (int)((float)thing.MaxHitPoints * Rand.Range(minHitPointsFactor, maxHitPointsFactor));
                }
            }
        }

        static void GenerateRuinFilth(ref OG_OutpostData outpostData)
        {
            const float dustDensity = 0.3f;
            const float slagDensity = 0.1f;
            if (outpostData.isRuined)
            {
                CellRect areaRect = new CellRect(outpostData.areaSouthWestOrigin.x, outpostData.areaSouthWestOrigin.z, areaSideLength, areaSideLength);
                foreach (IntVec3 cell in areaRect)
                {
                    if (cell.GetEdifice() != null)
                    {
                        continue;
                    }

                    if (Rand.Value < dustDensity)
                    {
                        GenSpawn.Spawn(ThingDefOf.FilthDirt, cell);
                    }
                    if (Rand.Value < slagDensity)
                    {
                        float slagSelector = Rand.Value;
                        if (slagSelector < 0.33f)
                        {
                            GenSpawn.Spawn(ThingDef.Named("RockRubble"), cell);
                        }
                        else if (slagSelector < 0.66f)
                        {
                            GenSpawn.Spawn(ThingDef.Named("BuildingRubble"), cell);
                        }
                        else
                        {
                            GenSpawn.Spawn(ThingDef.Named("SandbagRubble"), cell);
                        }
                    }
                }
            }
        }

        // ######## Laser fences functions ######## //
        
        static void GenerateLaserFence(ref OG_OutpostData outpostData)
        {
            if (ModsConfig.IsActive("M&Co. LaserFence") == false)
            {
                Log.Warning("M&Co. OutpostGenerator: M&Co. LaserFence mod is not active. Cannot generate laser fences.");
                return;
            }
            OG_LaserFence.GenerateLaserFence(zoneMap, ref outpostData);
        }
    }
}
