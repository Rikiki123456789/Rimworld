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
    /// Order a pawn to go and fish at a fishing zone.
    /// </summary>
    public class JobDriver_FishAtFishingZone : JobDriver
    {
        public PathEndMode pathEndMode = PathEndMode.OnCell;
        public TargetIndex fishingSpotIndex = TargetIndex.A;
        public Mote fishingRodMote = null;
        public IntVec3 cardinalDir = IntVec3.Invalid;

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.TargetLocA, this.job);
        }

        public bool FishingForbiddenOrNoFishAtTargetLocA()
        {
            Zone_Fishing fishingZone = this.Map.zoneManager.ZoneAt(this.TargetLocA) as Zone_Fishing;
            if ((fishingZone == null)
                || (fishingZone.allowFishing == false)
                || (fishingZone.fishesPosition.Contains(this.TargetLocA) == false))
            {
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntVec3>(ref this.cardinalDir, "cardinalDir");
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            const float baseFishingDuration = 2400f;
            const float skillGainPerTick = 0.15f;
            const float catchSuccessRateInZone = 0.70f;

            int fishingDuration = (int)baseFishingDuration;
            Passion passion = Passion.None;

            // Compute fishing duration.
            float fishingSkillLevel = 0f;
            fishingSkillLevel = this.pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDefOf.Hunting);
            float fishingSkillDurationFactor = fishingSkillLevel / 20f;
            fishingDuration = (int)(baseFishingDuration * (1.5f  - fishingSkillDurationFactor));

            // Compute pawn rotation.
            if (cardinalDir == IntVec3.Invalid)
            {
                cardinalDir = GenAdj.CardinalDirections.RandomElement();
                foreach (IntVec3 direction in GenAdj.CardinalDirections.InRandomOrder())
                {
                    if (Util_Zone_Fishing.IsAquaticTerrain(this.Map, this.TargetLocA + direction))
                    {
                        cardinalDir = direction;
                        break;
                    }
                }
                this.pawn.CurJob.SetTarget(TargetIndex.B, this.TargetLocA + cardinalDir);
                this.rotateToFace = TargetIndex.B;
            }

            yield return Toils_Goto.GotoCell(this.TargetLocA, this.pathEndMode).FailOn(FishingForbiddenOrNoFishAtTargetLocA);
            
            Toil fishToil = new Toil()
            {
                initAction = () =>
                {
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
                    this.pawn.skills.Learn(SkillDefOf.Shooting, skillGainPerTick);

                    // Spawn mote or maintain it.
                    if (this.fishingRodMote.DestroyedOrNull())
                    {
                        IntVec3 motePosition = this.TargetLocA + cardinalDir;
                        ThingDef moteDef = null;
                        if (cardinalDir == new IntVec3(0, 0, 1))
                        {
                            moteDef = Util_FishIndustry.MoteFishingRodNorthDef;
                        }
                        else if (cardinalDir == new IntVec3(1, 0, 0))
                        {
                            moteDef = Util_FishIndustry.MoteFishingRodEastDef;
                        }
                        else if (cardinalDir == new IntVec3(0, 0, -1))
                        {
                            moteDef = Util_FishIndustry.MoteFishingRodSouthDef;
                        }
                        else
                        {
                            moteDef = Util_FishIndustry.MoteFishingRodWestDef;
                        }
                        this.fishingRodMote = (Mote)ThingMaker.MakeThing(moteDef, null);
                        this.fishingRodMote.exactPosition = motePosition.ToVector3Shifted();
                        this.fishingRodMote.Scale = 1f;
                        GenSpawn.Spawn(this.fishingRodMote, motePosition, this.Map);
                    }
                    else
                    {
                        this.fishingRodMote.Maintain();
                    }
                },
                defaultDuration = fishingDuration,
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            yield return fishToil.WithProgressBarToilDelay(this.fishingSpotIndex).FailOn(FishingForbiddenOrNoFishAtTargetLocA); ;
            
            Toil catchFishToil = new Toil()
            {
                initAction = () =>
                {
                    Thing fishingCatch = null;

                    bool catchIsSuccessful = (Rand.Value <= catchSuccessRateInZone);
                    if (catchIsSuccessful == false)
                    {
                        MoteMaker.ThrowMetaIcon(this.pawn.Position, this.Map, ThingDefOf.Mote_IncapIcon);
                        this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }

                    float catchSelectorValue = Rand.Value;
                    
                    if ((catchSelectorValue < 0.02)
                        && Util_FishIndustry.GetFishSpeciesList(this.Map.Biome).Contains(Util_FishIndustry.TailteethPawnKindDef as PawnKindDef_FishSpecies))
                    {
                        // Get hurt by a tailteeth.
                        this.pawn.TakeDamage(new DamageInfo(DamageDefOf.Bite, Rand.Range(5, 12)));
                        Messages.Message(this.pawn.NameStringShort + "FishIndustry.FisherBitten".Translate(), this.pawn, MessageTypeDefOf.NegativeHealthEvent);
                        this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }
                    else if (catchSelectorValue < 0.04)
                    {
                        // Find oysters.
                        fishingCatch = GenSpawn.Spawn(Util_FishIndustry.OysterDef, this.pawn.Position, this.Map);
                        fishingCatch.stackCount = Rand.RangeInclusive(5, 27);
                    }
                    else
                    {
                        // Catch a fish.
                        bool fishSpotIsOcean = (this.Map.terrainGrid.TerrainAt(this.TargetLocA) == TerrainDefOf.WaterOceanShallow)
                                            || (this.Map.terrainGrid.TerrainAt(this.TargetLocA) == TerrainDefOf.WaterOceanDeep);
                        bool fishSpotIsMarshy = (this.Map.terrainGrid.TerrainAt(this.TargetLocA) == TerrainDef.Named("Marsh"));

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
                            Zone_Fishing fishingZone = this.Map.zoneManager.ZoneAt(this.TargetLocA) as Zone_Fishing;
                            if ((fishingZone != null)
                                && fishingZone.fishesPosition.Contains(this.TargetLocA))
                            {
                                fishingZone.fishesPosition.Remove(this.TargetLocA);
                            }
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
