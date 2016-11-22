using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
//using RimWorld.Planet;

namespace MechanoidTerraformer
{
    /// <summary>
    /// Order a pawn to go and work on the rerouting of the terraformer power network.
    /// </summary>
    public class JobDriver_ReroutePower : JobDriver
    {
        public TargetIndex terraformerIndex = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Passion passion = Passion.None;
            const float skillGainPerTick = 0.15f;
            float skillGainFactor = 0f;
            int reroutingDuration = 0;

            float statValue = this.pawn.GetStatValue(StatDefOf.ResearchSpeed, true);
            reroutingDuration = (int)Math.Round((double)(1200f / statValue));

            yield return Toils_Reserve.Reserve(terraformerIndex);

            yield return Toils_Goto.GotoCell(terraformerIndex, PathEndMode.InteractionCell).FailOnDestroyedOrNull(terraformerIndex);

            Toil rerouteToil = new Toil()
            {
                initAction = () =>
                {
                    passion = this.pawn.skills.MaxPassionOfRelevantSkillsFor(WorkTypeDefOf.Research);
                    if (passion == Passion.None)
                    {
                        skillGainFactor = 0.3f;
                    }
                    else if (passion == Passion.Minor)
                    {
                        skillGainFactor = 1f;
                    }
                    else
                    {
                        skillGainFactor = 1.5f;
                    }
                },
                tickAction = () =>
                {
                    this.pawn.skills.Learn(SkillDefOf.Research, skillGainPerTick * skillGainFactor);
                },
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = reroutingDuration
            };
            yield return rerouteToil.FailOnDestroyedOrNull(terraformerIndex);


            Toil incrementReroutingCounterToil = new Toil()
            {
                initAction = () =>
                {
                    Building_MechanoidTerraformer terraformer = this.TargetThingA as Building_MechanoidTerraformer;
                    terraformer.reroutingCounter++;
                    if (terraformer.reroutingCounter >= Building_MechanoidTerraformer.reroutingCounterTargetValue)
                    {
                        terraformer.FinishPowerRerouting();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return incrementReroutingCounterToil;

            yield return Toils_Reserve.Release(terraformerIndex);
        }
    }
}
