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

using MechanoidTerraformer;

namespace SkywrathController
{
    /// <summary>
    /// Building_SkywrathController class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_SkywrathController : Building_TurretGun
    {
        public enum SkywrathState
        {
            Offline,
            Charging,
            Ready,
            Channeling
        };
        public SkywrathState skywrathState = SkywrathState.Offline;

        public const float chargeTargetValue = 90000;
        public float charge = 0;

        public int channelingTicksCounter = 0;
        public const int channelingWithoutStrikesTicks = 600;
        public const int channelingWithStrikesTicks = 1200;
        public const float chanceToStrikePerTick = 2f / 50f; // 2 strikes per 50 ticks.

        private TargetInfo currentTargetInternal = TargetInfo.Invalid;
        public override TargetInfo CurrentTarget
        {
            get
            {
                return this.currentTargetInternal;
            }
        }

        public float areaOfEffectRadius;

        // Explosive component.
        public CompExplosive explosiveComp;

        // Power needed.
        public float powerOutputDuringCharge;

        // Drawing.
        static Material skywrathTexture;

        // ===================== Spawn setup =====================

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            this.powerOutputDuringCharge = this.powerComp.PowerOutput;
            this.explosiveComp = GetComp<CompExplosive>();
            skywrathTexture = MaterialPool.MatFrom(this.def.graphicData.texPath, ShaderDatabase.Transparent);
            this.areaOfEffectRadius = this.def.building.turretGunDef.Verbs[0].projectileDef.projectile.explosionRadius;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue<float>(ref this.charge, "charge");
            Scribe_Values.LookValue<int>(ref this.channelingTicksCounter, "channelingTicksCounter");
            Scribe_TargetInfo.LookTargetInfo(ref this.currentTargetInternal, "currentTargetInternal");
        }

        // ===================== Main treatments functions =====================

        public override void Tick()
        {
            // Redo the base tick treatment to avoid ticking the fake turret gun's verb.
            base.powerComp.CompTick();
            base.mannableComp.CompTick();
            this.explosiveComp.CompTick();
            this.stunner.StunHandlerTick();

            if (this.stunner.Stunned)
            {
                return;
            }

            if ((base.powerComp.PowerOn == false)
                || base.stunner.Stunned)
            {
                this.skywrathState = SkywrathState.Offline;
            }
            else
            {
                switch (this.skywrathState)
                {
                    case SkywrathState.Offline:
                        this.powerComp.powerOutputInt = this.powerOutputDuringCharge;
                        this.skywrathState = SkywrathState.Charging;
                        break;
                    case SkywrathState.Charging:
                        this.charge++;
                        this.powerComp.powerOutputInt = this.powerOutputDuringCharge;
                        if (this.charge >= chargeTargetValue)
                        {
                            this.charge = chargeTargetValue;
                            this.skywrathState = SkywrathState.Ready;
                        }
                        break;
                    case SkywrathState.Ready:
                        this.powerComp.powerOutputInt = this.powerOutputDuringCharge / 5f;
                        // Wait for an attack order.
                        break;
                    case SkywrathState.Channeling:
                        this.powerComp.powerOutputInt = this.powerOutputDuringCharge / 5f;
                        PerformChannelingTreatment();
                        break;
                }
            }
        }

        public void PerformChannelingTreatment()
        {
            this.channelingTicksCounter++;

            if (this.channelingTicksCounter == 1)
            {
                Find.WeatherManager.TransitionTo(Util_MechanoidTerraformer.TerraformingThunderstormDef);
            }
            else if ((this.channelingTicksCounter >= channelingWithoutStrikesTicks)
                && (this.channelingTicksCounter < channelingWithoutStrikesTicks + channelingWithStrikesTicks))
            {
                if (Rand.Value < chanceToStrikePerTick)
                {
                    IntVec3 strikePosition = new IntVec3();
                    if (Rand.Value < 0.5f)
                    {
                        // Strike on a nearby pawn.
                        List<Pawn> nearbyPawns = new List<Pawn>();
                        foreach (Pawn pawn in Find.ListerPawns.AllPawns)
                        {
                            if (pawn.Position.InHorDistOf(base.forcedTarget.Cell, this.areaOfEffectRadius))
                            {
                                nearbyPawns.Add(pawn);
                            }
                        }
                        if (nearbyPawns.Count > 0)
                        {
                            strikePosition = nearbyPawns.RandomElement<Pawn>().Position;
                            WeatherEvent_LightningStrike lightningStrike = new WeatherEvent_LightningStrike(strikePosition);
                            Find.WeatherManager.eventHandler.AddEvent(lightningStrike);
                        }
                    }
                    else
                    {
                        Predicate<IntVec3> validator = (IntVec3 cell) =>
                            cell.Walkable()
                            && (Find.RoofGrid.Roofed(cell) == false)
                            && cell.InHorDistOf(base.forcedTarget.Cell, this.def.building.turretGunDef.Verbs[0].projectileDef.projectile.explosionRadius);
                        strikePosition = CellFinderLoose.RandomCellWith(validator);
                        WeatherEvent_LightningStrike lightningStrike = new WeatherEvent_LightningStrike(strikePosition);
                        Find.WeatherManager.eventHandler.AddEvent(lightningStrike);
                    }
                }
            }
            else if (this.channelingTicksCounter == channelingWithoutStrikesTicks + channelingWithStrikesTicks)
            {
                Find.WeatherManager.TransitionTo(WeatherDef.Named("Clear"));
            }
            else if (this.channelingTicksCounter == 2 * channelingWithoutStrikesTicks + channelingWithStrikesTicks)
            {
                this.charge = 0;
                this.channelingTicksCounter = 0;
                this.skywrathState = SkywrathState.Charging;
            }
        }

        // ===================== Interraction functions =====================

        public override void OrderAttack(TargetInfo targ)
        {
            if (this.skywrathState != SkywrathState.Ready)
            {
                Messages.Message("The skywrath controller is not ready.", MessageSound.RejectInput);
                return;
            }
            if (Find.FogGrid.IsFogged(targ.Cell))
            {
                Messages.Message("Cannot target a fogged position.", MessageSound.RejectInput);
                return;
            }
            if (this.mannableComp.MannedNow)
            {
                Pawn supervizor = this.mannableComp.ManningPawn;
                if (supervizor.skills.GetSkill(SkillDefOf.Research).TotallyDisabled
                    || (supervizor.skills.GetSkill(SkillDefOf.Research).level < 10))
                {
                    Messages.Message("The skywrath controller is not supervized by a skilled scientist (level 10 research).", MessageSound.RejectInput);
                    return;
                }
            }
            else
            {
                Messages.Message("The skywrath controller is not supervized.", MessageSound.RejectInput);
                return;
            }
            if ((targ.Cell - this.Position).LengthHorizontal < this.GunCompEq.PrimaryVerb.verbProps.minRange)
            {
                Messages.Message("MessageTargetBelowMinimumRange".Translate(), MessageSound.Silent);
                return;
            }
            if ((targ.Cell - base.Position).LengthHorizontal > this.GunCompEq.PrimaryVerb.verbProps.range)
            {
                Messages.Message("MessageTargetBeyondMaximumRange".Translate(), MessageSound.Silent);
                return;
            }
            base.forcedTarget = targ;
            this.skywrathState = SkywrathState.Channeling;
        }
        
        // ===================== Inspection functions =====================

        public override string GetInspectString()
        {
            float powerNeeded = -powerComp.powerOutputInt;
            float powerProduction = 0f;
            float powerStored = 0f;
            StringBuilder stringBuilder = new StringBuilder();

            if (powerComp.PowerNet != null)
            {
                powerProduction = powerComp.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
                powerStored = powerComp.PowerNet.CurrentStoredEnergy();
            }
            stringBuilder.AppendLine("Power needed: " + powerNeeded + " W");
            stringBuilder.AppendLine("Connected rate/stored: " + powerProduction.ToString("F0") + " W / " + powerStored.ToString("F0") + " W");
            stringBuilder.AppendLine("State: " + GetStateAsString(this.skywrathState));
            stringBuilder.AppendLine("Charge progress: " + (this.charge / chargeTargetValue * 100f).ToString("F0") + " %");
            if (this.mannableComp.MannedNow)
            {
                Pawn supervizor = this.mannableComp.ManningPawn;
                if (supervizor.skills.GetSkill(SkillDefOf.Research).TotallyDisabled
                    || (supervizor.skills.GetSkill(SkillDefOf.Research).level < 10))
                {
                    stringBuilder.AppendLine("Supervizor: (need a skilled scientist, min level 10 research)");
                }
                else
                {
                    stringBuilder.AppendLine("Supervizor: " + supervizor.Name.ToStringShort);
                }
            }
            else
            {
                stringBuilder.AppendLine("Supervizor: none");
            }
            return stringBuilder.ToString();
        }

        public string GetStateAsString(SkywrathState state)
        {
            string stateAsString = "";

            switch (state)
            {
                case SkywrathState.Offline:
                    stateAsString = "offline";
                    break;
                case SkywrathState.Charging:
                    stateAsString = "charging";
                    break;
                case SkywrathState.Ready:
                    stateAsString = "ready";
                    break;
                case SkywrathState.Channeling:
                    stateAsString = "channeling";
                    break;
            }
            return stateAsString;
        }
    }
}
