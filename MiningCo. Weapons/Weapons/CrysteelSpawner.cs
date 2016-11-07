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

namespace Weapons
{
    /// <summary>
    /// CrysteelSpawner class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class CrysteelSpawner : ThingWithComps
    {
        /// <summary>
        /// Randomly spawns a crysteel.
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            if (Rand.Value < 0.2f)
            {
                GenSpawn.Spawn(ThingDef.Named("Crysteel"), this.Position);
            }
            this.Destroy();
        }
    }
}
