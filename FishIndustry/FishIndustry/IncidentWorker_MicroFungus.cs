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

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            bool colonyHasAquacultureBasin = map.listerBuildings.AllBuildingsColonistOfDef(Util_FishIndustry.AquacultureBasinDef).Count() > 0;
            return colonyHasAquacultureBasin;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Building_AquacultureBasin aquacultureBasin = null;
            foreach (Building building in map.listerBuildings.AllBuildingsColonistOfDef(Util_FishIndustry.AquacultureBasinDef))
            {
                aquacultureBasin = building as Building_AquacultureBasin;
                int infestationDuration = (int)(GenDate.TicksPerDay * this.def.durationDays.min * Rand.Range(0.8f, 1.2f));
                Room room = aquacultureBasin.InteractionCell.GetRoom(map);
                if ((room == null)
                    || room.PsychologicallyOutdoors)
                {
                    // Maximum infestation duration.
                    infestationDuration = (int)(GenDate.TicksPerDay * this.def.durationDays.max * Rand.Range(0.8f, 1.2f));
                }
                else
                {
                    // Adjust infestation duration according to cleanliness.
                    float dirtyness = -room.GetStat(RoomStatDefOf.Cleanliness);
                    if (dirtyness > 0)
                    {
                        infestationDuration += (int)(GenDate.TicksPerDay * this.def.durationDays.min * dirtyness);
                    }
                }
                aquacultureBasin.StartMicroFungusInfestation(infestationDuration);
            }
            if (aquacultureBasin != null)
            {
                Find.LetterStack.ReceiveLetter("FishIndustry.LetterLabelMicroFungus".Translate(), "FishIndustry.MicroFungus".Translate(), LetterDefOf.BadNonUrgent, aquacultureBasin);
            }
            return true;
        }
    }
}
