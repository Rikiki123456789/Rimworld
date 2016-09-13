using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// OG_Common class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_Common
    {
        public static void GenerateEmptyRoomAt(IntVec3 rotatedOrigin,
            int width,
            int height,
            Rot4 rotation,
            TerrainDef defaultFloorDef,
            TerrainDef interiorFloorDef,
            ref OG_OutpostData outpostData)
        {
            CellRect rect = new CellRect(rotatedOrigin.x, rotatedOrigin.z, width, height);
            if (rotation == Rot4.North)
            {
                rect = new CellRect(rotatedOrigin.x, rotatedOrigin.z, width, height);
            }
            else if (rotation == Rot4.East)
            {
                rect = new CellRect(rotatedOrigin.x, rotatedOrigin.z - width + 1, height, width);
            }
            else if (rotation == Rot4.South)
            {
                rect = new CellRect(rotatedOrigin.x - width + 1, rotatedOrigin.z - height + 1, width, height);
            }
            else
            {
                rect = new CellRect(rotatedOrigin.x - height + 1, rotatedOrigin.z, height, width);
            }
            // Generate 4 walls and power conduits.
            foreach (IntVec3 cell in rect.Cells)
            {
                if ((cell.x == rect.minX) || (cell.x == rect.maxX) || (cell.z == rect.minZ) || (cell.z == rect.maxZ))
                {
                    OG_Common.TrySpawnWallAt(cell, ref outpostData);
                    OG_Common.SpawnFireproofPowerConduitAt(cell, ref outpostData);
                }
            }
            // Trigger to unfog area when a pawn enters the room.
            SpawnRectTriggerUnfogArea(rect, ref outpostData.triggerIntrusion);
            // Generate roof.
            foreach (IntVec3 cell in rect.Cells)
            {
                Find.RoofGrid.SetRoof(cell, OG_Util.IronedRoofDef);
            }
            // Generate room default floor.
            if (defaultFloorDef != null)
            {
                foreach (IntVec3 cell in rect.ExpandedBy(1).Cells)
                {
                    TerrainDef terrain = Find.TerrainGrid.TerrainAt(cell);
                    if (terrain != TerrainDef.Named("PavedTile")) // Don't recover already spawned paved tile.
                    {
                        Find.TerrainGrid.SetTerrain(cell, defaultFloorDef);
                    }
                }
            }
            // Generate room interior floor.
            if (interiorFloorDef != null)
            {
                foreach (IntVec3 cell in rect.ContractedBy(1).Cells)
                {
                    Find.TerrainGrid.SetTerrain(cell, interiorFloorDef);
                }
            }
        }

        public static void GenerateHorizontalAndVerticalPavedAlleys(IntVec3 origin)
        {
            for (int xOffset = 0; xOffset < Genstep_GenerateOutpost.zoneSideSize; xOffset++)
            {
                Find.TerrainGrid.SetTerrain(origin + new IntVec3(xOffset, 0, Genstep_GenerateOutpost.zoneSideCenterOffset), TerrainDef.Named("PavedTile"));
            }
            for (int zOffset = 0; zOffset < Genstep_GenerateOutpost.zoneSideSize; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(origin + new IntVec3(Genstep_GenerateOutpost.zoneSideCenterOffset, 0, zOffset), TerrainDef.Named("PavedTile"));
            }
        }

        public static void SpawnRectTriggerUnfogArea(CellRect rect, ref TriggerIntrusion triggerIntrusion)
        {
            RectTriggerUnfogArea rectTrigger = (RectTriggerUnfogArea)ThingMaker.MakeThing(ThingDef.Named("RectTriggerUnfogArea"));
            rectTrigger.Rect = rect;
            GenSpawn.Spawn(rectTrigger, rect.CenterCell);

            // Update the trigger intrusion watched cells.
            foreach (IntVec3 cell in rect.Cells)
            {
                if (triggerIntrusion.watchedCells.Contains(cell) == false)
                {
                    triggerIntrusion.watchedCells.Add(cell);
                }
            }
        }

        public static void TrySpawnResourceStock(IntVec3 rotatedOrigin, Rot4 rotation, ThingDef resourceDef, int stockWidth, int stockHeight, float spawnStockChance, int minQuantity, int maxQuantity)
        {
            for (int xOffset = 0; xOffset < stockWidth; xOffset++)
            {
                for (int zOffset = 0; zOffset < stockHeight; zOffset++)
                {
                    if (Rand.Value < spawnStockChance)
                    {
                        SpawnResourceAt(resourceDef, Rand.RangeInclusive(minQuantity, maxQuantity), rotatedOrigin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation));
                    }
                }
            }
        }

        public static void SpawnResourceAt(ThingDef resourceDef, int quantity, IntVec3 position, bool forceSpawn = false)
        {
            if ((position.GetEdifice() != null)
                && (forceSpawn == false))
            {
                return;
            }
            Thing thing = ThingMaker.MakeThing(resourceDef);
            thing.stackCount = quantity;
            thing.SetForbidden(true);
            GenSpawn.Spawn(thing, position);
        }

        public static Thing TrySpawnThingAt(ThingDef thingDef,
            ThingDef stuffDef,
            IntVec3 position,
            bool rotated,
            Rot4 rotation,
            ref OG_OutpostData outpostData,
            bool destroyThings = false,
            bool replaceStructure = false)
        {
            if (destroyThings)
            {
                List<Thing> thingList = position.GetThingList();
                for (int j = thingList.Count - 1; j >= 0; j--)
                {
                    thingList[j].Destroy(DestroyMode.Vanish);
                }
            }
            Building building = position.GetEdifice();
            if (building != null)
            {
                if (replaceStructure)
                {
                    if (outpostData.outpostThingList.Contains(building))
                    {
                        outpostData.outpostThingList.Remove(building);
                    }
                    building.Destroy(DestroyMode.Vanish);
                }
                else
                {
                    return null;
                }
            }
            Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);
            outpostData.outpostThingList.Add(thing);
            thing.SetFaction(OG_Util.FactionOfMiningCo);
            if (rotated && thingDef.rotatable)
            {
                return GenSpawn.Spawn(thing, position, rotation);
            }
            else
            {
                if ((thingDef == ThingDef.Named("TableShort"))
                    || (thingDef == ThingDef.Named("MultiAnalyzer")))
                {
                    if (rotation == Rot4.East)
                    {
                        position += new IntVec3(0, 0, -1);
                    }
                    else if (rotation == Rot4.South)
                    {
                        position += new IntVec3(-1, 0, -1);
                    }
                    else if (rotation == Rot4.West)
                    {
                        position += new IntVec3(-1, 0, 0);
                    }
                }
                return GenSpawn.Spawn(thing, position);
            }
        }

        public static void TrySpawnLampAt(IntVec3 position, Color color, ref OG_OutpostData outpostData)
        {
            if (position.GetEdifice() != null)
            {
                return;
            }
            ThingDef lampDef = null;
            if (color == Color.red)
            {
                lampDef = ThingDef.Named("StandingLamp_Red");
            }
            else if (color == Color.green)
            {
                lampDef = ThingDef.Named("StandingLamp_Green");
            }
            else if (color == Color.blue)
            {
                lampDef = ThingDef.Named("StandingLamp_Blue");
            }
            else
            {
                lampDef = ThingDef.Named("StandingLamp");
            }
            OG_Common.TrySpawnThingAt(lampDef, null, position, false, Rot4.North, ref outpostData, true, false);
            Find.TerrainGrid.SetTerrain(position, TerrainDef.Named("MetalTile"));
        }

        public static void TrySpawnHeaterAt(IntVec3 position, ref OG_OutpostData outpostData)
        {
            ThingDef heaterDef = ThingDef.Named("Heater");
            OG_Common.TrySpawnThingAt(heaterDef, null, position, false, Rot4.North, ref outpostData);
            Find.TerrainGrid.SetTerrain(position, TerrainDef.Named("MetalTile"));
        }

        public static void TrySpawnWallAt(IntVec3 position, ref OG_OutpostData outpostData)
        {
            ThingDef wallDef = ThingDefOf.Wall;
            OG_Common.TrySpawnThingAt(wallDef, ThingDef.Named("BlocksGranite"), position, false, Rot4.North, ref outpostData);
        }

        public static void SpawnFireproofPowerConduitAt(IntVec3 position, ref OG_OutpostData outpostData)
        {
            Thing fireproofPowerConduit = ThingMaker.MakeThing(OG_Util.FireproofPowerConduitDef);
            fireproofPowerConduit.SetFaction(OG_Util.FactionOfMiningCo);
            foreach (Thing thing in position.GetThingList())
            {
                if (thing.def == OG_Util.FireproofPowerConduitDef)
                {
                    return;
                }
            }
            outpostData.outpostThingList.Add(fireproofPowerConduit);
            GenSpawn.Spawn(fireproofPowerConduit, position);
        }

        public static void SpawnDoorAt(IntVec3 position, ref OG_OutpostData outpostData)
        {
            ThingDef autodoorDef = OG_Util.FireproofAutodoorDef;
            Building edifice = position.GetEdifice();
            if ((edifice != null)
                && (edifice.def == autodoorDef))
            {
                // Avoid spawning another door on the same spot. This creates troubles with region links...
                return;
            }
            Thing door = OG_Common.TrySpawnThingAt(autodoorDef, ThingDefOf.Steel, position, false, Rot4.North, ref outpostData, false, true);
            CompForbiddable compForbiddable = door.TryGetComp<CompForbiddable>();
            if (compForbiddable != null)
            {
                compForbiddable.Forbidden = true; // Avoid colonists going into outpost at start-up.
            }
        }

        public static Building_Cooler SpawnCoolerAt(IntVec3 position, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            ThingDef coolerDef = ThingDef.Named("Cooler");
            Building_Cooler cooler = OG_Common.TrySpawnThingAt(coolerDef, null, position, true, rotation, ref outpostData, false, true) as Building_Cooler;
            return cooler;
        }

        public static void SpawnVentAt(IntVec3 position, Rot4 rotation, ref OG_OutpostData outpostData)
        {
            ThingDef ventDef = ThingDef.Named("Vent");
            OG_Common.TrySpawnThingAt(ventDef, null, position, true, rotation, ref outpostData, false, true);
        }

        public static LaserFence.Building_LaserFencePylon TrySpawnLaserFencePylonAt(IntVec3 position, ref OG_OutpostData outpostData)
        {
            ThingDef laserFencePylonDef = ThingDef.Named("LaserFencePylon");
            return (OG_Common.TrySpawnThingAt(laserFencePylonDef, null, position, false, Rot4.North, ref outpostData) as LaserFence.Building_LaserFencePylon);
        }

        public static void GenerateSas(IntVec3 origin, Rot4 rotation, int width, int height, ref OG_OutpostData outpostData)
        {
            int sideWidth = (width - 1) / 2; // Distance of the wall from the central alley.

            OG_Common.GenerateEmptyRoomAt(origin + new IntVec3(-sideWidth, 0, 0).RotatedBy(rotation), 1 + 2 * sideWidth, height, rotation, null, null, ref outpostData);
            OG_Common.SpawnDoorAt(origin, ref outpostData);
            OG_Common.SpawnDoorAt(origin + new IntVec3(0, 0, height - 1).RotatedBy(rotation), ref outpostData);
            // Generate floor.
            for (int xOffset = -sideWidth - 1; xOffset <= sideWidth + 1; xOffset++)
            {
                for (int zOffset = 0; zOffset < height; zOffset++)
                {
                    Find.TerrainGrid.SetTerrain(origin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation), TerrainDefOf.Concrete);
                }
            }
            for (int zOffset = 0; zOffset < height; zOffset++)
            {
                Find.TerrainGrid.SetTerrain(origin + new IntVec3(0, 0, zOffset).RotatedBy(rotation), TerrainDef.Named("PavedTile"));
            }
        }

        public static void TrySpawnWeaponOnRack(Thing rack)
        {
            Building_Storage storage = rack as Building_Storage;
            QualityRange qualityRange = new QualityRange(QualityCategory.Good, QualityCategory.Legendary);
            storage.settings.filter.AllowedQualityLevels = qualityRange;
            FloatRange hitPointsRange = new FloatRange(0.8f, 1f);
            storage.settings.filter.AllowedHitPointsPercents = hitPointsRange;
            foreach (IntVec3 cell in rack.OccupiedRect())
            {
                if (Rand.Value < 0.33f)
                {
                    float weaponSelector = Rand.Value;
                    ThingDef thingDef = ThingDefOf.Gun_Pistol;
                    const float weaponsRatio = 1f / 7f;
                    if (weaponSelector < weaponsRatio * 1f)
                    {
                        thingDef = ThingDef.Named("Gun_PumpShotgun");
                    }
                    else if (weaponSelector < weaponsRatio * 2f)
                    {
                        thingDef = ThingDef.Named("Gun_AssaultRifle");
                    }
                    else if (weaponSelector < weaponsRatio * 3f)
                    {
                        thingDef = ThingDef.Named("Gun_SniperRifle");
                    }
                    else if (weaponSelector < weaponsRatio * 4f)
                    {
                        thingDef = ThingDef.Named("Gun_IncendiaryLauncher");
                    }
                    else if (weaponSelector < weaponsRatio * 5f)
                    {
                        thingDef = ThingDef.Named("Gun_LMG");
                    }
                    else if (weaponSelector < weaponsRatio * 6f)
                    {
                        thingDef = ThingDef.Named("Gun_ChargeRifle");
                    }
                    else
                    {
                        thingDef = ThingDefOf.Gun_Pistol;
                    }

                    Thing weapon = ThingMaker.MakeThing(thingDef);
                    CompQuality qualityComp = weapon.TryGetComp<CompQuality>();
                    if (qualityComp != null)
                    {
                        QualityCategory quality = (QualityCategory)Rand.RangeInclusive((int)QualityCategory.Normal, (int)QualityCategory.Excellent);
                        qualityComp.SetQuality(quality, ArtGenerationContext.Outsider);
                    }
                    GenSpawn.Spawn(weapon, cell);
                }
            }
        }

        public static ZoneType GetRandomZoneTypeBigRoom(OG_OutpostData outpostData)
        {
            List<ZoneTypeWithWeight> bigRoomsList = new List<ZoneTypeWithWeight>();
            if (outpostData.isMilitary)
            {
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomLivingRoom, 2f));
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomWarehouse, 2f));
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomPrison, 6f));
            }
            else
            {
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomLivingRoom, 4f));
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomWarehouse, 4f));
                bigRoomsList.Add(new ZoneTypeWithWeight(ZoneType.BigRoomPrison, 2f));
            }

            ZoneType bigRoomType = GetRandomZoneTypeByWeight(bigRoomsList);
            return bigRoomType;
        }

        public static ZoneType GetRandomZoneTypeMediumRoom(OG_OutpostData outpostData)
        {
            List<ZoneTypeWithWeight> mediumRoomZonesList = new List<ZoneTypeWithWeight>();
            if (outpostData.isMilitary)
            {
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomMedibay, 7f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomPrison, 5f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomKitchen, 3f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomWarehouse, 2f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomWeaponRoom, 7f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomBarn, 5f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomLaboratory, 2f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomRecRoom, 4f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.Empty, 1f));
            }
            else
            {
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomMedibay, 3f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomPrison, 1f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomKitchen, 4f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomWarehouse, 5f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomWeaponRoom, 2f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomBarn, 2f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomLaboratory, 7f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.MediumRoomRecRoom, 7f));
                mediumRoomZonesList.Add(new ZoneTypeWithWeight(ZoneType.Empty, 1f));
            }

            ZoneType exteriorZoneType = GetRandomZoneTypeByWeight(mediumRoomZonesList);
            return exteriorZoneType;
        }

        public static ZoneType GetRandomZoneTypeSmallRoom(OG_OutpostData outpostData)
        {
            List<ZoneTypeWithWeight> smallRoomsList = new List<ZoneTypeWithWeight>();
            if (outpostData.isMilitary)
            {
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomBarracks, 2f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomMedibay, 1f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomWeaponRoom, 5f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SecondaryEntrance, 6f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.Empty, 1f));
            }
            else
            {
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomBarracks, 4f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomMedibay, 3f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SmallRoomWeaponRoom, 1f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.SecondaryEntrance, 2f));
                smallRoomsList.Add(new ZoneTypeWithWeight(ZoneType.Empty, 1f));
            }

            ZoneType smallRoomType = GetRandomZoneTypeByWeight(smallRoomsList);
            return smallRoomType;
        }

        public static ZoneType GetRandomZoneTypeExteriorZone(OG_OutpostData outpostData)
        {
            List<ZoneTypeWithWeight> exteriorZonesList = new List<ZoneTypeWithWeight>();
            if (outpostData.isMilitary)
            {
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.WaterPool, 5f));
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.Cemetery, 4f));
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.ExteriorRecRoom, 2f));
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.ShootingRange, 7f));
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.MortarBay, 5f));
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.Empty, 2f));
            }
            else
            {
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.WaterPool, 4f));
                if ((Find.Map.Biome != BiomeDef.Named("ExtremeDesert"))
                    && (Find.Map.Biome != BiomeDef.Named("IceSheet")))
                {
                    exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.Farm, 5f));
                }
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.Cemetery, 3f));
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.ExteriorRecRoom, 5f));
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.ShootingRange, 1f));
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.MortarBay, 1f));
                exteriorZonesList.Add(new ZoneTypeWithWeight(ZoneType.Empty, 1f));
            }

            ZoneType exteriorZoneType = GetRandomZoneTypeByWeight(exteriorZonesList);
            return exteriorZoneType;
        }

        private static ZoneType GetRandomZoneTypeByWeight(List<ZoneTypeWithWeight> list)
        {
            float weightTotalSum = 0;
            foreach (ZoneTypeWithWeight element in list)
            {
                weightTotalSum += element.weight;
            }
            float elementSelector = Rand.Range(0f, weightTotalSum);

            float weightSum = 0;
            foreach (ZoneTypeWithWeight element in list)
            {
                weightSum += element.weight;
                if (elementSelector <= weightSum)
                {
                    return element.zoneType;
                }
            }

            return ZoneType.Empty;
        }
    }
}
