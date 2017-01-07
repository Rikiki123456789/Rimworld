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

namespace MiningHelmet
{
    /// <summary>
    /// ApparelWithMiningLight class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class ApparelWithMiningLight : Apparel
    {
        public Thing light;
        public bool lightIsOn = false;
        public bool refreshIsNecessary = false;

        /// <summary>
        /// Perform the main treatment:
        /// - switch on the light if the pawn is awake and under a natural roof or in the open dark and mining,
        /// - switch off the headlight otherwise.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            // Only tick once a second when light is off.
            if ((this.lightIsOn == false)
                && (Find.TickManager.TicksGame % GenTicks.TicksPerRealSecond != 0))
            {
                return;
            }

            // Apparel on ground or wearer is sleeping.
            if ((this.wearer == null)
                || this.wearer.InBed())
            {
                SwitchOffLight();
                return;
            }

            // Colonist is mining.
            if ((this.wearer.CurJob != null)
                && (this.wearer.CurJob.def == JobDefOf.Mine))
            {
                SwitchOnLight();
                return;
            }

            // Colonist is under a natural roof.
            if (this.wearer.Map.roofGrid.Roofed(this.wearer.Position)
                && this.wearer.Map.roofGrid.RoofAt(this.wearer.Position).isNatural)
            {
                SwitchOnLight();
                return;
            }

            // Other cases.
            SwitchOffLight();
        }

        public void SwitchOnLight()
        {
            IntVec3 newPosition = this.wearer.DrawPos.ToIntVec3();

            // Switch off previous light if pawn moved.
            if (((this.light.DestroyedOrNull() == false)
                && (newPosition != this.light.Position))
                || this.refreshIsNecessary)
            {
                SwitchOffLight();
                this.refreshIsNecessary = false;
            }

            // Try to spawn a new light.
            if (this.light.DestroyedOrNull())
            {
                Thing potentialLight = newPosition.GetFirstThing(this.wearer.Map, Util_MiningHelmet.miningLightDef);
                if (potentialLight == null)
                {
                    this.light = GenSpawn.Spawn(Util_MiningHelmet.miningLightDef, newPosition, this.wearer.Map);
                }
                // else another light is already here.
            }
            this.lightIsOn = true;
        }

        public void SwitchOffLight()
        {
            if (this.light.DestroyedOrNull() == false)
            {
                this.light.Destroy();
                this.light = null;
            }
            this.lightIsOn = false;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.LookReference<Thing>(ref this.light, "headLight");
            Scribe_Values.LookValue<bool>(ref this.lightIsOn, "lightIsOn");
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                this.refreshIsNecessary = true;
            }
        }
    }
}
