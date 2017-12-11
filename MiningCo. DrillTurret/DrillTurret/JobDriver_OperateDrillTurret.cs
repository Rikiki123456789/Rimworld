using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace DrillTurret
{
    public class JobDriver_OperateDrillTurret : JobDriver
    {
        public TargetIndex drillTurretIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.TargetThingA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            const float skillGainPerTick = 0.11f;

            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(delegate
            {
                return ((this.TargetThingA as Building_DrillTurret).targetPosition == IntVec3.Invalid);
            });

            yield return Toils_Goto.GotoCell(drillTurretIndex, PathEndMode.InteractionCell);
            
            Toil operateDrillTurretToil = new Toil()
            {
                tickAction = () =>
                {
                    Pawn actor = this.GetActor();
                    float miningEfficiency = (float)actor.skills.GetSkill(SkillDefOf.Mining).Level / (float)SkillRecord.MaxLevel;
                    (this.TargetThingA as Building_DrillTurret).SetOperatorEfficiency(miningEfficiency);
                    this.GetActor().skills.Learn(SkillDefOf.Mining, skillGainPerTick);
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
            yield return operateDrillTurretToil;

            yield return Toils_Reserve.Release(drillTurretIndex);
        }
    }
}

