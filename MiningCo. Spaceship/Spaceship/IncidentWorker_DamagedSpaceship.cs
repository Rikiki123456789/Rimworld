using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{
    public class IncidentWorker_DamagedSpaceship : IncidentWorker
    {
        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            if (base.CanFireNowSub(target) == false)
            {
                return false;
            }
            if (Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer))
            {
                return false;
            }
            Map map = (Map)target;
            List<Building_LandingPad> freeLandingPads = Util_LandingPad.GetAllFreeLandingPads(map);
            if (freeLandingPads != null)
            {
                return true;
            }
            return false;
        }
        
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Building_LandingPad> freeLandingPads = Util_LandingPad.GetAllFreeLandingPads(map);
            if (freeLandingPads == null)
            {
                // Should not happen if CanFireNowSub returned true.
                return false;
            }
            Building_LandingPad landingPad = freeLandingPads.RandomElement();

            // Spawn landing damaged spaceship.
            FlyingSpaceshipLanding damagedSpaceship = Util_Spaceship.SpawnSpaceship(landingPad, SpaceshipKind.Damaged);
            damagedSpaceship.HitPoints = Mathf.RoundToInt(Rand.Range(0.15f, 0.45f) * damagedSpaceship.HitPoints);
            string letterText = "-- Comlink with MiningCo. --\n\n"
                + "MiningCo. pilot:\n\n"
                + "\"Hello partner!\n"
                + "Our ship is damaged and we need some repairs before going back to the orbital station.\n"
                + "Help us and we will reward you. Business as usual!\n\n"
                + "-- End of transmission --\n\n"
                + "WARNING! Not helping the ship will negatively impact your partnership with MiningCo..";
            Find.LetterStack.ReceiveLetter("Repairs request", letterText, LetterDefOf.NeutralEvent, new TargetInfo(landingPad.Position, landingPad.Map));
            return true;
        }
    }
}
