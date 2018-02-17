using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// Alert_AquacultureBasinNeedsHopper class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Alert_AquacultureBasinNeedsHopper : Alert
    {
        public override AlertReport GetReport()
        {
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                foreach (Building aquacultureBasin in maps[i].listerBuildings.AllBuildingsColonistOfDef(Util_FishIndustry.AquacultureBasinDef))
                {
                    bool hopperIsFound = false;
                    foreach (IntVec3 adjacentCell in GenAdj.CellsAdjacentCardinal(aquacultureBasin))
                    {
                        Thing edifice = adjacentCell.GetEdifice(maps[i]);
                        if ((edifice != null)
                            && (edifice.def == ThingDefOf.Hopper))
                        {
                            hopperIsFound = true;
                            break;
                        }
                    }
                    if (hopperIsFound == false)
                    {
                        return AlertReport.CulpritIs(aquacultureBasin);
                    }
                }
            }
            return AlertReport.Inactive;
        }
        public Alert_AquacultureBasinNeedsHopper()
        {
            this.defaultLabel = "FishIndustry.NeedHopperLabel".Translate();
            this.defaultExplanation = "FishIndustry.NeedHopperExplanation".Translate();
        }
    }
}
