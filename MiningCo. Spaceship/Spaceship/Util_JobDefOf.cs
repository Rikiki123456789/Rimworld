using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;

namespace Spaceship
{
    public static class Util_JobDefOf
    {
        public static JobDef UseOrbitalRelayConsole
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_UseOrbitalRelayConsole");
            }
        }

        public static JobDef TradeWithCargoSpaceship
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_TradeWithCargoSpaceship");
            }
        }

        public static JobDef RequestSpaceshipTakeOff
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_RequestSpaceshipTakeOff");
            }
        }

        public static JobDef BoardSpaceship
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_BoardSpaceship");
            }
        }

        public static JobDef CarryDownedPawn
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_CarryDownedPawn");
            }
        }

        public static JobDef BoardMedicalSpaceship
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_BoardMedicalSpaceship");
            }
        }

        public static JobDef TransferToMedibay
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_TransferToMedibay");
            }
        }

    }
}
