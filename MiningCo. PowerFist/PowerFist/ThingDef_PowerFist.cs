using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;

namespace PowerFist
{
    public class ThingDef_PowerFist : ThingDef
    {
        public float repelDurationInTicks = 20f;

        // Repel distance is set according to target body size.
        public float bodySizeSmall = 0.5f;
        public float bodySizeMedium = 1.2f;
        public float bodySizeBig = 1.7f;

        public float repelDistanceShort = 1;
        public float repelDistanceMedium = 2;
        public float repelDistanceLong = 3;
        public float repelDistanceFactorWithPowerArmor = 2f;

        public int stunDurationInTicksShort = 100;
        public int stunDurationInTicksMedium = 140;
        public int stunDurationInTicksLong = 180;
        public int empDurationInTicks = 1200;
        
        // Crush damage is applied when a pawn hits an obstacle during repel.
        public float crushDamageFactor = 0.25f;
        public float crushDamageFactorWithPowerArmor = 0.5f;
        // Electric damage is applied to mechanoids.
        public float electricDamageFactor = 0.0f;
        public float electricDamageFactorWithPowerArmor = 0.5f;
    }
}
