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
    [StaticConstructorOnStartup]
    class Building_AquacultureBasin : Building_WorkTable
    {
        // Power comp.
        public CompPowerTrader powerComp;

        // Breeding parameters.
        public PawnKindDef breedingSpeciesDef = null;
        public int breedingDurationInTicks = 0;
        public int breedingProgressInTicks = 0;
        public bool breedingIsFinished = false;

        // Food.
        public const int foodDispensePeriodInTicks = GenDate.TicksPerDay; // Only once a day or this will be very time-consuming for besin technicians.
        public int nextFeedingDateInTicks = 0;
        public bool foodIsAvailable = false;

        // Water quality.
        public const float maxWaterQuality = GenDate.TicksPerDay / 2;
        public const float minWaterQuality = maxWaterQuality / 10f; // Fishes will die if water quality lasts under this limit for too long.
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
            if (this.breedingSpeciesDef != null)
            {
                if (this.breedingSpeciesDef == Util_FishIndustry.MashgonPawnKindDef)
                {
                    this.breedingSpeciesTexture = mashgonTexture;
                }
                else if (this.breedingSpeciesDef == Util_FishIndustry.BluebladePawnKindDef)
                {
                    this.breedingSpeciesTexture = bluebladeTexture;
                }
                else if (this.breedingSpeciesDef == Util_FishIndustry.TailteethPawnKindDef)
                {
                    this.breedingSpeciesTexture = tailTeethTexture;
                }
            }
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
            Scribe_Defs.Look<PawnKindDef>(ref breedingSpeciesDef, "breedingSpeciesDef");
            Scribe_Values.Look<int>(ref breedingDurationInTicks, "breedingDuratinInTicks");
            Scribe_Values.Look<int>(ref breedingProgressInTicks, "breedingProgressInTicks");
            Scribe_Values.Look<bool>(ref breedingIsFinished, "breedingIsFinished");

            // Food.
            Scribe_Values.Look<int>(ref nextFeedingDateInTicks, "nextFeedingDateInTicks");
            Scribe_Values.Look<bool>(ref foodIsAvailable, "foodIsAvailable");

            // Water quality.
            Scribe_Values.Look<float>(ref waterQuality, "waterQuality");
            Scribe_Values.Look<int>(ref microFungusRemainingDurationInTicks, "microFungusRemainingDurationInTicks");

            // Fishes health.
            Scribe_Values.Look<int>(ref fishesHealthInPercent, "fishesHealthInPercent");
        }
        
        // ===================== Main Work Function =====================
        /// <summary>
        /// Breed some fishes:
        /// - update the remaining microfungus infestation duration.
        /// - try to get some food from an adjacent hopper.
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
                        this.breedingProgressInTicks += (int)(GenTicks.TickRareInterval * waterQualityFactor);
                        if (this.breedingProgressInTicks >= this.breedingDurationInTicks)
                        {
                            this.breedingProgressInTicks = this.breedingDurationInTicks;
                            this.breedingIsFinished = true;
                        }
                    }
                }
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
                    this.foodIsAvailable = ConsumeFoodFromHoppers();
                    this.nextFeedingDateInTicks = Find.TickManager.TicksGame + foodDispensePeriodInTicks;
                }
            }
            else
            {
                this.foodIsAvailable = false;
            }
        }

        /// <summary>
        /// Check if there is enough food in the adjacent hoppers and consume it if available.
        /// </summary>
        public bool ConsumeFoodFromHoppers()
        {
            bool foodIsAvailable = IsEnoughFoodInHoppers();
            if (foodIsAvailable)
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
                        int maxCountToConsume = Mathf.Min(food.stackCount, Mathf.CeilToInt(foodToConsume / food.def.ingestible.nutrition));
                        food.SplitOff(maxCountToConsume);
                        foodToConsume -= maxCountToConsume * food.def.ingestible.nutrition;
                        if (foodToConsume <= 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if there is enough food in the adjacent hoppers.
        /// </summary>
        public bool IsEnoughFoodInHoppers()
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
                    foodSum += (float)food.stackCount * food.def.ingestible.nutrition;
                    if (foodSum >= this.def.building.nutritionCostPerDispense)
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
                    || (def.ingestible.foodType == FoodTypeFlags.AnimalProduct));
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
                this.waterQuality -= 6f * waterQualityVariationPerRareTick;
            }
            else if (this.powerComp.PowerOn)
            {
                this.waterQuality += 3f * waterQualityVariationPerRareTick;
            }
            else
            {
                this.waterQuality -= waterQualityVariationPerRareTick;
            }
            this.waterQuality = Mathf.Clamp(this.waterQuality, 0, maxWaterQuality);
        }

        /// <summary>
        /// Update the fishes health. Health decreases when:
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
            Messages.Message("FishIndustry.KillBredSpecies".Translate(), this, MessageTypeDefOf.NegativeEvent);
        }

        /// <summary>
        /// Start a new breeding cycle of the given species.
        /// </summary>
        public void StartNewBreedingCycle(PawnKindDef breedingSpeciesDef)
        {
            RemoveAllBills();
            this.breedingSpeciesDef = breedingSpeciesDef;
            this.breedingDurationInTicks = (int)(GenDate.TicksPerDay * (breedingSpeciesDef as PawnKindDef_FishSpecies).breedingDurationInDays);
            this.breedingProgressInTicks = 0;
            this.breedingIsFinished = false;

            if (this.waterQuality < minWaterQuality)
            {
                this.waterQuality = minWaterQuality;
            }
            this.fishesHealthInPercent = 100;

            this.breedingSpeciesTexture = MaterialPool.MatFrom(this.breedingSpeciesDef.lifeStages.First().bodyGraphicData.texPath, ShaderDatabase.Transparent);
        }

        /// <summary>
        /// Remove any pending bill.
        /// </summary>
        public void RemoveAllBills()
        {
            this.billStack.Clear();
        }
        
        /// <summary>
        /// Gather the aquaculture basin production and restart a new breeding cycle of the same species.
        /// </summary>
        public Thing GetProduction()
        {
            Thing product = ThingMaker.MakeThing(this.breedingSpeciesDef.race.race.meatDef);
            
            product.stackCount = ((PawnKindDef_FishSpecies)this.breedingSpeciesDef).breedQuantity;
            StartNewBreedingCycle(this.breedingSpeciesDef);

            return product;
        }

        /// <summary>
        /// Start a micro fungus infestation.
        /// </summary>
        public void StartMicroFungusInfestation(int infestationDuration)
        {
            this.microFungusRemainingDurationInTicks = infestationDuration;
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            int groupKeyBase = 700000113;

            IList<Gizmo> buttonList = new List<Gizmo>();
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                buttonList.Add(gizmo);
            }

            Bill bill = null;
            if (this.BillStack.Count > 0)
            {
                bill = this.BillStack.Bills.First();
            }
            buttonList.Add(GetBreedButton("FishIndustry.FishLabelPluralMashgon".Translate(), "FishIndustry.FishDescriptionMashgon".Translate(), Util_FishIndustry.MashgonTexturePath, Util_FishIndustry.MashgonPawnKindDef,
                bill, Util_FishIndustry.SupplyMashgonEggsRecipeDef, RequestMashgonBreeding, groupKeyBase + 1));

            buttonList.Add(GetBreedButton("FishIndustry.FishLabelPluralBlueblade".Translate(), "FishIndustry.FishDescriptionBlueblade".Translate(), Util_FishIndustry.BluebladeTexturePath, Util_FishIndustry.BluebladePawnKindDef,
                bill, Util_FishIndustry.SupplyBluebladeEggsRecipeDef, RequestBluebladeBreeding, groupKeyBase + 2));

            buttonList.Add(GetBreedButton("FishIndustry.FishLabelPluralTailteeth".Translate(), "FishIndustry.FishDescriptionTailteeth".Translate(), Util_FishIndustry.TailteethTexturePath, Util_FishIndustry.TailteethPawnKindDef,
                bill, Util_FishIndustry.SupplyTailteethEggsRecipeDef, RequestTailteethBreeding, groupKeyBase + 3));

            return buttonList;
        }

        public Command_Action GetBreedButton(string fishLabelPlural, string speciesDescription, string buttonTexturePath, PawnKindDef fishSpecies,
            Bill currentActiveBill, RecipeDef supplyEggsRecipe, Action actionOnClick, int keyOffset)
        {
            Command_Action breedButton = new Command_Action();
            breedButton.icon = ContentFinder<Texture2D>.Get(buttonTexturePath);
            if (this.breedingSpeciesDef == fishSpecies)
            {
                breedButton.defaultLabel = "FishIndustry.Breeding".Translate() + fishLabelPlural;
                breedButton.defaultDesc = "FishIndustry.BasinIsBreeding".Translate() + fishLabelPlural + ". " + speciesDescription;
                breedButton.action = RemoveAllBills;
            }
            else if ((currentActiveBill != null)
                && (currentActiveBill.recipe == supplyEggsRecipe))
            {
                breedButton.defaultLabel = "FishIndustry.WaitingForEggs".Translate();
                breedButton.defaultDesc = "FishIndustry.ClickToCancel".Translate() + fishLabelPlural + ". " + "FishIndustry.EnsureHaveHunterAndEggs".Translate();
                breedButton.action = RemoveAllBills;
            }
            else
            {
                breedButton.defaultLabel = "FishIndustry.Breed".Translate() + fishLabelPlural;
                breedButton.defaultDesc = "FishIndustry.ClickToStart".Translate() + fishLabelPlural + ". " + speciesDescription + " " + "FishIndustry.EnsureHaveHunterAndEggs".Translate();
                breedButton.action = actionOnClick;
            }
            breedButton.activateSound = SoundDef.Named("Click");
            breedButton.groupKey = keyOffset;
            return breedButton;
        }

        public void RequestMashgonBreeding()
        {
            RemoveAllBills();
            Bill_Production_AquacultureBasin bill = new Bill_Production_AquacultureBasin(Util_FishIndustry.SupplyMashgonEggsRecipeDef);
            bill.repeatMode = BillRepeatModeDefOf.RepeatCount;
            bill.repeatCount = 1;
            this.billStack.AddBill(bill);
            bool eggIsFound = false;
            foreach (Thing meat in this.Map.listerThings.ThingsOfDef(Util_FishIndustry.MashgonMeatDef))
            {
                if (meat.IsInAnyStorage())
                {
                    eggIsFound = true;
                    break;
                }
            }
            if (eggIsFound)
            {
                Messages.Message("FishIndustry.BreedingRequestedMashgon".Translate(), this, MessageTypeDefOf.SilentInput);
            }
            else
            {
                Messages.Message("FishIndustry.BreedingRequestedButNoEggsMashgon".Translate(), this, MessageTypeDefOf.NeutralEvent);
            }
        }

        public void RequestBluebladeBreeding()
        {
            RemoveAllBills();
            Bill_Production_AquacultureBasin bill = new Bill_Production_AquacultureBasin(Util_FishIndustry.SupplyBluebladeEggsRecipeDef);
            bill.repeatMode = BillRepeatModeDefOf.RepeatCount;
            bill.repeatCount = 1;
            this.billStack.AddBill(bill);
            bool eggIsFound = false;
            foreach (Thing meat in this.Map.listerThings.ThingsOfDef(Util_FishIndustry.BluebladeMeatDef))
            {
                if (meat.IsInAnyStorage())
                {
                    eggIsFound = true;
                    break;
                }
            }
            if (eggIsFound)
            {
                Messages.Message("FishIndustry.BreedingRequestedBlueblade".Translate(), this, MessageTypeDefOf.SilentInput);
            }
            else
            {
                Messages.Message("FishIndustry.BreedingRequestedButNoEggsBlueblade".Translate(), this, MessageTypeDefOf.NeutralEvent);
            }
        }

        public void RequestTailteethBreeding()
        {
            RemoveAllBills();
            Bill_Production_AquacultureBasin bill = new Bill_Production_AquacultureBasin(Util_FishIndustry.SupplyTailteethEggsRecipeDef);
            bill.repeatMode = BillRepeatModeDefOf.RepeatCount;
            bill.repeatCount = 1;
            this.billStack.AddBill(bill);
            bool eggIsFound = false;
            foreach (Thing meat in this.Map.listerThings.ThingsOfDef(Util_FishIndustry.TailteethMeatDef))
            {
                if (meat.IsInAnyStorage())
                {
                    eggIsFound = true;
                    break;
                }
            }
            if (eggIsFound)
            {
                Messages.Message("FishIndustry.BreedingRequestedTailteeth".Translate(), this, MessageTypeDefOf.SilentInput);
            }
            else
            {
                Messages.Message("FishIndustry.BreedingRequestedButNoEggsTailteeth".Translate(), this, MessageTypeDefOf.NeutralEvent);
            }
        }

        // ===================== Inspection pannel functions =====================
        /// <summary>
        /// Get the string displayed in the inspection panel.
        /// </summary>
        public override string GetInspectString()
        {
            string bredSpeciesLabel = "";
            string progressLabel = "";
            string problemText = "";
            bool foodAvailabilityDoesMatter = true;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.AppendLine();

            if (this.breedingSpeciesDef == null)
            {
                bredSpeciesLabel = "NoneLower".Translate();
                progressLabel = "FishIndustry.ProgressLabel".Translate("0%");
                foodAvailabilityDoesMatter = false;
            }
            else
            {
                bredSpeciesLabel = this.breedingSpeciesDef.label;
                progressLabel = "FishIndustry.ProgressLabel".Translate(((float)this.breedingProgressInTicks / (float)breedingDurationInTicks * 100f).ToString("F0") + "%");
            }
            if (foodAvailabilityDoesMatter
                && (this.foodIsAvailable == false))
            {
                problemText = "FishIndustry.Problem_NoFood".Translate();
            }
            else if (this.waterQuality < minWaterQuality)
            {
                problemText = "FishIndustry.Problem_WaterQualityCritical".Translate();
            }
            else if (this.microFungusRemainingDurationInTicks > 0)
            {
                problemText = "FishIndustry.Problem_MicroFungus".Translate();
            }
            stringBuilder.AppendLine("FishIndustry.BredSpecies".Translate(bredSpeciesLabel));
            stringBuilder.AppendLine(progressLabel + " " + problemText);
            stringBuilder.Append("FishIndustry.WaterQualityFishesHealth".Translate((this.waterQuality / maxWaterQuality * 100f).ToString("F0") + "%" + "/" + this.fishesHealthInPercent + "%"));

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
                this.nextBubbleThrowTick = Find.TickManager.TicksGame + 6 + (int)(Rand.Value * 120f * (1f - (this.waterQuality / maxWaterQuality))) ;
                if (this.powerComp.PowerOn)
                {
                    ThrowBubble();
                }
            }
        }

        /// <summary>
        /// Throw a bubble.
        /// </summary>
        public Mote ThrowBubble()
        {
            if (!this.Position.ShouldSpawnMotesAt(this.Map))
            {
                return null;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(Util_FishIndustry.MoteBubbleDef, null);
            moteThrown.Scale = 0.3f;
            moteThrown.rotationRate = Rand.Range(-0.15f, 0.15f);
            moteThrown.exactPosition = this.Position.ToVector3Shifted();
            moteThrown.SetVelocity((float)Rand.Range(-30, 30), 0.33f);
            GenSpawn.Spawn(moteThrown, this.Position, this.Map);
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
                Graphics.DrawMesh(MeshPool.plane10, this.microFungusMatrix, FadedMaterialPool.FadedVersionOf(Building_AquacultureBasin.microFungusTexture, microFungusFadingFactor), 0);
            }
        }
    }
}
