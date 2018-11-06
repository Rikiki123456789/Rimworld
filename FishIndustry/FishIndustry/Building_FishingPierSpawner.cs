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
    /// Building_FishingPierSpawner class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    class Building_FishingPierSpawner : Building_WorkTable
    {
        /// <summary>
        /// Spawns the fishing pier.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.Destroy();
            Building_FishingPier fishingPier = ThingMaker.MakeThing(Util_FishIndustry.FishingPierDef) as Building_FishingPier;
            IntVec3 fishingPierPosition = this.Position + new IntVec3(0, 0, 1).RotatedBy(this.Rotation);
            GenSpawn.Spawn(fishingPier, fishingPierPosition, map, this.Rotation);
            fishingPier.SetFactionDirect(this.Faction);
        }
    }
}
