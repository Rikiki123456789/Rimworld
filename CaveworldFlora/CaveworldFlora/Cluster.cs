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
        public static ClusterPlant SpawnNewClusterAt(IntVec3 spawnCell, ThingDef_ClusterPlant plantDef, int desiredSize)
        {
            ClusterPlant newPlant = ThingMaker.MakeThing(plantDef) as ClusterPlant;
            GenSpawn.Spawn(newPlant, spawnCell);
            Cluster newCluster = ThingMaker.MakeThing(Util_CaveworldFlora.ClusterDef) as Cluster;
            newCluster.Initialize(plantDef, desiredSize);
            GenSpawn.Spawn(newCluster, spawnCell);
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
            this.UpdateClusterPosition();
        }

        public void NotifyPlantRemoved()
        {
            this.actualSize--;
            if (this.actualSize <= 0)
            {
                Log.Message("Destroying cluster (" + this.plantDef + ") at " + this.Position);
                this.Destroy();
                return;
            }
            this.UpdateClusterPosition();
        }

        protected void UpdateClusterPosition()
        {
            Room clusterRoom = this.GetRoom();
            int size = 0;
            IntVec3 center = IntVec3.Zero;
            // We only check with clusterSpawnRadius (+ a small offset). No need to check entire cluster exclusivity area.
            IEnumerable<IntVec3> cellsInCluster = GenRadial.RadialCellsAround(this.Position, this.plantDef.clusterSpawnRadius + 3f, true);
            foreach (IntVec3 cell in cellsInCluster)
            {
                if ((cell.GetRoom() == clusterRoom)
                    && (Find.ThingGrid.ThingAt(cell, this.plantDef) != null))
                {
                    size++;
                    center += cell;
                }
            }
            center.x = (int)Mathf.Round(center.x / (float)size);
            center.z = (int)Mathf.Round(center.z / (float)size);
            if (center.x < 0)
            {
                center.x = 0;
            }
            else if (center.x > Find.Map.Size.x)
            {
                center.x = Find.Map.Size.x;
            }
            if (center.z < 0)
            {
                center.z = 0;
            }
            else if (center.z > Find.Map.Size.z)
            {
                center.z = Find.Map.Size.z;
            }
            this.Position = center;
            this.actualSize = size;
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
