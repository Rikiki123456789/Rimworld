using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace CaveworldFlora
{
    /// <summary>
    /// GleamcapSporeSpawner class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class GleamcapSporeSpawner : Building
    {
        public ClusterPlant_Gleamcap parent = null;

        public const int sporeEffectRadius = 5;
        public const int minSporeSpawningDurationInTicks = 20 * GenTicks.TicksPerRealSecond;
        public const int maxSporeSpawningDurationInTicks = 60 * GenTicks.TicksPerRealSecond;
        public int sporeSpawnEndTick = 0;
        public int nextSporeThrowTick = 0;
        public int nextNearbyPawnCheckTick = 0;

        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.sporeSpawnEndTick = Find.TickManager.TicksGame + Rand.RangeInclusive(minSporeSpawningDurationInTicks, maxSporeSpawningDurationInTicks);
        }

        // ===================== Saving =====================
        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.sporeSpawnEndTick, "sporeSpawnEndTick");
            Scribe_References.LookReference<ClusterPlant_Gleamcap>(ref this.parent, "parentGleamcap");
        }
        
        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - throw some spore.
        /// - try to apply a mood effect on nearby colonists.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame > this.nextSporeThrowTick)
            {
                this.nextSporeThrowTick = Find.TickManager.TicksGame + 10;
                MoteMaker.ThrowDustPuff(this.TrueCenter(), Rand.Value);
            }

            if (Find.TickManager.TicksGame > this.nextNearbyPawnCheckTick)
            {
                this.nextNearbyPawnCheckTick = Find.TickManager.TicksGame + GenTicks.TicksPerRealSecond;
                foreach (Pawn pawn in Find.MapPawns.AllPawns)
                {
                    if ((pawn.Position.InHorDistOf(this.Position, sporeEffectRadius))
                        && (pawn.health != null))
                    {
                        pawn.health.AddHediff(Util_CaveworldFlora.gleamcapSmokeDef);
                    }
                }
            }
            if (Find.TickManager.TicksGame > sporeSpawnEndTick)
            {
                // Inform the gleamcap that the spore spawner is destroyed.
                this.parent.sporeSpawner = null;
                this.Destroy();
            }
        }
    }
}
