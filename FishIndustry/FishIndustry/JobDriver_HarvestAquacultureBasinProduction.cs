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
//using RimWorld.SquadAI;


namespace FishIndustry
{
    /// <summary>
    /// Order a pawn to go and harvest aquaculture basin's production.
    /// </summary>
    public class JobDriver_HarvestAquacultureBasinProduction : JobDriver
    {
        public TargetIndex aquacultureBasinIndex = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(aquacultureBasinIndex);

            Building_AquacultureBasin aquacultureBasin = this.TargetThingA as Building_AquacultureBasin;
            yield return Toils_Goto.GotoThing(aquacultureBasinIndex, PathEndMode.InteractionCell);

            yield return Toils_General.Wait(120).WithProgressBarToilDelay(aquacultureBasinIndex);

            Toil getAquacultureBasinProduction = new Toil()
            {
                initAction = () =>
                {
                    Job curJob = this.pawn.jobs.curJob;

                    Thing product = aquacultureBasin.GetProduction();
                    if (product == null)
                    {
                        this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                    }
                    else
                    {
                        GenSpawn.Spawn(product, aquacultureBasin.InteractionCell, this.Map);

                        IntVec3 storageCell;
                        if (StoreUtility.TryFindBestBetterStoreCellFor(product, this.pawn, this.Map, StoragePriority.Unstored, this.pawn.Faction, out storageCell, true))
                        {
                            this.pawn.Reserve(product, 1);
                            this.pawn.Reserve(storageCell, 1);
                            this.pawn.CurJob.SetTarget(TargetIndex.B, storageCell);
                            this.pawn.CurJob.SetTarget(TargetIndex.A, product);
                            this.pawn.CurJob.count = 99999;
                            this.pawn.CurJob.haulMode = HaulMode.ToCellStorage;
                        }
                        else
                        {
                            this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                        }
                    }
                }
            };
            yield return getAquacultureBasinProduction;
            
            yield return Toils_Reserve.Release(aquacultureBasinIndex);

            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            yield return carryToCell;

            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
        }
    }
}
