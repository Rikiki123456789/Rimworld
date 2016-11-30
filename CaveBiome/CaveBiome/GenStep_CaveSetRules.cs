using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;

namespace CaveBiome
{
    public class GenStep_CaveSetRules : GenStep
    {
		public override void Generate()
		{
            // Note that the same treatment is performed in MapComponent_CaveWellLight.ExposeData when loading a savegame.
            SetCaveRules();
        }

        public static void SetCaveRules()
        {
            if (Find.Map.Biome == Util_CaveBiome.CaveBiomeDef)
            {
                // Disallow building of solar panels and vanilla mortars.
                Current.Game.Rules.SetAllowBuilding(ThingDefOf.SolarGenerator, false);
                Current.Game.Rules.SetAllowBuilding(ThingDef.Named("Turret_MortarBomb"), false);
                Current.Game.Rules.SetAllowBuilding(ThingDef.Named("Turret_MortarIncendiary"), false);
                Current.Game.Rules.SetAllowBuilding(ThingDef.Named("Turret_MortarEMP"), false);

                // Disable siege RaidStrategyDef.
                RaidStrategyDef siegeDef = DefDatabase<RaidStrategyDef>.GetNamed("Siege");
                if (siegeDef != null)
                {
                    siegeDef.selectionChance = 0f;
                }

                // Disable RaidStrategyDef arrive mode other than EdgeWalkIn.
                foreach (RaidStrategyDef raidDef in DefDatabase<RaidStrategyDef>.AllDefs)
                {
                    List<PawnsArriveMode> newArriveModes = new List<PawnsArriveMode>();
                    newArriveModes.Add(PawnsArriveMode.EdgeWalkIn);
                    raidDef.arriveModes = newArriveModes;
                }

                // Disable flashstorm incident.
                IncidentDef flashstormDef = DefDatabase<IncidentDef>.GetNamed("Flashstorm");
                if (flashstormDef != null)
                {
                    flashstormDef.baseChance = 0;
                }
            }
            else
            {
                // Disallow building of cave mortars.
                Current.Game.Rules.SetAllowBuilding(ThingDef.Named("Turret_MortarBombCave"), false);
                Current.Game.Rules.SetAllowBuilding(ThingDef.Named("Turret_MortarIncendiaryCave"), false);
                Current.Game.Rules.SetAllowBuilding(ThingDef.Named("Turret_MortarEMPCave"), false);
            }
        }
    }
}
