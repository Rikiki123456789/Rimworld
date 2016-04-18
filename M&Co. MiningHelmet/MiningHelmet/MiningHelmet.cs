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
    /// MiningHelmet class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class MiningHelmet : Apparel
    {
        public Thing headLight;
        public bool lightIsOn = false;
        public bool refreshIsNecessary = false;

        /// <summary>
        /// Perform the MiningHelmet main treatment:
        /// - switch on the headlight if the pawn is awake and under a natural roof or in the open dark and mining,
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

            // Helmet on ground or wearer is sleeping.
            if ((this.wearer == null)
                || this.wearer.InBed())
            {
                SwitchOffHeadLight();
                return;
            }

            // Colonist is mining.
            bool colonistIsMining = (this.wearer.CurJob != null)
                && (this.wearer.CurJob.def == JobDefOf.Mine);
            if (colonistIsMining)
            {
                SwitchOnHeadLight();
                return;
            }
            
            // Colonist is under a natural roof.
            bool colonistIsUnderARoof = Find.RoofGrid.Roofed(this.wearer.Position);
            if (colonistIsUnderARoof)
            {
                RoofDef roofType = Find.RoofGrid.RoofAt(this.wearer.Position);
                if ((roofType == DefDatabase<RoofDef>.GetNamed("RoofRockThin"))
                    || (roofType == DefDatabase<RoofDef>.GetNamed("RoofRockThick")))
                {
                    SwitchOnHeadLight();
                    return;
                }
            }

            // Other cases.
            SwitchOffHeadLight();
        }

        public void SwitchOnHeadLight()
        {
            IntVec3 newPosition = this.wearer.DrawPos.ToIntVec3();
            if (this.headLight.DestroyedOrNull())
            {
                this.headLight = GenSpawn.Spawn(Util_MiningHelmet.miningHelmetGlowerDef, newPosition);
            }
            if ((newPosition != this.headLight.Position)
                || this.refreshIsNecessary)
            {
                SwitchOffHeadLight();
                this.headLight = GenSpawn.Spawn(Util_MiningHelmet.miningHelmetGlowerDef, newPosition);
                this.refreshIsNecessary = false;
            }
            this.lightIsOn = true;
        }

        public void SwitchOffHeadLight()
        {
            if (this.headLight.DestroyedOrNull() == false)
            {
                this.headLight.Destroy();
                this.headLight = null;
            }
            this.lightIsOn = false;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.LookReference<Thing>(ref this.headLight, "headLight");
            Scribe_Values.LookValue<bool>(ref this.lightIsOn, "lightIsOn");
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                this.refreshIsNecessary = true;
            }
        }
    }
}
