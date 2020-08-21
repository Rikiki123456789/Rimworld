using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
//using RimWorld.Planet;
//using RimWorld.SquadAI;


namespace FishIndustry
{
    /// <summary>
    /// Order a pawn to go and fish at the fishing pier.
    /// </summary>
    public class JobDriver_FishAtFishingPier : JobDriver
    {
        public const int grainCountToAttractFishes = 5;

        public TargetIndex fishingPierIndex = TargetIndex.A;
        public Mote fishingRodMote = null;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.TargetA, this.job);
        }

        public bool FishingForbiddenOrPierDestroyedOrNoFish()
        {
            Building_FishingPier fishingPier = this.TargetThingA as Building_FishingPier;
            if ((fishingPier.DestroyedOrNull())
                || fishingPier.IsBurning()
                || fishingPier.IsForbidden(fishingPier.Faction)
                || (fishingPier.allowFishing == false)
                || (fishingPier.fishStock == 0))
            {
                return true;
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            const float baseFishingDuration = 1600f;
            const float catchSuccessRateOnPier = 0.90f;
            const float skillGainPerTick = 0.15f;

            int fishingDuration = (int)baseFishingDuration;
            Building_FishingPier fishingPier = this.TargetThingA as Building_FishingPier;
            int thrownGrainCount = 0;
            int nextGrainThrowTick = 0;

            this.FailOnBurningImmobile(fishingPierIndex);

            // Drawing.
            this.rotateToFace = TargetIndex.B;
            this.pawn.CurJob.SetTarget(TargetIndex.C, fishingPier.riverCell);
            TargetIndex riverCellIndex = TargetIndex.C;

            // Compute fishing duration.
            float fishingSkillLevel = 0f;
            fishingSkillLevel = this.pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDefOf.Hunting);
            float fishingSkillDurationFactor = fishingSkillLevel / 20f;
            fishingDuration = (int)(baseFishingDuration * (1.5f  - fishingSkillDurationFactor));

            yield return Toils_Goto.GotoThing(fishingPierIndex, fishingPier.riverCell).FailOn(FishingForbiddenOrPierDestroyedOrNoFish);

            Toil attractfishesToil = new Toil()
            {
                tickAction = () =>
                {
                    if (fishingPier.allowUsingGrain == false)
                    {
                        this.ReadyForNextToil();
                        return;
                    }
                    if (Find.TickManager.TicksGame >= nextGrainThrowTick)
                    {
                        nextGrainThrowTick = Find.TickManager.TicksGame + 3 * GenTicks.TicksPerRealSecond;
                        // Look for grain in inventory.
                        Thing grain = null;
                        foreach (Thing thing in this.pawn.inventory.innerContainer)
                        {
                            if ((thing.def == Util_FishIndustry.RawCornDef)
                                || (thing.def == Util_FishIndustry.RawRiceDef))
                            {
                                grain = thing;
                                break;
                            }
                        }
                        if ((grain == null)
                            || (thrownGrainCount >= grainCountToAttractFishes))
                        {
                            fishingDuration -= Mathf.CeilToInt((float)fishingDuration * 0.75f * ((float)thrownGrainCount / (float)grainCountToAttractFishes));
                            this.ReadyForNextToil();
                            return;
                        }
                        // Throw grains.
                        thrownGrainCount++;
                        this.pawn.inventory.innerContainer.Take(grain, 1);
                        for (int cornGrainIndex = 0; cornGrainIndex < 5; cornGrainIndex++)
                        {
                            if (!this.pawn.Position.ShouldSpawnMotesAt(this.pawn.Map) || this.pawn.Map.moteCounter.Saturated)
                            {
                                break;
                            }
                            float speed = Rand.Range(1.5f, 3f);
                            Vector3 targetPosition = this.pawn.Position.ToVector3Shifted() + new Vector3(0f, 0f, 2f).RotatedBy(fishingPier.Rotation.AsAngle) + Vector3Utility.RandomHorizontalOffset(1.5f);
                            targetPosition.y = this.pawn.DrawPos.y;
                            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Stone, null);
                            moteThrown.Scale = 0.2f;
                            moteThrown.rotationRate = (float)Rand.Range(-300, 300);
                            moteThrown.exactPosition = this.pawn.DrawPos;
                            moteThrown.SetVelocity((targetPosition - moteThrown.exactPosition).AngleFlat(), speed);
                            moteThrown.airTimeLeft = (float)Mathf.RoundToInt((moteThrown.exactPosition - targetPosition).MagnitudeHorizontal() / speed);
                            GenSpawn.Spawn(moteThrown, this.pawn.Position, this.pawn.Map);
                            MoteMaker.MakeWaterSplash(targetPosition, this.pawn.Map, 1.8f, 0.5f);
                        }
                    }
                },
                defaultDuration = fishingDuration,
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            yield return attractfishesToil.FailOn(FishingForbiddenOrPierDestroyedOrNoFish);
            
            Toil fishToil = new Toil()
            {
                initAction = () =>
                {
                    // Set toil duration according to number of used grains.
                    this.CurToil.defaultDuration = fishingDuration;
                    this.ticksLeftThisToil = fishingDuration;

                    ThingDef moteDef = null;
                    if (fishingPier.Rotation == Rot4.North)
                    {
                        moteDef = Util_FishIndustry.MoteFishingRodNorthDef;
                    }
                    else if (fishingPier.Rotation == Rot4.East)
                    {
                        moteDef = Util_FishIndustry.MoteFishingRodEastDef;
                    }
                    else if (fishingPier.Rotation == Rot4.South)
                    {
                        moteDef = Util_FishIndustry.MoteFishingRodSouthDef;
                    }
                    else
                    {
                        moteDef = Util_FishIndustry.MoteFishingRodWestDef;
                    }
                    this.fishingRodMote = (Mote)ThingMaker.MakeThing(moteDef, null);
                    this.fishingRodMote.exactPosition = fishingPier.fishingSpotCell.ToVector3Shifted();
                    this.fishingRodMote.Scale = 1f;
                    GenSpawn.Spawn(this.fishingRodMote, fishingPier.fishingSpotCell, this.Map);
                },
                tickAction = () =>
                {
                    this.pawn.skills.Learn(SkillDefOf.Shooting, skillGainPerTick);

                    if (this.fishingRodMote != null)
                    {
                        this.fishingRodMote.Maintain();
                    }
                },
                defaultDuration = fishingDuration,
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            yield return fishToil.WithProgressBarToilDelay(riverCellIndex).FailOn(FishingForbiddenOrPierDestroyedOrNoFish);
            
            Toil catchFishToil = new Toil()
            {
                initAction = () =>
                {
                    Thing fishingCatch = null;

                    bool catchIsSuccessful = (Rand.Value <= catchSuccessRateOnPier);
                    if (catchIsSuccessful == false)
                    {
                        MoteMaker.ThrowMetaIcon(this.pawn.Position, this.Map, ThingDefOf.Mote_IncapIcon);
                        this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }

                    float catchSelectorValue = Rand.Value;

                    if (catchSelectorValue < 0.02)
                    {
                        float bonusCatchValue = Rand.Value;
                        if (bonusCatchValue < 0.01f)
                        {
                            // Really small chance to find a sunken treasure!!!
                            fishingCatch = GenSpawn.Spawn(ThingDefOf.Gold, this.pawn.Position, this.Map);
                            fishingCatch.stackCount = Rand.RangeInclusive(58, 289);
                            Thing treasureSilver = GenSpawn.Spawn(ThingDefOf.Silver, fishingPier.middleCell, this.Map);
                            treasureSilver.stackCount = Rand.RangeInclusive(237, 2154);
                            Find.LetterStack.ReceiveLetter("FishIndustry.LetterLabelSunkenTreasure".Translate(), "FishIndustry.SunkenTreasure".Translate(this.pawn.Name.ToStringShort.CapitalizeFirst()),
                                LetterDefOf.PositiveEvent, this.pawn);
                        }
                        else if (bonusCatchValue < 0.02f)
                        {
                            // Really small chance to find a complete power armor set + sniper or charge rifle.
                            Thing powerArmor = GenSpawn.Spawn(ThingDef.Named("Apparel_PowerArmor"), this.pawn.Position, this.Map);
                            fishingCatch = powerArmor; // Used to carry the power armor.
                            Thing powerArmorHelmet = GenSpawn.Spawn(ThingDef.Named("Apparel_PowerArmorHelmet"), this.pawn.Position, this.Map);
                            Thing rifle = null;
                            if (Rand.Value < 0.5f)
                            {
                                rifle = GenSpawn.Spawn(ThingDef.Named("Gun_ChargeRifle"), this.pawn.Position, this.Map);
                            }
                            else
                            {
                                rifle = GenSpawn.Spawn(ThingDef.Named("Gun_SniperRifle"), this.pawn.Position, this.Map);
                            }
                            CompQuality qualityComp = powerArmor.TryGetComp<CompQuality>();
                            if (qualityComp != null)
                            {
                                qualityComp.SetQuality(QualityCategory.Masterwork, ArtGenerationContext.Outsider);
                            }
                            qualityComp = powerArmorHelmet.TryGetComp<CompQuality>();
                            if (qualityComp != null)
                            {
                                qualityComp.SetQuality(QualityCategory.Masterwork, ArtGenerationContext.Outsider);
                            }
                            qualityComp = rifle.TryGetComp<CompQuality>();
                            if (qualityComp != null)
                            {
                                qualityComp.SetQuality(QualityCategory.Masterwork, ArtGenerationContext.Outsider);
                            }

                            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile);
                            Pawn deadMarine = PawnGenerator.GeneratePawn(PawnKindDefOf.AncientSoldier, faction);
                            HealthUtility.DamageUntilDead(deadMarine);
                            Corpse corpse = deadMarine.ParentHolder as Corpse;
                            CompRottable rotComp = corpse.TryGetComp<CompRottable>();
                            if (rotComp != null)
                            {
                                rotComp.RotProgress = 20f * GenDate.TicksPerDay; // 20 days so the corpse is dessicated.
                            }
                            GenSpawn.Spawn(corpse, fishingPier.bankCell, this.Map);
                            string eventText = "FishIndustry.LetterDescDeadMarine".Translate(this.pawn.Name.ToStringShort.CapitalizeFirst());
                            Find.LetterStack.ReceiveLetter("FishIndustry.LetterLabelDeadMarine".Translate(), eventText, LetterDefOf.PositiveEvent, this.pawn);
                        }
                        else
                        {
                            // Find a small amount of gold.
                            fishingCatch = GenSpawn.Spawn(ThingDefOf.Gold, this.pawn.Position, this.Map);
                            fishingCatch.stackCount = Rand.RangeInclusive(1, 7);
                        }
                    }
                    else if (catchSelectorValue < 0.04)
                    {
                        // Find oysters.
                        fishingCatch = GenSpawn.Spawn(Util_FishIndustry.OysterDef, this.pawn.Position, this.Map);
                        fishingCatch.stackCount = Rand.RangeInclusive(2, 9);
                    }
                    else
                    {
                        // Catch a fish.
                        IntVec3 fishSpot = fishingPier.aquaticCells.RandomElement();
                        bool fishSpotIsOcean = (this.Map.terrainGrid.TerrainAt(fishSpot) == TerrainDefOf.WaterOceanShallow)
                                            || (this.Map.terrainGrid.TerrainAt(fishSpot) == TerrainDefOf.WaterOceanDeep);
                        bool fishSpotIsMarshy = (this.Map.terrainGrid.TerrainAt(fishSpot) == TerrainDef.Named("Marsh"));

                        PawnKindDef caugthFishDef = null;
                        if (fishSpotIsOcean)
                        {
                            caugthFishDef = (from fishSpecies in Util_FishIndustry.GetFishSpeciesList(this.Map.Biome)
                                             where fishSpecies.livesInOcean
                                             select fishSpecies).RandomElementByWeight((PawnKindDef_FishSpecies def) => def.commonality);
                        }
                        else if (fishSpotIsMarshy)
                        {
                            caugthFishDef = (from fishSpecies in Util_FishIndustry.GetFishSpeciesList(this.Map.Biome)
                                             where fishSpecies.livesInMarsh
                                             select fishSpecies).RandomElementByWeight((PawnKindDef_FishSpecies def) => def.commonality);
                        }
                        else
                        {
                            caugthFishDef = (from fishSpecies in Util_FishIndustry.GetFishSpeciesList(this.Map.Biome)
                                             where fishSpecies.livesInRiver
                                             select fishSpecies).RandomElementByWeight((PawnKindDef_FishSpecies def) => def.commonality);
                        }
                        Pawn caughtFish = PawnGenerator.GeneratePawn(caugthFishDef);
                        ExecutionUtility.DoExecutionByCut(this.pawn, caughtFish);
                        Corpse corpse = caughtFish.ParentHolder as Corpse;
                        GenSpawn.Spawn(corpse, this.pawn.Position, this.Map);
                        fishingCatch = corpse;
                        fishingCatch.SetForbidden(false);
                        if (caughtFish.BodySize >= 0.1f)
                        {
                            fishingPier.fishStock--;
                        }
                    }
                    IntVec3 storageCell;
                    if (StoreUtility.TryFindBestBetterStoreCellFor(fishingCatch, this.pawn, this.Map, StoragePriority.Unstored, this.pawn.Faction, out storageCell, true))
                    {
                        this.pawn.Reserve(fishingCatch, this.job);
                        this.pawn.Reserve(storageCell, this.job);
                        this.pawn.CurJob.SetTarget(TargetIndex.B, storageCell);
                        this.pawn.CurJob.SetTarget(TargetIndex.A, fishingCatch);
                        this.pawn.CurJob.count = 9999;
                        this.pawn.CurJob.haulMode = HaulMode.ToCellStorage;
                    }
                    else
                    {
                        this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                    }
                }
            };
            yield return catchFishToil;

            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            yield return carryToCell;

            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
        }
    }
} 
