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
    public enum SpaceshipKind
    {
        CargoPeriodic,
        CargoRequested,
        Damaged,
        DispatcherDrop,
        DispatcherPick,
        Medical,
        Airstrike
    }
}
