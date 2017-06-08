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
        public enum LightMode
        {
            Automatic,
            ForcedOn,
            ForcedOff
        }

        public Thing light;
        public bool lightIsOn = false;
        public bool refreshIsNecessary = false;
        public LightMode lightMode = LightMode.Automatic;

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
            if ((this.Wearer == null)
                || this.Wearer.InBed())
            {
                SwitchOffLight();
                return;
            }
            
            if (this.lightMode == LightMode.ForcedOn)
            {
                SwitchOnLight();
                return;
            }
            else if (this.lightMode == LightMode.ForcedOff)
            {
                SwitchOffLight();
                return;
            }

            // Automatic mode.
            // Colonist is mining.
            if ((this.Wearer.CurJob != null)
                && (this.Wearer.CurJob.def == JobDefOf.Mine))
            {
                SwitchOnLight();
                return;
            }
            
            // Colonist is under a natural roof.
            if ((this.Wearer.Map != null)
                && this.Wearer.MapHeld.roofGrid.Roofed(this.Wearer.Position)
                && this.Wearer.MapHeld.roofGrid.RoofAt(this.Wearer.Position).isNatural)
            {
                SwitchOnLight();
                return;
            }
            
            // Other cases.
            SwitchOffLight();
        }

        public void SwitchOnLight()
        {
            IntVec3 newPosition = this.Wearer.DrawPos.ToIntVec3();

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
                Thing potentialLight = newPosition.GetFirstThing(this.Wearer.Map, Util_MiningHelmet.miningLightDef);
                if (potentialLight == null)
                {
                    this.light = GenSpawn.Spawn(Util_MiningHelmet.miningLightDef, newPosition, this.Wearer.Map);
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

            Scribe_References.Look<Thing>(ref this.light, "headLight");
            Scribe_Values.Look<bool>(ref this.lightIsOn, "lightIsOn");
            Scribe_Values.Look<LightMode>(ref this.lightMode, "lightMode");
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                this.refreshIsNecessary = true;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerable<Gizmo> buttonList = GetWornGizmos();
            IEnumerable<Gizmo> resultButtonList;
            IEnumerable<Gizmo> basebuttonList = base.GetGizmos();
            if (basebuttonList != null)
            {
                resultButtonList = buttonList.Concat(basebuttonList);
            }
            else
            {
                resultButtonList = buttonList;
            }
            return resultButtonList;
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000101;

            Command_Action lightModeButton = new Command_Action();
            switch (this.lightMode)
            {
                case (LightMode.Automatic):
                    lightModeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_LigthModeAutomatic");
                    lightModeButton.defaultLabel = "Ligth: automatic.";
                    break;
                case (LightMode.ForcedOn):
                    lightModeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_LigthModeForcedOn");
                    lightModeButton.defaultLabel = "Ligth: on.";
                    break;
                case (LightMode.ForcedOff):
                    lightModeButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_LigthModeForcedOff");
                    lightModeButton.defaultLabel = "Ligth: off.";
                    break;
            }
            lightModeButton.defaultDesc = "Switch mode.";
            lightModeButton.activateSound = SoundDef.Named("Click");
            lightModeButton.action = new Action(PerformLigthModeAction);
            lightModeButton.groupKey = groupKeyBase + 1;
            buttonList.Add(lightModeButton);

            return buttonList;
        }

        /// <summary>
        /// Switch light mode.
        /// </summary>
        public void PerformLigthModeAction()
        {
            switch (this.lightMode)
            {
                case LightMode.Automatic:
                    this.lightMode = LightMode.ForcedOn;
                    break;
                case LightMode.ForcedOn:
                    this.lightMode = LightMode.ForcedOff;
                    break;
                case LightMode.ForcedOff:
                    this.lightMode = LightMode.Automatic;
                    break;
            }
        }
    }
}
