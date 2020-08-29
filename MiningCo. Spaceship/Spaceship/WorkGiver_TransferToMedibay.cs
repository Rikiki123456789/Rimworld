using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{
    public class WorkGiver_TransferToMedibay : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.OnCell;
            }
        }
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return ((Find.Selector.IsSelected(pawn) == false)
                || pawn.Map.listerThings.ThingsOfDef(Util_Spaceship.SpaceshipMedical).NullOrEmpty());
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Thing> factionPawnList = new List<Thing>();
            foreach (Pawn factionPawn in pawn.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(pawn.Faction))
            {
                factionPawnList.Add(factionPawn as Thing);
            }
            return factionPawnList;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (forced == false)
            {
                return false;
            }
            Pawn otherPawn = t as Pawn;
            if ((otherPawn == null)
                || (pawn.Faction != otherPawn.Faction)
                || otherPawn.def.race.Animal)
            {
                return false;
            }
            if (Util_Misc.OrbitalHealing.HasAnyTreatableHediff(otherPawn) == false)
            {
                return false;
            }
            foreach (Thing spaceship in pawn.Map.listerThings.ThingsOfDef(Util_Spaceship.SpaceshipMedical))
            {
                Building_SpaceshipMedical medicalSpaceship = spaceship as Building_SpaceshipMedical;
                if ((medicalSpaceship != null)
                    && otherPawn.Downed
                    && pawn.CanReserveAndReach(otherPawn, this.PathEndMode, Danger.Deadly, ignoreOtherReservations: true)
                    && pawn.CanReach(spaceship, this.PathEndMode, Danger.Deadly)
                    && (medicalSpaceship.orbitalHealingPawnsAboardCount < Building_SpaceshipMedical.orbitalHealingPawnsAboardMaxCount)
                    && TradeUtility.ColonyHasEnoughSilver(pawn.Map, Util_Spaceship.orbitalHealingCost))
                {
                    return true;
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // Get nearest reachable medical spaceship.
            float minDistance = 99999f;
            Building_SpaceshipMedical nearestMedicalSpaceship = null;
            foreach (Thing spaceship in pawn.Map.listerThings.ThingsOfDef(Util_Spaceship.SpaceshipMedical))
            {
                float distance = IntVec3Utility.DistanceTo(t.Position, spaceship.Position);
                if ((distance < minDistance)
                    && pawn.CanReach(spaceship, this.PathEndMode, Danger.Deadly))
                {
                    minDistance = distance;
                    nearestMedicalSpaceship = spaceship as Building_SpaceshipMedical;
                }
            }
            Job job = JobMaker.MakeJob(Util_JobDefOf.TransferToMedibay, t, nearestMedicalSpaceship);
            job.count = 1;
            return job;
        }
    }
}
