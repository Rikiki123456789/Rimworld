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
    public class Building_AirStrikeBeacon : Building
    {        
        public AirStrikeDef airStrikeDef = null;
        
        public const int ticksBetweenRuns = 15 * GenTicks.TicksPerRealSecond;
        public int nextStrikeTick = 0;
        public int remainingRuns = 0;

        // Draw.
        public const int flashPeriodInTicks = 2 * GenTicks.TicksPerRealSecond;
        public int nextFlashTick = 0;

        // ===================== Setup work =====================
        public void InitializeAirStrike(IntVec3 targetPosition, AirStrikeDef airStrikeDef)
        {
            this.Position = targetPosition;
            this.airStrikeDef = airStrikeDef;
            this.remainingRuns = this.airStrikeDef.runsNumber;
            this.nextStrikeTick = Find.TickManager.TicksGame + ticksBetweenRuns;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<AirStrikeDef>(ref this.airStrikeDef, "airStrikeDef");
            Scribe_Values.Look<int>(ref this.nextStrikeTick, "nextStrikeTick");
            Scribe_Values.Look<int>(ref this.remainingRuns, "runNumber");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();

            // Light flash to help beacon localization.
            if (Find.TickManager.TicksGame >= this.nextFlashTick)
            {
                MoteMaker.MakeStaticMote(this.Position.ToVector3Shifted(), this.Map, ThingDefOf.Mote_ShotFlash, 5f);
                this.nextFlashTick = Find.TickManager.TicksGame + flashPeriodInTicks;
            }

            if ((this.remainingRuns > 0)
                && (Find.TickManager.TicksGame >= this.nextStrikeTick))
            {
                // Start new run.
                Util_Spaceship.SpawnStrikeShip(this.Map, this.Position, this.airStrikeDef);
                this.remainingRuns--;
                this.nextStrikeTick = Find.TickManager.TicksGame + ticksBetweenRuns;
            }
            if (this.remainingRuns == 0)
            {
                // Air strike is finished.
                this.Destroy();
            }
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000104;

            Command_Action setTargetButton = new Command_Action();
            setTargetButton.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack");
            setTargetButton.defaultLabel = "Set new target";
            setTargetButton.defaultDesc = "Designate a new air strike target. This can delay the next run as the strike ship has to change its planned trajectory. Can only designate an unfogged position.";
            setTargetButton.activateSound = SoundDef.Named("Click");
            setTargetButton.action = new Action(SelectNewAirStrikeTarget);
            setTargetButton.groupKey = groupKeyBase + 1;
            buttonList.Add(setTargetButton);

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

        /// <summary>
        /// Select a new air strike target.
        /// </summary>
        public void SelectNewAirStrikeTarget()
        {
            Util_Misc.SelectAirStrikeTarget(this.Map, SetNewAirStrikeTarget);
        }

        /// <summary>
        /// Set a new air strike target.
        /// </summary>
        public void SetNewAirStrikeTarget(LocalTargetInfo targetPosition)
        {
            this.Position = targetPosition.Cell;
            this.nextStrikeTick = Find.TickManager.TicksGame + ticksBetweenRuns;
            Messages.Message("New air strike target designated.", this, MessageTypeDefOf.CautionInput);
        }
    }
}
