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
    /// Order a pawn to go and secure the mechanoid terraformer.
    /// </summary>
    public class JobDriver_SecureStrangeArtifact : JobDriver
    {
        public TargetIndex terraformerIndex = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(terraformerIndex);

            yield return Toils_Goto.GotoCell(terraformerIndex, PathEndMode.InteractionCell).FailOnDestroyed(terraformerIndex);

            yield return Toils_General.Wait(600).FailOnDestroyed(terraformerIndex);

            Toil secureBuildingToil = new Toil()
            {
                initAction = () =>
                {
                    (this.TargetThingA as Building_MechanoidTerraformer).reverseEngineeringState = Building_MechanoidTerraformer.ReverseEngineeringState.Studying;

                    string eventText = "   You have secured the strange building.\n\nYou are now sure of one thing: this is a mechanoid device.\n" +
                    "As usual when dealing with mechanoid technology, be really careful. You should better be prepared for anything...\n";
                    Find.LetterStack.ReceiveLetter("Artifact secured", eventText, LetterType.BadNonUrgent, this.pawn.Position);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return secureBuildingToil;

            yield return Toils_Reserve.Release(terraformerIndex);
        }
    }
}
