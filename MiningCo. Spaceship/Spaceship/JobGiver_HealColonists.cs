using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship
{
    public class JobGiver_HealColonists : ThinkNode_JobGiver
	{
		protected int jobMaxDuration = 999999;
		public override ThinkNode DeepCopy(bool resolve = true)
		{
            JobGiver_HealColonists jobGiver_HealColonists = (JobGiver_HealColonists)base.DeepCopy(resolve);
            jobGiver_HealColonists.jobMaxDuration = this.jobMaxDuration;
            return jobGiver_HealColonists;
		}
		protected override Job TryGiveJob(Pawn pawn)
		{
            Pawn patient = GetTendableColonist(pawn.Position, pawn.Map);
            if (patient != null)
            {
                Thing medicine = null;
                if (Medicine.GetMedicineCountToFullyHeal(patient) > 0)
                {
                    medicine = FindBestUnforbiddenMedicine(pawn, patient);
                }
                if (medicine != null)
                {
                    return JobMaker.MakeJob(JobDefOf.TendPatient, patient, medicine);
                }
                return JobMaker.MakeJob(JobDefOf.TendPatient, patient);
            }
            else
            {
                Lord lord = pawn.GetLord();
                if (lord != null)
                {
                    lord.ReceiveMemo("HealFinished");
                }
            }
            return null;
		}

        public static Pawn GetTendableColonist(IntVec3 medicPosition, Map medicMap)
        {
            foreach (Pawn colonist in medicMap.mapPawns.FreeColonistsSpawned.InRandomOrder())
            {
                if (colonist.health.HasHediffsNeedingTend()
                    && (medicMap.reservationManager.IsReservedByAnyoneOf(colonist, Faction.OfPlayer) == false)
                    && (medicMap.reservationManager.IsReservedByAnyoneOf(colonist, Util_Faction.MiningCoFaction) == false)
                    && colonist.InBed()
                    && medicMap.reachability.CanReach(medicPosition, colonist, PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors)))
                {
                    return colonist;
                }
            }
            return null;
        }

        // Just a rewrite of the HealthAIUtility.FindBestMedicine function to avoid using forbidden medicine.
        public static Thing FindBestUnforbiddenMedicine(Pawn healer, Pawn patient)
        {
            Thing result;
            if (patient.playerSettings == null || patient.playerSettings.medCare <= MedicalCareCategory.NoMeds)
            {
                result = null;
            }
            else
            {
                Predicate <Thing> predicate = delegate(Thing medicine)
                {
                    return !medicine.IsForbidden(Faction.OfPlayer) && patient.playerSettings.medCare.AllowsMedicine(medicine.def) && healer.CanReserve(medicine);
                };
                Func<Thing, float> priorityGetter = (Thing t) => t.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
                IntVec3 position = patient.Position;
                List<Thing> searchSet = patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
                TraverseParms traverseParams = TraverseParms.For(healer, Danger.Some, TraverseMode.ByPawn, false);
                result = GenClosest.ClosestThing_Global_Reachable(position, patient.Map, searchSet, PathEndMode.ClosestTouch, traverseParams, 100f, predicate, priorityGetter);
            }
            return result;
        }
    }
}
