using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{
    public class Settings : ModSettings
    {
        public static bool landingPadLightIsEnabled = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref landingPadLightIsEnabled, "landingPadLightIsEnabled", true);
        }

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.ColumnWidth = inRect.width;
            list.Begin(inRect);
            list.CheckboxLabeled("Enable landing pad lights", ref landingPadLightIsEnabled, "Disable the beacon lights if your are getting a performance hit with dynamic glowers. Switching the landing pad power off and on may be necessary.");
            list.End();
        }
    }
}
