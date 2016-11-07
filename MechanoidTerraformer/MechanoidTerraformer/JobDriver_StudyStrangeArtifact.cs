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
using RimWorld.SquadAI;


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

            yield return Toils_Goto.GotoCell(terraformerIndex, PathEndMode.InteractionCell).FailOnDestroyed(terraformerIndex);

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
                        string herHisOrIts = "its";
                        if (pawn.gender == Gender.Female)
                        {
                            herHisOrIts = "her";
                        }
                        else if (pawn.gender == Gender.Male)
                        {
                            herHisOrIts = "his";
                        }
                        string studyReportHeader = "   " + pawn.Name.ToStringShort + " has finished the study of the strange artifact. "
                        + herHisOrIts.CapitalizeFirst() + " report is quite alarming!\n\n\n"
                        + "### Study references ###\n\n"
                        + "Date: " + GenDate.DateFullStringAt(Find.TickManager.TicksAbs) + "\n"
                        + "Researcher: " + pawn.Name.ToStringFull + "\n\n";
                        terraformer.DisplayStudyReport(studyReportHeader);
                        terraformer.def.label = "Mechanoid terraformer";
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
