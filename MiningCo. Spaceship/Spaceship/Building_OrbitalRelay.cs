using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;    // Always needed
using RimWorld;       // RimWorld specific functions are found here
using Verse;          // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI.
using Verse.AI.Group; // Needed when you do something with squad AI.
using Verse.Sound;    // Needed when you do something with the Sound

namespace Spaceship
{
    [StaticConstructorOnStartup]
    public class Building_OrbitalRelay : Building
    {
        // Supply ship data.
        public const int spaceshipLandingCheckPeriodInTick = GenTicks.TicksPerRealSecond;

        public int lastPeriodicSupplyTick = 0;
        public int lastRequestedSupplyTick = 0;
        public int lastMedicalSupplyTick = 0;
        public int nextSpaceshipLandingCheckTick = 0;
        public bool landingPadIsAvailable = false;

        // Dish periodical rotation.
        public const float dishRotationPerTick = 0.06f;
        public const int rotationIntervalMin = 1200;
        public const int rotationIntervalMax = 2400;
        public int ticksToNextRotation = rotationIntervalMin;
        public const int rotationDurationMin = 500;
        public const int rotationDurationMax = 1500;
        public int ticksToRotationEnd = 0;
        public bool clockwiseRotation = true;

        // Power.
        public CompPowerTrader powerComp = null;

        // Sound.
        public Sustainer rotationSoundSustainer = null;

        // Texture.
        public static Material dishTexture = MaterialPool.MatFrom("Things/Building/OrbitalRelay/SatelliteDish");
        public float dishRotation = 0f;
        public static Vector3 dishScale = new Vector3(5f, 0, 5f);
        public Matrix4x4 dishMatrix = default(Matrix4x4);

        public bool canUseConsoleNow
        {
            get
            {
                return ((this.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare) == false)
                    && this.powerComp.PowerOn);
            }
        }

        // ===================== Setup work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();

            if (respawningAfterLoad == false)
            {
                this.dishRotation = this.Rotation.AsAngle;
                Util_Misc.Partnership.InitializeFeeIfNeeded(this.Map);
                Util_Misc.Partnership.InitializePeriodicSupplyTickIfNeeded(this.Map);
                Util_Misc.Partnership.InitializeRequestedSupplyTickIfNeeded(this.Map);
                Util_Misc.Partnership.InitializeMedicalSupplyTickIfNeeded(this.Map);
                Util_Misc.Partnership.InitializeAirstrikeTickIfNeeded(this.Map);
                if (Util_Misc.Partnership.feeInSilver[this.Map] > 0)
                {
                    Find.LetterStack.ReceiveLetter("Partnership fee", "To establish a regular trading route or resolve a dispute with MiningCo., you must pay a \"partnership fee\".\nUse the orbital relay console to proceed.", LetterDefOf.NeutralEvent, new TargetInfo(this.Position, this.Map));
                }
                UpdateLandingPadAvailability();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            StopRotationSound();
            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastPeriodicSupplyTick, "lastPeriodicSupplyTick");
            Scribe_Values.Look<int>(ref this.lastRequestedSupplyTick, "lastRequestedSupplyTick");
            Scribe_Values.Look<int>(ref this.lastMedicalSupplyTick, "lastMedicalSupplyTick");
            Scribe_Values.Look<bool>(ref this.landingPadIsAvailable, "landingPadIsAvailable");
            Scribe_Values.Look<int>(ref this.nextSpaceshipLandingCheckTick, "nextSpaceshipLandingCheckTick");

            Scribe_Values.Look<int>(ref this.ticksToNextRotation, "ticksToNextRotation");
            Scribe_Values.Look<float>(ref this.dishRotation, "dishRotation");
            Scribe_Values.Look<bool>(ref this.clockwiseRotation, "clockwiseRotation");
            Scribe_Values.Look<int>(ref this.ticksToRotationEnd, "ticksToRotationEnd");
        }

        // ===================== Main function =====================
        /// <summary>
        /// When powered on, periodically spawn a supply spaceship and update dish rotation.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            
            if (powerComp.PowerOn == false)
            {
                StopRotationSound();
            }
            else
            {
                if (Find.TickManager.TicksGame >= this.nextSpaceshipLandingCheckTick)
                {
                    this.nextSpaceshipLandingCheckTick = Find.TickManager.TicksGame + spaceshipLandingCheckPeriodInTick;
                    // Update landing pads availability: it can change due to blocking trees for example.
                    UpdateLandingPadAvailability();

                    // Try spawn periodic supply spaceship.
                    if ((Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer) == false)
                        && (Util_Misc.Partnership.feeInSilver[this.Map] == 0)
                        && (Find.TickManager.TicksGame >= Util_Misc.Partnership.nextPeriodicSupplyTick[this.Map])
                        && this.landingPadIsAvailable
                        && (this.Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare) == false)
                        && (this.Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) == false))
                    {
                        Building_LandingPad landingPad = Util_LandingPad.GetBestAvailableLandingPad(this.Map);
                        if (landingPad != null)
                        {
                            // Found an available landing pad. Spawn periodic supply spaceship.
                            Util_Spaceship.SpawnLandingSpaceship(landingPad, SpaceshipKind.CargoPeriodic);
                        }
                    }
                }
                
                // Satellite dish rotation.
                UpdateDishRotation();
            }
        }

        /// <summary>
        /// Check if a landing pad is available.
        /// </summary>
        public void UpdateLandingPadAvailability()
        {
            this.landingPadIsAvailable = (Util_LandingPad.GetAllFreeAndPoweredLandingPads(this.Map) != null);
        }

        // ===================== Other functions =====================
        /// <summary>
        /// Notify that a periodic cargo supply spaceship is landing.
        /// </summary>
        public void Notify_CargoSpaceshipPeriodicLanding()
        {
            this.lastPeriodicSupplyTick = Find.TickManager.TicksGame;
        }

        /// <summary>
        /// Notify that a requested cargo supply spaceship is landing.
        /// </summary>
        public void Notify_CargoSpaceshipRequestedLanding()
        {
            this.lastRequestedSupplyTick = Find.TickManager.TicksGame;
        }

        /// <summary>
        /// Notify that a medical spaceship is landing.
        /// </summary>
        public void Notify_MedicalSpaceshipLanding()
        {
            this.lastMedicalSupplyTick = Find.TickManager.TicksGame;
        }

        // ===================== Inspect panel =====================
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (this.powerComp.PowerOn == false)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Orbital link down");
                return stringBuilder.ToString();
            }

            // Goodwill.
            stringBuilder.AppendLine();
            stringBuilder.Append("MiningCo. goodwill: " + Util_Faction.MiningCoFaction.GoodwillWith(Faction.OfPlayer));
            if (Util_Faction.MiningCoFaction.GoodwillWith(Faction.OfPlayer) <= -80)
            {
                stringBuilder.Append(" (hostile)");
            }

            if ((Util_Misc.Partnership.feeInSilver[this.Map] > 0)
                || (Util_Misc.Partnership.globalGoodwillFeeInSilver > 0))
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Partnership fee unpaid");
                return stringBuilder.ToString();
            }
            else
            {
                // Periodic cargo supply.
                stringBuilder.AppendLine();
                stringBuilder.Append("Periodic supply: ");
                if ((this.lastPeriodicSupplyTick > 0)
                    && (Find.TickManager.TicksGame >= this.lastPeriodicSupplyTick)
                    && ((Find.TickManager.TicksGame - this.lastPeriodicSupplyTick) < (FlyingSpaceshipLanding.horizontalTrajectoryDurationInTicks + FlyingSpaceshipLanding.verticalTrajectoryDurationInTicks)))
                {
                    stringBuilder.Append("in approach");
                }
                else if (this.landingPadIsAvailable == false)
                {
                    stringBuilder.Append("no landing pad");
                }
                else
                {
                    string remainingTimeAsString = GenDate.ToStringTicksToPeriodVerbose(Util_Misc.Partnership.nextPeriodicSupplyTick[this.Map] - Find.TickManager.TicksGame);
                    stringBuilder.Append(remainingTimeAsString);
                }
                // Requested cargo supply.
                stringBuilder.AppendLine();
                stringBuilder.Append("Requested supply: ");
                if ((this.lastRequestedSupplyTick > 0)
                    && (Find.TickManager.TicksGame >= this.lastRequestedSupplyTick)
                    && ((Find.TickManager.TicksGame - this.lastRequestedSupplyTick) < (FlyingSpaceshipLanding.horizontalTrajectoryDurationInTicks + FlyingSpaceshipLanding.verticalTrajectoryDurationInTicks)))
                {
                    stringBuilder.Append("in approach");
                }
                else if (this.landingPadIsAvailable == false)
                {
                    stringBuilder.Append("no landing pad");
                }
                else if (Find.TickManager.TicksGame >= Util_Misc.Partnership.nextRequestedSupplyMinTick[this.Map])
                {
                    stringBuilder.Append("available");
                }
                else
                {
                    string remainingTimeAsString = GenDate.ToStringTicksToPeriodVerbose(Util_Misc.Partnership.nextRequestedSupplyMinTick[this.Map] - Find.TickManager.TicksGame);
                    stringBuilder.Append("available in " + remainingTimeAsString);
                }

                // Medical supply.
                stringBuilder.AppendLine();
                stringBuilder.Append("Medical supply: ");
                if ((this.lastMedicalSupplyTick > 0)
                    && (Find.TickManager.TicksGame >= this.lastMedicalSupplyTick)
                    && ((Find.TickManager.TicksGame - this.lastMedicalSupplyTick) < (FlyingSpaceshipLanding.horizontalTrajectoryDurationInTicks + FlyingSpaceshipLanding.verticalTrajectoryDurationInTicks)))
                {
                    stringBuilder.Append("in approach");
                }
                else if (this.landingPadIsAvailable == false)
                {
                    stringBuilder.Append("no landing pad");
                }
                else if (Find.TickManager.TicksGame >= Util_Misc.Partnership.nextMedicalSupplyMinTick[this.Map])
                {
                    stringBuilder.Append("available");
                }
                else
                {
                    string remainingTimeAsString = GenDate.ToStringTicksToPeriodVerbose(Util_Misc.Partnership.nextMedicalSupplyMinTick[this.Map] - Find.TickManager.TicksGame);
                    stringBuilder.Append("available in " + remainingTimeAsString);
                }

                // Air strike.
                stringBuilder.AppendLine();
                if (Find.TickManager.TicksGame >= Util_Misc.Partnership.nextAirstrikeMinTick[this.Map])
                {
                    stringBuilder.Append("Air strike: available");
                }
                else
                {
                    string remainingTimeAsString = GenDate.ToStringTicksToPeriodVerbose(Util_Misc.Partnership.nextAirstrikeMinTick[this.Map] - Find.TickManager.TicksGame);
                    stringBuilder.Append("Air strike: available in " + remainingTimeAsString);
                }
            }

            // Orbital healing.
            if (Util_Misc.OrbitalHealing.healingPawns.Count > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Colonist aboard orbital station:");
                foreach (HealingPawn healingPawn in Util_Misc.OrbitalHealing.healingPawns)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("- " + healingPawn.pawn.Name.ToStringShort + ": ");
                    if (Util_Faction.MiningCoFaction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Hostile)
                    {
                        stringBuilder.Append("\"guest\"");
                    }
                    else
                    {
                        if (healingPawn.healEndTick > Find.TickManager.TicksGame)
                        {
                            stringBuilder.Append("healing, " + GenDate.ToStringTicksToPeriodVerbose(healingPawn.healEndTick - Find.TickManager.TicksGame));
                        }
                        else
                        {
                            stringBuilder.Append("need landing pad");
                        }
                    }
                }
            }
            return stringBuilder.ToString();
        }

        // ===================== Float menu options =====================
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, TraverseMode.ByPawn) == false)
            {
                FloatMenuOption item = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                return new List<FloatMenuOption>
                {
                    item
                };
            }
            if (base.Spawned && base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
            {
                FloatMenuOption item2 = new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
                return new List<FloatMenuOption>
                {
                    item2
                };
            }
            if (this.powerComp.PowerOn == false)

            {
                FloatMenuOption item3 = new FloatMenuOption("CannotUseNoPower".Translate(), null);
                return new List<FloatMenuOption>
                {
                    item3
                };
            }
            if (selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) == false)
            {
                FloatMenuOption item4 = new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label)), null);
                return new List<FloatMenuOption>
                {
                    item4
                };
            }

            // Call MiningCo.
            Action action = delegate
            {
                Job job = new Job(Util_JobDefOf.UseOrbitalRelayConsole, this);
                selPawn.jobs.TryTakeOrderedJob(job);
            };
            FloatMenuOption callMiningCoOption = new FloatMenuOption("Call MiningCo.", action);
            return new List<FloatMenuOption>
            {
                callMiningCoOption
            };
        }
        
        // ===================== Sound =====================
        public void StartRotationSound()
        {
            StopRotationSound();
            this.rotationSoundSustainer = SoundDef.Named("GeothermalPlant_Ambience").TrySpawnSustainer(new TargetInfo(this.Position, this.Map));
        }

        public void StopRotationSound()
        {
            if (this.rotationSoundSustainer != null)
            {
                this.rotationSoundSustainer.End();
                this.rotationSoundSustainer = null;
            }
        }

        // ===================== Draw =====================
        public void UpdateDishRotation()
        {
            if (this.ticksToNextRotation > 0)
            {
                this.ticksToNextRotation--;
                if (this.ticksToNextRotation == 0)
                {
                    this.ticksToRotationEnd = Rand.RangeInclusive(rotationDurationMin, rotationDurationMax);
                    if (Rand.Value < 0.5f)
                    {
                        this.clockwiseRotation = true;
                    }
                    else
                    {
                        this.clockwiseRotation = false;
                    }
                    StartRotationSound();
                }
            }
            else
            {
                if (this.clockwiseRotation)
                {
                    this.dishRotation += dishRotationPerTick;
                }
                else
                {
                    this.dishRotation -= dishRotationPerTick;
                }
                this.ticksToRotationEnd--;
                if (this.ticksToRotationEnd == 0)
                {
                    this.ticksToNextRotation = Rand.RangeInclusive(rotationIntervalMin, rotationIntervalMax);
                    StopRotationSound();
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
            dishMatrix.SetTRS(this.DrawPos + Altitudes.AltIncVect + new Vector3(0f, 3f, 0f), this.dishRotation.ToQuat(), dishScale); // Mind the small offset so dish is above colonists.
            Graphics.DrawMesh(MeshPool.plane10, dishMatrix, Building_OrbitalRelay.dishTexture, 0);
        }
        
        // Debug gizmo.
        // ===================== Gizmos =====================
        /*public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000112;

            Command_Action setHostileButton = new Command_Action();
            setHostileButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/Commands_Primary");
            setHostileButton.defaultLabel = "Set hostile";
            setHostileButton.defaultDesc = "Set hostile.";
            setHostileButton.activateSound = SoundDef.Named("Click");
            setHostileButton.action = new Action(SetHostile);
            setHostileButton.groupKey = groupKeyBase + 1;
            buttonList.Add(setHostileButton);

            Command_Action setAllyButton = new Command_Action();
            setAllyButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/Commands_Primary");
            setAllyButton.defaultLabel = "Set ally";
            setAllyButton.defaultDesc = "Set ally.";
            setAllyButton.activateSound = SoundDef.Named("Click");
            setAllyButton.action = new Action(SetAlly);
            setAllyButton.groupKey = groupKeyBase + 2;
            buttonList.Add(setAllyButton);

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
        public void SetHostile()
        {
            Messages.Message("Set MiningCo. hostile to colony!", MessageTypeDefOf.NegativeEvent);
            Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, -200);
        }
        public void SetAlly()
        {
            Messages.Message("Set MiningCo. ally of colony!", MessageTypeDefOf.NegativeEvent);
            Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, +200);
        }*/
    }
}
