using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;    // Always needed
using RimWorld;       // RimWorld specific functions are found here
using Verse;          // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI
using Verse.AI.Group; // Needed when you do something with group of pawn AI
using Verse.Sound;    // Needed when you do something with the Sound

namespace Spaceship
{
    // TODO: add a chance for the ship to crash (nearby tile with pirate around after some times).
    // TODO: cannot build ship buildings in cave biome (add NotinCave place worker).
    // TODO: add event witgh pirates in hijacked cargo spaceship.
    // TODO: Should medics rescue downed colonist?

    public abstract class Building_Spaceship : Building, IThingHolder
    {
        public const int pilotsNumber = 2;

        public int takeOffTick = 0;
        public SpaceshipKind spaceshipKind = SpaceshipKind.CargoPeriodic;
        public TraderKindDef cargoKind;
        public List<Pawn> pawnsAboard = new List<Pawn>();

        public virtual bool takeOffRequestIsEnabled
        {
            get
            {
                return true;
            }
        }
        
        // ===================== Setup work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            // Initialize cargo and destroy roof.
            if (this.things == null)
            {
                this.things = new ThingOwner<Thing>(this);
                GenerateThings();

                DestroyRoof();
            }

            // Generate pilots.
            if (this.pawnsAboard.Count == 0)
            {
                for (int pilotIndex = 0; pilotIndex < pilotsNumber; pilotIndex++)
                {
                    Pawn pilot = MiningCoPawnGenerator.GeneratePawn(Util_PawnKindDefOf.Pilot, this.Map);
                    if (pilot != null)
                    {
                        this.pawnsAboard.Add(pilot);
                    }
                }
            }
        }

        public void InitializeData(Faction faction, int hitPoints, int landingDuration, SpaceshipKind spaceshipKind)
        {
            this.SetFaction(faction);
            this.HitPoints = hitPoints;
            this.takeOffTick = Find.TickManager.TicksGame + landingDuration;
            this.spaceshipKind = spaceshipKind;
            this.cargoKind = GetCargoKind(this.spaceshipKind);      
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // Free landing pad.
            Thing landingPad = this.Position.GetFirstThing(this.Map, Util_ThingDefOf.LandingPad);
            if (landingPad != null)
            {
                (landingPad as Building_LandingPad).NotifyShipTakingOff();
            }
            if (mode == DestroyMode.KillFinalize)
            {
                // Spaceship is destroyed.
                SpawnSurvivingPawns();
                SpawnExplosions();
                SpawnFuelPuddleAndFire();
                // Add spaceship cost fee.
                Util_Misc.Partnership.feeInSilver[this.Map] += Mathf.RoundToInt(this.def.BaseMarketValue * 0.5f);
                // Add spaceship cargo cost fee and spread it around.
                int cargoFeeInSilver = SpawnCargoContent(0.5f);
                Util_Misc.Partnership.feeInSilver[this.Map] += Mathf.RoundToInt(cargoFeeInSilver * 0.5f);
                Util_Faction.AffectFactionGoodwillWithOther(Util_Faction.MiningCoFaction, Faction.OfPlayer, -30);

                string spaceshipDestroyedText = "-- Comlink with MiningCo. --\n\n"
                + "\"We just lost contact with one of our ships in your sector.\n"
                + "Whatever happened, you are held responsible of this loss.\n\n"
                + "Pay a compensation or say goodbye to our partnership!\"\n\n"
                + "-- End of transmission --";
                Find.LetterStack.ReceiveLetter("Spaceship destroyed", spaceshipDestroyedText, LetterDefOf.ThreatSmall, new TargetInfo(this.Position, this.Map));
            }
            else
            {
                // Spaceship is taking off.
                // Destroy remaining pawns aboard.
                foreach (Pawn pawn in this.pawnsAboard)
                {
                    pawn.Destroy();
                }
                this.pawnsAboard.Clear();
                // Spawn taking off spaceship.
                FlyingSpaceshipTakingOff spaceship = ThingMaker.MakeThing(Util_Spaceship.SpaceshipTakingOff) as FlyingSpaceshipTakingOff;
                GenSpawn.Spawn(spaceship, this.Position, this.Map, this.Rotation);
                spaceship.InitializeTakingOffParameters(this.Position, this.Rotation, this.spaceshipKind);
                spaceship.HitPoints = this.HitPoints;
                if (this.Map.listerBuildings.ColonistsHaveBuilding((Thing b) => b.def == Util_ThingDefOf.OrbitalRelay))
                {
                    Messages.Message("A MiningCo. spaceship is taking off.", spaceship, MessageTypeDefOf.NeutralEvent);
                }
                DestroyRoof();
            }
            this.things.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.takeOffTick, "takeOffTick");
            Scribe_Values.Look<SpaceshipKind>(ref this.spaceshipKind, "spaceshipKind");
            Scribe_Defs.Look<TraderKindDef>(ref this.cargoKind, "cargoKind");
            Scribe_Deep.Look<ThingOwner>(ref this.things, "things");
            Scribe_Collections.Look<Pawn>(ref this.pawnsAboard, "pawnsAboard", LookMode.Deep);
        }

        // Destroy any roof under or over the spaceship.
        public void DestroyRoof()
        {
            foreach (IntVec3 cell in this.OccupiedRect().Cells)
            {
                if (cell.Roofed(this.Map))
                {
                    RoofDef roof = this.Map.roofGrid.RoofAt(cell);
                    if (roof.filthLeaving != null)
                    {
                        FilthMaker.MakeFilth(cell, this.Map, roof.filthLeaving, Rand.RangeInclusive(1, 3));
                    }
                    this.Map.roofGrid.SetRoof(cell, null);
                }
            }
        }
        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();  
            if (this.DestroyedOrNull())
            {
                return;
            }
            if ((Find.TickManager.TicksGame >= this.takeOffTick)
                && (this.Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare) == false))
            {
                this.Destroy(DestroyMode.Vanish);
            }
        }

        public virtual void RequestTakeOff()
        {
            this.takeOffTick = Find.TickManager.TicksGame;
        }

        // ===================== Other functions =====================
        public TraderKindDef GetCargoKind(SpaceshipKind spaceshipKind)
        {
            TraderKindDef cargokind = Util_TraderKindDefOf.spaceshipCargoPeriodicSupply;
            switch (spaceshipKind)
            {
                case SpaceshipKind.CargoPeriodic:
                    cargokind = Util_TraderKindDefOf.spaceshipCargoPeriodicSupply;
                    break;
                case SpaceshipKind.CargoRequested:
                    cargokind = Util_TraderKindDefOf.spaceshipCargoRequestedSupply;
                    break;
                case SpaceshipKind.Damaged:
                    cargokind = Util_TraderKindDefOf.spaceshipCargoDamaged;
                    break;
                case SpaceshipKind.DispatcherDrop:
                case SpaceshipKind.DispatcherPick:
                    cargokind = Util_TraderKindDefOf.spaceshipCargoDispatcher;
                    break;
                case SpaceshipKind.Medical:
                    cargokind = Util_TraderKindDefOf.spaceshipCargoMedical;
                    break;
                default:
                    Log.ErrorOnce("MiningCo. Spaceship: unhandled SpaceshipKind (" + this.spaceshipKind.ToString() + ") in Building_Spaceship.GetCargoKind.", 123456781);
                    break;
            }
            return cargokind;
        }

        public void SpawnSurvivingPawns()
        {
            List<Pawn> survivingPawns = new List<Pawn>();
            foreach (Pawn pawn in this.pawnsAboard)
            {
                GenSpawn.Spawn(pawn, this.OccupiedRect().Cells.RandomElement(), this.Map);
                Expedition.RandomlyDamagePawn(pawn, Rand.RangeInclusive(1, 4), Rand.RangeInclusive(4, 16));
                if (pawn.Dead == false)
                {
                    survivingPawns.Add(pawn);
                }
            }
            this.pawnsAboard.Clear();
            if (survivingPawns.Count > 0)
            {
                IntVec3 exitSpot = IntVec3.Invalid;
                bool exitSpotIsValid = RCellFinder.TryFindBestExitSpot(survivingPawns.First(), out exitSpot, TraverseMode.PassAllDestroyableThings);
                if (exitSpotIsValid)
                {
                    Lord lord = LordMaker.MakeNewLord(Util_Faction.MiningCoFaction, new LordJob_ExitMap(exitSpot), this.Map, survivingPawns);
                }
            }
        }

        public void SpawnExplosions()
        {
            for (int explosionIndex = 0; explosionIndex < 5; explosionIndex++)
            {
                GenExplosion.DoExplosion(this.Position + IntVec3Utility.RandomHorizontalOffset(5f), this.Map, Rand.Range(3f, 7f), DamageDefOf.Bomb, this, Rand.Range(8, 45));
            }

        }

        public void SpawnFuelPuddleAndFire()
        {
            for (int fireIndex = 0; fireIndex < 150; fireIndex++)
            {
                IntVec3 spawnCell = this.Position + IntVec3Utility.RandomHorizontalOffset(12f);
                GenSpawn.Spawn(ThingDefOf.FilthFuel, spawnCell, this.Map);
                Fire fire = GenSpawn.Spawn(ThingDefOf.Fire, spawnCell, this.Map) as Fire;
                fire.fireSize = Rand.Range(0.25f, 1.25f);
            }
        }

        public int SpawnCargoContent(float integrity)
        {
            int cargoFeeInSilver = 0;
            for (int thingIndex = 0; thingIndex < this.things.Count; thingIndex++)
            {
                Thing thing = this.things[thingIndex];
                int quantity = Mathf.RoundToInt(thing.stackCount * integrity);
                SpawnItem(thing.def, thing.Stuff, quantity, this.Position, this.Map, 12f);
                cargoFeeInSilver += Mathf.RoundToInt(thing.def.BaseMarketValue * quantity);
            }
            return cargoFeeInSilver;
        }

        public static Thing SpawnItem(ThingDef itemDef, ThingDef stuff, int quantity, IntVec3 position, Map map, float radius)
        {
            Thing item = null;
            int remainingQuantity = quantity;
            while (remainingQuantity > 0)
            {
                int stackCount = 0;
                if (remainingQuantity > itemDef.stackLimit)
                {
                    stackCount = itemDef.stackLimit;
                }
                else
                {
                    stackCount = remainingQuantity;
                }
                remainingQuantity -= stackCount;
                item = ThingMaker.MakeThing(itemDef, stuff);
                item.stackCount = stackCount;
                Thing placedItem = null;
                GenDrop.TryDropSpawn(item, position + IntVec3Utility.RandomHorizontalOffset(radius), map, ThingPlaceMode.Near, out placedItem);
            }
            return item;
        }

        public void SpawnPayment(int pawnsCount)
        {
            int paymentTotalAmount = Util_Spaceship.feePerPawnInSilver * pawnsCount;
            Thing item = SpawnItem(ThingDefOf.Silver, null, paymentTotalAmount, this.Position, this.Map, 0f);
            Messages.Message("A dispatcher spaceship paid you " + paymentTotalAmount + " silver for using your landing pad.", item, MessageTypeDefOf.PositiveEvent);
        }

        public virtual void Notify_PawnBoarding(Pawn pawn, bool isLastLordPawn)
        {
            this.pawnsAboard.Add(pawn);
            pawn.DeSpawn();
        }

        public bool IsTakeOffImminent(int marginTimeInTicks)
        {
            int limitTime = Math.Max(0, this.takeOffTick - marginTimeInTicks);
            if (Find.TickManager.TicksGame >= limitTime)
            {
                return true;
            }
            return false;
        }

        // ===================== Cargo content =====================
        public ThingOwner things = null;
        public void GenerateThings()
        {
            ItemCollectionGeneratorParams parms = default(ItemCollectionGeneratorParams);
            parms.traderDef = this.cargoKind;
            parms.tile = this.Map.Tile;
            this.things.TryAddRangeOrTransfer(ItemCollectionGeneratorDefOf.TraderStock.Worker.Generate(parms), true);
        }
        public ThingOwner GetDirectlyHeldThings()
        {
            return this.things;
        }
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }
        
        // ===================== Float menu options =====================
        public IEnumerable<FloatMenuOption> GetFloatMenuOptionsCannotReach(Pawn selPawn)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            FloatMenuOption noPathOption = new FloatMenuOption("CannotUseNoPath".Translate(), null);
            options.Add(noPathOption);
            return options;
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000112;

            if (this.takeOffRequestIsEnabled)
            {
                Command_Action requestTakeOffButton = new Command_Action();
                requestTakeOffButton.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");
                requestTakeOffButton.defaultLabel = "Request ship take-off";
                requestTakeOffButton.defaultDesc = "Request ship take-off. Note: solar flare and other events may delay actual take-off.";
                requestTakeOffButton.activateSound = SoundDef.Named("Click");
                requestTakeOffButton.action = new Action(RequestTakeOff);
                requestTakeOffButton.groupKey = groupKeyBase + 1;
                buttonList.Add(requestTakeOffButton);
            }

            IEnumerable<Gizmo> resultButtonList;
            IEnumerable<Gizmo> basebuttonList = base.GetGizmos();
            if (basebuttonList != null)
            {
                resultButtonList = basebuttonList.Concat(buttonList);
            }
            else
            {
                resultButtonList = buttonList;
            }
            return resultButtonList;
        }
    }
}
