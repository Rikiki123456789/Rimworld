using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound
using RimWorld.SquadAI;

namespace MechanoidTerraformer
{
    /// <summary>
    /// Building_MechanoidTerraformer class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_MechanoidTerraformer : Building
    {
        public enum TerraformerState
        {
            Landing,

            DeployingPylon,

            StartingGenerator,
            Harnessingpower,
            StoppingGenerator,

            DeployingLightTurrets, // TODO: idea abandonned for now.
            DeployingHeavyTurrets,

            Idle
        };

        // Key state flags and starting total tick.
        public const int totalTicksBetweenPylonsDeployment = 30000 * 4; // 4 days between 2 pylons deployment.
        //public const int totalTicksBeforeDeployingLightTurrets = 30000 * 40; // 40 days after landing.
        //public const int totalTicksBeforeDeployingHeavyTurrets = 30000 * 60; // 60 days after landing.
        public const int totalTicksBeforeLaunchingInvasion = 30000 * 100; // 100 days after landing.
        public bool deployingLightTurretsIsDone = false;
        public bool deployingHeavyTurretsIsDone = false;
        public bool invasionIsDone = false;

        public TerraformerState terraformerState = TerraformerState.Landing;
        public int ticksCounter = 0;
        public int totalTicksCounter = 0; // Total time the terraformer landed.

        // Foundations expansion.
        public const int foundationRangeMax = 4;
        public int foundationRange = 0;

        // Pylons.
        public IntVec3[] pylonsPositions = new IntVec3[8];
        public bool[] pylonIsConstructed = new bool[8];
        public List<Thing> pylonsList = new List<Thing>(8);
        public bool pylonConstructionIsInProgress = false;
        public int currentPylonIndex = 0;

        // Terraforming thunderstorm.
        public const float strikeChancePerPylon = 0.002f;
        public int terraformingThunderstormDurationInTicks = 0;
        public int terraformingThunderstormNextStartTicks = 0; // Tick mark after which a new thunderstorm can begin.
        public WeatherDef desiredWeather = WeatherDef.Named("Clear");

        // Colony interractions.
        public enum ReverseEngineeringState
        {
            BuildingNotApproched,       // There has been no interraction with the building.
            BuildingNotSecured,         // Terraformer has been approched (scythers scouts should have been spawned).
            Studying,                   // Terraformer is secured and initial study can be performed.
            StudyCompleted,             // Initial study is completed. The player can choose between rerouting power or disassemble the terraformer.

            ReroutingPower,             // Modifying the terraformer to be able to use a part of the generated energy.
            PowerRerouted,              // Power has been rerouted.

            ExtractingWeatherController // Disassemble the terraformer to extract the weather controller.
        };
        public static bool studyIsCompleted = false;
        public ReverseEngineeringState reverseEngineeringState = ReverseEngineeringState.BuildingNotApproched;
        public const int minResearchLevelToStudyArtifact = 5;
        public int studyCounter = 0;
        public const int studyCounterTargetValue = 100;
        public bool studyIsPaused = true;
        public const int minResearchLevelToReroutePower = 8;
        public int reroutingCounter = 0;
        public const int reroutingCounterTargetValue = 100;
        public bool reroutingIsPaused = true;
        public const int minResearchLevelToExtractWeatherController = 10;
        public int extractionCounter = 0;
        public const int extractionCounterTargetValue = 100;
        public bool extractionIsPaused = true;

        // Power component.
        public CompPowerTrader powerComp = null;
        public const int powerOutputFromInternalGenerator = 5000;
        public const int powerOutputPerPylonDuringTerraformerThunderstorm = 4000;
        public const int powerOutputPerPylonDuringNaturalThunderstorm = 2000;

        // Drawing.
        public Material generatorTexture;
        public Matrix4x4 generatorMatrix = default(Matrix4x4);
        public Vector3 generatorScale = new Vector3(3f, 1f, 3f);
        public bool generatorIsStarted = false;
        public float generatorAngleInDegrees = 0f;
        public float generatorAngleIncrementPerTickInDegrees = 0f;

        // Icons texture.
        public Texture2D displayStudyReportIcon;

        // ===================== Initialization and save/load functions =====================

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            // Case where this is not the first terraformer to land.
            if (Building_MechanoidTerraformer.studyIsCompleted
                && (this.reverseEngineeringState == ReverseEngineeringState.BuildingNotApproched))
            {
                this.reverseEngineeringState = ReverseEngineeringState.StudyCompleted;
            }

            // Pylons initialization.
            GetPylonsPositions();

            // Components initialization.
            this.powerComp = base.GetComp<CompPowerTrader>();

            // Drawing and icons.
            generatorTexture = MaterialPool.MatFrom("Things/Building/Generator", ShaderDatabase.Transparent);
            displayStudyReportIcon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_DisplayStudyReport");
        }

        public void GetPylonsPositions()
        {
            for (int rotationIndex = 0; rotationIndex < 4; rotationIndex++)
            {
                Rot4 rotation = new Rot4(rotationIndex);
                this.pylonsPositions[2 * rotationIndex] = this.Position + new IntVec3(-2, 0, 6).RotatedBy(rotation);
                this.pylonsPositions[2 * rotationIndex + 1] = this.Position + new IntVec3(2, 0, 6).RotatedBy(rotation);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<TerraformerState>(ref this.terraformerState, "terraformerState");
            Scribe_Values.LookValue<int>(ref this.ticksCounter, "ticksCounter");
            Scribe_Values.LookValue<int>(ref this.totalTicksCounter, "totalTicksCounter");
            Scribe_Values.LookValue<bool>(ref this.deployingLightTurretsIsDone, "deployingLightTurretsIsDone");
            Scribe_Values.LookValue<bool>(ref this.deployingHeavyTurretsIsDone, "deployingHeavyTurretsIsDone");
            Scribe_Values.LookValue<bool>(ref this.invasionIsDone, "invasionIsDone");

            // Landing.
            Scribe_Values.LookValue<int>(ref this.foundationRange, "foundationRange");

            // Pylons.
            for (int pylonIndex = 0; pylonIndex < 8; pylonIndex++)
            {
                Scribe_Values.LookValue<bool>(ref this.pylonIsConstructed[pylonIndex], "pylonIsConstructed" + pylonIndex.ToString());
            }
            Scribe_Collections.LookList<Thing>(ref this.pylonsList, "pylonsList", LookMode.MapReference);
            Scribe_Values.LookValue<bool>(ref this.pylonConstructionIsInProgress, "pylonConstructionIsInProgress");
            Scribe_Values.LookValue<int>(ref this.currentPylonIndex, "currentPylonIndex");

            // Thunderstorm.
            Scribe_Values.LookValue<int>(ref this.terraformingThunderstormDurationInTicks, "terraformingThunderstormDurationInTicks");
            Scribe_Values.LookValue<int>(ref this.terraformingThunderstormNextStartTicks, "terraformingThunderstormNextStartTicks");
            Scribe_Defs.LookDef<WeatherDef>(ref this.desiredWeather, "desiredWeather");

            // Colony interraction.
            Scribe_Values.LookValue<bool>(ref Building_MechanoidTerraformer.studyIsCompleted, "studyIsCompleted");
            Scribe_Values.LookValue<ReverseEngineeringState>(ref this.reverseEngineeringState, "reverseEngineeringState");
            if (this.reverseEngineeringState >= Building_MechanoidTerraformer.ReverseEngineeringState.StudyCompleted)
            {
                this.def.label = "Mechanoid terraformer";
            }
            Scribe_Values.LookValue<int>(ref this.studyCounter, "studyCounter");
            Scribe_Values.LookValue<bool>(ref this.studyIsPaused, "studyIsPaused");
            Scribe_Values.LookValue<int>(ref this.reroutingCounter, "reroutingCounter");
            Scribe_Values.LookValue<bool>(ref this.reroutingIsPaused, "reroutingIsPaused");
            Scribe_Values.LookValue<int>(ref this.extractionCounter, "extractingCounter");
            Scribe_Values.LookValue<bool>(ref this.extractionIsPaused, "extractingIsPaused");
        }

        // ===================== Destroying =====================

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);

            for (int pylonIndex = 0; pylonIndex < this.pylonsList.Count; pylonIndex++)
            {
                IntVec3 pylonPosition = this.pylonsList[pylonIndex].Position;
                this.pylonsList[pylonIndex].Destroy(DestroyMode.Vanish);
                Thing destructiblePylon = ThingMaker.MakeThing(Util_MechanoidTerraformer.MechanoidPylonDestructibleDef);
                destructiblePylon.SetFaction(this.Faction);
                GenSpawn.Spawn(destructiblePylon, pylonPosition);
            }
        }

        // ===================== Main treatments functions =====================

        public override void Tick()
        {
            base.Tick();
            this.ticksCounter++;
            this.totalTicksCounter++;

            switch (this.terraformerState)
            {
                case TerraformerState.Landing:
                    PerformLandingTreatment();
                    break;

                case TerraformerState.DeployingPylon:
                    PerformDeployingPylonTreatment();
                    break;

                case TerraformerState.StartingGenerator:
                    PerformStartingGeneratorTreatment();
                    break;
                case TerraformerState.Harnessingpower:
                    PerformHarnessingPowerTreatment();
                    break;
                case TerraformerState.StoppingGenerator:
                    PerformStoppingGeneratorTreatment();
                    break;

                case TerraformerState.DeployingLightTurrets:
                    //PerformDeployingInitialPylonsTreatment(); // TODO: or not...
                    break;
                case TerraformerState.DeployingHeavyTurrets:
                    //PerformDeployingInitialPylonsTreatment(); // TODO: or not...
                    break;

                case TerraformerState.Idle:
                    PerformIdleTreatment();
                    break;
            }
            PerformTreatmentAccordingToWeather();

            if ((Find.TickManager.TicksGame % 300 == 0)
                && (this.invasionIsDone == false))
            {
                CheckIfBuildingIsAttacked();
            }

            ComputeDrawingParameters();
        }

        public void PerformLandingTreatment()
        {
            const int periodInTicks = 600;

            if (this.ticksCounter % periodInTicks != 0)
            {
                return;
            }
            IntVec2 foundationsSize = new IntVec2();
            foundationsSize.x = 1 + 2 * this.foundationRange;
            foundationsSize.z = 1 + 2 * this.foundationRange;
            if (this.foundationRange <= foundationRangeMax)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.Position, this.foundationRange, true))
                {
                    Find.TerrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
                }
                this.foundationRange++;
            }
            else
            {
                for (int rotationIndex = 0; rotationIndex < 4; rotationIndex++)
                {
                    Rot4 rotation = new Rot4(rotationIndex);
                    Find.TerrainGrid.SetTerrain(this.Position + new IntVec3(0, 0, 2).RotatedBy(rotation), TerrainDef.Named("MetalTile"));
                }
                this.ticksCounter = 0;
                this.terraformerState = TerraformerState.Idle;
            }
        }

        public void PerformDeployingPylonTreatment()
        {
            const int phasePeriodInTicks = 600;
            
            if (this.pylonConstructionIsInProgress == false)
            {
                if (this.pylonsList.Count == 8)
                {
                    // This case should not happen.
                    this.ticksCounter = 0;
                    this.terraformerState = TerraformerState.Idle;
                    return;
                }

                // Find a random free pylon position.
                do
                {
                    this.currentPylonIndex = Rand.RangeInclusive(0, 7);
                } while (this.pylonIsConstructed[this.currentPylonIndex] != false);
                this.pylonConstructionIsInProgress = true;
                this.ticksCounter = 0;
            }
            if (this.ticksCounter == phasePeriodInTicks)
            {
                Find.TerrainGrid.SetTerrain(this.pylonsPositions[this.currentPylonIndex], TerrainDefOf.Concrete);
            }
            else if (this.ticksCounter == 2 * phasePeriodInTicks)
            {
                for (int cellIndex = 0; cellIndex < 5; cellIndex++)
                {
                    Find.TerrainGrid.SetTerrain(this.pylonsPositions[this.currentPylonIndex] + GenRadial.RadialPattern[cellIndex], TerrainDefOf.Concrete);
                }
            }
            else if (this.ticksCounter == 3 * phasePeriodInTicks)
            {
                for (int cellIndex = 0; cellIndex < 9; cellIndex++)
                {
                    Find.TerrainGrid.SetTerrain(this.pylonsPositions[this.currentPylonIndex] + GenRadial.RadialPattern[cellIndex], TerrainDefOf.Concrete);
                }
            }
            else if (this.ticksCounter == 4 * phasePeriodInTicks)
            {
                for (int cellIndex = 0; cellIndex < 13; cellIndex++)
                {
                    Find.TerrainGrid.SetTerrain(this.pylonsPositions[this.currentPylonIndex] + GenRadial.RadialPattern[cellIndex], TerrainDefOf.Concrete);
                }
            }
            else if (this.ticksCounter == 5 * phasePeriodInTicks)
            {
                Thing pylon = ThingMaker.MakeThing(Util_MechanoidTerraformer.MechanoidPylonDef);
                pylon.SetFactionDirect(Faction.OfMechanoids);
                GenSpawn.Spawn(pylon, this.pylonsPositions[this.currentPylonIndex]);
                this.pylonsList.Add(pylon);
                this.pylonIsConstructed[this.currentPylonIndex] = true;
            }
            else if (this.ticksCounter == 6 * phasePeriodInTicks)
            {
                this.pylonConstructionIsInProgress = false;
                this.ticksCounter = 0;
                this.terraformerState = TerraformerState.Idle;
            }
            else
            {
                Vector3 dustMotePosition = this.pylonsPositions[this.currentPylonIndex].ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead) + Gen.RandomHorizontalVector(1.2f);
                MoteThrower.ThrowDustPuff(dustMotePosition, 0.8f);
            }
        }

        public void PerformStartingGeneratorTreatment()
        {
            const int startingGeneratorDurationInTicks = 600;

            if (this.generatorIsStarted == false)
            {
                StartNewTerraformingThunderStorm();
                this.generatorIsStarted = true;
            }

            this.generatorAngleIncrementPerTickInDegrees += 0.01f;
            if (this.ticksCounter >= startingGeneratorDurationInTicks)
            {
                this.ticksCounter = 0;
                this.terraformerState = TerraformerState.Harnessingpower;
            }
        }

        public void StartNewTerraformingThunderStorm()
        {
            WeatherDef thunderStormDef = Util_MechanoidTerraformer.TerraformingThunderstormDef;
            this.terraformingThunderstormDurationInTicks = (int)(Rand.Range(thunderStormDef.durationRange.min, thunderStormDef.durationRange.max) * (this.pylonsList.Count / 8f));
            Find.WeatherManager.TransitionTo(thunderStormDef);
        }

        public void PerformHarnessingPowerTreatment()
        {
            if (this.ticksCounter >= this.terraformingThunderstormDurationInTicks)
            {
                this.ticksCounter = 0;
                this.terraformerState = TerraformerState.StoppingGenerator;
                Find.WeatherManager.TransitionTo(this.desiredWeather);
                this.desiredWeather = WeatherDef.Named("Clear");
                return;
            }

            // The weather should not change before the storm is finished.
            if (Find.WeatherManager.curWeather != Util_MechanoidTerraformer.TerraformingThunderstormDef)
            {
                this.desiredWeather = Find.WeatherManager.curWeather;
                Find.WeatherManager.TransitionTo(Util_MechanoidTerraformer.TerraformingThunderstormDef);
            }
        }

        public void PerformStoppingGeneratorTreatment()
        {
            this.generatorAngleIncrementPerTickInDegrees -= 0.01f;
            if (this.generatorAngleIncrementPerTickInDegrees <= 0)
            {
                this.generatorAngleIncrementPerTickInDegrees = 0;
                this.generatorIsStarted = false;
                // Next storm in [15 .. 25] * stormMaxDuration.
                this.terraformingThunderstormNextStartTicks = this.totalTicksCounter + (int)(Util_MechanoidTerraformer.TerraformingThunderstormDef.durationRange.max * (20f + Rand.Range(-5f, 5f)));         
                this.ticksCounter = 0;
                this.terraformerState = TerraformerState.Idle;
            }
        }

        public void PerformIdleTreatment()
        {
            if ((this.totalTicksCounter >= (totalTicksBetweenPylonsDeployment * (this.pylonsList.Count + 1))) && (this.pylonsList.Count < 8))
            {
                this.ticksCounter = 0;
                this.terraformerState = TerraformerState.DeployingPylon;
            }
            /*else if ((this.totalTicksCounter >= totalTicksBeforeDeployingLightTurrets) && (this.deployingLightTurretsIsDone == false))
            {
                this.ticksCounter = 0;
                this.terraformerState = TerraformerState.DeployingLightTurrets;
                this.deployingLightTurretsIsDone = true;
            }
            else if ((this.totalTicksCounter >= totalTicksBeforeDeployingHeavyTurrets) && (this.deployingHeavyTurretsIsDone == false))
            {
                this.ticksCounter = 0;
                this.terraformerState = TerraformerState.DeployingHeavyTurrets;
                this.deployingHeavyTurretsIsDone = true;
            }*/
            else if ((this.totalTicksCounter >= totalTicksBeforeLaunchingInvasion) && (this.invasionIsDone == false))
            {
                this.ticksCounter = 0;
                string eventText = "   Beware! The mechanoid invasion has begun. This world will soon become a new mechanoid hive!";
                LaunchInvasion("Invasion", eventText, 2f, 10, LetterType.BadUrgent);
            }
            else if ((this.totalTicksCounter >= this.terraformingThunderstormNextStartTicks) && (this.pylonsList.Count >= 1))
            {
                this.ticksCounter = 0;
                this.terraformerState = TerraformerState.StartingGenerator;
            }
        }

        public void PerformTreatmentAccordingToWeather()
        {
            if (this.reverseEngineeringState == ReverseEngineeringState.PowerRerouted)
            {
                if (((Find.WeatherManager.curWeather == Util_MechanoidTerraformer.TerraformingThunderstormDef)
                    || (Find.WeatherManager.curWeather == WeatherDef.Named("DryThunderstorm"))
                    || (Find.WeatherManager.curWeather == WeatherDef.Named("RainyThunderstorm")))
                    && (Find.WeatherManager.curWeatherAge >= 600))
                {
                    TryFireLightningStrikeOnPylon();

                    if (Find.WeatherManager.curWeather == Util_MechanoidTerraformer.TerraformingThunderstormDef)
                    {
                        this.powerComp.powerOutputInt = powerOutputFromInternalGenerator + powerOutputPerPylonDuringTerraformerThunderstorm * this.pylonsList.Count;
                    }
                    else if ((Find.WeatherManager.curWeather == WeatherDef.Named("DryThunderstorm"))
                        || (Find.WeatherManager.curWeather == WeatherDef.Named("RainyThunderstorm")))
                    {
                        this.powerComp.powerOutputInt = powerOutputFromInternalGenerator + powerOutputPerPylonDuringNaturalThunderstorm * this.pylonsList.Count;
                    }
                }
                else
                {
                    this.powerComp.powerOutputInt = powerOutputFromInternalGenerator;
                }
            }
            else
            {
                this.powerComp.powerOutputInt = 0;
            }
        }

        public void TryFireLightningStrikeOnPylon()
        {
            if (Rand.Value < (float)this.pylonsList.Count * strikeChancePerPylon)
            {
                if (this.pylonsList.Count > 0)
                {
                    IntVec3 strikePosition = this.pylonsList.RandomElement<Thing>().Position;
                    WeatherEvent_LightningStrike lightningStrike = new WeatherEvent_LightningStrike(strikePosition);
                    Find.WeatherManager.eventHandler.AddEvent(lightningStrike);
                }
            }
        }

        public void FinishPowerRerouting()
        {
            this.SetFaction(Faction.OfColony);

            string eventText = "   You have successfully rerouted the power network of the mechanoid terraformer.\n\n"
                + "Remember that you need some batteries to stock the brief surges of energy generated by the thunderstorms.";
            Find.LetterStack.ReceiveLetter("Rerouting", eventText, LetterType.Good, this.Position);
            this.reverseEngineeringState = Building_MechanoidTerraformer.ReverseEngineeringState.PowerRerouted;
        }

        public void FinishWeatherControllerExtraction()
        {
            string eventText = "   You have successfully extracted the weather controller from the mechanoid terraformer.\n\n"
                + "You can now build your own one to unleash the sky's wrath on your ennemies.";
            Find.LetterStack.ReceiveLetter("Extraction", eventText, LetterType.Good, this.Position);
            this.Destroy(DestroyMode.Deconstruct);
            GenSpawn.Spawn(ThingMaker.MakeThing(Util_MechanoidTerraformer.WeatherControllerCoreDef), this.Position);
            if (Find.ResearchManager.IsFinished(ResearchProjectDef.Named("ResearchSkywrathController")) == false)
            {
                Find.ResearchManager.currentProj = ResearchProjectDef.Named("ResearchSkywrathController");
                Find.ResearchManager.InstantFinish(ResearchProjectDef.Named("ResearchSkywrathController"));
            }

            // Beacon has not been disabled, anticipated invasion is launched.
            if (this.invasionIsDone == false)
            {
                string eventTextInvasion = "   Before its extraction, the mechanoid terraformer AI core controller somehow detected your attempts at shutting it down.\n"
                    + "It had enough time to emit an emergency call. Be prepared to welcome the incoming terrafomer defending force.";
                this.LaunchInvasion("Invasion", eventTextInvasion, 0.8f, 4, LetterType.BadUrgent);
            }
        }

        public void LaunchInvasion(string eventTitle, string eventText, float raidPointsFactor, int dropsNumber, LetterType letterType)
        {
            this.LaunchInvasion(eventTitle, eventText, raidPointsFactor, dropsNumber, letterType, IntVec3.Invalid);
        }

        public void LaunchInvasion(string eventTitle, string eventText, float raidPointsFactor, int dropsNumber, LetterType letterType, IntVec3 spawnPosition)
        {
            this.invasionIsDone = true;

            // Get an indicative amount of points based on the colony wealth so it scales up well for late-game colonies.
            IncidentParms invasionParameters = IncidentMakerUtility.DefaultParmsNow(Find.Storyteller.def, IncidentCategory.ThreatBig);
            invasionParameters.faction = Faction.OfMechanoids;
            invasionParameters.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            invasionParameters.raidArrivalMode = PawnsArriveMode.EdgeDrop;
            invasionParameters.raidNeverFleeIndividual = true;
            invasionParameters.raidPodOpenDelay = 800;
            if (dropsNumber > 0)
            {
                invasionParameters.points *= (raidPointsFactor / dropsNumber);
                if (invasionParameters.points < 320)
                {
                    invasionParameters.points = 320;
                }
                for (int dropIndex = 0; dropIndex < dropsNumber; dropIndex++)
                {
                    IntVec3 dropPodSpawningPosition;
                    float squadPoint = invasionParameters.points;
                    if (spawnPosition.IsValid)
                    {
                        invasionParameters.spawnCenter = spawnPosition;
                    }
                    else
                    {
                        RCellFinder.TryFindRandomPawnEntryCell(out invasionParameters.spawnCenter);
                    }
                    List<Pawn> mechanoidsList = new List<Pawn>();
                    while (squadPoint >= PawnKindDef.Named("Scyther").combatPower)
                    {
                        bool validDropPodCellIsFound = DropCellFinder.TryFindDropSpotNear(invasionParameters.spawnCenter, out dropPodSpawningPosition, false, true);
                        if (validDropPodCellIsFound)
                        {
                            Faction faction = Faction.OfMechanoids;
                            Pawn squadMember;
                            if (Rand.Value < 0.6f)
                            {
                                squadMember = PawnGenerator.GeneratePawn(PawnKindDef.Named("Scyther"), faction);
                                squadPoint -= PawnKindDef.Named("Scyther").combatPower;
                            }
                            else
                            {
                                squadMember = PawnGenerator.GeneratePawn(PawnKindDef.Named("Centipede"), faction);
                                squadPoint -= (int)PawnKindDef.Named("Centipede").combatPower;
                            }
                            mechanoidsList.Add(squadMember);
                            DropPodUtility.MakeDropPodAt(dropPodSpawningPosition, new DropPodInfo
                            {
                                SingleContainedThing = squadMember,
                                openDelay = 800,
                                leaveSlag = true
                            });
                        }
                    }
                    StateGraph stateGraph = GraphMaker.AssaultColonyGraph(Faction.OfMechanoids, false, false);
                    BrainMaker.MakeNewBrain(Faction.OfMechanoids, stateGraph, mechanoidsList);
                }
            }
            Find.LetterStack.ReceiveLetter(eventTitle, eventText, letterType, this.Position);
        }

        // TODO: cannot use the default incident maker as it complains with error the pawn points is too low...
        /*public void LaunchInvasion(string eventText, float raidPointsFactor, int dropsNumber, GameEventType letterType, IntVec3 spawnPosition)
        {
            this.invasionIsDone = true;

            // Get an indicative amount of points based on the colony wealth so it scales up well for late-game colonies.
            IncidentParms invasionParameters = IncidentMakerUtility.DefaultParmsNow(Find.Storyteller.def, IncidentCategory.ThreatBig);
            invasionParameters.faction = Faction.OfMechanoids;
            invasionParameters.raidStyle = RaidStyle.ImmediateAttack;
            invasionParameters.raidArrivalMode = PawnsArriveMode.EdgeDrop;
            invasionParameters.raidNeverFleeIndividual = true;
            invasionParameters.raidPodOpenDelay = 800;
            if (dropsNumber > 0)
            {
                invasionParameters.points *= (raidPointsFactor / dropsNumber);
                if (invasionParameters.points < 300)
                {
                    invasionParameters.points = 300;
                }
                for (int dropIndex = 0; dropIndex < dropsNumber; dropIndex++)
                {
                    if (spawnPosition.IsValid)
                    {
                        invasionParameters.spawnCenter = spawnPosition;
                    }
                    else
                    {
                        RCellFinder.TryFindRandomPawnEntryCell(out invasionParameters.spawnCenter);
                    }
                    DefDatabase<IncidentDef>.GetNamed("RaidEnemy", true).Worker.TryExecute(invasionParameters);
                }
            }
            Find.History.AddGameEvent(eventText, letterType, true);
        }*/

        public void CheckIfBuildingIsAttacked()
        {
            if (this.HitPoints < this.MaxHitPoints)
            {
                string eventText = "   Attacking the artifact seems to be a bad idea. You have detected some sort of emergency call sent from it.\n"
                    + "Whatever it is, this does not sound good at all...";
                this.LaunchInvasion("Invasion", eventText, 1f, 1, LetterType.BadUrgent, this.InteractionCell);
            }
        }

        // ===================== Pawns interraction functions =====================

        public override bool ClaimableBy(Faction faction)
        {
            // Capture must be performed by reverse engineering the artifact.
            return false;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            if (pawn.Dead
                || pawn.Downed
                || pawn.IsBurning())
            {
                FloatMenuOption item = new FloatMenuOption("Cannot use (incapacitated)", null);
                yield return item;
                yield break;
            }

            if (pawn.CanReserve(this) == false)
            {
                FloatMenuOption item = new FloatMenuOption("Cannot use (reserved)", null);
                yield return item;
                yield break;
            }
            else if (pawn.CanReach(this, PathEndMode.ClosestTouch, Danger.Some) == false)
            {
                FloatMenuOption item = new FloatMenuOption("Cannot use (no path)", null);
                yield return item;
                yield break;
            }
            else
            {
                switch (this.reverseEngineeringState)
                {
                    case ReverseEngineeringState.BuildingNotApproched:
                        Action action = delegate
                        {
                            Job job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_ScoutStrangeArtifact), this);
                            pawn.drafter.TakeOrderedJob(job);
                        };
                        FloatMenuOption item = new FloatMenuOption("Scout strange artifact", action);
                        yield return item;
                        break;

                    case ReverseEngineeringState.BuildingNotSecured:
                        Action action2 = delegate
                        {
                            Job job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_SecureStrangeArtifact), this);
                            pawn.drafter.TakeOrderedJob(job);
                        };
                        FloatMenuOption item2 = new FloatMenuOption("Secure strange artifact", action2);
                        yield return item2;
                        break;

                    case ReverseEngineeringState.Studying:
                        if (pawn.drafter.Drafted)
                        {
                            yield break;
                        }
                        if (this.studyIsPaused)
                        {
                            Action action3 = delegate
                            {
                                this.studyIsPaused = false;
                                if ((pawn.skills.GetSkill(SkillDefOf.Research).TotallyDisabled == false)
                                    && (pawn.skills.GetSkill(SkillDefOf.Research).level >= minResearchLevelToStudyArtifact))
                                {
                                    Job job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_StudyStrangeArtifact), this);
                                    pawn.drafter.TakeOrderedJob(job);
                                } 
                                else
                                {
                                    Messages.Message(pawn.Name.ToStringShort + " is not skilled enough to study the strange artifact (research level " + minResearchLevelToStudyArtifact + " is required).", MessageSound.RejectInput);
                                    pawn.jobs.StopAll(true);
                                }
                            };
                            FloatMenuOption item3 = new FloatMenuOption("Start study", action3);
                            yield return item3;
                        }
                        else
                        {
                            Action action4 = delegate
                            {
                                this.studyIsPaused = true;
                                pawn.jobs.StopAll(true);
                            };
                            FloatMenuOption item4 = new FloatMenuOption("Pause study", action4);
                            yield return item4;
                        }
                        break;

                    case ReverseEngineeringState.StudyCompleted:
                        if (pawn.drafter.Drafted)
                        {
                            yield break;
                        }
                        Action action5 = delegate
                        {
                            this.reroutingIsPaused = false;
                            this.reverseEngineeringState = ReverseEngineeringState.ReroutingPower;
                            if ((pawn.skills.GetSkill(SkillDefOf.Research).TotallyDisabled == false)
                                && (pawn.skills.GetSkill(SkillDefOf.Research).level >= minResearchLevelToReroutePower))
                            {
                                Job job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_ReroutePower), this);
                                pawn.drafter.TakeOrderedJob(job);
                            }
                            else
                            {
                                Messages.Message(pawn.Name.ToStringShort + " is not skilled enough to reroute the terraformer power network (research level " + minResearchLevelToReroutePower + " is required).", MessageSound.RejectInput);
                                pawn.jobs.StopAll(true);
                            }
                        };
                        FloatMenuOption item5 = new FloatMenuOption("1) Reroute power network OR", action5);
                        yield return item5;

                        Action action6 = delegate
                        {
                            this.extractionIsPaused = false;
                            this.reverseEngineeringState = ReverseEngineeringState.ExtractingWeatherController;
                            if ((pawn.skills.GetSkill(SkillDefOf.Research).TotallyDisabled == false)
                                && (pawn.skills.GetSkill(SkillDefOf.Research).level >= minResearchLevelToExtractWeatherController))
                            {
                                Job job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_ExtractWeatherController), this);
                                pawn.drafter.TakeOrderedJob(job);
                            }
                            else
                            {
                                Messages.Message(pawn.Name.ToStringShort + " is not skilled enough to extract the terraformer weather controller (research level " + minResearchLevelToExtractWeatherController + " is required).", MessageSound.RejectInput);
                                pawn.jobs.StopAll(true);
                            }
                        };
                        FloatMenuOption item6 = new FloatMenuOption("2) Extract weather controller", action6);
                        yield return item6;
                        break;

                    case ReverseEngineeringState.ReroutingPower:
                        if (pawn.drafter.Drafted)
                        {
                            yield break;
                        }
                        if (this.reroutingIsPaused)
                        {
                            Action action7 = delegate
                            {
                                this.reroutingIsPaused = false;
                                if ((pawn.skills.GetSkill(SkillDefOf.Research).TotallyDisabled == false)
                                    && (pawn.skills.GetSkill(SkillDefOf.Research).level >= minResearchLevelToReroutePower))
                                {
                                    Job job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_ReroutePower), this);
                                    pawn.drafter.TakeOrderedJob(job);
                                }
                                else
                                {
                                    Messages.Message(pawn.Name.ToStringShort + " is not skilled enough to reroute the terraformer power network (research level " + minResearchLevelToReroutePower + " is required).", MessageSound.RejectInput);
                                    pawn.jobs.StopAll(true);
                                }
                            };
                            FloatMenuOption item7 = new FloatMenuOption("Reroute power network", action7);
                            yield return item7;
                        }
                        else
                        {
                            Action action8 = delegate
                            {
                                this.reroutingIsPaused = true;
                                pawn.jobs.StopAll(true);
                            };
                            FloatMenuOption item8 = new FloatMenuOption("Pause power rerouting", action8);
                            yield return item8;
                        }
                        break;

                    case ReverseEngineeringState.ExtractingWeatherController:
                        if (pawn.drafter.Drafted)
                        {
                            yield break;
                        }
                        if (this.extractionIsPaused)
                        {
                            Action action9 = delegate
                            {
                                this.extractionIsPaused = false;
                                if ((pawn.skills.GetSkill(SkillDefOf.Research).TotallyDisabled == false)
                                    && (pawn.skills.GetSkill(SkillDefOf.Research).level >= minResearchLevelToExtractWeatherController))
                                {
                                    Job job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_ReroutePower), this);
                                    pawn.drafter.TakeOrderedJob(job);
                                }
                                else
                                {
                                    Messages.Message(pawn.Name.ToStringShort + " is not skilled enough to extract the terraformer weather controller (research level " + minResearchLevelToExtractWeatherController + " is required).", MessageSound.RejectInput);
                                    pawn.jobs.StopAll(true);
                                }
                            };
                            FloatMenuOption item9 = new FloatMenuOption("Extract weather controller", action9);
                            yield return item9;
                        }
                        else
                        {
                            Action action10 = delegate
                            {
                                this.extractionIsPaused = true;
                                pawn.jobs.StopAll(true);
                            };
                            FloatMenuOption item10 = new FloatMenuOption("Pause weather controller extraction", action10);
                            yield return item10;
                        }
                        break;
                }

                if ((this.reverseEngineeringState >= ReverseEngineeringState.StudyCompleted)
                    && (this.invasionIsDone == false))
                {
                    Action action11 = delegate
                    {
                        Job job = new Job(DefDatabase<JobDef>.GetNamed(Util_MechanoidTerraformer.JobDefName_DisableBeacon), this);
                        pawn.drafter.TakeOrderedJob(job);
                    };
                    FloatMenuOption item11 = new FloatMenuOption("Disable beacon", action11);
                    yield return item11;
                }
            }
            yield break;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            int groupKeyBase = 700000200;

            // The deconstruction option is not available, thus we don't add the base gizmos.
            List<Gizmo> gizmosList = new List<Gizmo>();
            /*foreach (Gizmo gizmo in base.GetGizmos())
            {
                gizmosList.Add(gizmo);
            }*/

            if (this.reverseEngineeringState >= Building_MechanoidTerraformer.ReverseEngineeringState.StudyCompleted)
            {
                Command_Action displayStudyReportButton = new Command_Action();
                displayStudyReportButton.icon = displayStudyReportIcon;
                displayStudyReportButton.defaultDesc = "Display study report.";
                displayStudyReportButton.defaultLabel = "Display study report.";
                displayStudyReportButton.activateSound = SoundDef.Named("Click");
                displayStudyReportButton.action = new Action(DisplayStudyReport);
                displayStudyReportButton.groupKey = groupKeyBase + 1;
                gizmosList.Add(displayStudyReportButton);
            }
            return gizmosList;
        }
        
        // ===================== Inspection functions =====================
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.AppendLine();
            if (this.reverseEngineeringState == ReverseEngineeringState.Studying)
            {
                string pausedText = "";
                if (this.studyIsPaused)
                {
                    pausedText = " (paused)";
                }
                float studyProgress = (float)this.studyCounter / (float)studyCounterTargetValue * 100f;
                stringBuilder.AppendLine("Study progress" + pausedText + ": " + studyProgress.ToString("F0") + " %");
            }
            else if (this.reverseEngineeringState == ReverseEngineeringState.ReroutingPower)
            {
                string pausedText = "";
                if (this.reroutingIsPaused)
                {
                    pausedText = " (paused)";
                }
                float reroutingProgress = (float)this.reroutingCounter / (float)reroutingCounterTargetValue * 100f;
                stringBuilder.AppendLine("Rerouting progress" + pausedText + ": " + reroutingProgress.ToString("F0") + " %");
            }
            else if (this.reverseEngineeringState == ReverseEngineeringState.ExtractingWeatherController)
            {
                string pausedText = "";
                if (this.extractionIsPaused)
                {
                    pausedText = " (paused)";
                }
                float extractionProgress = (float)this.extractionCounter / (float)extractionCounterTargetValue * 100f;
                stringBuilder.AppendLine("Extraction progress" + pausedText + ": " + extractionProgress.ToString("F0") + " %");
            }

            return stringBuilder.ToString();
        }

        public void DisplayStudyReport()
        {
            DisplayStudyReport("");
        }

        public void DisplayStudyReport(string studyReportHeader)
        {
            string studyReport = studyReportHeader
                + "### Study report ###\n\n"
                + "Study subject: strange mechanoid artifact\n\n"
                + "- Artifact location: " + this.Position.ToString() + "\n"
                + "- Observations:\n"
                + "  1. the artifact has deployed underground foundations.\n"
                + "  2. some pylons have been erected near the main structure.\n"
                + "  3. the artifact seems to generate powerful artificial thunderstorms.\n\n"
                + "- Deductions:\n"
                + "  1. this device appears to be a mechanoid terraformer.\n"
                + "  2. its purpose is to re-shape the surface of this planet to prepare a large-scale invasion.\n\n"
                + "- Recommandations:\n"
                + "  1. proceed with *extreme* caution when attempting anything on the artifact!\n"
                + "  2. deactivating the onboard beacon should be a priority to avoid future invasion.\n"
                + "  3. consider re-routing the power generated during artificial thunderstorms.\n"
                + "  4. further research may allow us to take advantage of those artificial thunderstorms.\n";
            Find.LetterStack.ReceiveLetter("Study report", studyReport, LetterType.BadUrgent, this.Position);
        }

        /// <summary>
        /// Gets the state as a string.
        /// </summary>
        public string GetStateAsString(TerraformerState state)
        {
            string stateAsString = "";

            switch (state)
            {
                case TerraformerState.Idle:
                    stateAsString = "idle";
                    break;
                case TerraformerState.Landing:
                    stateAsString = "landing";
                    break;
                case TerraformerState.DeployingPylon:
                    stateAsString = "deploying pylon";
                    break;
                case TerraformerState.StartingGenerator:
                    stateAsString = "starting generator";
                    break;
                case TerraformerState.Harnessingpower:
                    stateAsString = "harnessing power";
                    break;
                case TerraformerState.StoppingGenerator:
                    stateAsString = "stopping generator";
                    break;
                case TerraformerState.DeployingLightTurrets:
                    stateAsString = "deploying light turrets";
                    break;
                case TerraformerState.DeployingHeavyTurrets:
                    stateAsString = "deploying heavy turrets";
                    break;
            }

            return stateAsString;
        }

        // ===================== Drawing functions =====================
        public void ComputeDrawingParameters()
        {
            this.generatorAngleInDegrees += this.generatorAngleIncrementPerTickInDegrees;
            if ((this.generatorIsStarted)
                && (Rand.Value < 0.015))
            {
                MoteThrower.ThrowStatic(this.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead) + Gen.RandomHorizontalVector(0.6f), ThingDefOf.Mote_ExplosionFlash, 3f);
            }

        }

        public override void Draw()
        {
            // Mind the Vector3 so the generator is drawn over the main terraformer building.
            generatorMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect + new Vector3(0f, 0.1f, 0f), this.generatorAngleInDegrees.ToQuat(), this.generatorScale);
            Graphics.DrawMesh(MeshPool.plane10, this.generatorMatrix, this.generatorTexture, 0);
        }
    }
}
