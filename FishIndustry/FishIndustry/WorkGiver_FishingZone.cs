using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// WorkGiver_FishingZone class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class WorkGiver_FishingZone : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.OnCell;
            }
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            List<IntVec3> jobCells = new List<IntVec3>();

            foreach (Zone zone in pawn.Map.zoneManager.AllZones)
            {
                Zone_Fishing fishingZone = zone as Zone_Fishing;
                if ((fishingZone != null)
                    && fishingZone.allowFishing)
                {
                    foreach (IntVec3 cell in fishingZone.fishingSpots)
                    {
                        if ((cell.IsForbidden(pawn) == false)
                            && pawn.CanReserveAndReach(cell, this.PathEndMode, Danger.Some))
                        {
                            jobCells.Add(cell);
                        }
                    }
                }
            }
            return jobCells;
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
        {
            return JobMaker.MakeJob(Util_FishIndustry.FishAtFishingZoneJobDef, cell);
        }
    }
}
