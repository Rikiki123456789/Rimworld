using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// Item_Oyster class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class Item_Oyster : ThingWithComps
    {
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // Small chance to spawn a pearl.
            if (Rand.Value < 0.02f)
            {
                GenSpawn.Spawn(Util_FishIndustry.PearlDef, this.Position, this.MapHeld);
            }
            base.Destroy(mode);
        }

    }
}
