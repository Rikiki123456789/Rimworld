using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// OG_SmallOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_SmallOutpost
    {
        public const int horizontalZonesNumber = 3;
        public const int verticalZonesNumber = 3;
        public const int mainRoomZoneAbs = 1;
        public const int mainRoomZoneOrd = 1;
        public const int areaSideLength = 3 * Genstep_GenerateOutpost.zoneSideSize;
        static int smallRoomWallOffset = 2; // Empty space in every direction of the room in the zone.

        static ZoneProperties[,] zoneMap = new ZoneProperties[verticalZonesNumber, horizontalZonesNumber];

        public static OG_OutpostData outpostData = new OG_OutpostData();
        static Building_OutpostCommandConsole commandConsole = null;
        
        public static void GenerateOutpost(OG_OutpostData outpostDataParameter)
        {
            outpostData = outpostDataParameter;
            outpostData.triggerIntrusion = null;
            outpostData.outpostThingList = new List<Thing>();

            // Reset zoneMap.
            for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
            {
                for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
                {
                    zoneMap[zoneOrd, zoneAbs] = new ZoneProperties(ZoneType.NotYetGenerated, Rot4.North, Rot4.North);
                }
            }
            // Clear the whole area and remove any roof.
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
            }
            // Create the intrusion trigger.
            outpostData.triggerIntrusion = (TriggerIntrusion)ThingMaker.MakeThing(ThingDef.Named("TriggerIntrusion"));
            GenSpawn.Spawn(outpostData.triggerIntrusion, rect.CenterCell);

            GenerateOutpostLayout();
            
            // TODO: debug. Display the generated layout.
            /*for (int zoneOrd = 0; zoneOrd < verticalZonesNumber; zoneOrd++)
            {
                for (int zoneAbs = 0; zoneAbs < horizontalZonesNumber; zoneAbs++)
                {
                    ZoneProperties zone = zoneMap[zoneOrd, zoneAbs];
                    Log.Message("Layout: zoneMap[" + zoneOrd + "," + zoneAbs + "] => " + zone.zoneType.ToString() + "," + zone.rotation.ToString() + "," + zone.linkedZoneRelativeRotation.ToString());
                }
            }*/
            GenerateOutpostZones(outpostData.areaSouthWestOrigin);
            
            IntVec3 mainRoomZoneOrigin = Zone.GetZoneOrigin(outpostData.areaSouthWestOrigin, mainRoomZoneAbs, mainRoomZoneOrd);
            OG_Common.GenerateSas(mainRoomZoneOrigin + new IntVec3(5, 0, 10), Rot4.North, 3, smallRoomWallOffset * 2, ref outpostData);
            OG_Common.GenerateSas(mainRoomZoneOrigin + new IntVec3(10, 0, 5), Rot4.East, 3, smallRoomWallOffset * 2, ref outpostData);
            OG_Common.GenerateSas(mainRoomZoneOrigin + new IntVec3(5, 0, 0), Rot4.South, 3, smallRoomWallOffset * 2, ref outpostData);
            OG_Common.GenerateSas(mainRoomZoneOrigin + new IntVec3(0, 0, 5), Rot4.West, 3, smallRoomWallOffset * 2, ref outpostData);
            
            // Generate laser fences.
            OG_LaserFence.GenerateLaserFence(zoneMap, ref outpostData);
            // Generate battle remains.
            OG_WarfieldEffects.GenerateWarfieldEffects(zoneMap, horizontalZonesNumber, verticalZonesNumber, outpostData);
            // Damage outpost to reflect its history.
            OG_RuinEffects.GenerateRuinEffects(ref outpostData);
            // Don't generate permanent inhabitants for small outposts. Those are just used as a shack by exploration teams.

            // Initialize command console data.
            outpostData.outpostThingList = OG_Util.RefreshThingList(outpostData.outpostThingList);
            commandConsole.outpostThingList = outpostData.outpostThingList.ListFullCopy<Thing>();
            commandConsole.dropZoneCenter = outpostData.dropZoneCenter;
            // Initialize intrusion trigger data.
            outpostData.triggerIntrusion.commandConsole = commandConsole;

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
                eventTitle = "MiningCo. military outpost";
                eventText = "   MiningCo. warning message\n\n" +
                    "   Hello! I am Coralie, the AI in charge of outpost X" + sectorCoordX + ".Z" + sectorCoordZ + " Mil.\n\n" +
                    "My scanners detected the signature of your escape pods crashing nearby.\n\n" +
                    "WARNING! You are entering an MiningCo. military sector. No activity is authorized here.\n\n" +
                    "Please notice that anyone found in the sector will be shot on sight with regards to the RimLaw RL-" + rimLawNumber + ".\n\n" +
                    "---- End of transmision ---";
                letterType = LetterType.BadUrgent;
            }
            else
            {
                eventTitle = "MiningCo. civil outpost";
                eventText = "   MiningCo. information message\n\n" +
                    "   Hello! I am Coralie, the AI in charge of outpost X" + sectorCoordX + ".Z" + sectorCoordZ + " Civ.\n\n" +
                    "My scanners detected the signature of your escape pods crashing nearby.\n" +
                    "My emotional analyzer tells me to be sorry for your ship and the people who most probably died within.\n\n" +
                    "I remind you this sector is the legal property of MiningCo. However, the company generously grants you the right to pass through this sector.\n\n" +
                    "Please notice that any intruder found inside the outpost perimeter will be shot on sight with regards to the RimLaw RL-" + rimLawNumber + ".\n\n" +
                    "---- End of transmision ---";
                letterType = LetterType.Good;
            }
            Find.LetterStack.ReceiveLetter(eventTitle, eventText, letterType);
        }

        // ######## Layout generation functions ######## //

        static void GenerateOutpostLayout()
        {
            GenerateOutpostLayoutMainRoom();
            GenerateOutpostLayoutPowerSupply();
            GenerateOutpostLayoutCommandRoom();
            GenerateOutpostLayoutSecondaryRooms();
        }

        static void GenerateOutpostLayoutMainRoom()
        {
            ZoneType mainRoomType = OG_Common.GetRandomZoneTypeBigRoom(outpostData);
            Rot4 mainRoomRotation = Rot4.Random;
            zoneMap[mainRoomZoneOrd, mainRoomZoneAbs] = new ZoneProperties(mainRoomType, mainRoomRotation, Rot4.North);
        }

        static void GenerateOutpostLayoutPowerSupply()
        {
            Rot4 batteryRoomCardinal;
            bool freeZoneIsFound = Zone.GetRandomFreeCardinalZoneAdjacentTo(mainRoomZoneAbs, mainRoomZoneOrd, out batteryRoomCardinal, zoneMap, horizontalZonesNumber, verticalZonesNumber);
            if (freeZoneIsFound == false)
            {
                Log.Warning("MiningCo. OutpostGenerator: failed to find a free zone for the battery room.");
                return;
            }
            int batteryRoomZoneAbs = 0;
            int batteryRoomZoneOrd = 0;
            Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, batteryRoomCardinal, out batteryRoomZoneAbs, out batteryRoomZoneOrd);

            float solarPanelZoneSideSelector = Rand.Value;
            Rot4 relativeRotation = Rot4.North;
            Rot4 absoluteRotation = Rot4.North;
            if (solarPanelZoneSideSelector < 0.5f)
            {
                // Solar zone is "on the west" of battery room.
                relativeRotation = Rot4.West;
                absoluteRotation = new Rot4((batteryRoomCardinal.AsInt + relativeRotation.AsInt) % 4);
            }
            else
            {
                // Solar zone is "on the east" of battery room.
                relativeRotation = Rot4.East;
                absoluteRotation = new Rot4((batteryRoomCardinal.AsInt + relativeRotation.AsInt) % 4);
            }
            int solarPanelZoneAbs = 0;
            int solarPanelZoneOrd = 0;
            Zone.GetAdjacentZone(batteryRoomZoneAbs, batteryRoomZoneOrd, absoluteRotation, out solarPanelZoneAbs, out solarPanelZoneOrd);
            if ((Zone.ZoneIsInArea(solarPanelZoneAbs, solarPanelZoneOrd, horizontalZonesNumber, verticalZonesNumber) == false)
                || (zoneMap[solarPanelZoneOrd, solarPanelZoneAbs].zoneType != ZoneType.NotYetGenerated))
            {
                // Get the opposite side.
                relativeRotation = new Rot4((relativeRotation.AsInt + 2) % 4);
                absoluteRotation = new Rot4((absoluteRotation.AsInt + 2) % 4);
                Zone.GetAdjacentZone(batteryRoomZoneAbs, batteryRoomZoneOrd, absoluteRotation, out solarPanelZoneAbs, out solarPanelZoneOrd);
                if ((Zone.ZoneIsInArea(solarPanelZoneAbs, solarPanelZoneOrd, horizontalZonesNumber, verticalZonesNumber) == false)
                    || (zoneMap[solarPanelZoneOrd, solarPanelZoneAbs].zoneType != ZoneType.NotYetGenerated))
                {
                    Log.Warning("MiningCo. OutpostGenerator: failed to find a free zone for the solar panels.");
                    return;
                }
            }
            zoneMap[batteryRoomZoneOrd, batteryRoomZoneAbs] = new ZoneProperties(ZoneType.SmallRoomBatteryRoom, batteryRoomCardinal, relativeRotation);
            relativeRotation.Rotate(RotationDirection.Clockwise); // Rotate 2 times to get the opposite direction.
            relativeRotation.Rotate(RotationDirection.Clockwise);
            zoneMap[solarPanelZoneOrd, solarPanelZoneAbs] = new ZoneProperties(ZoneType.SolarPanelZone, batteryRoomCardinal, relativeRotation);
        }

        static void GenerateOutpostLayoutCommandRoom()
        {
            Rot4 commandRoomCardinal;
            bool freeZoneIsFound = Zone.GetRandomFreeCardinalZoneAdjacentTo(mainRoomZoneAbs, mainRoomZoneOrd, out commandRoomCardinal, zoneMap, horizontalZonesNumber, verticalZonesNumber);
            if (freeZoneIsFound == false)
            {
                Log.Warning("MiningCo. OutpostGenerator: failed to find a free zone for the command room.");
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
                    Log.Warning("MiningCo. OutpostGenerator: failed to find a free zone for the drop zone.");
                    return;
                }
            }
            zoneMap[commandRoomZoneOrd, commandRoomZoneAbs] = new ZoneProperties(ZoneType.SmallRoomCommandRoom, commandRoomCardinal, relativeRotation);
            relativeRotation.Rotate(RotationDirection.Clockwise); // Rotate 2 times to get the opposite direction.
            relativeRotation.Rotate(RotationDirection.Clockwise);
            zoneMap[dropZoneOrd, dropZoneAbs] = new ZoneProperties(ZoneType.DropZone, commandRoomCardinal, relativeRotation);
        }

        static void GenerateOutpostLayoutSecondaryRooms()
        {
            for (int rotationAsInt = 0; rotationAsInt < 4; rotationAsInt++)
            {
                int zoneAbs = 0;
                int zoneOrd = 0;
                Rot4 rotation = new Rot4(rotationAsInt);
                Zone.GetAdjacentZone(mainRoomZoneAbs, mainRoomZoneOrd, rotation, out zoneAbs, out zoneOrd);
                if (zoneMap[zoneOrd, zoneAbs].zoneType == ZoneType.NotYetGenerated)
                {
                    ZoneType secondaryRoomType = OG_Common.GetRandomZoneTypeSmallRoom(outpostData);
                    zoneMap[zoneOrd, zoneAbs] = new ZoneProperties(secondaryRoomType, rotation, Rot4.North);
                }
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
                        // Standard big rooms.
                        case ZoneType.BigRoomLivingRoom:
                            OG_ZoneBigRoom.GenerateBigRoomLivingRoom(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                        case ZoneType.BigRoomWarehouse:
                            OG_ZoneBigRoom.GenerateBigRoomWarehouse(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
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
                            // Nothing to do;
                            break;
                        case ZoneType.SecondaryEntrance:
                            OG_ZoneOther.GenerateSecondaryEntranceZone(areaSouthWestOrigin, zoneAbs, zoneOrd, zone.rotation, ref outpostData);
                            break;
                    }
                }
            }
        }
    }
}
