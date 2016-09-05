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
    /// CavePlant_Gleamcap class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class GleamcapSporeSpawner : Building
    {
        public CavePlant_Gleamcap parent = null;

        public const int sporeEffectRadius = 5;
        public const int minSporeSpawningDurationInTicks = 1200;
        public const int maxSporeSpawningDurationInTicks = 3600;
        public int lifeCounterInTicks = 0;
        public int nextNearbyPawnCheckTick = 0;

        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            this.lifeCounterInTicks = Rand.RangeInclusive(minSporeSpawningDurationInTicks, maxSporeSpawningDurationInTicks);
        }

        // ===================== Saving =====================
        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.lifeCounterInTicks, "lifeCounterInTicks");
            Scribe_References.LookReference<CavePlant_Gleamcap>(ref this.parent, "parentGleamcap");
        }
        
        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - display some spore.
        /// - try to attach a mood effect on nearby colonists.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            this.lifeCounterInTicks--;
            if (this.lifeCounterInTicks > 0)
            {
                MoteMaker.ThrowDustPuff(this.TrueCenter(), Rand.Value);

                if (Find.TickManager.TicksGame > this.nextNearbyPawnCheckTick)
                {
                    this.nextNearbyPawnCheckTick = Find.TickManager.TicksGame + GenTicks.TicksPerRealSecond;
                    foreach (Pawn pawn in Find.MapPawns.AllPawns)
                    {
                        if ((pawn.Position.InHorDistOf(this.Position, sporeEffectRadius))
                            && (pawn.needs.mood != null))
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(Util_CavePlant.breathedGleamcapSmokeDef);
                        }
                    }
                }
            }
            else
            {
                // Inform the gleamcap that the spore spawner is destroyed.
                this.parent.sporeSpawnerBuilding = null;
                this.Destroy();
            }
        }
    }
}
