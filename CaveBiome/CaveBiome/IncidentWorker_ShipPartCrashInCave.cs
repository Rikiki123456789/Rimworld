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
        protected override bool CanFireNowSub()
        {
            return Find.ListerThings.ThingsOfDef(this.def.shipPart).Count <= 0;
        }
        public override bool TryExecute(IncidentParms parms)
        {
            int num = 0;
            int countToSpawn = this.CountToSpawn;
            IntVec3 vec = IntVec3.Invalid;
            for (int i = 0; i < countToSpawn; i++)
            {
                IntVec3 spawnCell = IntVec3.Invalid;
                TryFindShipCrashSite(out spawnCell);
                if (spawnCell.IsValid == false)
                {
                    break;
                }
                GenExplosion.DoExplosion(spawnCell, 3f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
                Building_CrashedShipPartCopy building_CrashedShipPart = (Building_CrashedShipPartCopy)GenSpawn.Spawn(this.def.shipPart, spawnCell);
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
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType, vec, null);
            }
            return num > 0;
        }

        public void TryFindShipCrashSite(out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;
            List<Thing> caveWellsList = Find.ListerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
            foreach (Thing caveWell in caveWellsList.InRandomOrder())
            {
                if (IsValidPositionForShipCrashSite(caveWell.Position))
                {
                    spawnCell = caveWell.Position;
                    return;
                }
            }
        }

        public bool IsValidPositionForShipCrashSite(IntVec3 position)
        {
            if ((position.InBounds() == false)
                || position.Fogged())
            {
                return false;
            }
			foreach (IntVec3 checkedPosition in GenAdj.CellsOccupiedBy(position, Rot4.North, this.def.shipPart.size))
			{
				if ((checkedPosition.Standable() == false)
                    || checkedPosition.Roofed()
                    || checkedPosition.CanReachColony() == false)
				{
					return false;
				}
			}
            return true;
        }
    }
}
