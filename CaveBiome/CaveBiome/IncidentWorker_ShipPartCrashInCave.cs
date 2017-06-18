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
    public abstract class IncidentWorker_ShipPartCrashInCave : IncidentWorker
    {
        private const float ShipPointsFactor = 0.9f;
        private const int IncidentMinimumPoints = 300;
        protected virtual int CountToSpawn
        {
            get
            {
                return 1;
            }
        }
        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            return map.listerThings.ThingsOfDef(this.def.shipPart).Count <= 0;
        }
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            int num = 0;
            int countToSpawn = this.CountToSpawn;
            IntVec3 vec = IntVec3.Invalid;
            for (int i = 0; i < countToSpawn; i++)
            {
                IntVec3 spawnCell = IntVec3.Invalid;
                if (map.Biome == Util_CaveBiome.CaveBiomeDef)
                {
                    TryFindShipCrashSite(map, out spawnCell);
                }
                else
                {
                    Predicate<IntVec3> validator = delegate (IntVec3 c)
                    {
                        if (c.Fogged(map))
                        {
                            return false;
                        }
                        foreach (IntVec3 current in GenAdj.CellsOccupiedBy(c, Rot4.North, this.def.shipPart.size))
                        {
                            if (!current.Standable(map))
                            {
                                bool result = false;
                                return result;
                            }
                            if (map.roofGrid.Roofed(current))
                            {
                                bool result = false;
                                return result;
                            }
                        }
                        return map.reachability.CanReachColony(c);
                    };
                    if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(14, validator, map, out spawnCell))
                    {
                        break;
                    }
                }
                if (spawnCell.IsValid == false)
                {
                    break;
                }
                GenExplosion.DoExplosion(spawnCell, map, 3f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
                Building_CrashedShipPart building_CrashedShipPart = (Building_CrashedShipPart)GenSpawn.Spawn(this.def.shipPart, spawnCell, map);
                building_CrashedShipPart.SetFaction(Faction.OfMechanoids, null);
                building_CrashedShipPart.pointsLeft = parms.points * ShipPointsFactor;
                if (building_CrashedShipPart.pointsLeft < IncidentMinimumPoints)
                {
                    building_CrashedShipPart.pointsLeft = IncidentMinimumPoints;
                }
                num++;
                vec = spawnCell;
            }
            if (num > 0)
            {
                Find.CameraDriver.shaker.DoShake(1f);
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, new TargetInfo(vec, map, false), null);
            }
            return num > 0;
        }

        public void TryFindShipCrashSite(Map map, out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;
            List<Thing> caveWellsList = map.listerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
            foreach (Thing caveWell in caveWellsList.InRandomOrder())
            {
                if (IsValidPositionForShipCrashSite(map, caveWell.Position))
                {
                    spawnCell = caveWell.Position;
                    return;
                }
            }
        }

        public bool IsValidPositionForShipCrashSite(Map map, IntVec3 position)
        {
            if ((position.InBounds(map) == false)
                || position.Fogged(map))
            {
                return false;
            }
			foreach (IntVec3 checkedPosition in GenAdj.CellsOccupiedBy(position, Rot4.North, this.def.shipPart.size))
			{
				if ((checkedPosition.Standable(map) == false)
                    || checkedPosition.Roofed(map)
                    || map.reachability.CanReachColony(checkedPosition) == false)
				{
					return false;
				}
			}
            return true;
        }
    }
}
