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
    /// Simply update the thoughts of a pawn about the party.
    /// </summary>
    public class JobDriver_UpdateThought : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toilsList = new List<Toil>();

            Toil updateThought = new Toil()
            {
                initAction = () =>
                {
                    /*// Look for Thought_HadCampfireParty3 and replace it with Thought_HadCampfireParty4.
                    IEnumerable<Thought> thoughts = this.pawn.needs.mood.thoughts.ThoughtsOfDef(Util_CampfireParty.Thought_HadCampfireParty3);
                    if (thoughts.Count() != 0)
                    {
                        (thoughts.First() as Thought_Memory).age = thoughts.First().def.DurationTicks;
                        this.pawn.needs.mood.thoughts.TryGainThought(Util_CampfireParty.Thought_HadCampfireParty4);
                        return;
                    }
                    // Look for Thought_HadCampfireParty2 and replace it with Thought_HadCampfireParty3.
                    thoughts = this.pawn.needs.mood.thoughts.ThoughtsOfDef(Util_CampfireParty.Thought_HadCampfireParty2);
                    if (thoughts.Count() != 0)
                    {
                        (thoughts.First() as Thought_Memory).age = thoughts.First().def.DurationTicks;
                        this.pawn.needs.mood.thoughts.TryGainThought(Util_CampfireParty.Thought_HadCampfireParty3);
                        return;
                    }
                    // Look for Thought_HadCampfireParty1 and replace it with Thought_HadCampfireParty2.
                    thoughts = this.pawn.needs.mood.thoughts.ThoughtsOfDef(Util_CampfireParty.Thought_HadCampfireParty1);
                    if (thoughts.Count() != 0)
                    {
                        (thoughts.First() as Thought_Memory).age = thoughts.First().def.DurationTicks;
                        this.pawn.needs.mood.thoughts.TryGainThought(Util_CampfireParty.Thought_HadCampfireParty2);
                        return;
                    }
                    // Pawn has no party thought, just add Thought_HadCampfireParty1.
                    this.pawn.needs.mood.thoughts.TryGainThought(Util_CampfireParty.Thought_HadCampfireParty1);*/
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            toilsList.Add(updateThought);

            return toilsList;
        }
    }
}
