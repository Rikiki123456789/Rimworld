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
    public static class Util_PawnKindDefOf
    {
        public static PawnKindDef Technician
        {
            get
            {
                return PawnKindDef.Named("MiningCoTechnician");
            }
        }

        public static PawnKindDef Miner
        {
            get
            {
                return PawnKindDef.Named("MiningCoMiner");
            }
        }

        public static PawnKindDef Geologist
        {
            get
            {
                return PawnKindDef.Named("MiningCoGeologist");
            }
        }

        public static PawnKindDef Medic
        {
            get
            {
                return PawnKindDef.Named("MiningCoMedic");
            }
        }

        public static PawnKindDef Pilot
        {
            get
            {
                return PawnKindDef.Named("MiningCoPilot");
            }
        }

        public static PawnKindDef Scout
        {
            get
            {
                return PawnKindDef.Named("MiningCoScout");
            }
        }

        public static PawnKindDef Guard
        {
            get
            {
                return PawnKindDef.Named("MiningCoGuard");
            }
        }

        public static PawnKindDef ShockTrooper
        {
            get
            {
                return PawnKindDef.Named("MiningCoShockTrooper");
            }
        }

        public static PawnKindDef HeavyGuard
        {
            get
            {
                return PawnKindDef.Named("MiningCoHeavyGuard");
            }
        }

        public static PawnKindDef Officer
        {
            get
            {
                return PawnKindDef.Named("MiningCoOfficer");
            }
        }
    }
}
