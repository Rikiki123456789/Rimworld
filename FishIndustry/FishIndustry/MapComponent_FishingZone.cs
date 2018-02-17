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
    /// MapComponent_FishingZone class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class MapComponent_FishingZone : MapComponent
    {
        public const int updatePeriodInTicks = GenTicks.TickRareInterval;

        public MapComponent_FishingZone(Map map) : base(map)
        {
        }
        
        // ===================== Main Work Function =====================
        public override void MapComponentTick()
        {
            base.MapComponentTick();

            foreach (Zone zone in this.map.zoneManager.AllZones)
            {
                if (zone is Zone_Fishing)
                {
                    Zone_Fishing fishingZone = zone as Zone_Fishing;
                    if (Find.TickManager.TicksGame >= fishingZone.nextUpdateTick)
                    {
                        fishingZone.nextUpdateTick = Find.TickManager.TicksGame + updatePeriodInTicks;
                        fishingZone.UpdateZone();
                    }
                }
            }
        }
    }
}
