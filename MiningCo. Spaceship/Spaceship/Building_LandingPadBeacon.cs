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
    public class Building_LandingPadBeacon : Building
    {
        public Building_LandingPad landingPad = null;
        public bool isPoweredOn = false;
        public Color color = Color.white;
        public int lightPeriodInTicks = 15 * (int)GenTicks.TicksPerRealSecond;
        public int lightDurationInTicks = 10;
        public int lightDelayInTicks = 0; // Used to synchronize several beacons.
        public int nextLightStartTick = 0;
        public int nextLightStopTick = 0;

        public Thing glower = null;

        // ===================== Setup work =====================
        public void InitializeParameters(Building_LandingPad landingPad, Color color,  int periodInTicks, int durationInTicks, int delayInTicks)
        {
            this.landingPad = landingPad;
            this.SetFaction(landingPad.Faction);
            this.color = color;
            this.lightPeriodInTicks = periodInTicks;
            this.lightDurationInTicks = durationInTicks;
            this.lightDelayInTicks = delayInTicks;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            SwitchOffLight();
            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Building_LandingPad>(ref this.landingPad, "landingPad");
            Scribe_Values.Look<bool>(ref this.isPoweredOn, "isPoweredOn");
            Scribe_Values.Look<Color>(ref this.color, "color");
            Scribe_Values.Look<int>(ref this.lightPeriodInTicks, "lightPeriodInTicks");
            Scribe_Values.Look<int>(ref this.lightDurationInTicks, "lightDurationInTicks");
            Scribe_Values.Look<int>(ref this.lightDelayInTicks, "lightDelayInTicks");
            Scribe_Values.Look<int>(ref this.nextLightStartTick, "nextLightStartTick");
            Scribe_Values.Look<int>(ref this.nextLightStopTick, "nextLightStopTick");
            Scribe_References.Look<Thing>(ref this.glower, "glower");
        }

        // ===================== Main function =====================
        /// <summary>
        /// periodically switch the light on/off when powered.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (Settings.landingPadLightIsEnabled == false)
            {
                return;
            }

            if (this.isPoweredOn)
            {
                if (Find.TickManager.TicksGame >= this.nextLightStartTick)
                {
                    this.nextLightStopTick = Find.TickManager.TicksGame + this.lightDurationInTicks;
                    this.nextLightStartTick = Find.TickManager.TicksGame + this.lightPeriodInTicks;
                    SwitchOnLight();
                }
                if (Find.TickManager.TicksGame >= this.nextLightStopTick)
                {
                    SwitchOffLight();
                }
            }
        }

        /// <summary>
        /// Spawn a colored glower.
        /// </summary>
        public void SwitchOnLight()
        {
            if (this.glower.DestroyedOrNull())
            {
                if (this.color == Color.white)
                {
                    this.glower = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeaconGlowerWhite, this.Position, this.Map);
                }
                else if (this.color == Color.red)
                {
                    this.glower = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeaconGlowerRed, this.Position, this.Map);
                }
                else if (this.color == Color.green)
                {
                    this.glower = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeaconGlowerGreen, this.Position, this.Map);
                }
                else
                {
                    Log.ErrorOnce("MiningCo. spaceship: beacon color (" + this.color.ToString() + ") not handled!", 123456789);
                }
            }
        }

        /// <summary>
        /// Remove the glower.
        /// </summary>
        public void SwitchOffLight()
        {
            if (this.glower.DestroyedOrNull() == false)
            {
                this.glower.Destroy();
                this.glower = null;
            }
        }

        // ===================== Exported functions =====================
        /// <summary>
        /// Action when landing pad is powered on: reset light cycle.
        /// </summary>
        public void Notify_PowerStarted()
        {
            this.isPoweredOn = true;
            this.nextLightStartTick = Find.TickManager.TicksGame + this.lightDelayInTicks;
        }

        /// <summary>
        /// Action when landing pad is powered off: switch off light
        /// </summary>
        public void Notify_PowerStopped()
        {
            this.isPoweredOn = false;
            SwitchOffLight();
        }
    }
}
