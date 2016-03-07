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
        // Note: this function must be overriden as the one in WorkGiver_Grower class only takes into account colony's hydroponics basins.
        // Growing zone are not managed though!
        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            if ((pawn.Faction != null)
                && (pawn.Faction == OG_Util.FactionOfMAndCo))
            {
                List<IntVec3> workCells = new List<IntVec3>();

                List<Thing> hydroponicsList = Find.ListerThings.ThingsOfDef(ThingDef.Named("HydroponicsBasin"));
                for (int hydroponicsIndex = 0; hydroponicsIndex < hydroponicsList.Count; hydroponicsIndex++)
                {
                    Thing potentialHydroponics = hydroponicsList[hydroponicsIndex];
                    if ((potentialHydroponics.Faction != null)
                        && (pawn.Faction == OG_Util.FactionOfMAndCo))
                    {
                        Building_PlantGrower hydroponics = potentialHydroponics as Building_PlantGrower;
                        if (hydroponics == null)
                        {
                            Log.Warning("WorkGiver_GrowerOutpost: found a thing of def HydroponicsBasin which is not a Building_PlantGrower.");
                            continue;
                        }
                        if (GenPlant.GrowthSeasonNow(hydroponics.Position) == false)
                        {
                            continue;
                        }
                        if (this.ExtraRequirements(hydroponics) == false)
                        {
                            continue;
                        }
                        if (hydroponics.IsForbidden(pawn))
                        {
                            continue;
                        }
                        if (pawn.CanReach(hydroponics, PathEndMode.OnCell, pawn.NormalMaxDanger(), false) == false)
                        {
                            continue;
                        }
                        if (hydroponics.IsBurning())
                        {
                            continue;
                        }
                        base.DetermineWantedPlantDef(hydroponics.Position);
                        if (WorkGiver_Grower.wantedPlantDef == null)
                        {
                            continue;
                        }
                        foreach (IntVec3 cell in hydroponics.OccupiedRect().Cells)
                        {
                            workCells.Add(cell);
                        }
                    }
                }
                return workCells;
            }
            else
            {
                return new List<IntVec3>();
            }
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 c)
        {
            WorkGiver_Grower.wantedPlantDef = null;
            return base.JobOnCell(pawn, c);
        }
    }
}
