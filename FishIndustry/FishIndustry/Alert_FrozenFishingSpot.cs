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
    /// Alert_SnowyFishingSpot class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Alert_FrozenFishingSpot : Alert_High
    {
        public override AlertReport Report
        {
            get
            {
                foreach (Building_FishingPier fishingPier in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_FishingPier>())
                {
                    if ((fishingPier.TryGetComp<CompForbiddable>().Forbidden == false)
                        && (Find.SnowGrid.GetDepth(fishingPier.fishingSpotCell) > 0.25f))
                    {
                        return AlertReport.CulpritIs(fishingPier);
                    }
                }
                return AlertReport.Inactive;
            }
        }
        public Alert_FrozenFishingSpot()
        {
            this.baseLabel = "Frozen fishing spot";
            this.baseExplanation = "One of your fishing spot is frozen.\n\nYour fishers' efficiency will be greatly reduced.\n\nMake a hole in the ice (clear snow) so they can work.";
        }
    }
}
