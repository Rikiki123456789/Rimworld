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
        public TargetIndex fishingPierIndex = TargetIndex.A;
        public Mote fishingRodMote = null;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            const float baseFishingDuration = 2000f;

            int fishingDuration = (int)baseFishingDuration;
            float catchSomethingThreshold = 0f;
            Building_FishingPier fishingPier = this.TargetThingA as Building_FishingPier;
            Passion passion = Passion.None;
            const float skillGainPerTick = 0.15f;
            float skillGainFactor = 0f;

            this.AddEndCondition(() =>
            {
                var targ = this.pawn.jobs.curJob.GetTarget(fishingPierIndex).Thing;
                if (targ is Building && !targ.Spawned)
                    return JobCondition.Incompletable;
                return JobCondition.Ongoing;
            });

            this.FailOnBurningImmobile(fishingPierIndex); // Bill giver or product burning in carry phase.

            this.rotateToFace = TargetIndex.B;

            yield return Toils_Reserve.Reserve(fishingPierIndex);

            float fishingSkillLevel = 0f;
            fishingSkillLevel = this.pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDefOf.Hunting);
            float fishingSkillDurationFactor = fishingSkillLevel / 20f;
            fishingDuration = (int)(baseFishingDuration * (1.5f  - fishingSkillDurationFactor));

            yield return Toils_Goto.GotoThing(fishingPierIndex, fishingPier.riverCell).FailOnDespawnedOrNull(fishingPierIndex);
            
            Toil fishToil = new Toil()
            {
                initAction = () =>
                {
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
                    WorkTypeDef fishingWorkDef = WorkTypeDefOf.Hunting;
                    passion = this.pawn.skills.MaxPassionOfRelevantSkillsFor(fishingWorkDef);
                    if (passion == Passion.None)
                    {
                        skillGainFactor = 0.3f;
                    }
                    else if (passion == Passion.Minor)
                    {
                        skillGainFactor = 1f;
                    }
                    else
                    {
                        skillGainFactor = 1.5f;
                    }
                },
                tickAction = () =>
                {
                    if (passion == Passion.Minor)
                    {
                        this.pawn.needs.joy.GainJoy(NeedTunings.JoyPerXpForPassionMinor, JoyKindDefOf.Work);
                    }
                    else if (passion == Passion.Major)
                    {
                        this.pawn.needs.joy.GainJoy(NeedTunings.JoyPerXpForPassionMajor, JoyKindDefOf.Work);
                    }
                    this.pawn.skills.Learn(SkillDefOf.Shooting, skillGainPerTick * skillGainFactor);

                    if (this.ticksLeftThisToil == 1)
                    {
                        if (this.fishingRodMote != null)
                        {
                            this.fishingRodMote.Destroy();
                        }
                    }
                },
                defaultDuration = fishingDuration,
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            yield return fishToil.WithProgressBarToilDelay(fishingPierIndex);
            
            Toil computeChanceToCatchToil = new Toil()
            {
                initAction = () =>
                {
                    catchSomethingThreshold = fishingSkillLevel / 20f;
                    // Reframe min and max chance (min 5%, max 75 % chance of success).
                    Mathf.Clamp(catchSomethingThreshold, 0.05f, 0.75f);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return computeChanceToCatchToil;

            Toil catchFishToil = new Toil()
            {
                initAction = () =>
                {
                    Job curJob = this.pawn.jobs.curJob;
                    Thing fishingCatch = null;

                    // 90% chance to successfully catch something.
                    bool catchIsSuccessful = (Rand.Value >= 0.1f);
                    if (catchIsSuccessful == false)
                    {
                        MoteMaker.ThrowMetaIcon(this.pawn.Position, this.Map, ThingDefOf.Mote_IncapIcon);
                        this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }

                    float catchSelectorValue = Rand.Value;
                    if (catchSelectorValue > 0.04f)
                    {
                        // Catch a fish.
                        bool fishSpotIsOcean = (this.Map.terrainGrid.TerrainAt(fishingPier.fishingSpotCell) == TerrainDefOf.WaterOceanShallow)
                                            || (this.Map.terrainGrid.TerrainAt(fishingPier.fishingSpotCell) == TerrainDefOf.WaterOceanDeep);
                        bool fishSpotIsMarshy = (this.Map.terrainGrid.TerrainAt(fishingPier.fishingSpotCell) == TerrainDef.Named("Marsh"));

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
                        GenSpawn.Spawn(caughtFish, this.pawn.Position, this.Map);
                        HealthUtility.DamageUntilDead(caughtFish);
                        foreach (Thing thing in this.pawn.Position.GetThingList(this.Map))
                        {
                            Corpse fishCorpse = thing as Corpse;
                            if (fishCorpse != null)
                            {
                                fishingCatch = fishCorpse;
                                fishingCatch.SetForbidden(false);
                            }
                        }
                        if (caughtFish.BodySize >= 0.1f)
                        {
                            fishingPier.fishStock--;
                            fishingPier.ComputeMaxFishStockAndRespawnPeriod();
                        }
                    }
                    else if (catchSelectorValue > 0.02)
                    {
                        fishingCatch = GenSpawn.Spawn(Util_FishIndustry.OysterDef, this.pawn.Position, this.Map);
                        fishingCatch.stackCount = Rand.RangeInclusive(5, 27);
                    }
                    else
                    {
                        float bonusCatchValue = Rand.Value;
                        if (bonusCatchValue < 0.01f)
                        {
                            // Really small chance to find a sunken treasure!!!
                            fishingCatch = GenSpawn.Spawn(ThingDefOf.Gold, this.pawn.Position, this.Map);
                            fishingCatch.stackCount = Rand.RangeInclusive(58, 289);
                            Thing treasureSilver = GenSpawn.Spawn(ThingDefOf.Silver, fishingPier.middleCell, this.Map);
                            treasureSilver.stackCount = Rand.RangeInclusive(237, 2154);
                            string eventText = this.pawn.Name.ToStringShort.CapitalizeFirst() + " has found a sunken treasure while fishing! What a good catch!\n";
                            Find.LetterStack.ReceiveLetter("Sunken treasure!", eventText, LetterDefOf.Good, this.pawn);
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

                            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.SpacerHostile);
                            Pawn deadMarine = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, faction);
                            GenSpawn.Spawn(deadMarine, fishingPier.bankCell, this.Map);
                            HealthUtility.DamageUntilDead(deadMarine);
                            List<Thing> thingsList = deadMarine.Position.GetThingList(this.Map);
                            foreach (Thing thing in thingsList)
                            {
                                if (thing.def.defName.Contains("Corpse"))
                                {
                                    CompRottable rotComp = thing.TryGetComp<CompRottable>();
                                    if (rotComp != null)
                                    {
                                        rotComp.RotProgress = 20f * GenDate.TicksPerDay; // 20 days so the corpse is dessicated.
                                    }
                                }
                            }
                            string eventText = this.pawn.Name.ToStringShort.CapitalizeFirst() + " has cought a dead body while fishing!\n\n'This is really disgusting but look at his gear! This guy was probably a MiningCo. security member. I wonder what happend to him...'\n";
                            Find.LetterStack.ReceiveLetter("Dead marine", eventText, LetterDefOf.Good, this.pawn);
                        }
                        else
                        {
                            // Find a small amount of gold.
                            fishingCatch = GenSpawn.Spawn(ThingDefOf.Gold, this.pawn.Position, this.Map);
                            fishingCatch.stackCount = Rand.RangeInclusive(1, 7);
                        }
                        // TODO: add chance to get hurt by a tailteeth (missing finger or even hand!).
                    }
                    IntVec3 storageCell;
                    if (StoreUtility.TryFindBestBetterStoreCellFor(fishingCatch, this.pawn, this.Map, StoragePriority.Unstored, this.pawn.Faction, out storageCell, true))
                    {
                        this.pawn.Reserve(fishingCatch, 1);
                        this.pawn.Reserve(storageCell, 1);
                        this.pawn.CurJob.SetTarget(TargetIndex.B, storageCell);
                        this.pawn.CurJob.SetTarget(TargetIndex.A, fishingCatch);
                        this.pawn.CurJob.count = 1;
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

            yield return Toils_Reserve.Release(fishingPierIndex);
        }
    }
}
