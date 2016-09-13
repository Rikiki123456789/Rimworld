using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// RectTriggerUnfogArea class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class RectTriggerUnfogArea : RectTrigger
    {
        public override void Tick()
        {
            if (this.IsHashIntervalTick(60))
            {
                if (this.Rect.CenterCell.Fogged() == false)
                {
                    this.UnfogArea();
                }
                foreach (IntVec3 cell in Rect.Cells)
                {
                    List<Thing> thingList = cell.GetThingList();
                    for (int thingIndex = 0; thingIndex < thingList.Count; thingIndex++)
                    {
                        if (thingList[thingIndex].def.category == ThingCategory.Pawn && thingList[thingIndex].Faction == Faction.OfPlayer)
                        {
                            this.UnfogArea();
                        }
                    }
                }
            }
        }

        protected void UnfogArea()
        {
            foreach (IntVec3 cell in Rect.Cells)
            {
                Find.FogGrid.Unfog(cell);
            }
            if (base.Destroyed == false)
            {
                this.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
