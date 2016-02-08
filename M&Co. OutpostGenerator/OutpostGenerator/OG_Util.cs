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

namespace OutpostGenerator
{
    /// <summary>
    /// Outpost generator utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_Util
    {
        public static string JobDefName_TryToCaptureOutpost = "JobDef_TryToCaptureOutpost";

        public static ThingDef FireproofPowerConduitDef
        {
            get
            {
                return ThingDef.Named("FireproofPowerConduit");
            }
        }

        // TODO: add corresponding designator (check console has been captured/destroyed to allow removing it) OR get rid of it :/
        public static RoofDef IronedRoofDef
        {
            get
            {
                return DefDatabase<RoofDef>.GetNamed("IronedRoof");
            }
        }

        /// <summary>
        /// Return a copy of the listToRefresh but remove any destroyed item.
        /// </summary>
        public static List<Thing> RefreshThingList(List<Thing> listToRefresh)
        {
            List<Thing> refreshedList = new List<Thing>();

            if (listToRefresh.NullOrEmpty() == false)
            {
                foreach (Thing thing in listToRefresh)
                {
                    if (thing.Destroyed == false)
                    {
                        refreshedList.Add(thing);
                    }
                }
                return refreshedList;
            }
            else
            {
                return null;
            }
        }
    }
}
