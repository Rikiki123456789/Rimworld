using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;      // Needed when you do something with the AI
//using RimWorld.SquadAI;
//using Verse.Sound; // Needed when you do something with the Sound

namespace LaserFence
{
    /// <summary>
    /// Building_LaserFencePylon class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class Building_LaserFenceConsole : Building
    {
        public bool manualSwitchIsPending = false;

        // ===================== Setup work =====================
        /// <summary>
        /// Used to initialize the console.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            CheckPylonConfigurationChangePending(map);
        }

        // ===================== Main function =====================
        public void CheckPylonConfigurationChangePending(Map map)
        {
            this.manualSwitchIsPending = false;
            foreach (Building_LaserFencePylon pylon in map.listerBuildings.AllBuildingsColonistOfClass<Building_LaserFencePylon>())
            {
                if (pylon.manualSwitchIsPending)
                {
                    this.manualSwitchIsPending = true;
                    break;
                }
            }
        }

        // ===================== Exported functions =====================
        /// <summary>
        /// Used by a pylon to notify that laser fence configuration has been changed.
        /// </summary>
        public void Notify_ConfigurationChanged(Map map)
        {
            CheckPylonConfigurationChangePending(map);
        }

        /// <summary>
        /// Called when a pawn is operating the console.
        /// </summary>
        public void Notify_ApplyCachedConfiguration()
        {
            this.manualSwitchIsPending = false;
            foreach (Building_LaserFencePylon pylon in this.Map.listerBuildings.AllBuildingsColonistOfClass<Building_LaserFencePylon>())
            {
                pylon.Notify_ApplyCachedConfiguration();
            }
        }
    }
}
