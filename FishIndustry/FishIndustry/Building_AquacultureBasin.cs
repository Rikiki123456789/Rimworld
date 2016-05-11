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
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class Building_AquacultureBasin : Building_WorkTable
    {
        // Power comp.
        public CompPowerTrader powerComp;

        // Breeding parameters.
        public ThingDef breedingSpeciesDef = null;
        public int breedingDuratinInTicks = 0;
        public int breedingProgressInTicks = 0;
        public bool breedingIsFinished = false;

        // Food.
        public const int foodDispensePeriodInTicks = 60000; // Only once a day or this will be very time-consuming for the fishers.
        public int nextFeedingDateInTicks = 0;
        public bool foodIsAvailable = false;

        // Water quality.
        public const float minWaterTemperature = 0;
        public const float maxWaterTemperature = 40;
        public float temperature = 0f;
        public const float maxWaterQuality = 30000f;
        public const float minWaterQuality = maxWaterQuality / 10f; // Fishes will die if water quality last under this limit for too long.
        public const float waterQualityVariationPerRareTick = maxWaterQuality / GenTicks.TickRareInterval; // Quality completely degrades in 0.5 day.
        public float waterQuality = 2f * minWaterQuality; // Determine the breeding rate.
        public int microFungusRemainingDurationInTicks = 0;

        // Fishes health.
        public int fishesHealthInPercent = 100;

        // Draw.
        public int nextBubbleThrowTick = 0;
        public Material breedingSpeciesTexture = null;
        public Matrix4x4 breedingSpeciesMatrix = default(Matrix4x4);
        public Vector3 breedingSpeciesScale = new Vector3(0.5f, 1f, 0.5f);
        public Material microFungusTexture = null;
        public Matrix4x4 microFungusMatrix = default(Matrix4x4);
        public Vector3 microFungusScale = new Vector3(3f, 1f, 3f);
        public float microFungusFadingFactor = 1f;

        // ===================== Setup Work =====================

        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.powerComp = base.GetComp<CompPowerTrader>();

            // Drawing.
            if (this.breedingSpeciesDef != null)
            {
                this.breedingSpeciesTexture = MaterialPool.MatFrom(this.breedingSpeciesDef.graphicData.texPath, ShaderDatabase.Transparent);
            }
            this.microFungusTexture = MaterialPool.MatFrom("Effects/MicroFungus", ShaderDatabase.Transparent);
            breedingSpeciesMatrix.SetTRS(base.DrawPos + new Vector3(-1f, 0, -0.7f).RotatedBy(this.Rotation.AsAngle) + Altitudes.AltIncVect + new Vector3(0f, 0.1f, 0f), 0f.ToQuat(), this.breedingSpeciesScale);
            microFungusMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect + new Vector3(0f, 0.1f, 0f), 0f.ToQuat(), this.microFungusScale);
        }

        /// <summary>
        /// Save and load variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            // Breeding parameters.
            Scribe_Defs.LookDef<ThingDef>(ref breedingSpeciesDef, "breedingSpeciesDef");
            Scribe_Values.LookValue<int>(ref breedingDuratinInTicks, "breedingDuratinInTicks");
            Scribe_Values.LookValue<int>(ref breedingProgressInTicks, "breedingProgressInTicks");
            Scribe_Values.LookValue<bool>(ref breedingIsFinished, "breedingIsFinished");

            // Food.
            Scribe_Values.LookValue<int>(ref nextFeedingDateInTicks, "nextFeedingDateInTicks");
            Scribe_Values.LookValue<bool>(ref foodIsAvailable, "foodIsAvailable");

            // Water quality.
            Scribe_Values.LookValue<float>(ref waterQuality, "waterQuality");
            Scribe_Values.LookValue<int>(ref microFungusRemainingDurationInTicks, "microFungusRemainingDurationInTicks");

            // Fishes health.
            Scribe_Values.LookValue<int>(ref fishesHealthInPercent, "fishesHealthInPercent");
        }
        
        // ===================== Main Work Function =====================
        /// <summary>
        /// Breed some fishes:
        /// - update the remaining microfungus infestation duration.
        /// - try to get some food from an adjacent aquaculture hopper.
        /// - update the water quality.
        /// - update the bred fishes health.
        /// - reset the bills if necessary.
        /// - compute the drawing parameters (bubbles generation).
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (this.microFungusRemainingDurationInTicks > 0)
            {
                this.microFungusRemainingDurationInTicks--;
            }
            if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
            {
                TryFeedFishes();
                UpdateWaterQuality();
                UpdateFishesHealth();

                if (this.powerComp.PowerOn)
                {
                    if (this.foodIsAvailable
                        && (this.breedingSpeciesDef != null)
                        && (this.breedingIsFinished == false))
                    {
                        float waterQualityFactor = this.waterQuality / maxWaterQuality;
                        this.breedingProgressInTicks += (int)(250f * waterQualityFactor);
                        if (this.breedingProgressInTicks >= this.breedingDuratinInTicks)
                        {
                            this.breedingProgressInTicks = this.breedingDuratinInTicks;
                            this.breedingIsFinished = true;
                        }
                    }
                }

                ResetBillsIfNecessary();
            }
            ComputeDrawingParameters();
        }

        /// <summary>
        /// Try to feed the fishes.
        /// </summary>
        public void TryFeedFishes()
        {
            if (this.breedingSpeciesDef == null)
            {
                return;
            }

            if (this.powerComp.PowerOn)
            {
                if ((this.foodIsAvailable == false)
                    || (Find.TickManager.TicksGame >= this.nextFeedingDateInTicks))
                {
                    this.foodIsAvailable = GetFoodFromAquacultureHopper();
                    this.nextFeedingDateInTicks = Find.TickManager.TicksGame + foodDispensePeriodInTicks;
                }
            }
            else
            {
                this.foodIsAvailable = false;
            }
        }

        /// <summary>
        /// Check if there is enough food in one of the adjacent aquaculture hoppers and consume it if available.
        /// </summary>
        public bool GetFoodFromAquacultureHopper()
        {
            bool foodIsAvailable = IsEnoughFoodInHopper();
            if (foodIsAvailable)
            {
                int foodToConsume = this.def.building.foodCostPerDispense;
                foreach (IntVec3 cell in GenAdj.CellsAdjacentCardinal(this))
                {
                    Thing food = null;
                    Thing aquacultureHopper = null;
                    List<Thing> thingList = cell.GetThingList();
                    for (int thingIndex = 0; thingIndex < thingList.Count; thingIndex++)
                    {
                        Thing currentThing = thingList[thingIndex];
                        if (currentThing.def == Util_FishIndustry.AquacultureHopperDef)
                        {
                            aquacultureHopper = currentThing;
                        }
                        if (currentThing.def.IsNutritionSource)
                        {
                            food = currentThing;
                        }
                    }
                    if (aquacultureHopper != null && food != null)
                    {
                        if (food.stackCount >= foodToConsume)
                        {
                            food.SplitOff(foodToConsume);
                            return true;
                        }
                        else
                        {
                            foodToConsume -= food.stackCount;
                            food.SplitOff(food.stackCount);
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if there is enough food in the adjacent aquaculture hoppers.
        /// </summary>
        public bool IsEnoughFoodInHopper()
        {
            int availableFoodSum = 0;
            foreach (IntVec3 cell in GenAdj.CellsAdjacentCardinal(this))
            {
                Thing food = null;
                Thing aquacultureHopper = null;
                List<Thing> thingList = cell.GetThingList();
                for (int thingIndex = 0; thingIndex < thingList.Count; thingIndex++)
                {
                    Thing currentThing = thingList[thingIndex];
                    if (currentThing.def == Util_FishIndustry.AquacultureHopperDef)
                    {
                        aquacultureHopper = currentThing;
                    }
                    if (currentThing.def.IsNutritionSource)
                    {
                        food = currentThing;
                    }
                }
                if (aquacultureHopper != null && food != null)
                {
                    availableFoodSum += food.stackCount;
                    if (availableFoodSum >= this.def.building.foodCostPerDispense)
                    {
                        return true;
                    }
                }
            }
            return false;
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
                this.waterQuality = 0f;
                return;
            }

            this.temperature = this.Position.GetTemperature();
            bool temperatureIsInOptimalRange = (this.temperature >= minWaterTemperature) && (this.temperature <= maxWaterTemperature);
            if (this.powerComp.PowerOn)
            {
                if (temperatureIsInOptimalRange)
                {
                    this.waterQuality += 3f * waterQualityVariationPerRareTick;
                }
                else
                {
                    this.waterQuality -= waterQualityVariationPerRareTick;
                }
            }
            else
            {
                if (temperatureIsInOptimalRange)
                {
                    this.waterQuality -= waterQualityVariationPerRareTick;
                }
                else
                {
                    this.waterQuality -= 2 * waterQualityVariationPerRareTick;
                }
            }

            // Saturation.
            if (this.waterQuality > maxWaterQuality)
            {
                this.waterQuality = maxWaterQuality;
            }
            if (this.waterQuality < 0f)
            {
                this.waterQuality = 0f;
            }
        }

        /// <summary>
        /// Update the fishes heatlh. Health decreases when:
        /// - water quality is too bad.
        /// - there is no food available.
        /// </summary>
        public void UpdateFishesHealth()
        {
            if (this.breedingSpeciesDef != null)
            {
                if (this.foodIsAvailable == false)
                {
                    this.fishesHealthInPercent -= 1;
                }
                if (this.waterQuality < minWaterQuality)
                {
                    this.fishesHealthInPercent -= 1;
                }
                if (this.fishesHealthInPercent <= 0)
                {
                    this.fishesHealthInPercent = 0;
                    KillBredSpecies();
                }
            }
        }

        /// <summary>
        /// The bred species is killed. 
        /// </summary>
        public void KillBredSpecies()
        {
            this.breedingSpeciesDef = null;
            this.breedingIsFinished = false;
            this.breedingProgressInTicks = 0;
            Messages.Message("Bred fishes have died in an aquaculture basin. You should check its conditions.", this, MessageSound.Negative);
        }

        /// <summary>
        /// Start a new breeding cycle of the given species.
        /// </summary>
        public void StartNewBreedCycle(ThingDef breedingSpeciesDef)
        {
            this.breedingSpeciesDef = breedingSpeciesDef;
            this.breedingDuratinInTicks = (int)(60000 * (breedingSpeciesDef as ThingDef_FishSpeciesProperties).breedingDurationInDays);
            this.breedingProgressInTicks = 0;
            this.breedingIsFinished = false;

            if (this.waterQuality < minWaterQuality)
            {
                this.waterQuality = minWaterQuality;
            }

            this.fishesHealthInPercent = 100;

            this.breedingSpeciesTexture = MaterialPool.MatFrom(this.breedingSpeciesDef.graphicData.texPath, ShaderDatabase.Transparent);
        }

        /// <summary>
        /// Reset the aquaculture bills if the user has messed with it (cancelled a bill for example).
        /// We need  to set custom Bill_Production_AquacultureBasin to call the StartNewBreedCycle function upon bill completion.
        /// </summary>
        public void  ResetBillsIfNecessary()
        {
            if (this.billStack.Count != 3)
            {
                this.billStack.Clear();
                Bill_Production_AquacultureBasin bill1 = new Bill_Production_AquacultureBasin(Util_FishIndustry.SupplyMashgonEggsRecipeDef);
                bill1.repeatMode = BillRepeatMode.RepeatCount;
                bill1.repeatCount = 0;
                this.billStack.AddBill(bill1);

                Bill_Production_AquacultureBasin bill2 = new Bill_Production_AquacultureBasin(Util_FishIndustry.SupplyBluebladeEggsRecipeDef);
                bill2.repeatMode = BillRepeatMode.RepeatCount;
                bill2.repeatCount = 0;
                this.billStack.AddBill(bill2);

                Bill_Production_AquacultureBasin bill3 = new Bill_Production_AquacultureBasin(Util_FishIndustry.SupplyTailteethEggsRecipeDef);
                bill3.repeatMode = BillRepeatMode.RepeatCount;
                bill3.repeatCount = 0;
                this.billStack.AddBill(bill3);
            }
        }

        /// <summary>
        /// Gather the aquaculture basin production and restart a new breeding cycle of the same species.
        /// </summary>
        public Thing GetProduction()
        {
            Thing product = ThingMaker.MakeThing(this.breedingSpeciesDef);
            product.stackCount = (this.breedingSpeciesDef as ThingDef_FishSpeciesProperties).breedQuantity;
            StartNewBreedCycle(this.breedingSpeciesDef);

            return product;
        }

        /// <summary>
        /// Start a micro fungus infestation.
        /// </summary>
        public void StartMicroFungusInfestation(int infestationDuration)
        {
            this.waterQuality = 0;
            this.microFungusRemainingDurationInTicks = infestationDuration;
        }

        // ===================== Inspection pannel functions =====================

        /// <summary>
        /// Get the string displayed in the inspection panel.
        /// </summary>
        public override string GetInspectString()
        {
            string bredSpeciesLabel = "";
            string progressLabel = "";
            string waterQualityStatusText = "";

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (this.breedingSpeciesDef == null)
            {
                bredSpeciesLabel = "none";
                progressLabel = "Progress: 0%";
            }
            else
            {
                bredSpeciesLabel = this.breedingSpeciesDef.label;
                progressLabel = "Progress: " + ((float)this.breedingProgressInTicks / (float)breedingDuratinInTicks * 100f).ToString("F0") + "%";
            }
            stringBuilder.AppendLine("Bred species: " + bredSpeciesLabel);
            stringBuilder.AppendLine(progressLabel);
            if (this.microFungusRemainingDurationInTicks > 0)
            {
                waterQualityStatusText = "(micro fungus)";
            }
            else if (this.temperature < minWaterTemperature)
            {
                waterQualityStatusText = "(cold)";
            }
            else if (this.temperature > maxWaterTemperature)
            {
                waterQualityStatusText = "(hot)";
            }
            else if (this.waterQuality < minWaterQuality)
            {
                waterQualityStatusText = "(critical)";
            }
            stringBuilder.AppendLine("Water quality/fishes health: " + (this.waterQuality / maxWaterQuality * 100f).ToString("F0") + "%" + waterQualityStatusText + "/" + this.fishesHealthInPercent + "%");

            return stringBuilder.ToString();
        }

        // ===================== Drawing functions =====================
        /// <summary>
        /// Throw bubbles according to the current water quality.
        /// </summary>
        public void ComputeDrawingParameters()
        {
            if (this.microFungusRemainingDurationInTicks > 0)
            {
                if (this.microFungusRemainingDurationInTicks <= 5000)
                {
                    microFungusFadingFactor = (float)this.microFungusRemainingDurationInTicks / 5000f;
                }
                else
                {
                    microFungusFadingFactor = 1f;
                }
                return;
            }

            if (Find.TickManager.TicksGame >= this.nextBubbleThrowTick)
            {
                this.nextBubbleThrowTick = Find.TickManager.TicksGame + 2 + (int)(Rand.Value * 60f * (1f - (this.waterQuality / maxWaterQuality))) ;
                if (this.powerComp.PowerOn)
                {
                    if (Rand.Value < 0.7f)
                    {
                        ThrowBubble(this.Position);
                    }
                }
            }

        }

        /// <summary>
        /// Throw a bubble.
        /// </summary>
        public static Mote ThrowBubble(IntVec3 cell)
        {
            if (!cell.ShouldSpawnMotesAt())
            {
                return null;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(Util_FishIndustry.MoteBubbleDef, null);
            moteThrown.ScaleUniform = 0.3f;
            moteThrown.exactRotationRate = Rand.Range(-0.15f, 0.15f);
            moteThrown.exactPosition = cell.ToVector3Shifted();
            moteThrown.SetVelocityAngleSpeed((float)Rand.Range(-30, 30), 0.009f);
            GenSpawn.Spawn(moteThrown, cell);
            return moteThrown;
        }

        /// <summary>
        /// Draw an icon of the currently breeding species and the micro fungus infestation when active.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            if (this.breedingSpeciesDef != null)
            {
                // Mind the Vector3 so the fish icon is drawn over the aquaculture console.
                Graphics.DrawMesh(MeshPool.plane10, this.breedingSpeciesMatrix, this.breedingSpeciesTexture, 0);
            }

            if (this.microFungusRemainingDurationInTicks > 0)
            {
                // Mind the Vector3 so the micro fungus is drawn over the aquaculture basin.
                Graphics.DrawMesh(MeshPool.plane10, this.microFungusMatrix, FadedMaterialPool.FadedVersionOf(this.microFungusTexture, microFungusFadingFactor), 0);
            }
        }
    }
}
