using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace MobileMineralSonar
{
    public class Settings : ModSettings
    {
        public static bool periodicLightIsEnabled = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref periodicLightIsEnabled, "periodicLightIsEnabled", false);
        }

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.ColumnWidth = inRect.width / 2f;
            list.Begin(inRect);
            list.CheckboxLabeled("Enable periodic light", ref periodicLightIsEnabled, "When enabled, the MMS will periodically emits a short flash of light. This can help you locate it.");
            list.End();
        }
    }
}
