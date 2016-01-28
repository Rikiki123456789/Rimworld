using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound
using RimWorld;      // RimWorld specific functions are found here
//using RimWorld.SquadAI;

namespace CampfireParty
{
    /// <summary>
    /// Generate a pseudo-random sequence of jobs for a campfire party.
    /// </summary>
    public class JobDriver_StartCampfireParty : JobDriver_Pyre
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            const int partyParts = 4;
            const int jobsPerPart = 3;
            const int maxTries = 3;

            List<PartyJobType> partyJobsType = new List<PartyJobType>();
            List<Toil> partyToils = new List<Toil>();
            Building_Pyre pyre = this.TargetThingA as Building_Pyre;

            bool revelerIsPsychopath = this.pawn.story.traits.HasTrait(TraitDef.Named("Psychopath"));
            bool revelerIsNudist = this.pawn.story.traits.HasTrait(TraitDefOf.Nudist);
            bool revelerHasTriggerHappyTrait = this.pawn.story.traits.HasTrait(TraitDef.Named("TriggerHappy"));
            int revelerAlcoholAddictionLevel = this.pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
            
            // Psychopaths don't like having parties with others... 3:p
            if (revelerIsPsychopath)
            {
                partyToils.Add(Toils_Goto.GotoCell(pyreIndex, PathEndMode.ClosestTouch));
                Toil toil = new Toil()
                {
                    tickAction = () =>
                    {
                        this.pawn.drawer.rotator.FaceCell(pyre.Position);
                    },
                    defaultDuration = 300,
                    defaultCompleteMode = ToilCompleteMode.Delay
                };
                partyToils.Add(toil);
                pawn.needs.mood.thoughts.TryGainThought(Util_CampfireParty.Thought_HadCampfirePartyPsychopaths);
                return partyToils;
            }

            // Initialize party jobs type.
            for (int jobTypeIndex = 0; jobTypeIndex < partyParts * jobsPerPart; jobTypeIndex++)
            {
                partyJobsType.Add(PartyJobType.Undefined);
            }

            // If the colonist is a nudist, add a drop clothes job in 1st, 2nd or 3rd part.
            if (revelerIsNudist)
            {
                for (int tries = 0; tries < maxTries; tries++)
                {
                    int jobIndex = Rand.Range(jobsPerPart, 3 * jobsPerPart) + tries;
                    if (jobIndex >= partyParts * jobsPerPart)
                    {
                        jobIndex -= jobsPerPart;
                    }
                    if (partyJobsType[jobIndex] == PartyJobType.Undefined)
                    {
                        partyJobsType[jobIndex] = PartyJobType.DropClothes;
                        break;
                    }
                }
            }

            // If the colonist has the trigger-happy trait, add a shoot in the air job in 3nd or 4th part.
            if (revelerHasTriggerHappyTrait)
            {
                for (int tries = 0; tries < maxTries; tries++)
                {
                    int jobIndex = Rand.Range(2 * jobsPerPart, 4 * jobsPerPart) + tries;
                    if (jobIndex >= partyParts * jobsPerPart)
                    {
                        jobIndex -= jobsPerPart;
                    }
                    if (partyJobsType[jobIndex] == PartyJobType.Undefined)
                    {
                        partyJobsType[jobIndex] = PartyJobType.ShootUpInTheAir;
                        break;
                    }
                }
            }
            
            // Add a drink beer job for each level of addiction.
            if (revelerAlcoholAddictionLevel >= 0)
            {
                for (int beerIndex = 0; beerIndex < 2 * (revelerAlcoholAddictionLevel + 1); beerIndex++)
                {
                    for (int tries = 0; tries < maxTries; tries++)
                    {
                        int jobIndex = Rand.Range(0, partyParts * jobsPerPart) + tries;
                        if (jobIndex >= partyParts * jobsPerPart)
                        {
                            jobIndex -= jobsPerPart;
                        }
                        if (partyJobsType[jobIndex] == PartyJobType.Undefined)
                        {
                            partyJobsType[jobIndex] = PartyJobType.DrinkBeer;
                            break;
                        }
                    }
                }
            }

            // Convert all remaining undefined jobs into wander/play the guitar/dance jobs.
            for (int jobIndex = 0; jobIndex < partyParts * jobsPerPart; jobIndex++)
            {
                if (partyJobsType[jobIndex] == PartyJobType.Undefined)
                {
                    float jobSelector = Rand.Value;
                    if (jobSelector < 0.2f)
                    {
                        partyJobsType[jobIndex] = PartyJobType.PlayTheGuitar;
                    }
                    else if (jobSelector < 0.5f)
                    {
                        partyJobsType[jobIndex] = PartyJobType.Dance;
                    }
                    else
                    {
                        partyJobsType[jobIndex] = PartyJobType.WanderAroundPyre;
                    }
                }
            }
            // Debug: display generated jobs sequence.
            /*for (int jobIndex = 0; jobIndex < partyParts * jobsPerPart; jobIndex++)
            {
                Log.Message("partyJobsType[" + jobIndex + "] = " + partyJobsType[jobIndex]);
            }*/

            // Generate actual toils.
            if (this.pawn.jobs.jobQueue == null)
            {
                // This case will only happen for a pawn who has never queued jobs before.
                this.pawn.jobs.jobQueue = new JobQueue();
            }

            for (int jobIndex = 0; jobIndex < partyParts * jobsPerPart; jobIndex++)
            {
                switch (partyJobsType[jobIndex])
                {
                    case PartyJobType.WanderAroundPyre:
                        // TODO: not used to generate job???
                        break;

                    case PartyJobType.PlayTheGuitar:
                        this.pawn.jobs.jobQueue.EnqueueLast(new Job(Util_CampfireParty.Job_PlayTheGuitar, pyre));
                        break;

                    case PartyJobType.Dance:
                        this.pawn.jobs.jobQueue.EnqueueLast(new Job(Util_CampfireParty.Job_Dance, pyre));
                        break;

                    case PartyJobType.DrinkBeer:
                        this.pawn.jobs.jobQueue.EnqueueLast(new Job(Util_CampfireParty.Job_IngestBeer, pyre));
                        break;

                    case PartyJobType.DropClothes:
                        this.pawn.jobs.jobQueue.EnqueueLast(new Job(Util_CampfireParty.Job_DropClothes, pyre));
                        break;

                    case PartyJobType.ShootUpInTheAir:
                        this.pawn.jobs.jobQueue.EnqueueLast(new Job(Util_CampfireParty.Job_ShootUpinTheAir, pyre));
                        break;
                }
                if ((jobIndex % jobsPerPart) == (jobsPerPart - 1))
                {
                    this.pawn.jobs.jobQueue.EnqueueLast(new Job(Util_CampfireParty.Job_UpdateThought));
                }
            }
            return partyToils;
        }
    }
}
