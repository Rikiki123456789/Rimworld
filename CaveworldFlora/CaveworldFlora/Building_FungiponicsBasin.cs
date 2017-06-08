using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace CaveworldFlora
{
    /// <summary>
    /// Building_FungiponicsBasin class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_FungiponicsBasin : Building_PlantGrower
    {
        public override string GetInspectString()
        {
            float temperature = GenTemperature.GetTemperatureForCell(this.Position, this.Map);
            ThingDef_ClusterPlant clusterPlantDef = this.GetPlantDefToGrow() as ThingDef_ClusterPlant;
            if (temperature < clusterPlantDef.minGrowTemperature)
            {
                return "Cannot grow now: too cold.";
            }
            else if (temperature > clusterPlantDef.maxGrowTemperature)
            {
                return "Cannot grow now: too hot.";
            }
            else
            {
                return "Growing.";
            }
        }
    }
}
