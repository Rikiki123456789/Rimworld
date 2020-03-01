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
    /// Alert_AquacultureBasinCriticalIssue class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class Alert_AquacultureBasinCriticalIssue : Alert
    {
        public override AlertReport GetReport()
        {
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                foreach (Building_AquacultureBasin aquacultureBasin in maps[i].listerBuildings.AllBuildingsColonistOfClass<Building_AquacultureBasin>())
                {
                    if ((aquacultureBasin.IsForbidden(Faction.OfPlayer) == false)
                        && (aquacultureBasin.speciesDef != null)
                        && ((aquacultureBasin.waterQuality < Building_AquacultureBasin.minWaterQuality)
                            || (aquacultureBasin.fishesAreFed == false)))
                    {
                        return AlertReport.CulpritIs(aquacultureBasin);
                    }
                }
            }
            return AlertReport.Inactive;
        }

        public Alert_AquacultureBasinCriticalIssue()
        {
            this.defaultLabel = "FishIndustry.CriticalIssueLabel".Translate();
            this.defaultExplanation = "FishIndustry.CriticalIssueExplanation".Translate();
        }
    }
}
