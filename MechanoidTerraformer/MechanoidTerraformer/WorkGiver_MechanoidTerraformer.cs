using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace MechanoidTerraformer
{
    /// <summary>
    /// WorkGiver_MechanoidTerraformer class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_MechanoidTerraformer: WorkGiver_Scanner
    {
        PathEndMode pathEndMode = PathEndMode.InteractionCell;

		public override ThingRequest PotentialWorkThingRequest
		{
			get
            {
                return ThingRequest.ForDef(Util_MechanoidTerraformer.MechanoidTerraformerDef);
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{

            if ((t is Building_MechanoidTerraformer) == false)
            {
                return false;
            }
                
            Building_MechanoidTerraformer terraformer = t as Building_MechanoidTerraformer;
            
            if (pawn.Dead
                || pawn.Downed
                || pawn.IsBurning())
            {
                return false;
            }
            if ((terraformer.reverseEngineeringState == Building_MechanoidTerraformer.ReverseEngineeringState.Studying)
                && (pawn.skills.GetSkill(SkillDefOf.Research).level >= Building_MechanoidTerraformer.minResearchLevelToStudyArtifact)
                && pawn.CanReserveAndReach(terraformer, this.pathEndMode, Danger.Some)
                && (terraformer.studyIsPaused == false)
                && (terraformer.studyCounter < Building_MechanoidTerraformer.studyCounterTargetValue))
            {
                return true;
            }
            else if ((terraformer.reverseEngineeringState == Building_MechanoidTerraformer.ReverseEngineeringState.ReroutingPower)
                && (pawn.skills.GetSkill(SkillDefOf.Research).level >= Building_MechanoidTerraformer.minResearchLevelToReroutePower)
                && pawn.CanReserveAndReach(terraformer, this.pathEndMode, Danger.Some)
                && (terraformer.reroutingIsPaused == false)
                && (terraformer.reroutingCounter < Building_MechanoidTerraformer.reroutingCounterTargetValue))
            {
                return true;
            }
            else if ((terraformer.reverseEngineeringState == Building_MechanoidTerraformer.ReverseEngineeringState.ExtractingWeatherController)
                && (pawn.skills.GetSkill(SkillDefOf.Research).level >= Building_MechanoidTerraformer.minResearchLevelToExtractWeatherController)
                && pawn.CanReserveAndReach(terraformer, this.pathEndMode, Danger.Some)
                && (terraformer.extractionIsPaused == false)
                && (terraformer.extractionCounter < Building_MechanoidTerraformer.extractionCounterTargetValue))
            {
                return true;
            }
            return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
            Job job = new Job();
            Building_MechanoidTerraformer terraformer = t as Building_MechanoidTerraformer;

            switch (terraformer.reverseEngineeringState)
            {
                case Building_MechanoidTerraformer.ReverseEngineeringState.Studying:
                    job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_StudyStrangeArtifact), terraformer);
                    break;
                case Building_MechanoidTerraformer.ReverseEngineeringState.ReroutingPower:
                    job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_ReroutePower), terraformer);
                    break;
                case Building_MechanoidTerraformer.ReverseEngineeringState.ExtractingWeatherController:
                    job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_ExtractWeatherController), terraformer);
                    break;
            }
            return job;
		}
    }
}
