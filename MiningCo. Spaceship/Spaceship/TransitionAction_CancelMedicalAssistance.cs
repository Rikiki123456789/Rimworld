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
    public class TransitionAction_CancelMedicalAssistance : TransitionAction
    {
        public override void DoAction(Transition trans)
        {
            Lord lord = trans.target.lord;
            foreach (Thing thing in (lord.LordJob as LordJob_MiningCoBase).targetDestination.GetThingList(lord.Map))
            {
                if (thing is Building_SpaceshipMedical)
                {
                    Building_SpaceshipMedical spaceship = thing as Building_SpaceshipMedical;
                    spaceship.RequestTakeOff();
                    break;
                }
            }
        }
    }
}
