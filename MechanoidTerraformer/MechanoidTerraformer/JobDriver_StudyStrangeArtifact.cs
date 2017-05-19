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
    /// Order a pawn to go and study the mechanoid terraformer.
    /// </summary>
    public class JobDriver_StudyStrangeArtifact : JobDriver
    {
        public TargetIndex terraformerIndex = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Passion passion = Passion.None;
            const float skillGainPerTick = 0.15f;
            float skillGainFactor = 0f;
            int studyDuration = 0;

            float statValue = this.pawn.GetStatValue(StatDefOf.ResearchSpeed, true);
            studyDuration = (int)Math.Round((double)(1200f / statValue));

            yield return Toils_Reserve.Reserve(terraformerIndex);

            yield return Toils_Goto.GotoCell(terraformerIndex, PathEndMode.InteractionCell).FailOnDestroyedOrNull(terraformerIndex);

            Toil studyToil = new Toil()
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
                defaultDuration = studyDuration
            };
            yield return studyToil;

            Toil incrementStudyCounterToil = new Toil()
            {
                initAction = () =>
                {
                    Building_MechanoidTerraformer terraformer = this.TargetThingA as Building_MechanoidTerraformer;
                    terraformer.studyCounter++;
                    if (terraformer.studyCounter >= Building_MechanoidTerraformer.studyCounterTargetValue)
                    {
                        string herHisOrIts = "its".Translate();
                        if (pawn.gender == Gender.Female)
                        {
                            herHisOrIts = "her".Translate();
                        }
                        else if (pawn.gender == Gender.Male)
                        {
                            herHisOrIts = "his".Translate();
                        }


                        string studyReportHeader = string.Concat(new string[]
                        {
                            "   ",
                            this.pawn.Name.ToStringShort,
                            "finish_study".Translate(),
                            herHisOrIts.CapitalizeFirst(),
                            "repstudy".Translate(),
                            GenDate.DateFullStringAt(Find.TickManager.TicksAbs),
                            "Researcher".Translate(),
                            this.pawn.Name.ToStringFull,
                            "\n\n"
                        });
                        terraformer.DisplayStudyReport(studyReportHeader);
                        terraformer.def.label = "Mechanoidterraformer".Translate();
                        terraformer.reverseEngineeringState = Building_MechanoidTerraformer.ReverseEngineeringState.StudyCompleted;
                        Building_MechanoidTerraformer.studyIsCompleted = true;
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return incrementStudyCounterToil;
            
            yield return Toils_Reserve.Release(terraformerIndex);
        }
    }
}
