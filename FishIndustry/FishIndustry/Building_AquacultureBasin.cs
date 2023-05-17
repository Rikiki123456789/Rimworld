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

namespace FishIndustry
{
    /// <summary>
    /// Building_AquacultureBasin class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    [StaticConstructorOnStartup]
    class Building_AquacultureBasin : Building_WorkTable
    {
        public const int updatePeriodInTicks = GenTicks.TickRareInterval;
        public int nextUpdateTick = 0;

        // Power comp.
        public CompPowerTrader powerComp;

        // Breeding parameters.
        public PawnKindDef desiredSpeciesDef = Util_FishIndustry.BluebladePawnKindDef; // Used when user wants to change bred species.
        public PawnKindDef speciesDef = null;
        public int breedingDurationInTicks
        {
            get
            {
                if (this.speciesDef != null)
                {
                    return (int)(GenDate.TicksPerDay * (this.speciesDef as PawnKindDef_FishSpecies).breedingDurationInDays);
                }
                return 0;
            }
        }
        public int breedingProgressInTicks = 0;

        // Food.
        public const int foodDispensePeriodInTicks = GenDate.TicksPerDay; // Only once a day or this will be very time-consuming for basin technicians.
        public int nextFeedingTick = 0;
        public bool foodIsAvailable = false; // Food is available in connected hoppers.
        public bool fishesAreFed = false;    // Fishes have been fed with food from connected hoppers.

        // Water quality.
        public const float minWaterQuality = 0.10f; // Fishes will die if water quality lasts under this limit for too long.
        public const float waterCompleteDegradationPeriodInTicks = GenDate.TicksPerDay / 2f; // Quality completely degrades in 0.5 day.
        public const float waterQualityVariation = 1f / (waterCompleteDegradationPeriodInTicks / updatePeriodInTicks);
        public float waterQuality = 2f * minWaterQuality; // [0, 1], determine the breeding rate.
        public int microFungusRemainingDurationInTicks = 0;

        // Fishes health.
        public float fishesHealth = 1f; // [0, 1].

        // Maintenance.
        public const int maintenanceCompleteDegradationPeriodInTicks = GenDate.TicksPerDay;
        public const float maintenanceVariation = 1f / (maintenanceCompleteDegradationPeriodInTicks / updatePeriodInTicks);
        public const int breedingProgressPerMaintenanceInTicks = 2 * GenDate.TicksPerHour;
        public const int microFungusReductionPerMaintenanceInTicks = 8 * GenDate.TicksPerHour;
        public float maintenanceQuality = 1f; // [0, 1].
        public bool isWellMaintained
        {
            get
            {
                return (this.maintenanceQuality >= 0.5f);
            }
        }
        public bool isBadlyMaintained
        {
            get
            {
                return (this.maintenanceQuality == 0f);
            }
        }

        // Job.
        public bool fishIsHarvestable
        {
            get
            {
                return (this.speciesDef != null)
                    && (this.breedingProgressInTicks == this.breedingDurationInTicks);
            }
        }
        public bool speciesShouldBeChanged
        {
            get
            {
                return this.powerComp.PowerOn
                    && (this.desiredSpeciesDef != null)
                    && (this.microFungusRemainingDurationInTicks == 0)
                    && this.foodIsAvailable;
            }
        }
        public bool shouldBeMaintained
        {
            get
            {
                return this.powerComp.PowerOn
                    && ((this.microFungusRemainingDurationInTicks > 0)
                    || ((this.speciesDef != null) && (this.isWellMaintained == false)));
            }
        }

        // Draw.
        public const int fishMotePeriodInTicks = 10 * GenTicks.TicksPerRealSecond;
        public int nextFishMoteTick = 0;
        public Material breedingSpeciesTexture = null;
        public Matrix4x4 breedingSpeciesMatrix = default(Matrix4x4);
        public Vector3 breedingSpeciesScale = new Vector3(0.5f, 1f, 0.5f);
        public static Material mashgonTexture = MaterialPool.MatFrom(Util_FishIndustry.MashgonTexturePath);
        public static Material bluebladeTexture = MaterialPool.MatFrom(Util_FishIndustry.BluebladeTexturePath);
        public static Material tailTeethTexture = MaterialPool.MatFrom(Util_FishIndustry.TailteethTexturePath);
        public static Material microFungusTexture = MaterialPool.MatFrom("Effects/MicroFungus", ShaderDatabase.Transparent);
        public Matrix4x4 microFungusMatrix = default(Matrix4x4);
        public Vector3 microFungusScale = new Vector3(3f, 1f, 3f);
        public float microFungusFadingFactor = 1f;

        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();

            // Drawing.
            if (this.speciesDef != null)
            {
                if (this.speciesDef == Util_FishIndustry.MashgonPawnKindDef)
                {
                    this.breedingSpeciesTexture = mashgonTexture;
                }
                else if (this.speciesDef == Util_FishIndustry.BluebladePawnKindDef)
                {
                    this.breedingSpeciesTexture = bluebladeTexture;
                }
                else if (this.speciesDef == Util_FishIndustry.TailteethPawnKindDef)
                {
                    this.breedingSpeciesTexture = tailTeethTexture;
                }
            }
            breedingSpeciesMatrix.SetTRS(this.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays) + new Vector3(-1f, 0f, -0.7f).RotatedBy(this.Rotation.AsAngle), 0f.ToQuat(), this.breedingSpeciesScale);
            microFungusMatrix.SetTRS(this.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), 0f.ToQuat(), this.microFungusScale);
        }

        /// <summary>
        /// Save and load variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.nextUpdateTick, "nextUpdateTick");

            // Breeding parameters.
            Scribe_Defs.Look<PawnKindDef>(ref this.desiredSpeciesDef, "desiredSpeciesDef");
            Scribe_Defs.Look<PawnKindDef>(ref this.speciesDef, "speciesDef");
            Scribe_Values.Look<int>(ref this.breedingProgressInTicks, "breedingProgressInTicks");

            // Food.
            Scribe_Values.Look<int>(ref this.nextFeedingTick, "nextFeedingTick");
            Scribe_Values.Look<bool>(ref this.foodIsAvailable, "foodIsAvailable");
            Scribe_Values.Look<bool>(ref this.fishesAreFed, "fishesAreFed");

            // Water quality.
            Scribe_Values.Look<float>(ref this.waterQuality, "waterQuality");
            Scribe_Values.Look<int>(ref this.microFungusRemainingDurationInTicks, "microFungusRemainingDurationInTicks");

            // Fishes health.
            Scribe_Values.Look<float>(ref fishesHealth, "fishesHealth");

            // Maintenance.
            Scribe_Values.Look<float>(ref this.maintenanceQuality, "maintenanceQuality");

            // Draw.
            Scribe_Values.Look<int>(ref this.nextFishMoteTick, "nextFishMoteTick");
        }
        
        // ===================== Main Work Function =====================
        /// <summary>
        /// Breed some fishes:
        /// - update the remaining microfungus infestation duration.
        /// - try to get some food from an adjacent hopper.
        /// - update the water quality.
        /// - update the bred fishes health.
        /// - compute the drawing parameters (bubbles generation).
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (this.microFungusRemainingDurationInTicks > 0)
            {
                this.microFungusRemainingDurationInTicks--;
            }
            if (Find.TickManager.TicksGame >= this.nextUpdateTick)
            {
                this.nextUpdateTick = Find.TickManager.TicksGame + updatePeriodInTicks;

                UpdateFoodAvailability();
                TryFeedFishes();
                UpdateWaterQuality();
                UpdateFishesHealth();
                UpdateMaintenanceQuality();

                if (this.powerComp.PowerOn)
                {
                    if ((this.speciesDef != null)
                        && this.fishesAreFed)
                    {
                        this.breedingProgressInTicks += (int)(updatePeriodInTicks * (0.75f * this.waterQuality + 0.25f * maintenanceQuality));
                        this.breedingProgressInTicks = Math.Min(this.breedingProgressInTicks, this.breedingDurationInTicks);
                    }
                }
            }
            ComputeDrawingParameters();
        }

        /// <summary>
        /// Check if food is available in connected hoppers.
        /// </summary>
        public void UpdateFoodAvailability()
        {
            float foodSum = 0;
            foreach (IntVec3 cell in GenAdj.CellsAdjacentCardinal(this))
            {
                Thing food = null;
                Thing hopper = null;
                List<Thing> thingList = cell.GetThingList(this.Map);
                for (int thingIndex = 0; thingIndex < thingList.Count; thingIndex++)
                {
                    Thing currentThing = thingList[thingIndex];
                    if (currentThing.def == ThingDefOf.Hopper)
                    {
                        hopper = currentThing;
                    }
                    if (IsAcceptableFeedstock(currentThing.def))
                    {
                        food = currentThing;
                    }
                }
                if (hopper != null && food != null)
                {
                    foodSum += (float)food.stackCount * food.def.ingestible.CachedNutrition;
                    if (foodSum >= this.def.building.nutritionCostPerDispense)
                    {
                        this.foodIsAvailable = true;
                        return;
                    }
                }
            }
            this.foodIsAvailable = false;
        }

        /// <summary>
        /// Feed the fishes once per day if food is available.
        /// </summary>
        public void TryFeedFishes()
        {
            if (this.speciesDef == null)
            {
                return;
            }

            if (Find.TickManager.TicksGame >= this.nextFeedingTick)
            {
                if (this.powerComp.PowerOn
                    && this.foodIsAvailable)
                {
                    ConsumeFoodFromHoppers();
                    this.fishesAreFed = true;
                    this.nextFeedingTick = Find.TickManager.TicksGame + foodDispensePeriodInTicks;
                }
                else
                {
                    this.fishesAreFed = false;
                }
            }
        }

        /// <summary>
        /// Consume food from adjacent hoppers. Warning! Food availability must be checked before consuming it!
        /// </summary>
        public bool ConsumeFoodFromHoppers()
        {
            float foodToConsume = this.def.building.nutritionCostPerDispense;
            foreach (IntVec3 cell in GenAdj.CellsAdjacentCardinal(this))
            {
                Thing food = null;
                Thing hopper = null;
                List<Thing> thingList = cell.GetThingList(this.Map);
                for (int thingIndex = 0; thingIndex < thingList.Count; thingIndex++)
                {
                    Thing currentThing = thingList[thingIndex];
                    if (currentThing.def == ThingDefOf.Hopper)
                    {
                        hopper = currentThing;
                    }
                    if (IsAcceptableFeedstock(currentThing.def))
                    {
                        food = currentThing;
                    }
                }
                if (hopper != null && food != null)
                {
                    int maxCountToConsume = Mathf.Min(food.stackCount, Mathf.CeilToInt(foodToConsume / food.def.ingestible.CachedNutrition));
                    food.SplitOff(maxCountToConsume);
                    foodToConsume -= maxCountToConsume * food.def.ingestible.CachedNutrition;
                    if (foodToConsume <= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsAcceptableFeedstock(ThingDef def)
        {
            return def.IsNutritionGivingIngestible
                && (def.ingestible.preferability != FoodPreferability.Undefined)
                && ((def.ingestible.foodType == FoodTypeFlags.Seed)
                    || (def.ingestible.foodType == FoodTypeFlags.Meat)
                    || (def.ingestible.foodType == FoodTypeFlags.VegetableOrFruit)
                    || (def.ingestible.foodType == FoodTypeFlags.AnimalProduct)
                    || (def.ingestible.foodType == FoodTypeFlags.Fungus));
        }

        /// <summary>
        /// Update the water quality:
        /// - water quality decrease when aquaculture basin is unpowered or temperature is not in optimal range.
        /// - water quality recovers when aquaculture basin is powered.
        /// </summary>
        public void UpdateWaterQuality()
        {
            if (this.microFungusRemainingDurationInTicks > 0)
            {
                this.waterQuality -= 6f * waterQualityVariation;
            }
            else if (this.powerComp.PowerOn)
            {
                this.waterQuality += 3f * waterQualityVariation;
            }
            else
            {
                this.waterQuality -= waterQualityVariation;
            }
            this.waterQuality = Mathf.Clamp01(this.waterQuality);
        }

        /// <summary>
        /// Update the fishes health. Health decreases when:
        /// - water quality is too bad.
        /// - there is no food available.
        /// </summary>
        public void UpdateFishesHealth()
        {
            if (this.speciesDef != null)
            {
                float healthOffset = 0.001f; // Health is slowy restored when all parameters are fine.
                if (this.fishesAreFed == false)
                {
                    healthOffset -= 0.01f;
                }
                if (this.waterQuality < minWaterQuality)
                {
                    healthOffset -= 0.01f;
                }
                this.fishesHealth += healthOffset;
                if (this.fishesHealth <= 0)
                {
                    this.fishesHealth = 0;
                    StopBreeding();
                    Messages.Message("FishIndustry.FishesDied".Translate(), this, MessageTypeDefOf.NegativeEvent);
                }
                this.fishesHealth = Mathf.Clamp01(this.fishesHealth);
            }
        }

        /// <summary>
        /// Update the maintenance quality. Maintenance impacts fishes growth rate.
        /// </summary>
        public void UpdateMaintenanceQuality()
        {
            this.maintenanceQuality -= maintenanceVariation;
            this.maintenanceQuality = Mathf.Max(0, this.maintenanceQuality);
        }

        /// <summary>
        /// Stop bredding and set desired species to restart breeding when possible. 
        /// </summary>
        public void StopBreeding()
        {
            this.desiredSpeciesDef = this.speciesDef;
            this.speciesDef = null;
            this.breedingProgressInTicks = 0;
        }

        /// <summary>
        /// Start a new breeding cycle of the given species.
        /// </summary>
        public void StartNewBreedingCycle()
        {
            if (this.desiredSpeciesDef != null)
            {
                this.speciesDef = this.desiredSpeciesDef;
                this.desiredSpeciesDef = null;
            }
            this.breedingProgressInTicks = 0;
            this.maintenanceQuality = 1f;

            if (this.waterQuality < minWaterQuality)
            {
                this.waterQuality = minWaterQuality;
            }
            this.fishesHealth = 1f;

            this.breedingSpeciesTexture = MaterialPool.MatFrom(this.speciesDef.lifeStages.First().bodyGraphicData.texPath, ShaderDatabase.Transparent);
        }

        /// <summary>
        /// Gather the aquaculture basin production and restart a new breeding cycle of the same species.
        /// </summary>
        public Thing GetProduction()
        {
            Thing product = ThingMaker.MakeThing(this.speciesDef.race.race.meatDef);

            product.stackCount = Mathf.CeilToInt(((PawnKindDef_FishSpecies)this.speciesDef).breedQuantity * this.fishesHealth * Settings.fishBreedQuantityFactor);
            if (this.microFungusRemainingDurationInTicks == 0)
            {
                StartNewBreedingCycle();
            }
            else
            {
                StopBreeding();
            }

            return product;
        }

        /// <summary>
        /// Start a micro fungus infestation.
        /// </summary>
        public void StartMicroFungusInfestation(int infestationDuration)
        {
            this.microFungusRemainingDurationInTicks = infestationDuration;
        }

        // ===================== Exported functions =====================
        /// <summary>
        /// Notify basin that it was maintained.
        /// </summary>
        public void Notify_MaintenanceDone()
        {
            this.maintenanceQuality = 1f;

            if (this.microFungusRemainingDurationInTicks > 0)
            {
                // Clean a bit the micro fungus.
                this.microFungusRemainingDurationInTicks = Math.Max(0, this.microFungusRemainingDurationInTicks - microFungusReductionPerMaintenanceInTicks);
            }
            else if (this.speciesDef != null)
            {
                this.breedingProgressInTicks = Math.Min(this.breedingProgressInTicks + breedingProgressPerMaintenanceInTicks, this.breedingDurationInTicks);
            }
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            bool changingBredSpecies = false;
            PawnKindDef localSpeciesDef = null;
            string fishesNamePlural = "";
            string fishesDescription = "";
            string buttonTexturePath = "";
            int groupKeyBase = 700000113;

            IList<Gizmo> buttonList = new List<Gizmo>();
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                buttonList.Add(gizmo);
            }

            // Get fishes name, description and button texture.
            changingBredSpecies = (this.desiredSpeciesDef != null);
            if (changingBredSpecies)
            {
                localSpeciesDef = this.desiredSpeciesDef;
            }
            else
            {
                localSpeciesDef = this.speciesDef;
            }
            GetFishesNamePluralAndDescription(localSpeciesDef, out fishesNamePlural, out fishesDescription);
            GetButtonTexturePath(localSpeciesDef, changingBredSpecies, out buttonTexturePath);
            
            Command_Action breedButton = new Command_Action();
            if (changingBredSpecies)
            {
                breedButton.defaultLabel = "FishIndustry.ChangeSpecies".Translate();
                breedButton.defaultDesc = "FishIndustry.ChangeSpeciesDescription1".Translate() + fishesNamePlural + " (" + fishesDescription + ") "
                    + "FishIndustry.ChangeSpeciesDescription2".Translate();

            }
            else
            {
                breedButton.defaultLabel = "FishIndustry.Breeding".Translate();
                breedButton.defaultDesc = "FishIndustry.BasinIsBreeding".Translate() + fishesNamePlural + " (" + fishesDescription + ").";

            }
            breedButton.icon = ContentFinder<Texture2D>.Get(buttonTexturePath);
            breedButton.action = SetDesiredSpecies;
            breedButton.activateSound = SoundDef.Named("Click");
            breedButton.groupKey = groupKeyBase + 1;
            buttonList.Add(breedButton);

            return buttonList;
        }

        public void GetFishesNamePluralAndDescription(PawnKindDef speciesDef, out String namePlural, out String description)
        {
            namePlural = "";
            description = "";

            if (speciesDef == Util_FishIndustry.MashgonPawnKindDef)
            {
                namePlural = "FishIndustry.FishLabelPluralMashgon".Translate();
                description = "FishIndustry.FishDescriptionMashgon".Translate();
            }
            else if (speciesDef == Util_FishIndustry.BluebladePawnKindDef)
            {
                namePlural = "FishIndustry.FishLabelPluralBlueblade".Translate();
                description = "FishIndustry.FishDescriptionBlueblade".Translate();
            }
            else if (speciesDef == Util_FishIndustry.TailteethPawnKindDef)
            {
                namePlural = "FishIndustry.FishLabelPluralTailteeth".Translate();
                description = "FishIndustry.FishDescriptionTailteeth".Translate();
            }
            else
            {
                Log.Warning("FishIndustry: unhandled PawnKindDef (" + speciesDef.ToString() + ").");
            }
        }

        public void GetButtonTexturePath(PawnKindDef species, bool changingBredSpecies, out String buttonTexturePath)
        {
            buttonTexturePath = "";

            if (changingBredSpecies)
            {
                if (species == Util_FishIndustry.MashgonPawnKindDef)
                {
                    buttonTexturePath = Util_FishIndustry.MashgonTexturePathWithChangeIcon;
                }
                else if (species == Util_FishIndustry.BluebladePawnKindDef)
                {
                    buttonTexturePath = Util_FishIndustry.BluebladeTexturePathWithChangeIcon;
                }
                else if (species == Util_FishIndustry.TailteethPawnKindDef)
                {
                    buttonTexturePath = Util_FishIndustry.TailteethTexturePathWithChangeIcon;
                }
                else
                {
                    Log.Warning("FishIndustry: unhandled PawnKindDef (" + species.ToString() + ").");
                }
            }
            else
            {
                if (species == Util_FishIndustry.MashgonPawnKindDef)
                {
                    buttonTexturePath = Util_FishIndustry.MashgonTexturePath;
                }
                else if (species == Util_FishIndustry.BluebladePawnKindDef)
                {
                    buttonTexturePath = Util_FishIndustry.BluebladeTexturePath;
                }
                else if (species == Util_FishIndustry.TailteethPawnKindDef)
                {
                    buttonTexturePath = Util_FishIndustry.TailteethTexturePath;
                }
                else
                {
                    Log.Warning("FishIndustry: unhandled PawnKindDef (" + species.ToString() + ").");
                }
            }
        }

        /// <summary>
        /// Change the desired species when button is clicked by the user.
        /// </summary>
        public void SetDesiredSpecies()
        {
            PawnKindDef currentDesiredSpeciesDef = null;
            PawnKindDef newDesiredSpeciesDef = null;

            if (this.desiredSpeciesDef != null)
            {
                currentDesiredSpeciesDef = this.desiredSpeciesDef;
            }
            else
            {
                currentDesiredSpeciesDef = this.speciesDef;
            }

            if (currentDesiredSpeciesDef == Util_FishIndustry.MashgonPawnKindDef)
            {
                newDesiredSpeciesDef = Util_FishIndustry.BluebladePawnKindDef;
            }
            else if (currentDesiredSpeciesDef == Util_FishIndustry.BluebladePawnKindDef)
            {
                newDesiredSpeciesDef = Util_FishIndustry.TailteethPawnKindDef;
            }
            else if (currentDesiredSpeciesDef == Util_FishIndustry.TailteethPawnKindDef)
            {
                newDesiredSpeciesDef = Util_FishIndustry.MashgonPawnKindDef;
            }
            if (newDesiredSpeciesDef == this.speciesDef)
            {
                // Cancel species change.
                this.desiredSpeciesDef = null;
            }
            else
            {
                this.desiredSpeciesDef = newDesiredSpeciesDef;
            }
        }

        // ===================== Inspection pannel functions =====================
        /// <summary>
        /// Get the string displayed in the inspection panel:
        /// - power info (from base),
        /// - breeding progress,
        /// - water quality/fishes health,
        /// - maintenance need,
        /// - eventual issues.
        /// </summary>
        public override string GetInspectString()
        {
            string progressLabel = "";
            string issuesLabel = "";
            List<string> issuesList = new List<string>();

            StringBuilder stringBuilder = new StringBuilder();

            // Power info.
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.AppendLine();

            // Breeding progress.                        
            if (this.speciesDef == null)
            {
                progressLabel = "FishIndustry.ProgressLabelNotBreeding".Translate();
            }
            else
            {
                progressLabel = "FishIndustry.ProgressLabel".Translate() + Mathf.RoundToInt((float)this.breedingProgressInTicks / (float)breedingDurationInTicks * 100f) + "%";
            }
            stringBuilder.AppendLine(progressLabel);

            if (this.speciesDef != null)
            {
                // Water quality/fishes health.
                stringBuilder.Append("FishIndustry.WaterQualityFishesHealth".Translate(Mathf.RoundToInt(this.waterQuality * 100f), Mathf.RoundToInt(this.fishesHealth * 100f)));

            }
            else
            {
                // Water quality.
                stringBuilder.Append("FishIndustry.WaterQuality".Translate(Mathf.RoundToInt(this.waterQuality * 100f)));
            }

            // Maintenance need.
            if (this.shouldBeMaintained)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("FishIndustry.NeedsMaintenanceNow".Translate());
            }

            // Eventual issues.
            if (this.microFungusRemainingDurationInTicks > 0)
            {
                issuesList.Add("FishIndustry.Issue_MicroFungus".Translate());
            }
            if (this.foodIsAvailable == false)
            {
                issuesList.Add("FishIndustry.Issue_NoFood".Translate());
            }
            if (this.waterQuality < minWaterQuality)
            {
                issuesList.Add("FishIndustry.Issue_CriticalWaterQuality".Translate());
            }
            if ((this.speciesDef != null)
                && this.isBadlyMaintained)
            {
                issuesList.Add("FishIndustry.Issue_BadlyMaintained".Translate());
            }
            if (issuesList.Count > 0)
            {
                if (issuesList.Count == 1)
                {
                    issuesLabel = string.Format("{0}: {1}", "FishIndustry.Issue".Translate(), issuesList.First());
                }
                else
                {
                    issuesLabel = string.Format("{0}: {1}", "FishIndustry.Issues".Translate(), issuesList.ToCommaList(false));
                }
                stringBuilder.AppendLine();
                stringBuilder.Append(issuesLabel);
            }

            return stringBuilder.ToString();
        }

        // ===================== Drawing functions =====================
        /// <summary>
        /// Set microfungus intensity or throw fish fleck.
        /// </summary>
        public void ComputeDrawingParameters()
        {
            if (this.microFungusRemainingDurationInTicks > 0)
            {
                microFungusFadingFactor = Mathf.Clamp01(0.2f + ((float)this.microFungusRemainingDurationInTicks / ((float)GenDate.TicksPerDay * IncidentDef.Named("MicroFungus").durationDays.max)));
                return;
            }

            if ((this.speciesDef != null)
                && (Find.TickManager.TicksGame >= this.nextFishMoteTick))
            {
                this.nextFishMoteTick = Find.TickManager.TicksGame + fishMotePeriodInTicks;
                ThrowFishMote();
            }
        }

        /// <summary>
        /// Throw a fish mote. So cute! :-)
        /// </summary>
        public Mote ThrowFishMote()
        {
            if (!this.Position.ShouldSpawnMotesAt(this.Map) || this.Map.moteCounter.Saturated)
            {
                return null;
            }
            
            bool startFromLeft = Rand.Chance(0.5f);
            ThingDef moteDef = null;
            if (this.speciesDef == Util_FishIndustry.MashgonPawnKindDef)
            {
                if (startFromLeft)
                {
                    moteDef = Util_FishIndustry.MoteFishMashgonEastDef;
                }
                else
                {
                    moteDef = Util_FishIndustry.MoteFishMashgonWestDef;
                }
            }
            else if (this.speciesDef == Util_FishIndustry.BluebladePawnKindDef)
            {
                if (startFromLeft)
                {
                    moteDef = Util_FishIndustry.MoteFishBluebladeEastDef;
                }
                else
                {
                    moteDef = Util_FishIndustry.MoteFishBluebladeWestDef;
                }
            }
            else if (this.speciesDef == Util_FishIndustry.TailteethPawnKindDef)
            {
                if (startFromLeft)
                {
                    moteDef = Util_FishIndustry.MoteFishTailteethEastDef;
                }
                else
                {
                    moteDef = Util_FishIndustry.MoteFishTailteethWestDef;
                }
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(moteDef, null);
            moteThrown.Scale = Mathf.Lerp(0.2f, 0.6f, (float)this.breedingProgressInTicks / (float)this.breedingDurationInTicks);
            Vector3 moteStartPosition = this.Position.ToVector3Shifted();
            moteThrown.exactRotation = Rand.Range(0f, 45f); // Texture rotation.
            if (startFromLeft)
            {
                // From left to right.
                moteStartPosition += 0.8f * Vector3Utility.HorizontalVectorFromAngle(270f + moteThrown.exactRotation);
                moteThrown.SetVelocity(90f + moteThrown.exactRotation, 0.25f);
            }
            else
            {
                // From east to west.
                moteStartPosition += 0.8f * Vector3Utility.HorizontalVectorFromAngle(90f + moteThrown.exactRotation);
                moteThrown.SetVelocity(270f + moteThrown.exactRotation, 0.25f);
            }
            moteThrown.exactPosition = moteStartPosition;
            GenSpawn.Spawn(moteThrown, this.Position, this.Map);
            return moteThrown;
        }

        /// <summary>
        /// Draw an icon of the currently breeding species and the micro fungus infestation when active.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            if (this.speciesDef != null)
            {
                // Mind the Vector3 so the fish icon is drawn over the aquaculture console.
                Graphics.DrawMesh(MeshPool.plane10, this.breedingSpeciesMatrix, this.breedingSpeciesTexture, 0);
            }

            if (this.microFungusRemainingDurationInTicks > 0)
            {
                // Mind the Vector3 so the micro fungus is drawn over the aquaculture basin.
                Graphics.DrawMesh(MeshPool.plane10, this.microFungusMatrix, FadedMaterialPool.FadedVersionOf(Building_AquacultureBasin.microFungusTexture, microFungusFadingFactor), 0);
            }
        }
    }
}
