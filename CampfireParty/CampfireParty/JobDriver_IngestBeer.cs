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
    /// Order a pawn to go to ingest a beer if available.
    /// </summary>
    public class JobDriver_IngestBeer : JobDriver_Pyre
    {
        private Thing Food
        {
            get
            {
                return base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toilsList = new List<Toil>();
            Building_Pyre pyre = this.TargetThingA as Building_Pyre;
            bool beerIsAvailable = false;

            if (this.pawn.Position.InHorDistOf(pyre.Position, Building_Pyre.partyAreaRadius) == false)
            {
                // Go around pyre.
                toilsList.Add(base.ToilGetWanderCell(pyre.Position));
                Find.PawnDestinationManager.ReserveDestinationFor(this.pawn, this.CurJob.targetB.Cell);
                toilsList.Add(Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell));
                // Release cell (the pawn will either go grab a beer or move on the next job).
                toilsList.Add(base.ToilReleaseCell());
            }
            // Look for an available beer.
            List<Thing> list = Find.ListerThings.ThingsOfDef(ThingDefOf.Beer);
            if (list.Count > 0)
            {
                Predicate<Thing> validator = (Thing t) => pawn.CanReserve(t, 1) && !t.IsForbidden(pawn);
                Thing beer = GenClosest.ClosestThing_Global_Reachable(pyre.Position, list, PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), Building_Pyre.beerSearchAreaRadius, validator, null);
                if (beer != null)
                {
                    beerIsAvailable = true;
                    this.CurJob.SetTarget(TargetIndex.A, beer);
                    //this.CurJob.targetA = beer;
                    this.CurJob.maxNumToCarry = 1;
                    toilsList.Add(Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A));
                    toilsList.Add(Toils_Ingest.PickupIngestible(TargetIndex.A, this.pawn)); // TargetIndex.A becomes the carried beer.
                    toilsList.Add(Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, TargetIndex.A));
                    toilsList.Add(Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A));
                    // float durationMultiplier = 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true); // Don't use it so the job duration is nearly the same for all pawns.
                    float durationMultiplier = 1f;
                    toilsList.Add(Toils_Ingest.ChewIngestible(this.pawn, durationMultiplier, TargetIndex.A, TargetIndex.B).FailOn((Toil x) => !this.Food.Spawned && (this.pawn.carrier == null || this.pawn.carrier.CarriedThing != this.Food)));
                    toilsList.Add(Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A));
                }
            }
            // Draw a mote.
            ThingDef moteDef = null;
            if (beerIsAvailable)
            {
                moteDef = Util_CampfireParty.Mote_BeerAvailable;
            }
            else
            {
                moteDef = Util_CampfireParty.Mote_BeerUnavailable;
            }
            MoteDualAttached moteAttached = (MoteDualAttached)ThingMaker.MakeThing(moteDef);
            moteAttached.Attach(this.pawn);
            GenSpawn.Spawn(moteAttached, this.pawn.Position);

            return toilsList;
        }
    }
}
