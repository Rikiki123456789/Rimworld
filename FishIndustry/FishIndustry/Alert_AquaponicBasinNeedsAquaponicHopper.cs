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
    /// Alert_AquacultureBasinNeedsAquacultureHopper class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Alert_AquacultureBasinNeedsAquacultureHopper : Alert
    {
        public override AlertReport GetReport()
        {
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                foreach (Building_AquacultureBasin aquacultureBasin in maps[i].listerBuildings.AllBuildingsColonistOfClass<Building_AquacultureBasin>())
                {
                    bool aquacultureHopperIsFound = false;
                    foreach (IntVec3 adjacentCell in GenAdj.CellsAdjacentCardinal(aquacultureBasin))
                    {
                        Thing potentialAquacultureHopper = adjacentCell.GetEdifice(maps[i]);
                        if ((potentialAquacultureHopper != null)
                            && (potentialAquacultureHopper.def == Util_FishIndustry.AquacultureHopperDef))
                        {
                            aquacultureHopperIsFound = true;
                            break;
                        }
                    }
                    if (aquacultureHopperIsFound == false)
                    {
                        return AlertReport.CulpritIs(aquacultureBasin);
                    }
                }
            }
            return AlertReport.Inactive;
        }
        public Alert_AquacultureBasinNeedsAquacultureHopper()
        {
            this.defaultLabel = "Need aquaculture hopper";
            this.defaultExplanation = "You have an aquaculture basin with no aquaculture hopper next to it.\n\nTo work, aquaculture basin must draw from an adjacent aquaculture hopper filled with raw vegetables.\n\nBuild an aquaculture hopper adjacent to the aquaculture basin.";
        }
    }
}
