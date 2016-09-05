using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;      // Needed when you do something with the AI
//using RimWorld.SquadAI;
//using Verse.Sound; // Needed when you do something with the Sound

namespace LaserFence
{
    /// <summary>
    /// Building_LaserFence class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_LaserFence : Building
    {
        public Building_LaserFencePylon pylon = null;

        // ######## Tick ######## //

        public override void Tick()
        {
            // Check if a new building is cutting the laser fence.
            if ((Find.TickManager.TicksGame % 30) == 0)
            {
                if (this.Position.GetEdifice() != null)
                {
                    if (pylon != null)
                    {
                        pylon.InformEdificeIsBlocking();
                    }
                }
            }
            // Check if a plant or pawn is in the laser fence path.
            if ((Find.TickManager.TicksGame % 200) == 0)
            {
                List<Thing> thingList = this.Position.GetThingList();
                for (int thingIndex = thingList.Count - 1; thingIndex >= 0; thingIndex--)
                {
                    Thing thing = thingList[thingIndex];
                    if (thing is Plant)
                    {
                        FireUtility.TryStartFireIn(this.Position, 0.1f);
                        break;
                    }
                    if (thing is Pawn)
                    {
                        FireUtility.TryAttachFire(thing, 0.1f);
                        break;
                    }
                }
            }
        }

        // ######## ExposeData ######## //

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference<Building_LaserFencePylon>(ref pylon, "pylon");
        }

        // ######## Properties. ######## //
        
        public override bool ClaimableBy(Faction faction)
        {
            return false;
        }
    }
}
