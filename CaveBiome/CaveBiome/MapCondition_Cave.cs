using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace CaveBiome
{
    public class MapCondition_Cave : MapCondition
    {
        private int LerpTicks = 200;

        public override float PlantDensityFactor()
        {
            // To avoid getting plant seeds from map edges.
            return 0f;
        }

        private SkyColorSet caveSkyColors = new SkyColorSet(new Color(0.482f, 0.603f, 0.682f), Color.white, new Color(0.6f, 0.6f, 0.6f), 1f);
        public override float SkyTargetLerpFactor()
        {
            return MapConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, (float)this.LerpTicks, 1f);
        }
        public override SkyTarget? SkyTarget()
        {
            return new SkyTarget?(new SkyTarget(this.caveSkyColors));
        }
        public override bool AllowEnjoyableOutsideNow()
        {
            return false;
        }
    }
}
