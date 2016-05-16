using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// WorkGiver_GrowerSowOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_GrowerHarvestOutpost : WorkGiver_GrowerHarvest
    {
        // This workgiver is specific to M&Co. employees.
        public override bool ShouldSkip(Pawn pawn)
        {
            if ((pawn.Faction != null)
                && (pawn.Faction == OG_Util.FactionOfMAndCo))
            {
                return base.ShouldSkip(pawn);
            }
            return true;
        }

        // Note: this function must be overriden as the one in WorkGiver_Grower class only takes into account colony's hydroponics basins.
        // Growing zone are not managed though!
        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            List<IntVec3> workCells = new List<IntVec3>();

            List<Thing> hydroponicsList = Find.ListerThings.ThingsOfDef(ThingDef.Named("HydroponicsBasin"));
            for (int plantGrowerIndex = 0; plantGrowerIndex < hydroponicsList.Count; plantGrowerIndex++)
            {
                Thing potentialPlantGrower = hydroponicsList[plantGrowerIndex];
                if ((potentialPlantGrower.Faction != null)
                    && (potentialPlantGrower.Faction == OG_Util.FactionOfMAndCo))
                {
                    Building_PlantGrower plantGrower = potentialPlantGrower as Building_PlantGrower;
                    if (plantGrower == null)
                    {
                        Log.Warning("WorkGiver_GrowerOutpost: found a thing of def HydroponicsBasin or PlantPot which is not a Building_PlantGrower.");
                        continue;
                    }
                    if (GenPlant.GrowthSeasonNow(plantGrower.Position) == false)
                    {
                        continue;
                    }
                    if (this.ExtraRequirements(plantGrower) == false)
                    {
                        continue;
                    }
                    if (plantGrower.IsForbidden(pawn))
                    {
                        continue;
                    }
                    if (pawn.CanReach(plantGrower, PathEndMode.OnCell, pawn.NormalMaxDanger(), false) == false)
                    {
                        continue;
                    }
                    if (plantGrower.IsBurning())
                    {
                        continue;
                    }
                    base.DetermineWantedPlantDef(plantGrower.Position);
                    if (WorkGiver_Grower.wantedPlantDef == null)
                    {
                        continue;
                    }
                    foreach (IntVec3 cell in plantGrower.OccupiedRect().Cells)
                    {
                        workCells.Add(cell);
                    }
                }
            }
            return workCells;
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 c)
        {
            WorkGiver_Grower.wantedPlantDef = null;
            return base.JobOnCell(pawn, c);
        }
    }
}
