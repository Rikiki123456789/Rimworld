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
    /// Order a (nudist) pawn to drop all worn apparel! :D
    /// </summary>
    public class JobDriver_DropClothes : JobDriver_Pyre
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toilsList = new List<Toil>();
            Building_Pyre pyre = this.TargetThingA as Building_Pyre;
            IntVec3 memorizedFacingCell = Find.Map.Center;

            // Go around pyre.
            toilsList.Add(base.ToilGetWanderCell(pyre.Position));
            Find.PawnDestinationManager.ReserveDestinationFor(this.pawn, this.CurJob.targetB.Cell);
            toilsList.Add(Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell));
            // Add toils to drops all worn apparels.
            if (this.pawn.apparel != null && this.pawn.apparel.WornApparelCount > 0)
            {
                for (int apparelIndex = 0; apparelIndex < this.pawn.apparel.WornApparelCount; apparelIndex++)
                {
                    Toil dropApparel = new Toil()
                    {
                        initAction = () =>
                        {
                            if (this.pawn.apparel != null && this.pawn.apparel.WornApparelCount > 0)
                            {
                                Apparel apparel;
                                this.pawn.apparel.TryDrop(this.pawn.apparel.WornApparel.RandomElement<Apparel>(), out apparel);
                            }
                        },
                        defaultCompleteMode = ToilCompleteMode.Instant
                    };
                    toilsList.Add(dropApparel);
                    Toil waitFacingRandomCell = new Toil()
                    {
                        initAction = () =>
                        {
                            memorizedFacingCell = this.pawn.Position + new IntVec3(0, 0, 1).RotatedBy(new Rot4(Rand.Range(0, 4)));
                        },
                        tickAction = () =>
                        {
                            this.pawn.Drawer.rotator.FaceCell(memorizedFacingCell);
                        },
                        defaultDuration = 150,
                        defaultCompleteMode = ToilCompleteMode.Delay
                    };
                    toilsList.Add(waitFacingRandomCell);
                }
            }
            // Release cell.
            toilsList.Add(base.ToilReleaseCell());

            return toilsList;
        }
    }
}
