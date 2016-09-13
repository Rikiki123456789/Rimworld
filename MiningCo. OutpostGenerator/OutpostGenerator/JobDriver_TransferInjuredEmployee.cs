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
    /// Order a pawn to go and open a cryptosleep casket containing an injured MiningCo. employee.
    /// </summary>
    public class JobDriver_TransferInjuredEmployee : JobDriver
    {
        public TargetIndex casketTarget = TargetIndex.A;
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(casketTarget);

            yield return Toils_Goto.GotoCell(casketTarget, PathEndMode.InteractionCell).FailOnDestroyedOrNull(casketTarget);

            yield return Toils_General.Wait(300).FailOnDestroyedOrNull(casketTarget);

            Toil openCasketToil = new Toil()
            {
                initAction = () =>
                {
                    Building_CryptosleepCasket casket = this.CurJob.targetA.Thing as Building_CryptosleepCasket;
                    if (casket != null)
                    {
                        casket.Open();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return openCasketToil;

            yield return Toils_Reserve.Release(casketTarget);
        }
    }
}
