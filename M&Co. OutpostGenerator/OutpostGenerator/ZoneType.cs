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
    /// ZoneType class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>

    public enum ZoneType
    {
        NotYetGenerated,  // Zone not yet generated.
        
        // Big rooms.
            // Standard: used in random selector.
        BigRoomLivingRoom,
        BigRoomWarehouse,
        BigRoomPrison,
            // Special: used during big outpost generation.
        BigRoomRefectory,
        BigRoomBatteryRoom,
        BigRoomCommandRoom,
        BigRoomBarracks,
        BigRoomHydroponics,

        // Medium rooms.
        // Standard: used in random selector.
        MediumRoomMedibay,
        MediumRoomPrison,
        MediumRoomKitchen,
        MediumRoomWarehouse,
        MediumRoomWeaponRoom,
        MediumRoomKenel,
        MediumRoomLaboratory,
        MediumRoomRecRoom,


        // Small rooms.
        // Standard: used in random selector.
        SmallRoomBarracks,
        SmallRoomMedibay,
        SmallRoomWeaponRoom,
            // Special: used during small outpost generation.
        SmallRoomBatteryRoom,
        SmallRoomCommandRoom,

        // Special zones: used during small/big outpost generation.
        SolarPanelZone,
        DropZone,
        LandingPadTop,
        LandingPadBottom,
        RadarDome,

        // Exterior zones: used in random selector.
        WaterPool,
        Farm,
        ExteriorRecRoom,
        ShootingRange,
        Cemetery,
        MortarBay,

        // Other zones.
        Empty, // Zone is empty.
        MainEntrance,
        SecondaryEntrance,
        EntranchedZone,
        SamSite,
        BigSas,
        Plaza,
        StraightAlley
    }

}
