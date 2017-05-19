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
using Verse.AI.Group;

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

            yield return Toils_Goto.GotoCell(terraformerIndex, PathEndMode.InteractionCell).FailOnDestroyedOrNull(terraformerIndex);

            yield return Toils_General.Wait(240).FailOnDestroyedOrNull(terraformerIndex);

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

                    /*
                    StateGraph stateGraph = GraphMaker.MechanoidsDefendShipGraph(this.TargetThingA, defensiveRadiusAroundTerraformer);
                    BrainMaker.MakeNewBrain(Faction.OfMechanoids, stateGraph, scytherScoutsList);
                    */

                    LordJob_MechanoidsDefendShip lordJob = new LordJob_MechanoidsDefendShip(this.TargetThingA, Faction.OfMechanoids, defensiveRadiusAroundTerraformer, this.TargetThingA.Position);
                    LordMaker.MakeNewLord(Faction.OfMechanoids, lordJob, scytherScoutsList);
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
                    thingRequest.singleDef = ThingDef.Named("CommsConsole");
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
                    
                    string herHimOrIt = "it".Translate();
                    string sheHeOrIt = "it".Translate();
                    if (pawn.gender == Gender.Female)
                    {
                        herHimOrIt = "her".Translate();
                        sheHeOrIt = "she".Translate();
                    }
                    else if (pawn.gender == Gender.Male)
                    {
                        herHimOrIt = "him".Translate();
                        sheHeOrIt = "he".Translate();
                    }
                    string eventText = string.Concat(new string[]
                    {
                        "   ",
                        this.pawn.Name.ToStringShort,
                        "strange_building".Translate(),
                        sheHeOrIt,
                        "hear".Translate(),
                        herHimOrIt,
                        "to_safety".Translate()
                    });

                    Find.LetterStack.ReceiveLetter("Droppods".Translate(), eventText, LetterType.BadUrgent, this.pawn.Position);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return pawnEscapingToil;

            yield return Toils_Reserve.Release(terraformerIndex);
        }
    }
}
