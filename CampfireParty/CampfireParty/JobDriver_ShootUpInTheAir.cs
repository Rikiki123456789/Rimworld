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
    /// Order a (trigger-happy) pawn to shoot up in the air. This is only an animation, no actual bullet is fired.
    /// </summary>
    public class JobDriver_ShootUpInTheAir : JobDriver_Pyre
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toilsList = new List<Toil>();
            Building_Pyre pyre = this.TargetThingA as Building_Pyre;

            if (this.pawn.equipment == null)
            {
                // Release cell.
                toilsList.Add(base.ToilReleaseCell());
                return toilsList;
            }

            toilsList.Add(base.ToilGetWanderCell(pyre.Position));
            Find.VisibleMap.pawnDestinationManager.ReserveDestinationFor(this.pawn, this.CurJob.targetB.Cell);
            toilsList.Add(Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell));

            // Add toils to shoot up in the air.
            int numberOfShots = Rand.Range(5, 9);
            bool isSlowFiringGun = false;
            if (this.pawn.equipment.Primary != null)
            {
                ThingDef weaponDef = this.pawn.equipment.Primary.def;
                if ((weaponDef == ThingDef.Named("Gun_PumpShotgun"))
                    || (weaponDef == ThingDefOf.Gun_SurvivalRifle))
                {
                    isSlowFiringGun = true;
                }
            }
            for (int shotIndex = 0; shotIndex < numberOfShots; shotIndex++)
            {
                int durationInTicks = Rand.Range(5, 50);
                if (isSlowFiringGun)
                {
                    durationInTicks = Rand.Range(30, 70);
                };
                Toil shootUpInTheAir = new Toil()
                {
                    initAction = () =>
                    {
                        // Check gun has not been dropped since job start.
                        if (this.pawn.equipment.Primary != null)
                        {
                            ThingDef weaponDef = this.pawn.equipment.Primary.def;
                            if ((weaponDef == ThingDefOf.Gun_Pistol)
                                || (weaponDef == ThingDef.Named("Gun_PumpShotgun"))
                                || (weaponDef == ThingDef.Named("Gun_AssaultRifle"))
                                || (weaponDef == ThingDef.Named("Gun_PDW"))
                                || (weaponDef == ThingDef.Named("Gun_HeavySMG"))
                                || (weaponDef == ThingDef.Named("Gun_LMG"))
                                || (weaponDef == ThingDef.Named("Gun_ChargeRifle"))
                                || (weaponDef == ThingDef.Named("Gun_Minigun"))
                                || (weaponDef == ThingDefOf.Gun_SurvivalRifle))
                            {
                                this.pawn.equipment.Primary.def.Verbs.First().soundCast.PlayOneShot(this.pawn.Position);
                            }
                        }
                        else
                        {
                            durationInTicks = 1;
                        }
                    },
                    defaultDuration = durationInTicks,
                    defaultCompleteMode = ToilCompleteMode.Delay
                };
                toilsList.Add(shootUpInTheAir);
            }

            // Release cell.
            toilsList.Add(base.ToilReleaseCell());

            return toilsList;
        }
    }
}
