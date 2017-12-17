using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;

namespace LaserFence
{
    public static class Util_LaserFence
    {
        // Laser fence pylon.
        public static ThingDef LaserFencePylonDef
        {
            get
            {
                return ThingDef.Named("LaserFencePylon");
            }
        }
        // Laser fence.
        public static ThingDef LaserFenceDef
        {
            get
            {
                return ThingDef.Named("LaserFence");
            }
        }

        // Pylon switch job.
        public static JobDef SwitchLaserFenceDef
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_SwitchLaserFence");
            }
        }
        
    }
}
