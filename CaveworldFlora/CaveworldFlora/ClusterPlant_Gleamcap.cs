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
    /// ClusterPlant_Gleamcap class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class ClusterPlant_Gleamcap : ClusterPlant
    {
        public const float chanceToSpawnSpore = 0.01f;
        public const int minDelayBetweenSporeSpawnInTicks = GenDate.TicksPerDay / 2;
        public int lastSporeSpawnTick = 0;
        public GleamcapSporeSpawner sporeSpawner = null;

        // ===================== Saving =====================
        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.lastSporeSpawnTick, "lastSporeSpawnTick");
            Scribe_References.LookReference<GleamcapSporeSpawner>(ref this.sporeSpawner, "sporeSpawner");
        }

        // ===================== Destroy =====================
        /// <summary>
        /// Destroy the plant and the associated glower if existing.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.sporeSpawner.DestroyedOrNull() == false)
            {
                this.sporeSpawner.Destroy();
            }
            base.Destroy(mode);
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - perform the cluster plant normal treatment.
        /// - when mature, has a small chance to spawn a spore spawner.
        /// </summary>
        public override void TickLong()
        {
            base.TickLong();

            TrySpawnSporeSpawner();
        }
        
        /// <summary>
        /// Try to spawn some spores if the plant is mature.
        /// </summary>
        public void TrySpawnSporeSpawner()
        {
            bool sporeSpawnOccuredLongAgo = (this.lastSporeSpawnTick == 0)
                || ((Find.TickManager.TicksGame - this.lastSporeSpawnTick) > minDelayBetweenSporeSpawnInTicks);

            if ((this.LifeStage == PlantLifeStage.Mature)
                && (this.Dying == false)
                && (this.isInCryostasis == false)
                && sporeSpawnOccuredLongAgo
                && ((Rand.Value < chanceToSpawnSpore)
                || this.Map.mapConditionManager.ConditionIsActive(MapConditionDefOf.Eclipse)))
            {
                this.lastSporeSpawnTick = Find.TickManager.TicksGame;
                this.sporeSpawner = ThingMaker.MakeThing(Util_CaveworldFlora.gleamcapSporeSpawnerDef) as GleamcapSporeSpawner;
                GenSpawn.Spawn(this.sporeSpawner, this.Position, this.Map);
                this.sporeSpawner.parent = this;
            }
        }
    }
}
