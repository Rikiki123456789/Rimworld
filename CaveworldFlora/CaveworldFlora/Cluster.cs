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
    /// Cluster class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Cluster : Thing
    {
        // Plant def.
        public ThingDef_ClusterPlant plantDef = null;

        // Size.
        public int actualSize = 0;
        public int desiredSize = 0;

        // Symbiosis cluster.
        public Cluster symbiosisCluster = null;

        // New cluster initialization.
        public static ClusterPlant SpawnNewClusterAt(Map map, IntVec3 spawnCell, ThingDef_ClusterPlant plantDef, int desiredSize)
        {
            ClusterPlant newPlant = ThingMaker.MakeThing(plantDef) as ClusterPlant;
            GenSpawn.Spawn(newPlant, spawnCell, map);
            Cluster newCluster = ThingMaker.MakeThing(Util_CaveworldFlora.ClusterDef) as Cluster;
            newCluster.Initialize(plantDef, desiredSize);
            GenSpawn.Spawn(newCluster, spawnCell, map);
            newPlant.cluster = newCluster;
            return newPlant;
        }
        public void Initialize(ThingDef_ClusterPlant plantDef, int desiredSize)
        {
            this.plantDef = plantDef;
            this.actualSize = 1;
            this.desiredSize = desiredSize;
        }

        // Exclusivity radius.
        public float exclusivityRadius
        {
            get
            {
                return (this.plantDef.clusterExclusivityRadiusOffset + ((float)this.desiredSize) * this.plantDef.clusterExclusivityRadiusFactor);
            }
        }
        public static float GetExclusivityRadius(ThingDef_ClusterPlant plantDef, int clusterSize)
        {
            return (plantDef.clusterExclusivityRadiusOffset + (float)clusterSize * plantDef.clusterExclusivityRadiusFactor);
        }
        public static float GetMaxExclusivityRadius(ThingDef_ClusterPlant plantDef)
        {
            return (plantDef.clusterExclusivityRadiusOffset + ((float)plantDef.clusterSizeRange.max) * plantDef.clusterExclusivityRadiusFactor);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (this.symbiosisCluster.DestroyedOrNull() == false)
            {
                this.symbiosisCluster.NotifySymbiosisClusterRemoved(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            string plantDefAsString = "";
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                plantDefAsString = this.plantDef.defName;
                Scribe_Values.LookValue<string>(ref plantDefAsString, "plantDefAsString");
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.LookValue<string>(ref plantDefAsString, "plantDefAsString");
                this.plantDef = ThingDef.Named(plantDefAsString) as ThingDef_ClusterPlant;
            }
            Scribe_Values.LookValue<int>(ref this.actualSize, "actualSize");
            Scribe_Values.LookValue<int>(ref this.desiredSize, "desiredSize");

            Scribe_References.LookReference<Cluster>(ref this.symbiosisCluster, "symbiosisCluster");
        }
        
        public void NotifyPlantAdded()
        {
            this.actualSize++;
        }

        public void NotifyPlantRemoved()
        {
            this.actualSize--;
            if (this.actualSize <= 0)
            {
                this.Destroy();
                return;
            }
            this.UpdateClusterPosition();
        }

        // Only move cluster root to another plant if current position becomes invalid (for example: a building is built over it).
        protected void UpdateClusterPosition()
        {
            Room clusterRoom = this.GetRoom();
            IntVec3 center = IntVec3.Zero;
            // We only check with clusterSpawnRadius (+ a small offset). No need to check entire cluster exclusivity area.
            List<IntVec3> plantsInClusterList = new List<IntVec3>();
            IEnumerable<IntVec3> cellsInCluster = GenRadial.RadialCellsAround(this.Position, this.plantDef.clusterSpawnRadius + 3f, true);
            foreach (IntVec3 cell in cellsInCluster)
            {
                if ((this.Map.thingGrid.ThingAt(cell, this.plantDef) != null)
                    && (cell.GetRoom(this.Map) == clusterRoom))
                {
                    plantsInClusterList.Add(cell);
                }
            }
            if (plantsInClusterList.Count > 0)
            {
                this.Position = plantsInClusterList.RandomElement();
            }
            else
            {
                this.Destroy();
            }
        }

        public void NotifySymbiosisClusterAdded(Cluster symbiosisCluster)
        {
            this.symbiosisCluster = symbiosisCluster;
            symbiosisCluster.symbiosisCluster = this;
        }

        public void NotifySymbiosisClusterRemoved(Cluster symbiosisCluster)
        {
            this.symbiosisCluster = null;
            symbiosisCluster.symbiosisCluster = null;
        }
    }
}
