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
        private int flashStartInTicks = flashPeriodInTicks;
        public const int flashDurationInTicks = 10;
        private int flashStopInTicks = 0;

        private CompGlower glower;

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            glower = this.TryGetComp<CompGlower>();
            glower.Props.glowRadius = 0f;
            glower.Props.overlightRadius = 0f;
            Find.MapDrawer.MapMeshDirty(this.Position, MapMeshFlag.Things);
            Find.GlowGrid.MarkGlowGridDirty(this.Position);
        }

        public void SetFlashStartOffset(int offsetInTicks)
        {
            flashStartInTicks += offsetInTicks;
        }

        public override void Tick()
        {
            base.Tick();

            if (flashStartInTicks > 0)
            {
                flashStartInTicks--;
                if (flashStartInTicks == 0)
                {
                    glower.Props.glowRadius = 3f;
                    glower.Props.overlightRadius = 2.9f;
                    Find.MapDrawer.MapMeshDirty(this.Position, MapMeshFlag.Things);
                    Find.GlowGrid.MarkGlowGridDirty(this.Position);
                    flashStopInTicks = flashDurationInTicks;
                }
            }
            else
            {
                flashStopInTicks--;
                if (flashStopInTicks == 0)
                {
                    glower.Props.glowRadius = 0f;
                    glower.Props.overlightRadius = 0f;
                    Find.MapDrawer.MapMeshDirty(this.Position, MapMeshFlag.Things);
                    Find.GlowGrid.MarkGlowGridDirty(this.Position);
                    flashStartInTicks = flashPeriodInTicks;
                }
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.flashStartInTicks, "flashStartInTicks");
            Scribe_Values.LookValue<int>(ref this.flashStopInTicks, "flashStopInTicks");
        }
    }
}
