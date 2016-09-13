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


namespace OutpostGenerator
{
    /// <summary>
    /// Order a pawn to board the supply ship.
    /// </summary>
    public class JobDriver_BoardSupplyShip : JobDriver
    {
        public TargetIndex supplyShip = TargetIndex.A;
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(supplyShip);

            yield return Toils_Goto.GotoCell(supplyShip, PathEndMode.OnCell).FailOnDestroyedOrNull(supplyShip);

            yield return Toils_General.Wait(60).FailOnDestroyedOrNull(supplyShip);

            Toil boardToil = new Toil()
            {
                initAction = () =>
                {
                    this.GetActor().Destroy();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return boardToil;

            yield return Toils_Reserve.Release(supplyShip);
        }
    }
}
