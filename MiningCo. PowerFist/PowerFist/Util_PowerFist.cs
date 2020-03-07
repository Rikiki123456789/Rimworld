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
    public static class Util_PowerFist
    {
        // Repeller.
        public static ThingDef PowerFistRepellerDef
        {
            get
            {
                return ThingDef.Named("PowerFistRepeller");
            }
        }

        // Electric damage.
        public static DamageDef ElectricDamageDef
        {
            get
            {
                return DefDatabase<DamageDef>.GetNamed("Electric");
            }
        }

        // Crush mote.
        public static ThingDef CrushMoteDef
        {
            get
            {
                return DefDatabase<ThingDef>.GetNamed("Mote_Crush");
            }
        }
    }
}
