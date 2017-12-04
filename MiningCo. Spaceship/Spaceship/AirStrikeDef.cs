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
    public class AirStrikeDef : Def
    {
        public const int maxWeapons = 3;

        public int runsNumber = 1;
        public int costInSilver = 500;
        public float ammoResupplyDays = 1;

        public float cellsTravelledPerTick = 0.25f;
        public int ticksBeforeOverflightInitialValue = 10 * GenTicks.TicksPerRealSecond;
        public int ticksBeforeOverflightPlaySound = 4 * GenTicks.TicksPerRealSecond;
        public int ticksBeforeOverflightReducedSpeed = 4 * GenTicks.TicksPerRealSecond;
        public int ticksAfterOverflightReducedSpeed = 0;
        public int ticksAfterOverflightFinalValue = 10 * GenTicks.TicksPerRealSecond;

        public List<WeaponDef> weapons = new List<WeaponDef>();
    }
}
