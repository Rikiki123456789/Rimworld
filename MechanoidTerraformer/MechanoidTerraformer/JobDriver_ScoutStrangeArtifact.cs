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
    /// Order a pawn to go and scout the mechanoid terraformer.
    /// </summary>
    public class JobDriver_ScoutStrangeArtifact : JobDriver
    {
        public TargetIndex terraformerIndex = TargetIndex.A;
        public const int defensiveRadiusAroundTerraformer = 10;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(terraformerIndex);

            yield return Toils_Goto.GotoCell(terraformerIndex, PathEndMode.InteractionCell).FailOnDestroyed(terraformerIndex);

            yield return Toils_General.Wait(240).FailOnDestroyed(terraformerIndex);

            Toil scytherScoutsArrivalToil = new Toil()
            {
                initAction = () =>
                {
                    IntVec3 spawningCell;
                    List<Pawn> scytherScoutsList = new List<Pawn>();
                    for (int scytherIndex = 0; scytherIndex < 4; scytherIndex++)
                    {
                        bool validDropPodCellIsFound = DropCellFinder.TryFindDropSpotNear(this.TargetThingA.InteractionCell, out spawningCell, true, true);
                        if (validDropPodCellIsFound)
                        {
                            Faction faction = Faction.OfMechanoids;
                            Pawn scytherScout = PawnGenerator.GeneratePawn(PawnKindDef.Named("Scyther"), faction);
                            scytherScoutsList.Add(scytherScout);
                            DropPodUtility.MakeDropPodAt(spawningCell, new DropPodInfo
                            {
                                SingleContainedThing = scytherScout,
                                openDelay = 600,
                                leaveSlag = false
                            });
                        }
                    }
                    StateGraph stateGraph = GraphMaker.MechanoidsDefendShipGraph(this.TargetThingA, defensiveRadiusAroundTerraformer);
                    BrainMaker.MakeNewBrain(Faction.OfMechanoids, stateGraph, scytherScoutsList);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return scytherScoutsArrivalToil;
                        
            Toil pawnEscapingToil = new Toil()
            {
                initAction = () =>
                {
                    (this.TargetThingA as Building_MechanoidTerraformer).reverseEngineeringState = Building_MechanoidTerraformer.ReverseEngineeringState.BuildingNotSecured;

                    ThingRequest thingRequest = new ThingRequest();
                    thingRequest.singleDef = ThingDefOf.CommsConsole;
                    Thing commsConsole = GenClosest.ClosestThingReachable(pawn.Position, thingRequest, PathEndMode.InteractionCell, TraverseParms.For(pawn));
                    if (commsConsole != null)
                    {
                        pawn.pather.StartPath(commsConsole, PathEndMode.InteractionCell);
                    }
                    else
                    {
                        // The player has no comms console. He should move his colonist manually... and fast!
                        pawn.pather.StartPath(pawn.Position, PathEndMode.OnCell);
                    }
                    
                    string herHimOrIt = "it";
                    string sheHeOrIt = "it";
                    if (pawn.gender == Gender.Female)
                    {
                        herHimOrIt = "her";
                        sheHeOrIt = "she";
                    }
                    else if (pawn.gender == Gender.Male)
                    {
                        herHimOrIt = "him";
                        sheHeOrIt = "he";
                    }
                    string eventText = "   " + pawn.Name.ToStringShort + " is just arriving near the strange building when " + sheHeOrIt + " hears the loud noise of incoming drop pods.\n\n"
                        + "You should better take " + herHimOrIt + " to safety... and fast!\n";
                    Find.LetterStack.ReceiveLetter("Drop pods", eventText, LetterType.BadUrgent, this.pawn.Position);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return pawnEscapingToil;

            yield return Toils_Reserve.Release(terraformerIndex);
        }
    }
}
