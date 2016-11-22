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
    /// Order a pawn to go and work on the extraction of the terraformer weather controller.
    /// </summary>
    public class JobDriver_ExtractWeatherController : JobDriver
    {
        public TargetIndex terraformerIndex = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Passion passion = Passion.None;
            const float skillGainPerTick = 0.15f;
            float skillGainFactor = 0f;
            int extractionDuration = 0;

            float statValue = this.pawn.GetStatValue(StatDefOf.ResearchSpeed, true);
            extractionDuration = (int)Math.Round((double)(1200f / statValue));

            yield return Toils_Reserve.Reserve(terraformerIndex);

            yield return Toils_Goto.GotoCell(terraformerIndex, PathEndMode.InteractionCell).FailOnDestroyedOrNull(terraformerIndex);

            Toil extractionToil = new Toil()
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
                defaultDuration = extractionDuration
            };
            yield return extractionToil.FailOnDestroyedOrNull(terraformerIndex);

            yield return Toils_Reserve.Release(terraformerIndex);

            Toil incrementExtractionCounterToil = new Toil()
            {
                initAction = () =>
                {
                    Building_MechanoidTerraformer terraformer = this.TargetThingA as Building_MechanoidTerraformer;
                    terraformer.extractionCounter++;
                    if (terraformer.extractionCounter >= Building_MechanoidTerraformer.extractionCounterTargetValue)
                    {
                        terraformer.FinishWeatherControllerExtraction();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return incrementExtractionCounterToil;
        }
    }
}
