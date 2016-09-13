using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Building_SupplyShip class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_SupplyShip : Building
    {
        private const int ressuplyDuration = GenDate.TicksPerDay / 2;
        private int takeOffTick = 0;
        private Thing cryptosleepBay1 = null;
        private Thing cryptosleepBay2 = null;
        private Thing cargoBay1 = null;
        private Thing cargoBay2 = null;
        private bool reinforcementsAreSpawned = false;

        // Reinforcement data.
        public int requestedOfficersNumber = 0;
        public int requestedHeavyGuardsNumber = 0;
        public int requestedGuardsNumber = 0;
        public int requestedScoutsNumber = 0;
        public int requestedTechniciansNumber = 0;

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            // Initialize take-off tick.
            takeOffTick = Find.TickManager.TicksGame + ressuplyDuration;

            // Spawn cryptosleep bays.
            if (this.cryptosleepBay1 == null)
            {
                cryptosleepBay1 = ThingMaker.MakeThing(OG_Util.SupplyShipCryptosleepBayLeftDef);
                cryptosleepBay1.SetFactionDirect(this.Faction);
                GenSpawn.Spawn(cryptosleepBay1, this.Position + new IntVec3(-4, 0, -2).RotatedBy(this.Rotation), this.Rotation);
            }
            if (this.cryptosleepBay2 == null)
            {
                cryptosleepBay2 = ThingMaker.MakeThing(OG_Util.SupplyShipCryptosleepBayRightDef);
                cryptosleepBay2.SetFactionDirect(this.Faction);
                GenSpawn.Spawn(cryptosleepBay2, this.Position + new IntVec3(3, 0, -2).RotatedBy(this.Rotation), this.Rotation);
            }
            // Spawn cargo bays.
            if (this.cargoBay1 == null)
            {
                cargoBay1 = ThingMaker.MakeThing(OG_Util.SupplyShipCargoBayLeftDef);
                cargoBay1.SetFactionDirect(this.Faction);
                GenSpawn.Spawn(cargoBay1, this.Position + new IntVec3(-4, 0, 1).RotatedBy(this.Rotation), this.Rotation);
            }
            if (this.cargoBay2 == null)
            {
                cargoBay2 = ThingMaker.MakeThing(OG_Util.SupplyShipCargoBayRightDef);
                cargoBay2.SetFactionDirect(this.Faction);
                GenSpawn.Spawn(cargoBay2, this.Position + new IntVec3(4, 0, 1).RotatedBy(this.Rotation), this.Rotation);
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            if (this.reinforcementsAreSpawned == false)
            {
                // Only spawn reinforcement pawns and supply once.
                this.reinforcementsAreSpawned = true;
                UpdateRequestedReinforcements();
                SpawnRequestedReinforcements();
                SpawnNecessarySupply();
                UnforbidItemsToLoadInCargoBay();
            }
            
            if (Find.TickManager.TicksGame >= this.takeOffTick)
            {
                // Spawn taking off supply ship.
                SupplyShipTakingOff supplyShip = ThingMaker.MakeThing(OG_Util.SupplyShipTakingOffDef) as SupplyShipTakingOff;
                supplyShip.InitializeLandingData(this.Position, this.Rotation);
                GenSpawn.Spawn(supplyShip, this.Position);
                this.Destroy();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (!this.cryptosleepBay1.DestroyedOrNull())
            {
                this.cryptosleepBay1.Destroy();
            }
            if (!this.cryptosleepBay2.DestroyedOrNull())
            {
                this.cryptosleepBay2.Destroy();
            }
            if (!this.cargoBay1.DestroyedOrNull())
            {
                this.cargoBay1.Destroy();
            }
            if (!this.cargoBay2.DestroyedOrNull())
            {
                this.cargoBay2.Destroy();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.takeOffTick, "takeOffTick");
            Scribe_References.LookReference<Thing>(ref this.cryptosleepBay1, "cryptosleepBay1");
            Scribe_References.LookReference<Thing>(ref this.cryptosleepBay2, "cryptosleepBay2");
            Scribe_References.LookReference<Thing>(ref this.cargoBay1, "cargoBay1");
            Scribe_References.LookReference<Thing>(ref this.cargoBay2, "cargoBay2");
            Scribe_Values.LookValue<bool>(ref this.reinforcementsAreSpawned, "reinforcementsAreSpawned");
        }

        public void UpdateRequestedReinforcements()
        {
            int officersNumber = 0;
            int heavyGuardsNumber = 0;
            int guardsNumber = 0;
            int scoutsNumber = 0;
            int techniciansNumber = 0;

            // Get the list of MiningCo. pawns in the outpost area.
            foreach (Pawn pawn in Find.MapPawns.PawnsInFaction(OG_Util.FactionOfMiningCo))
            {
                if (pawn.kindDef == OG_Util.OutpostOfficerDef)
                {
                    officersNumber++;
                }
                else if (pawn.kindDef == OG_Util.OutpostHeavyGuardDef)
                {
                    heavyGuardsNumber++;
                }
                else if (pawn.kindDef == OG_Util.OutpostGuardDef)
                {
                    guardsNumber++;
                }
                else if (pawn.kindDef == OG_Util.OutpostScoutDef)
                {
                    scoutsNumber++;
                }
                else if (pawn.kindDef == OG_Util.OutpostTechnicianDef)
                {
                    techniciansNumber++;
                }
            }

            // Compute the number of necessary reinforcements.
            this.requestedOfficersNumber = OG_Inhabitants.officersTargetNumber - officersNumber;
            this.requestedHeavyGuardsNumber = OG_Inhabitants.heavyGuardsTargetNumber - heavyGuardsNumber;
            this.requestedGuardsNumber = OG_Inhabitants.guardsTargetNumber - guardsNumber;
            this.requestedScoutsNumber = OG_Inhabitants.scoutsTargetNumber - scoutsNumber;
            this.requestedTechniciansNumber = OG_Inhabitants.techniciansTargetNumber - techniciansNumber;
        }

        private void SpawnRequestedReinforcements()
        {
            for (int pawnIndex = 0; pawnIndex < this.requestedOfficersNumber; pawnIndex++)
            {
                Pawn pawn = OG_Inhabitants.GeneratePawn(OG_Util.OutpostOfficerDef);
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(this.Position, 4));
            }
            for (int pawnIndex = 0; pawnIndex < this.requestedHeavyGuardsNumber; pawnIndex++)
            {
                Pawn pawn = OG_Inhabitants.GeneratePawn(OG_Util.OutpostHeavyGuardDef);
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(this.Position, 4));
            }
            for (int pawnIndex = 0; pawnIndex < this.requestedGuardsNumber; pawnIndex++)
            {
                Pawn pawn = OG_Inhabitants.GeneratePawn(OG_Util.OutpostGuardDef);
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(this.Position, 4));
            }
            for (int pawnIndex = 0; pawnIndex < this.requestedScoutsNumber; pawnIndex++)
            {
                Pawn pawn = OG_Inhabitants.GeneratePawn(OG_Util.OutpostScoutDef);
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(this.Position, 4));
            }
            for (int pawnIndex = 0; pawnIndex < this.requestedTechniciansNumber; pawnIndex++)
            {
                Pawn pawn = OG_Inhabitants.GeneratePawn(OG_Util.OutpostTechnicianDef);
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(this.Position, 4));
            }
        }

        private void SpawnNecessarySupply()
        {
            const int mealsInStockTarget = 80;
            const int beersInStockTarget = 100;
            const int componentsInStockTarget = 20;
            int mealsInOutpost = 0;
            int beersInOutpost = 0;
            int componentsInOutpost = 0;
            int mealsToSupply = 0;
            int beersToSupply = 0;
            int componentsToSupply = 0;

            if (OG_Util.FindOutpostArea() == null)
            {
                // Outpost has been captured, do not resupply.
                return;
            }
            CountResourcesInOutpost(out mealsInOutpost, out beersInOutpost, out componentsInOutpost);
            mealsToSupply = mealsInStockTarget - mealsInOutpost;
            beersToSupply = beersInStockTarget - beersInOutpost;
            componentsToSupply = componentsInStockTarget - componentsInOutpost;
            SpawnSupplyNearPosition(ThingDefOf.MealSurvivalPack, mealsToSupply, this.Position);
            SpawnSupplyNearPosition(ThingDefOf.Beer, beersToSupply, this.Position);
            SpawnSupplyNearPosition(ThingDefOf.Component, componentsToSupply, this.Position);
        }

        private static void CountResourcesInOutpost(out int meals, out int beers, out int components)
        {
            meals = 0;
            beers = 0;
            components = 0;
            Area outpostArea = OG_Util.FindOutpostArea();
            if (outpostArea != null)
            {
                foreach (IntVec3 cell in outpostArea.ActiveCells)
                {
                    foreach (Thing thing in cell.GetThingList())
                    {
                        if (thing.def == ThingDefOf.MealSurvivalPack)
                        {
                            meals += thing.stackCount;
                        }
                        if (thing.def == ThingDefOf.Beer)
                        {
                            beers += thing.stackCount;
                        }
                        if (thing.def == ThingDefOf.Component)
                        {
                            components += thing.stackCount;
                        }
                    }
                }
            }
        }

        private static void SpawnSupplyNearPosition(ThingDef def, int quantity, IntVec3 center)
        {
            while (quantity > 0)
            {
                Thing supplyStack = ThingMaker.MakeThing(def);
                if (quantity >= def.stackLimit)
                {
                    supplyStack.stackCount = def.stackLimit;
                }
                else
                {
                    supplyStack.stackCount = quantity;
                }
                quantity -= supplyStack.stackCount;
                GenPlace.TryPlaceThing(supplyStack, center, ThingPlaceMode.Near);
            }
        }

        private static void UnforbidItemsToLoadInCargoBay()
        {
            // Unforbid any weapon, apparel, raw food or corpse in the outpost area so it can be carried to a cargo bay.
            Area outpostArea = OG_Util.FindOutpostArea();
            if (outpostArea != null)
            {
                foreach (IntVec3 cell in outpostArea.ActiveCells)
                {
                    foreach (Thing thing in cell.GetThingList())
                    {
                        if (thing.def.thingCategories == null)
                        {
                            continue;
                        }
                        if (thing.def.thingCategories.Contains(ThingCategoryDefOf.Apparel)
                            || thing.def.thingCategories.Contains(ThingCategoryDef.Named("Headgear"))
                            || thing.def.thingCategories.Contains(ThingCategoryDef.Named("WeaponsMelee"))
                            || thing.def.thingCategories.Contains(ThingCategoryDef.Named("WeaponsRanged"))
                            || thing.def.thingCategories.Contains(ThingCategoryDef.Named("CorpsesHumanlike"))
                            || thing.def.thingCategories.Contains(ThingCategoryDef.Named("Textiles"))
                            || (thing.def == ThingDef.Named("RawHops"))
                            || (thing.def.thingCategories.Contains(ThingCategoryDef.Named("PlantFoodRaw"))
                                && (thing.def != ThingDef.Named("Hay"))))
                        {
                            thing.SetForbidden(false);
                        }
                    }
                }
            }
        }
    }
}
