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
    /// Order a pawn to go to a cell around the pyre and play the guitar.
    /// </summary>
    public class JobDriver_PlayTheGuitar : JobDriver_Pyre
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toilsList = new List<Toil>();
            Building_Pyre pyre = this.TargetThingA as Building_Pyre;

            // Get a valid cell to wander on.
            toilsList.Add(base.ToilGetWanderCell(pyre.Position));
            Find.PawnDestinationManager.ReserveDestinationFor(this.pawn, this.CurJob.targetB.Cell);
            toilsList.Add(Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell));
            // Speak/dance with nearby pawn.
            toilsList.Add(GetToilPlayTheGuitar());
            // Release cell.
            toilsList.Add(base.ToilReleaseCell());

            return toilsList;
        }

        protected Toil GetToilPlayTheGuitar()
        {
            int tickCounter = 0;

            Toil toil = new Toil()
            {
                initAction = () =>
                {
                    tickCounter = Rand.Range(35, 50);
                    MoteAttached moteAttached = (MoteAttached)ThingMaker.MakeThing(Util_CampfireParty.Mote_Guitar);
                    moteAttached.AttachTo(this.pawn);
                    GenSpawn.Spawn(moteAttached, this.pawn.Position);
                    this.pawn.Drawer.rotator.FaceCell(this.pawn.Position + new IntVec3(0, 0, -1));
                },
                tickAction = () =>
                {
                    tickCounter--;
                    if (tickCounter <= 0)
                    {
                        tickCounter = Rand.Range(35, 50);
                        MoteThrower.ThrowDrift(this.pawn.Position, Util_CampfireParty.Mote_MusicNote);
                    }
                    // Gain some joy.
                    this.pawn.needs.joy.GainJoy(this.CurJob.def.joyGainRate * 0.000144f, Util_CampfireParty.JoyKindDefOf_Social);
                    this.pawn.Drawer.rotator.FaceCell(this.pawn.Position + new IntVec3(0, 0, -1));
                },
                defaultDuration = 240,
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            return toil;
        }
    }
}
