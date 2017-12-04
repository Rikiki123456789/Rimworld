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
    public static class Util_TraderKindDefOf
    {
        public static TraderKindDef spaceshipCargoPeriodicSupply
        {
            get
            {
                return DefDatabase<TraderKindDef>.GetNamed("SpaceshipCargoPeriodicSupply");
            }
        }

        public static TraderKindDef spaceshipCargoRequestedSupply
        {
            get
            {
                return DefDatabase<TraderKindDef>.GetNamed("SpaceshipCargoRequestedSupply");
            }
        }

        public static TraderKindDef spaceshipCargoDamaged
        {
            get
            {
                return DefDatabase<TraderKindDef>.GetNamed("SpaceshipCargoDamaged");
            }
        }

        public static TraderKindDef spaceshipCargoDispatcher
        {
            get
            {
                return DefDatabase<TraderKindDef>.GetNamed("SpaceshipCargoDispatcher");
            }
        }

        public static TraderKindDef spaceshipCargoMedical
        {
            get
            {
                return DefDatabase<TraderKindDef>.GetNamed("SpaceshipCargoMedical");
            }
        }
    }
}
