using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;     // Always needed
using RimWorld;        // RimWorld specific functions are found here
using RimWorld.Planet; // RimWorld planet specific functions are found here
using Verse;           // RimWorld universal objects are here
using Verse.AI;        // Needed when you do something with the AI
using Verse.AI.Group;  // Needed when you do something with the AI
//using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{
    public class WorldComponent_Partnership : WorldComponent
    {
        public const int feeInitialCostInSilver = 1000;
        public const int globalGoodwillCostInSilver = 2000;
        public const int cargoSpaceshipPeriodicSupplyPeriodInTicks = 10 * GenDate.TicksPerDay; // Time between periodical cargo supply ships.
        public const int cargoSpaceshipRequestedSupplyPeriodInTicks = 5 * GenDate.TicksPerDay; // Time between requested cargo supply ships.
        public const int medicalSpaceshipRequestedSupplyPeriodInTicks = 5 * GenDate.TicksPerDay; // Time between medical supply ships.

        public int globalGoodwillFeeInSilver = 0;
        public Dictionary<Map, int> feeInSilver = new Dictionary<Map, int>();
        public Dictionary<Map, int> nextPeriodicSupplyTick = new Dictionary<Map, int>();
        public Dictionary<Map, int> nextRequestedSupplyMinTick = new Dictionary<Map, int>();
        public Dictionary<Map, int> nextMedicalSupplyMinTick = new Dictionary<Map, int>();
        public Dictionary<Map, int> nextAirstrikeMinTick = new Dictionary<Map, int>();

        private List<Map> maps = new List<Map>();                             // Only used to expose data.
        private List<int> feeInSilverValues = new List<int>();                // Only used to expose data.
        private List<int> nextPeriodicSupplyTickValues = new List<int>();     // Only used to expose data.
        private List<int> nextRequestedSupplyMinTickValues = new List<int>(); // Only used to expose data.
        private List<int> nextMedicalSupplyMinTickValues = new List<int>();   // Only used to expose data.
        private List<int> nextAirstrikeMinTickValues = new List<int>();       // Only used to expose data.
        
        // ===================== Setup work =====================
        public WorldComponent_Partnership(World world)
            : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.globalGoodwillFeeInSilver, "globalGoodwillFeeInSilver");

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                CleanNullMap(ref this.feeInSilver);
                CleanNullMap(ref this.nextPeriodicSupplyTick);
                CleanNullMap(ref this.nextRequestedSupplyMinTick);
                CleanNullMap(ref this.nextMedicalSupplyMinTick);
                CleanNullMap(ref this.nextAirstrikeMinTick);
                // Store partnership data in separate lists.
                this.maps.Clear();
                this.feeInSilverValues.Clear();
                this.nextPeriodicSupplyTickValues.Clear();
                this.nextRequestedSupplyMinTickValues.Clear();
                this.nextMedicalSupplyMinTickValues.Clear();
                this.nextAirstrikeMinTickValues.Clear();
                foreach (Map map in this.feeInSilver.Keys)
                {
                    this.maps.Add(map);
                    this.feeInSilverValues.Add(this.feeInSilver[map]);
                    this.nextPeriodicSupplyTickValues.Add(this.nextPeriodicSupplyTick[map]);
                    this.nextRequestedSupplyMinTickValues.Add(this.nextRequestedSupplyMinTick[map]);
                    this.nextMedicalSupplyMinTickValues.Add(this.nextMedicalSupplyMinTick[map]);
                    this.nextAirstrikeMinTickValues.Add(this.nextAirstrikeMinTick[map]);
                }
            }
            Scribe_Collections.Look<Map>(ref this.maps, "maps", LookMode.Reference);
            Scribe_Collections.Look<int>(ref this.feeInSilverValues, "feeInSilver");
            Scribe_Collections.Look<int>(ref this.nextPeriodicSupplyTickValues, "nextPeriodicSupplyTick");
            Scribe_Collections.Look<int>(ref this.nextRequestedSupplyMinTickValues, "nextRequestedSupplyMinTick");
            Scribe_Collections.Look<int>(ref this.nextMedicalSupplyMinTickValues, "nextMedicalSupplyMinTick");
            Scribe_Collections.Look<int>(ref this.nextAirstrikeMinTickValues, "nextAirstrikeMinTick");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // Restore partnership data from separate lists.
                this.feeInSilver.Clear();
                this.nextPeriodicSupplyTick.Clear();
                this.nextRequestedSupplyMinTick.Clear();
                this.nextMedicalSupplyMinTick.Clear();
                this.nextAirstrikeMinTick.Clear();
                for (int mapIndex = 0; mapIndex < this.maps.Count; mapIndex++)
                {
                    this.feeInSilver.Add(this.maps[mapIndex], this.feeInSilverValues[mapIndex]);
                    this.nextPeriodicSupplyTick.Add(this.maps[mapIndex], this.nextPeriodicSupplyTickValues[mapIndex]);
                    this.nextRequestedSupplyMinTick.Add(this.maps[mapIndex], this.nextRequestedSupplyMinTickValues[mapIndex]);
                    this.nextMedicalSupplyMinTick.Add(this.maps[mapIndex], this.nextMedicalSupplyMinTickValues[mapIndex]);
                    this.nextAirstrikeMinTick.Add(this.maps[mapIndex], this.nextAirstrikeMinTickValues[mapIndex]);
                }
            }
        }

        public void CleanNullMap(ref Dictionary<Map, int> dictionary)
        {
            Dictionary<Map, int> cleanedDictionnary = new Dictionary<Map, int>();
            foreach (Map map in dictionary.Keys)
            {
                if (Find.Maps.Contains(map))
                {
                    cleanedDictionnary.Add(map, dictionary[map]);
                }
            }
            dictionary = cleanedDictionnary;
        }

        // Fee initialization.
        public void InitializeFeeIfNeeded(Map map)
        {
            if (this.feeInSilver.ContainsKey(map) == false)
            {
                this.feeInSilver.Add(map, feeInitialCostInSilver);
            }
        }

        // Supply and airstrike ticks initialization.
        public void InitializePeriodicSupplyTickIfNeeded(Map map)
        {
            if (this.nextPeriodicSupplyTick.ContainsKey(map) == false)
            {
                this.nextPeriodicSupplyTick.Add(map, 0);
            }
        }

        public void InitializeRequestedSupplyTickIfNeeded(Map map)
        {
            if (this.nextRequestedSupplyMinTick.ContainsKey(map) == false)
            {
                this.nextRequestedSupplyMinTick.Add(map, 0);
            }
        }

        public void InitializeMedicalSupplyTickIfNeeded(Map map)
        {
            if (this.nextMedicalSupplyMinTick.ContainsKey(map) == false)
            {
                this.nextMedicalSupplyMinTick.Add(map, 0);
            }
        }

        public void InitializeAirstrikeTickIfNeeded(Map map)
        {
            if (this.nextAirstrikeMinTick.ContainsKey(map) == false)
            {
                this.nextAirstrikeMinTick.Add(map, 0);
            }
        }
    }
}
