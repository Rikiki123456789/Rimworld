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
    /// Order a pawn to go to a cell around the pyre and dance.
    /// </summary>
    public class JobDriver_Dance : JobDriver_Pyre
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toilsList = new List<Toil>();
            Building_Pyre pyre = this.TargetThingA as Building_Pyre;

            // Get a valid cell to wander on.
            toilsList.Add(base.ToilGetWanderCell(pyre.Position));
            Find.VisibleMap.pawnDestinationManager.ReserveDestinationFor(this.pawn, this.CurJob.targetB.Cell);
            toilsList.Add(Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell));
            // Speak/dance with nearby pawn.
            toilsList.Add(GetToilDance());
            // Release cell.
            toilsList.Add(base.ToilReleaseCell());

            return toilsList;
        }

        private Toil GetToilDance()
        {
            int tickCounter = 0;
            IntVec3 facingCell = this.TargetLocA;

            Toil toil = new Toil()
            {
                initAction = () =>
                {
                    tickCounter = Rand.Range(15, 35);
                },
                tickAction = () =>
                {
                    // Turn in random direction.
                    tickCounter--;
                    if (tickCounter <= 0)
                    {
                        tickCounter = Rand.Range(15, 35);
                        Rot4 rotation = Rot4.North;
                        if (Rand.Value < 0.5f)
                        {
                            rotation = new Rot4(this.pawn.Rotation.AsInt + 1);
                        }
                        else
                        {
                            rotation = new Rot4(this.pawn.Rotation.AsInt + 3);
                        }
                        facingCell = this.pawn.Position + new IntVec3(0, 0, 1).RotatedBy(rotation);
                    }
                    this.pawn.Drawer.rotator.FaceCell(facingCell);
                    // Gain some joy.
                    this.pawn.needs.joy.GainJoy(this.CurJob.def.joyGainRate * 0.000144f, Util_CampfireParty.JoyKindDefOf_Social);
                },
                defaultDuration = Rand.Range(180, 300),
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            return toil;
        }
    }
}
