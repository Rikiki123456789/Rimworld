using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// OG_OutpostData class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class OG_OutpostData
    {
        public OG_OutpostSize size;
        public bool isMilitary;
        public bool battleOccured;
        public bool isRuined;
        public bool isInhabited;
        public IntVec3 areaSouthWestOrigin;
        public IntVec3 dropZoneCenter;

        // Parameters only used during generation.
        public ThingDef structureStuffDef;
        public ThingDef furnitureStuffDef;
        public TriggerIntrusion triggerIntrusion;
        public List<Thing> outpostThingList;
    }
}
