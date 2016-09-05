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
    /// IncidentWorker_MicroFungus class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class IncidentWorker_MicroFungus : IncidentWorker
    {
        protected override bool CanFireNowSub()
        {
            bool colonyHasAquacultureBasin = Find.ListerBuildings.AllBuildingsColonistOfDef(Util_FishIndustry.AquacultureBasinDef).Count() > 0;
            return colonyHasAquacultureBasin;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Building_AquacultureBasin aquacultureBasin = null;
            foreach (Building building in Find.ListerBuildings.AllBuildingsColonistOfDef(Util_FishIndustry.AquacultureBasinDef))
            {
                aquacultureBasin = building as Building_AquacultureBasin;
                aquacultureBasin.StartMicroFungusInfestation((int)(60000f * Rand.Range(this.def.durationDays.min, this.def.durationDays.max)));
            }
            if (aquacultureBasin != null)
            {
                Find.LetterStack.ReceiveLetter("Micro fungus", "Some of your aquaculture basins have been infected by a strange aquatic fungus.\nIt seems to consume all the water's oxygen.\nLet's hope the fishes will survive this deprivation.", LetterType.BadNonUrgent, aquacultureBasin);
            }

            return true;
        }
    }
}
