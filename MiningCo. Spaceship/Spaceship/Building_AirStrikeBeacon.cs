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
    public class Building_AirstrikeBeacon : Building
    {        
        public AirstrikeDef airStrikeDef = null;
        
        public const int ticksBetweenRuns = 15 * GenTicks.TicksPerRealSecond;
        public int nextStrikeTick = 0;
        public int remainingRuns = 0;

        public const int fireStartCheckPeriodInTicks = GenTicks.TicksPerRealSecond;

        // ===================== Setup work =====================
        public void InitializeAirstrike(IntVec3 targetPosition, AirstrikeDef airStrikeDef)
        {
            this.Position = targetPosition;
            this.airStrikeDef = airStrikeDef;
            this.remainingRuns = this.airStrikeDef.runsNumber;
            this.nextStrikeTick = Find.TickManager.TicksGame + ticksBetweenRuns;
            GetComp<CompOrbitalBeam>().StartAnimation(this.remainingRuns * ticksBetweenRuns, 10, Rand.Range(-12f, 12f));
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<AirstrikeDef>(ref this.airStrikeDef, "airStrikeDef");
            Scribe_Values.Look<int>(ref this.nextStrikeTick, "nextStrikeTick");
            Scribe_Values.Look<int>(ref this.remainingRuns, "runNumber");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();
            
            if (Find.TickManager.TicksGame % fireStartCheckPeriodInTicks == 0)
            {
                IntVec3 adjacentCell = (this.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1.5f)).ToIntVec3();
                if (adjacentCell.InBounds(this.Map))
                {
                    FireUtility.TryStartFireIn(this.Position, this.Map, 0.1f);
                }
            }

            if ((this.remainingRuns > 0)
                && (Find.TickManager.TicksGame >= this.nextStrikeTick))
            {
                // Start new run.
                Util_Spaceship.SpawnStrikeShip(this.Map, this.Position, this.airStrikeDef, this.Faction);
                this.remainingRuns--;
                this.nextStrikeTick = Find.TickManager.TicksGame + ticksBetweenRuns;
            }
            if (this.remainingRuns == 0)
            {
                // Airstrike is finished.
                this.Destroy();
            }
        }
        
        // ===================== Inspect panel =====================
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            int nextStrikeDelayInTicks = this.nextStrikeTick - Find.TickManager.TicksGame;
            if (nextStrikeDelayInTicks > 0)
            stringBuilder.Append("Next strike in " + nextStrikeDelayInTicks.ToStringSecondsFromTicks() + ".");

            return stringBuilder.ToString();
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000104;

            Command_Action setTargetButton = new Command_Action();
            setTargetButton.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack");
            setTargetButton.defaultLabel = "Set new target";
            setTargetButton.defaultDesc = "Designate a new airstrike target. This can delay the next run as the strike ship has to change its planned trajectory. Can only designate an unfogged position.";
            setTargetButton.activateSound = SoundDef.Named("Click");
            setTargetButton.action = new Action(SelectNewAirstrikeTarget);
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
        /// Select a new airstrike target.
        /// </summary>
        public void SelectNewAirstrikeTarget()
        {
            Util_Misc.SelectAirstrikeTarget(this.Map, SetNewAirstrikeTarget);
        }

        /// <summary>
        /// Set a new airstrike target.
        /// </summary>
        public void SetNewAirstrikeTarget(LocalTargetInfo targetPosition)
        {
            this.Position = targetPosition.Cell;
            this.nextStrikeTick = Find.TickManager.TicksGame + ticksBetweenRuns;
            Messages.Message("New airstrike target designated.", this, MessageTypeDefOf.CautionInput);
            GetComp<CompOrbitalBeam>().StartAnimation(this.remainingRuns * ticksBetweenRuns, 10, Rand.Range(-12f, 12f));
        }
    }
}
