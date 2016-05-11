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
        public enum FishingEquipment
        {
            NoEquipment = 0,
            Harpoon = 1,
            FishingRod = 2,
        };

        public TargetIndex fishingPierIndex = TargetIndex.A;
        FishingEquipment fishingEquipment = FishingEquipment.NoEquipment;
        MoteThrown fishingEquipmentMote = null;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.LookDeep<MoteThrown>(ref fishingEquipmentMote, "fishingEquipmentMote");
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            QualityCategory fishingEquipmentQuality = QualityCategory.Normal;
            float catchSomethingThreshold = 0f;
            Building_FishingPier fishingPier = this.TargetThingA as Building_FishingPier;
            Passion passion = Passion.None;
            int fishingDuration = 1000;
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

            yield return Toils_Reserve.Reserve(fishingPierIndex);

            float statValue = this.pawn.GetStatValue(Util_FishIndustry.FishingSpeedDef, true);
            fishingDuration = (int)Math.Round((double)(800f / statValue));

            yield return Toils_Goto.GotoThing(fishingPierIndex, fishingPier.riverCell).FailOnDespawnedOrNull(fishingPierIndex);

            Toil verifyFisherHasFishingEquipmentToil = new Toil()
            {
                initAction = () =>
                {
                    if ((this.pawn.equipment.Primary != null)
                        && (this.pawn.equipment.Primary.def == Util_FishIndustry.HarpoonDef))
                    {
                        this.fishingEquipment = FishingEquipment.Harpoon;
                        this.pawn.equipment.Primary.TryGetQuality(out fishingEquipmentQuality);
                    }
                    foreach (Apparel apparel in this.pawn.apparel.WornApparel)
                    {
                        if (apparel.def == Util_FishIndustry.FishingRodDef)
                        {
                            this.fishingEquipment = FishingEquipment.FishingRod;
                            apparel.TryGetQuality(out fishingEquipmentQuality);
                            break;
                        }
                    }
                    if (this.fishingEquipment == FishingEquipment.NoEquipment)
                    {
                        this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                }
            };
            yield return verifyFisherHasFishingEquipmentToil;

            Toil fishToil = new Toil()
            {
                initAction = () =>
                {
                    ThingDef moteDef = null;

                    switch (fishingEquipment)
                    {
                        case FishingEquipment.FishingRod:
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
                            break;
                    }
                    if (moteDef != null)
                    {
                        fishingEquipmentMote = (MoteThrown)ThingMaker.MakeThing(moteDef);
                        fishingEquipmentMote.exactPosition = fishingPier.fishingSpotCell.ToVector3Shifted();
                        fishingEquipmentMote.exactPosition.y = Altitudes.AltitudeFor(AltitudeLayer.VisEffects);
                        GenSpawn.Spawn(fishingEquipmentMote, fishingPier.fishingSpotCell);
                    }
                    WorkTypeDef fishingWorkDef = DefDatabase<WorkTypeDef>.GetNamed("Fishing");
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
                    switch (fishingEquipment)
                    {
                        case FishingEquipment.FishingRod:
                            fishingEquipmentMote.Maintain();
                            break;
                        case FishingEquipment.Harpoon:
                            if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
                            {
                                Bullet thrownHarpoon = GenSpawn.Spawn(Util_FishIndustry.HarpoonDef.Verbs.First().projectileDef, this.pawn.Position) as Bullet;
                                TargetInfo targetCell = new TargetInfo(fishingPier.fishingSpotCell + new IntVec3(Rand.RangeInclusive(-1, 1), 0, Rand.RangeInclusive(0, 2)).RotatedBy(fishingPier.Rotation));
                                thrownHarpoon.Launch(this.pawn, targetCell);
                            }
                            break;
                    }
                    this.pawn.Drawer.rotator.FaceCell(fishingPier.fishingSpotCell);

                    if (passion == Passion.Minor)
                    {
                        this.pawn.needs.joy.GainJoy(NeedTunings.JoyPerXpForPassionMinor, JoyKindDefOf.Work);
                    }
                    else if (passion == Passion.Major)
                    {
                        this.pawn.needs.joy.GainJoy(NeedTunings.JoyPerXpForPassionMajor, JoyKindDefOf.Work);
                    }
                    SkillDef fishingSkillDef = DefDatabase<SkillDef>.GetNamed("Fishing");
                    this.pawn.skills.Learn(fishingSkillDef, skillGainPerTick * skillGainFactor);
                },
                defaultDuration = fishingDuration,
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            yield return fishToil.WithProgressBarToilDelay(fishingPierIndex);

            yield return verifyFisherHasFishingEquipmentToil; // Could be dropped during fishToil.

            Toil computeChanceToCatchToil = new Toil()
            {
                initAction = () =>
                {
                    float fishingSkillLevel = 0f;
                    WorkTypeDef fishingWorkDef = DefDatabase<WorkTypeDef>.GetNamed("Fishing");
                    fishingSkillLevel = this.pawn.skills.AverageOfRelevantSkillsFor(fishingWorkDef);
                    float fishingEquipmentQualityFactor = (float)fishingEquipmentQuality / (float)QualityCategory.Legendary;
                    float fishingSkillFactor = fishingSkillLevel / 20f;
                    float snowFactor = 1 - Find.SnowGrid.GetDepth(fishingPier.fishingSpotCell);
                    float fishingEquipmentOffset = 0f;
                    switch (this.fishingEquipment)
                    {
                        case FishingEquipment.Harpoon:
                            fishingEquipmentOffset = 0.2f;
                            break;
                        case FishingEquipment.FishingRod:
                            fishingEquipmentOffset = 0.5f;
                            break;
                    }
                    catchSomethingThreshold = ((fishingEquipmentOffset * fishingEquipmentQualityFactor) + 0.4f * fishingSkillFactor) * (0.25f + 0.75f * snowFactor);
                    // Reframe min and max chance (min 5%, max 75 % chance of success).
                    catchSomethingThreshold = catchSomethingThreshold * 0.75f;
                    catchSomethingThreshold = Math.Max(catchSomethingThreshold, 0.05f);
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

                    bool catchIsSuccessful = Rand.Value <= catchSomethingThreshold;
                    if (catchIsSuccessful == false)
                    {
                        MoteThrower.ThrowDrift(this.pawn.Position, ThingDefOf.Mote_IncapIcon);
                        this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }

                    float catchSelectorValue = Rand.Value;
                    if (catchSelectorValue > 0.04f)
                    {
                        // Catch a fish.
                        TerrainDef fishSpotType = Find.TerrainGrid.TerrainAt(fishingPier.fishingSpotCell);
                        List<ThingDef> fishSpeciesList = null;
                        ThingDef_FishSpeciesProperties.AquaticEnvironment aquaticEnvironment;
                        ThingDef_FishSpeciesProperties.LivingTime livingTime;
                        float fishSpeciesTotalCommonality = 0f;
                        float fishSpeciesCommonalitySum = 0f;

                        // Aquatic environment.
                        if (fishSpotType == TerrainDef.Named("Marsh"))
                        {
                            aquaticEnvironment = ThingDef_FishSpeciesProperties.AquaticEnvironment.Marsh;
                        }
                        else
                        {
                            aquaticEnvironment = ThingDef_FishSpeciesProperties.AquaticEnvironment.Sea;
                        }
                        // Day time.
                        if (SkyManager.CurSkyGlow >= 0.4f)
                        {
                            livingTime = ThingDef_FishSpeciesProperties.LivingTime.Day;
                        }
                        else
                        {
                            livingTime = ThingDef_FishSpeciesProperties.LivingTime.Night;
                        }

                        fishSpeciesList = Util_FishIndustry.GetFishSpeciesList(aquaticEnvironment, livingTime);
                        fishSpeciesTotalCommonality = Util_FishIndustry.GetFishSpeciesTotalCommonality(aquaticEnvironment, livingTime);

                        float randomSelector = Rand.Range(0f, fishSpeciesTotalCommonality);
                        ThingDef selectedFishSpecies = null;
                        for (int fishSpeciesIndex = 0; fishSpeciesIndex < fishSpeciesList.Count; fishSpeciesIndex++)
                        {
                            ThingDef_FishSpeciesProperties currentFishSpecies = fishSpeciesList[fishSpeciesIndex] as ThingDef_FishSpeciesProperties;
                            fishSpeciesCommonalitySum += currentFishSpecies.commonality;

                            if (randomSelector <= fishSpeciesCommonalitySum)
                            {
                                selectedFishSpecies = currentFishSpecies;
                                break;
                            }
                        }

                        fishingCatch = GenSpawn.Spawn(selectedFishSpecies, this.pawn.Position);
                        fishingCatch.stackCount = (selectedFishSpecies as ThingDef_FishSpeciesProperties).catchQuantity;
                        fishingPier.fishStock--;
                    }
                    else if (catchSelectorValue > 0.02)
                    {
                        fishingCatch = GenSpawn.Spawn(Util_FishIndustry.OysterDef, this.pawn.Position);
                        fishingCatch.stackCount = Rand.RangeInclusive(5, 27);
                    }
                    else
                    {
                        float bonusCatchValue = Rand.Value;
                        if (bonusCatchValue < 0.01f)
                        {
                            // Really small chance to find a sunken treasure!!!
                            fishingCatch = GenSpawn.Spawn(ThingDefOf.Gold, this.pawn.Position);
                            fishingCatch.stackCount = Rand.RangeInclusive(58, 289);
                            Thing treasureSilver = GenSpawn.Spawn(ThingDefOf.Silver, fishingPier.middleCell);
                            treasureSilver.stackCount = Rand.RangeInclusive(237, 2154);
                            string eventText = this.pawn.Name.ToStringShort.CapitalizeFirst() + " has found a sunken treasure while fishing! What a good catch!\n";
                            Find.LetterStack.ReceiveLetter("Sunken treasure!", eventText, LetterType.Good, this.pawn.Position);
                        }
                        else if (bonusCatchValue < 0.02f)
                        {
                            // Really small chance to find a complete power armor set + sniper or charge rifle.
                            Thing powerArmor = GenSpawn.Spawn(ThingDef.Named("Apparel_PowerArmor"), this.pawn.Position);
                            fishingCatch = powerArmor; // Used to carry the power armor.
                            Thing powerArmorHelmet = GenSpawn.Spawn(ThingDef.Named("Apparel_PowerArmorHelmet"), this.pawn.Position);
                            Thing rifle = null;
                            if (Rand.Value < 0.5f)
                            {
                                rifle = GenSpawn.Spawn(ThingDef.Named("Gun_ChargeRifle"), this.pawn.Position);
                            }
                            else
                            {
                                rifle = GenSpawn.Spawn(ThingDef.Named("Gun_SniperRifle"), this.pawn.Position);
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
                            GenSpawn.Spawn(deadMarine, fishingPier.bankCell);
                            HealthUtility.GiveInjuriesToKill(deadMarine);
                            List<Thing> thingsList = deadMarine.Position.GetThingList();
                            foreach (Thing thing in thingsList)
                            {
                                if (thing.def.defName.Contains("Corpse"))
                                {
                                    CompRottable rotComp = thing.TryGetComp<CompRottable>();
                                    if (rotComp != null)
                                    {
                                        rotComp.rotProgress = 20f * 60000f; // 20 days so the corpse is dessicated.
                                    }
                                }
                            }
                            string eventText = this.pawn.Name.ToStringShort.CapitalizeFirst() + " has cought a dead body while fishing!\n\n'This is really disgusting but look at his gear! This guy was probably a Mining & Co. security member. I wonder what happend to him...'\n";
                            Find.LetterStack.ReceiveLetter("Dead marine", eventText, LetterType.Good, this.pawn.Position);
                        }
                        else
                        {
                            // Find a small amount of gold.
                            fishingCatch = GenSpawn.Spawn(ThingDefOf.Gold, this.pawn.Position);
                            fishingCatch.stackCount = Rand.RangeInclusive(1, 7);
                        }
                        // TODO: add chance to get hurt by a tailteeth (missing finger or even hand!).
                    }
                    IntVec3 storageCell;
                    if (StoreUtility.TryFindBestBetterStoreCellFor(fishingCatch, this.pawn, StoragePriority.Unstored, this.pawn.Faction, out storageCell, true))
                    {
                        this.pawn.carrier.TryStartCarry(fishingCatch);
                        curJob.targetB = storageCell;
                        curJob.targetC = fishingCatch;
                        curJob.maxNumToCarry = 99999;
                    }
                    else
                    {
                        this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                    }
                }
            };
            yield return catchFishToil;

            // Reserve the product and storage cell.
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.C);

            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            yield return carryToCell;

            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);

            yield return Toils_Reserve.Release(fishingPierIndex);
        }
    }
}
