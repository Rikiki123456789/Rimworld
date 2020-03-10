using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{
    public class Building_SpaceshipCargo : Building_Spaceship, ITrader
    {
        public override bool takeOffRequestIsEnabled
        {
            get
            {
                return true;
            }
        }

        public TradeCurrency TradeCurrency
        {
            get
            {
                return TradeCurrency.Silver;
            }
        }

        // ===================== Setup work =====================
        public void InitializeData_Cargo(Faction faction, int hitPoints, int landingDuration, SpaceshipKind spaceshipKind)
        {
            base.InitializeData(faction, hitPoints, landingDuration, spaceshipKind);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode != DestroyMode.KillFinalize)
            {
                // Spaceship is taking off.
                float shipHealthProportion = (float)this.HitPoints / this.MaxHitPoints;
                if (shipHealthProportion < 1f)
                {
                    int delayInTicks = 0;
                    if (cargoKind == Util_TraderKindDefOf.spaceshipCargoPeriodicSupply)
                    {
                        delayInTicks = Mathf.RoundToInt(2f * WorldComponent_Partnership.cargoSpaceshipPeriodicSupplyPeriodInTicks * (1f - shipHealthProportion));
                        Util_Misc.Partnership.nextPeriodicSupplyTick[this.Map] += delayInTicks;
                    }
                    else
                    {
                        delayInTicks = Mathf.RoundToInt(2f * WorldComponent_Partnership.cargoSpaceshipRequestedSupplyPeriodInTicks * (1f - shipHealthProportion));
                        Util_Misc.Partnership.nextRequestedSupplyMinTick[this.Map] += delayInTicks;
                    }
                    string spaceshipDamagedText = "-- Comlink with MiningCo. --\n\n"
                    + "\"Our cargo spaceship was damaged during the last supply.\n"
                    + "Repairs will take some times.\n\n"
                    + "Remember the MiningCo. partnership contract stipulates that you must ensure landing ships security!\"\n\n"
                    + "-- End of transmission --";
                    Find.LetterStack.ReceiveLetter("Cargo spaceship damaged", spaceshipDamagedText, LetterDefOf.NegativeEvent, new TargetInfo(this.Position, this.Map));
                }
            }
 	        base.Destroy(mode);
        }
        
        // ===================== Float menu options =====================
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            if (selPawn.CanReach(this, PathEndMode.ClosestTouch, Danger.Some, false, TraverseMode.ByPawn) == false)
            {
                return GetFloatMenuOptionsCannotReach(selPawn);
            }

            // Base options.
            foreach (FloatMenuOption option in base.GetFloatMenuOptions(selPawn))
            {
                options.Add(option);
            }

            // Trade option.
            if (this.IsBurning())
            {
                FloatMenuOption burningOption = new FloatMenuOption("CannotUseReason".Translate("BurningLower".Translate()), null);
                options.Add(burningOption);
            }
            else if (selPawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
            {
                FloatMenuOption incapableOption = new FloatMenuOption("CannotPrioritizeWorkTypeDisabled".Translate(SkillDefOf.Social.LabelCap), null);
                options.Add(incapableOption);
            }
            else
            {
                Action action2 = delegate
                {
                    Job job = new Job(Util_JobDefOf.TradeWithCargoSpaceship, this);
                    selPawn.jobs.TryTakeOrderedJob(job);

                };
                FloatMenuOption tradeOption = new FloatMenuOption("Trade with cargo spaceship", action2);
                options.Add(tradeOption);
            }
            return options;
        }

        // ===================== Inspect panel =====================
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (Find.TickManager.TicksGame >= this.takeOffTick)
            {
                stringBuilder.Append("Taking off ASAP");
            }
            else
            {
                stringBuilder.Append("Planned take-off: " + GenDate.ToStringTicksToPeriodVerbose(this.takeOffTick - Find.TickManager.TicksGame));
            }

            return stringBuilder.ToString();
        }

        // ===================== Trader =====================
        private int randomPriceFactorSeed = -1;

        public bool CanTradeNow
        {
            get
            {
                return ((this.DestroyedOrNull() == false)
                    && (this.IsBurning() == false));
            }
        }
        public IEnumerable<Thing> Goods
        {
            get
            {
                for (int i = 0; i < this.things.Count; i++)
                {
                    yield return this.things[i];
                }
            }
        }
        public int RandomPriceFactorSeed
        {
            get
            {
                return this.randomPriceFactorSeed;
            }
        }
        public float TradePriceImprovementOffsetForPlayer
        {
            get
            {
                return 0f;
            }
        }
        public TraderKindDef TraderKind
        {
            get
            {
                return this.cargoKind;
            }
        }
        public string TraderName
        {
            get
            {
                return "MiningCo. spaceship";
            }
        }

        public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
        {
            List<Thing> thingsWillingToBuy = new List<Thing>();
            foreach (Thing thing in TradeUtility.AllLaunchableThingsForTrade(this.Map))
            {
                thingsWillingToBuy.Add(thing);
            }
            foreach (IntVec3 cell in this.OccupiedRect().Cells)
            {
                foreach (Thing thing in cell.GetThingList(this.Map))
                {
                    if ((TradeUtility.EverPlayerSellable(thing.def))
                        && this.TraderKind.WillTrade(thing.def)
                        && (thingsWillingToBuy.Contains(thing) == false)) // Do not count thing twice.
                    {
                        thingsWillingToBuy.Add(thing);
                    }
                }
            }
            return thingsWillingToBuy;
        }
        public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
            SpawnItem(thing.def, thing.Stuff, thing.stackCount, this.Position, this.Map, 5f);
            thing.Destroy();
        }
        public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
            Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);
            if (thing2 != null)
            {
                if (!thing2.TryAbsorbStack(thing, false))
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
            else
            {
                this.things.TryAdd(thing, false);
            }
        }
    }
}
