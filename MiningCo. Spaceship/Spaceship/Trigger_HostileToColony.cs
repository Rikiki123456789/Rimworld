using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;    // Always needed
using RimWorld;       // RimWorld specific functions are found here
using Verse;          // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI
using Verse.AI.Group; // Needed when you do something with the AI
using Verse.Sound;    // Needed when you do something with the Sound

namespace Spaceship
{
    public class Trigger_HostileToColony : Trigger
    {
		public const int checkInterval = GenTicks.TicksPerRealSecond + 2;

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
            if ((signal.type == TriggerSignalType.Tick)
                && (Find.TickManager.TicksGame % checkInterval == 0))
            {
                if (lord.faction.HostileTo(Faction.OfPlayer))
                {
                    return true;
                }
            }
			return false;
		}
    }
}
