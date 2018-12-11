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
    public class Trigger_SpaceshipNotFound : Trigger
    {
		public const int checkInterval = GenTicks.TicksPerRealSecond + 4;

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
            if ((signal.type == TriggerSignalType.Tick)
                && (Find.TickManager.TicksGame % checkInterval == 0))
            {
                foreach (Thing thing in (lord.LordJob as LordJob_MiningCoBase).targetDestination.GetThingList(lord.Map))
                {
                    if (thing is Building_Spaceship)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
		}
    }
}
