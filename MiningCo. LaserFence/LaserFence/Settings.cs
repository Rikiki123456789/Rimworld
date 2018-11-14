using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace LaserFence
{
    public class Settings : ModSettings
    {
        public static int laserFenceMaxRange = 7;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref laserFenceMaxRange, "laserFenceMaxRange");
        }

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.ColumnWidth = inRect.width / 2f;
            list.Begin(inRect);
            list.Label("Laser fence max range (default is 7): " + laserFenceMaxRange, -1f, "Set the maximum distance (in cells) between two pylons to be able to connect each other. Warning! Setting a high value will obviously break the relative balance of this mod... But this is a single player game, so do as you want!");
            laserFenceMaxRange = (int)list.Slider(laserFenceMaxRange, 1, 20);
            list.End();
        }
    }
}
