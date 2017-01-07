using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using UnityEngine;

namespace CaveBiome
{
    // TODO: remove it! Just a copy of the internal corresponding class...
    public class Building_PoisonShipPartCopy : Building_CrashedShipPartCopy
    {
        protected override float PlantHarmRange
        {
            get
            {
                return Mathf.Min(3f + 30f * ((float)this.age / 60000f), 140f);
            }
        }

        protected override int PlantHarmInterval
        {
            get
            {
                float f = 4f - 0.6f * (float)this.age / 60000f;
                return Mathf.Clamp(Mathf.RoundToInt(f), 2, 4);
            }
        }
    }
}
