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
        private const int maxTicksToTakeOff = 2500;
        private int ticksToTakeOff = maxTicksToTakeOff;
        private Thing cryptosleepBay1 = null;
        private Thing cryptosleepBay2 = null;
        private Thing cargoBay1 = null;
        private Thing cargoBay2 = null;
        
        public override void SpawnSetup()
        {
            base.SpawnSetup();
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
            
            if (this.ticksToTakeOff == maxTicksToTakeOff)
            {
                // Only spawn reinforcement pawns, packaged meals, beer and components once.
                // TODO.
                /*DropPodInfo info = new DropPodInfo();
                Thing meals = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack);
                meals.stackCount = Find.ListerPawns.PawnsInFaction(OG_Util.FactionOfMAndCo).Count;
                meals.SetForbidden(true);
                info.SingleContainedThing = meals;
                DropPodUtility.MakeDropPodAt(this.dropZoneCenter + new IntVec3(Rand.RangeInclusive(-4, 4), 0, Rand.RangeInclusive(-4, 4)), info);*/

                Building_OrbitalRelay orbitalRelay = OG_Util.FindOrbitalRelay(this.Faction);
                if (orbitalRelay != null)
                {
                    List<PawnKindDef> reinforcementRequestsList;
                    orbitalRelay.GetAndClearReinforcementRequestsList(out reinforcementRequestsList);
                    foreach (PawnKindDef pawnType in reinforcementRequestsList)
                    {
                        // TODO: reuse OG_Inhabitants pawn generation algo.
                        Pawn pawn = PawnGenerator.GeneratePawn(pawnType, this.Faction);
                        GenSpawn.Spawn(pawn, this.Position);
                    }
                }

                SpawnNecessarySupply();

                UnforbidItemsToLoadInCargoBay();
            }

            this.ticksToTakeOff--;
            if (this.ticksToTakeOff <= 0)
            {
                SupplyShipTakingOff supplyShip = ThingMaker.MakeThing(OG_Util.SupplyShipTakingOffDef) as SupplyShipTakingOff;
                supplyShip.InitializeLandingData(this.Position, this.Rotation);
                supplyShip.SetFaction(this.Faction);
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
            Scribe_Values.LookValue<int>(ref this.ticksToTakeOff, "ticksToTakeOff");
            Scribe_References.LookReference<Thing>(ref this.cryptosleepBay1, "cryptosleepBay1");
            Scribe_References.LookReference<Thing>(ref this.cryptosleepBay2, "cryptosleepBay2");
            Scribe_References.LookReference<Thing>(ref this.cargoBay1, "cargoBay1");
            Scribe_References.LookReference<Thing>(ref this.cargoBay2, "cargoBay2");
        }
        
        private static void SpawnNecessarySupply()
        {
            const int mealsInStockTarget = 0;
            const int beersInStockTarget = 0;
            const int componentsInStockTarget = 0;
            int mealsInOutpost = 0;
            int beersInOutpost = 0;
            int componentsInOutpost = 0;
            int mealsToSupply = 0;
            int beersToSupply = 0;
            int componentsToSupply = 0;

            CountResourcesInOutpost(out mealsInOutpost, out beersInOutpost, out componentsInOutpost);


        }

        private static void CountResourcesInOutpost(out int meals, out int beers, out int components)
        {
            meals = 0;
            beers = 0;
            components = 0;
            if (OG_Util.OutpostArea != null)
            {
                foreach (IntVec3 cell in OG_Util.OutpostArea.ActiveCells)
                {
                    foreach (Thing thing in cell.GetThingList())
                    {
                        if (thing.def == ThingDefOf.MealSurvivalPack)
                        {
                            meals++;
                        }
                        if (thing.def == ThingDefOf.Beer)
                        {
                            beers++;
                        }
                        if (thing.def == ThingDefOf.Components)
                        {
                            components++;
                        }
                    }
                }
            }
        }

        private static void UnforbidItemsToLoadInCargoBay()
        {
            // Unforbid any weapon, apparel, raw food or corpse in the outpost area so it can be carried to a cargo bay.
            if (OG_Util.OutpostArea != null)
            {
                foreach (IntVec3 cell in OG_Util.OutpostArea.ActiveCells)
                {
                    foreach (Thing thing in cell.GetThingList())
                    {
                        if (thing.def.thingCategories.Contains(ThingCategoryDefOf.Apparel)
                            || thing.def.thingCategories.Contains(ThingCategoryDefOf.Weapons)
                            || thing.def.thingCategories.Contains(ThingCategoryDef.Named("CorpsesHumanlike"))
                            || (thing.def.thingCategories.Contains(ThingCategoryDef.Named("FoodRaw"))
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
