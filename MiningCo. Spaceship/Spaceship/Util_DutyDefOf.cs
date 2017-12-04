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

namespace Spaceship
{
    public static class Util_DutyDefOf
    {
        public static DutyDef DutyBoardSpaceship
        {
            get
            {
                return DefDatabase<DutyDef>.GetNamed("DutyDef_BoardSpaceship");
            }
        }
        public static DutyDef CarryDownedPawn
        {
            get
            {
                return DefDatabase<DutyDef>.GetNamed("DutyDef_CarryDownedPawn");
            }
        }
        public static DutyDef EscortCarrier
        {
            get
            {
                return DefDatabase<DutyDef>.GetNamed("DutyDef_EscortCarrier");
            }
        }
        public static DutyDef HealColonists
        {
            get
            {
                return DefDatabase<DutyDef>.GetNamed("DutyDef_HealColonists");
            }
        }
    }
}
