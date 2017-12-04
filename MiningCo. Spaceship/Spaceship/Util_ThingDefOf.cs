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
    public static class Util_ThingDefOf
    {
        // Orbital relay.
        public static ThingDef OrbitalRelay
        {
            get
            {
                return ThingDef.Named("OrbitalRelay");
            }
        }

        // Landing pad.
        public static ThingDef LandingPad
        {
            get
            {
                return ThingDef.Named("LandingPad");
            }
        }

        public static ThingDef LandingPadBeacon
        {
            get
            {
                return ThingDef.Named("LandingPadBeacon");
            }
        }

        public static ThingDef LandingPadBeaconGlowerRed
        {
            get
            {
                return ThingDef.Named("LandingPadBeaconGlowerRed");
            }
        }

        public static ThingDef LandingPadBeaconGlowerGreen
        {
            get
            {
                return ThingDef.Named("LandingPadBeaconGlowerGreen");
            }
        }    
    
        public static ThingDef LandingPadBeaconGlowerWhite
        {
            get
            {
                return ThingDef.Named("LandingPadBeaconGlowerWhite");
            }
        }

        // Air strike beacon.
        public static ThingDef AirStrikeBeacon
        {
            get
            {
                return ThingDef.Named("AirStrikeBeacon");
            }
        }

        // Vulcan turret.
        public static ThingDef VulcanTurret
        {
            get
            {
                return ThingDef.Named("VulcanTurret");
            }
        }
    }
}
