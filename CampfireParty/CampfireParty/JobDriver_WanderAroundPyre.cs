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
    /// Order a pawn to go to a cell around the pyre and talk to a nearby pawn.
    /// </summary>
    public class JobDriver_WanderAroundPyre : JobDriver_Pyre
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toilsList = new List<Toil>();
            Building_Pyre pyre = this.TargetThingA as Building_Pyre;

            // Get a valid cell to wander on.
            toilsList.Add(base.ToilGetWanderCell(pyre.Position));
            Find.PawnDestinationManager.ReserveDestinationFor(this.pawn, this.CurJob.targetB.Cell);
            toilsList.Add(Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell));
            // Talk with nearby pawn.
            toilsList.Add(GetToilTalkWithNearbyPawn());
            // Release cell.
            toilsList.Add(base.ToilReleaseCell());

            return toilsList;
        }

        protected Toil GetToilTalkWithNearbyPawn()
        {
            int tickCounter = 0;
            IntVec3 facingCell = this.TargetLocA;
            Pawn facingPawn = null;

            Toil toil = new Toil()
            {
                initAction = () =>
                {
                    tickCounter = (int)GenTicks.TicksPerRealSecond;
                    GetFacingCell(ref facingPawn, ref facingCell);
                },
                tickAction = () =>
                {
                    // Only refresh facing direction once per second.
                    tickCounter--;
                    if (tickCounter <= 0)
                    {
                        tickCounter = (int)GenTicks.TicksPerRealSecond;
                        if ((facingPawn == null)
                            || (facingPawn.Position.InHorDistOf(this.pawn.Position, 2f) == false))
                        {
                            GetFacingCell(ref facingPawn, ref facingCell);
                        }
                    }
                    // Face nearby pawn or cell.
                    if ((facingPawn != null)
                        && (facingPawn.Destroyed == false))
                    {
                        this.pawn.Drawer.rotator.FaceCell(facingPawn.Position);
                    }
                    else
                    {
                        this.pawn.Drawer.rotator.FaceCell(facingCell);
                    }
                    // Gain some joy.
                    this.pawn.needs.joy.GainJoy(this.CurJob.def.joyGainRate * 0.000144f, Util_CampfireParty.JoyKindDefOf_Social);
                },
                defaultDuration = Rand.Range(180, 300),
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            return toil;
        }

        protected void GetFacingCell(ref Pawn facingPawn, ref IntVec3 facingCell)
        {
            List<Pawn> nearbyPawns = new List<Pawn>();
            foreach (Pawn colonist in Find.MapPawns.FreeColonists)
            {
                if (colonist == this.pawn)
                {
                    continue;
                }
                if (colonist.Position.InHorDistOf(this.pawn.Position, 2f))
                {
                    nearbyPawns.Add(colonist);
                }
            }
            if (nearbyPawns.NullOrEmpty())
            {
                facingPawn = null;
                facingCell = this.pawn.Position + new IntVec3(0, 0, 1).RotatedBy(new Rot4(Rand.Range(0, 4)));
            }
            else
            {
                facingPawn = nearbyPawns.RandomElement();
                facingCell = facingPawn.Position;
            }
        }
    }
}
