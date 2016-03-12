using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using RimWorld.SquadAI;
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Building_NoRoofAreaGenerator class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_NoRoofAreaGenerator : Building
    {
        public IntVec3 areaSouthWestOrigin;

        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame == 1)
            {
                for (int xOffset = 0; xOffset < OG_BigOutpost.areaSideLength; xOffset++)
                {
                    for (int zOffset = 0; zOffset < OG_BigOutpost.areaSideLength; zOffset++)
                    {
                        IntVec3 cell = areaSouthWestOrigin + new IntVec3(xOffset, 0, zOffset);
                        if (Find.RoofGrid.Roofed(cell) == false)
                        {
                            Find.AreaNoRoof.Set(cell);
                        }
                    }
                }
                this.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
