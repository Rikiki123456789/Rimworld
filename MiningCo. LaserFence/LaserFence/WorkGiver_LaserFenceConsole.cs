using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace LaserFence
{
    /// <summary>
    /// WorkGiver_LaserFenceConsole class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class WorkGiver_LaserFenceConsole : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }

        public override ThingRequest PotentialWorkThingRequest
		{
			get
            {
                return ThingRequest.ForDef(Util_LaserFence.LaserFenceConsoleDef);
			}
		}

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            Building_LaserFenceConsole console = t as Building_LaserFenceConsole;
            if (console == null)
            {
                return false;
            }
            if (console.IsForbidden(pawn)
                || console.IsBurning()
                || (console.manualSwitchIsPending == false)
                || (pawn.CanReserveAndReach(console, this.PathEndMode, pawn.NormalMaxDanger()) == false)
                || (console.Map.designationManager.DesignationOn(console, DesignationDefOf.Uninstall) != null))
            {
                return false;
            }
            CompPowerTrader compPowerTrader = console.TryGetComp<CompPowerTrader>();
            if ((compPowerTrader != null)
                && (compPowerTrader.PowerOn == false))
            {
                return false;
            }
            return true;
		}
        
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_LaserFenceConsole console = t as Building_LaserFenceConsole;
            return JobMaker.MakeJob(Util_LaserFence.SwitchLaserFenceConsoleDef, console);
		}
    }
}
