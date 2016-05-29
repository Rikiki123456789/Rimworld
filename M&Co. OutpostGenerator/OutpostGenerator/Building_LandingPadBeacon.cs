using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Building_LandingPadBeacon class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_LandingPadBeacon : Building
    {
        public const int flashPeriodInTicks = 15 * (int)GenTicks.TicksPerRealSecond;
        private int flashStartTick = flashPeriodInTicks;
        public const int flashDurationInTicks = 10;
        private int flashStopTick = 0;
        
        private Thing glower = null;
        
        public void SetFlashStartOffset(int offsetInTicks)
        {
            flashStartTick += offsetInTicks;
        }

        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame >= flashStartTick)
            {
                flashStopTick = Find.TickManager.TicksGame + flashDurationInTicks;
                flashStartTick = Find.TickManager.TicksGame + flashPeriodInTicks;
                if (this.glower.DestroyedOrNull())
                {
                    this.glower = GenSpawn.Spawn(OG_Util.LandingPadBeaconGlowerDef, this.Position);
                }
                return;
            }
            if (Find.TickManager.TicksGame >= flashStopTick)
            {
                if (this.glower.DestroyedOrNull() == false)
                {
                    this.glower.Destroy();
                    this.glower = null;
                }
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.flashStartTick, "flashStartTick");
            Scribe_Values.LookValue<int>(ref this.flashStopTick, "flashStopTick");
            Scribe_References.LookReference<Thing>(ref this.glower, "glower");
        }

        // Disable Gizmos.
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.Faction != Faction.OfColony)
            {
                return new List<Gizmo>();
            }
            else
            {
                return base.GetGizmos();
            }
        }

    }
}
