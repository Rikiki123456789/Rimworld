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
    /// Building_AquacultureHopper class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class Building_AquacultureHopper : Building_Storage
    {
        // Power comp.
        public CompPowerTrader powerComp;

        // ===================== Setup Work =====================

        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Periodically check if some food are stored and refrigerate it (reset the rotting progress).
        /// </summary>
        public override void TickRare()
        {
            base.TickRare();

            if (this.powerComp.PowerOn)
            {
                List<Thing> thingList = this.Position.GetThingList(this.Map);
                foreach (Thing thing in thingList)
                {
                    if (thing.def.IsNutritionGivingIngestible)
                    {
                        CompRottable rottableComp = thing.TryGetComp<CompRottable>();
                        if (rottableComp != null)
                        {
                            rottableComp.RotProgress = 0;
                        }
                    }
                }
            }
        }
    }
}
