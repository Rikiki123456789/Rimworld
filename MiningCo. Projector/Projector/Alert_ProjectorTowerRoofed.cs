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

namespace Projector
{
    /// <summary>
    /// Alert_ProjectorTowerRoofed class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class Alert_ProjectorTowerRoofed : Alert
    {
        public Alert_ProjectorTowerRoofed()
        {
            this.defaultLabel = "Projector tower roofed";
            this.defaultExplanation = "One of your projector towers is roofed and has been deactivated. Remove the roof above it to reactivate it.";
            this.defaultPriority = AlertPriority.Medium;
        }

        public override AlertReport GetReport()
        {
            List<Map> maps = Find.Maps;
            for (int mapIndex = 0; mapIndex < maps.Count; mapIndex++)
            {
                foreach (Building building in maps[mapIndex].listerBuildings.AllBuildingsColonistOfDef(Util_Projector.ProjectorTowerDef))
                {
                    Building_MobileProjectorTower tower = building as Building_MobileProjectorTower;
                    if (tower.isRoofed)
                    {
                        return AlertReport.CulpritIs(tower);
                    }
                }
            }
            return AlertReport.Inactive;
        }
    }
}