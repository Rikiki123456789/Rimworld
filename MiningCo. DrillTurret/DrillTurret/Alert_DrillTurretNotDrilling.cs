using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace DrillTurret
{
    /// <summary>
    /// Alert_DrillTurretNotDrilling class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Alert_DrillTurretNotDrilling : Alert
    {
        public override AlertReport GetReport()
        {
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                foreach (Building building in maps[i].listerBuildings.AllBuildingsColonistOfDef(Util_DrillTurret.drillTurretDef))
                {
                    if (building is Building_DrillTurret)
                    {
                        Building_DrillTurret drillTurret = building as Building_DrillTurret;
                        if (drillTurret.targetPosition.IsValid == false)
                        {
                            return AlertReport.CulpritIs(drillTurret);
                        }
                    }
                }
            }
            return AlertReport.Inactive;
        }

        public Alert_DrillTurretNotDrilling()
        {
            this.defaultLabel = "Idle drill turret";
            this.defaultExplanation = "You have an idle drill turret. You should maybe move it or designate more rocks to drill.";
            this.defaultPriority = AlertPriority.Medium;
        }
    }
}
