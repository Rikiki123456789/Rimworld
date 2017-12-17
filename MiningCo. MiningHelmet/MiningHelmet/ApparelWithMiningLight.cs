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

        public const int updatePeriodInTicks = GenTicks.TicksPerRealSecond;
        public int nextUpdateTick = 0;

        public Thing light;
        public bool lightIsOn = false;
        public LightMode lightMode = LightMode.Automatic;

        // ===================== Setup work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.nextUpdateTick = Find.TickManager.TicksGame + Rand.Range(0, updatePeriodInTicks);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look<Thing>(ref this.light, "light");
            Scribe_Values.Look<bool>(ref this.lightIsOn, "lightIsOn");
            Scribe_Values.Look<LightMode>(ref this.lightMode, "lightMode");
        }

        // ===================== Main function =====================
        /// <summary>
        /// Perform the main treatment:
        /// - respect on/off forced mode if active,
        /// - switch on the light if the pawn is awake and under a natural roof or in the dark,
        /// - switch off the headlight otherwise.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            
            // Only tick once a second when light is off.
            if (this.lightIsOn
                || (Find.TickManager.TicksGame >= this.nextUpdateTick))
            {
                RefreshLightState();
            }
        }

        public void RefreshLightState()
        {
            bool lightShouldBeOn = ComputeLightState();
            if (lightShouldBeOn)
            {
                SwitchOnLight();
            }
            else
            {
                SwitchOffLight();
            }
        }

        public bool ComputeLightState()
        {
            // Apparel on ground or wearer is dead/downed/sleeping.
            if ((this.Wearer == null)
                || this.Wearer.Dead
                || this.Wearer.Downed
                || (this.Wearer.Awake() == false))
            {
                return false;
            }

            // Forced light mode.
            if (this.lightMode == LightMode.ForcedOn)
            {
                return true;
            }
            if (this.lightMode == LightMode.ForcedOff)
            {
                return false;
            }

            // Automatic mode.
            if ((this.Wearer.Map != null)
                && ((this.Wearer.Position.Roofed(this.Wearer.Map)
                    && (this.Wearer.Map.glowGrid.PsychGlowAt(this.Wearer.Position) <= PsychGlow.Lit))
                || ((this.Wearer.Position.Roofed(this.Wearer.Map) == false)
                    && (this.Wearer.Map.glowGrid.PsychGlowAt(this.Wearer.Position) < PsychGlow.Overlit))))
            {
                return true;
            }

            return false;
        }

        public void SwitchOnLight()
        {
            IntVec3 newPosition = this.Wearer.DrawPos.ToIntVec3();

            // Switch off previous light if pawn moved.
            if ((this.light.DestroyedOrNull() == false)
                && (newPosition != this.light.Position))
            {
                SwitchOffLight();
            }

            // Try to spawn a new light.
            if (this.light.DestroyedOrNull())
            {
                Thing potentialLight = newPosition.GetFirstThing(this.Wearer.Map, Util_MiningLight.MiningLightDef);
                if (potentialLight == null)
                {
                    this.light = GenSpawn.Spawn(Util_MiningLight.MiningLightDef, newPosition, this.Wearer.Map);
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

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerable<Gizmo> buttonList = GetWornGizmos();
            IEnumerable<Gizmo> resultButtonList;
            IEnumerable<Gizmo> basebuttonList = base.GetGizmos();
            if (basebuttonList != null)
            {
                resultButtonList = basebuttonList.Concat(buttonList);
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
            lightModeButton.action = new Action(SwitchLigthMode);
            lightModeButton.groupKey = groupKeyBase + 1;
            buttonList.Add(lightModeButton);

            return buttonList;
        }

        /// <summary>
        /// Switch light mode.
        /// </summary>
        public void SwitchLigthMode()
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
            RefreshLightState();
        }
    }
}
