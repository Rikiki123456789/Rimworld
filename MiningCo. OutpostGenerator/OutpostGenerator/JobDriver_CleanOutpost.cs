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
    /// Order a pawn to clean some filth.
    /// </summary>
    public class JobDriver_CleanOutpost : JobDriver
    {
        public TargetIndex filthIndex = TargetIndex.A;

        private float cleaningWorkDone;
        private float totalCleaningWorkDone;
        private float totalCleaningWorkRequired = 0;

        private Filth Filth
        {
            get
            {
                return (Filth)base.CurJob.GetTarget(filthIndex).Thing;
            }
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(filthIndex);

            yield return Toils_Reserve.Reserve(filthIndex, 1);

            yield return Toils_Goto.GotoThing(filthIndex, PathEndMode.Touch);

            Toil openCasketToil = new Toil()
            {
                initAction = () =>
                {
                    this.totalCleaningWorkRequired = this.Filth.def.filth.cleaningWorkToReduceThickness * (float)this.Filth.thickness;
                },
                tickAction = () =>
                {
                    Filth filth = this.Filth;
                    this.cleaningWorkDone += 1f;
                    this.totalCleaningWorkDone += 1f;
                    if (this.cleaningWorkDone > filth.def.filth.cleaningWorkToReduceThickness)
                    {
                        filth.ThinFilth();
                        this.cleaningWorkDone = 0f;
                        if (filth.Destroyed)
                        {
                            this.GetActor().records.Increment(RecordDefOf.MessesCleaned);
                            this.ReadyForNextToil();
                            return;
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never
            }.WithEffect("Clean", filthIndex).WithProgressBar(filthIndex, () => this.totalCleaningWorkDone / this.totalCleaningWorkRequired, true, -0.5f).PlaySustainerOrSound(SoundDefOf.Interact_CleanFilth);
            yield return openCasketToil;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<float>(ref this.cleaningWorkDone, "cleaningWorkDone", 0f, false);
            Scribe_Values.LookValue<float>(ref this.totalCleaningWorkDone, "totalCleaningWorkDone", 0f, false);
            Scribe_Values.LookValue<float>(ref this.totalCleaningWorkRequired, "totalCleaningWorkRequired", 0f, false);
        }
    }
}
