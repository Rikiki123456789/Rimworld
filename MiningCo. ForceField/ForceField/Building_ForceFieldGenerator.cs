using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace ForceField
{
    /// <summary>
    /// Building_ForceField class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    [StaticConstructorOnStartup]
    public class Building_ForceFieldGenerator : Building
    {
        public struct ProjectileWithAngle
        {
            public Projectile projectile;
            public float incidentAngle;
        }

        public enum ForceFieldState
        {
            Offline,
            Initializing,
            Charging,
            Sustaining,
            Discharging
        }
        
        public ForceFieldState forceFieldState = ForceFieldState.Offline;
        public float forceFieldCharge = 0;
        public int initializationElapsedTimeInTicks = 0;
        public ThingDef_FieldGenerator properties = null;

        // Force field covered cells.
        public List<IntVec3> coveredCells = new List<IntVec3>();
        public List<Vector3> effectCells = new List<Vector3>();

        // Components references.
        public CompPowerTrader powerComp = null;
        
        // Drawing.
        public static readonly Material[] forceFieldTexture = new Material[5]
        {
            MaterialPool.MatFrom("Effects/ForceField1", ShaderDatabase.Transparent),
            MaterialPool.MatFrom("Effects/ForceField2", ShaderDatabase.Transparent),
            MaterialPool.MatFrom("Effects/ForceField3", ShaderDatabase.Transparent),
            MaterialPool.MatFrom("Effects/ForceField4", ShaderDatabase.Transparent),
            MaterialPool.MatFrom("Effects/ForceField5", ShaderDatabase.Transparent)
        };
        public Matrix4x4 forceFieldMatrix = default(Matrix4x4);
        public Vector3 forceFieldScale = new Vector3(5f, 1f, 2f);
        public static readonly Material[] forceFieldAbsorbtionTexture = new Material[5]
        {
            MaterialPool.MatFrom("Effects/ForceFieldAbsorbtion1", ShaderDatabase.Transparent),
            MaterialPool.MatFrom("Effects/ForceFieldAbsorbtion2", ShaderDatabase.Transparent),
            MaterialPool.MatFrom("Effects/ForceFieldAbsorbtion3", ShaderDatabase.Transparent),
            MaterialPool.MatFrom("Effects/ForceFieldAbsorbtion4", ShaderDatabase.Transparent),
            MaterialPool.MatFrom("Effects/ForceFieldAbsorbtion5", ShaderDatabase.Transparent)
        };
        public Matrix4x4 forceFieldAbsorbtionMatrix = default(Matrix4x4);
        
        public const int drawingPeriodInTicks = 120;
        public int drawingCounterInTicks = 0;
        public int nextSparkTick = 0;

        public float[] matrixFadingCoefficient = new float[5];
        public bool[] matrixIsStartingAbsorbion = new bool[5];
        public const int matrixAbsorbtionDurationInTicks = 20;
        public int[] matrixAbsorbtionCounterInTicks = new int[5];
        public float[] matrixAbsorbtionFadingCoefficient = new float[5];

        public static readonly Vector2 barSize = new Vector2(0.4f, 0.1f);
        public static readonly Material barFilledColor = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0.8f, 1f));
        public static readonly Material barUnfilledColor = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));
        
        public Vector3 generatorForwardVector
        {
            get
            {
                Vector3 vector;
                if (this.Rotation == Rot4.North)
                {
                    vector = new Vector3(0f, 0f, 1f);
                }
                else if (this.Rotation == Rot4.East)
                {
                    vector = new Vector3(1f, 0f, 0f);
                }
                else if (this.Rotation == Rot4.South)
                {
                    vector = new Vector3(0f, 0f, -1f);
                }
                else // West.
                {
                    vector = new Vector3(-1f, 0f, 0f);
                }
                return vector;
            }
        }

        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        /// 
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.properties = (ThingDef_FieldGenerator)this.def;

            // Force field covered cells initialization.
            this.coveredCells = Building_ForceFieldGenerator.GetCoveredCells(this.Position, this.Rotation);

            // Force field effect positions.
            Vector3 effectCell = new Vector3();
            effectCell = this.Position.ToVector3Shifted() + new Vector3(-2.1f, 0f, 0.1f).RotatedBy(this.Rotation.AsAngle);
            effectCells.Add(effectCell);
            effectCell = this.Position.ToVector3Shifted() + new Vector3(-1.2f, 0f, 0.8f).RotatedBy(this.Rotation.AsAngle);
            effectCells.Add(effectCell);
            effectCell = this.Position.ToVector3Shifted() + new Vector3(0f, 0f, 1f).RotatedBy(this.Rotation.AsAngle);
            effectCells.Add(effectCell);
            effectCell = this.Position.ToVector3Shifted() + new Vector3(1.2f, 0f, 0.8f).RotatedBy(this.Rotation.AsAngle);
            effectCells.Add(effectCell);
            effectCell = this.Position.ToVector3Shifted() + new Vector3(2.1f, 0f, 0.1f).RotatedBy(this.Rotation.AsAngle);
            effectCells.Add(effectCell);

            // Components initialization.
            powerComp = base.GetComp<CompPowerTrader>();
            
            // Textures initialization.
            forceFieldMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect + new Vector3(0f, 0f, 0.5f).RotatedBy(this.Rotation.AsAngle), this.Rotation.AsAngle.ToQuat(), forceFieldScale);
            forceFieldAbsorbtionMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect + new Vector3(0f, 0.1f, 0.5f).RotatedBy(this.Rotation.AsAngle), this.Rotation.AsAngle.ToQuat(), forceFieldScale);
        }

        /// <summary>
        /// Saves and loads internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<ForceFieldState>(ref forceFieldState, "forceFieldState", ForceFieldState.Offline);
            Scribe_Values.Look<float>(ref forceFieldCharge, "forceFieldCharge");
        }

        public static List<IntVec3> GetCoveredCells(IntVec3 origin, Rot4 rotation)
        {
            List<IntVec3> coveredCells = new List<IntVec3>();
            for (int xOffset = -2; xOffset <= 2; xOffset++)
            {
                for (int zOffset = 0; zOffset <= 1; zOffset++)
                {
                    IntVec3 coveredCell = origin + new IntVec3(xOffset, 0, zOffset).RotatedBy(rotation);
                    coveredCells.Add(coveredCell);
                }
            }
            return coveredCells;
        }

        // ===================== Main treatment =====================
        public override void Tick()
        {
            base.Tick();

            if (this.powerComp.PowerOn)
            {
                switch (this.forceFieldState)
                {
                    case ForceFieldState.Offline:
                        this.powerComp.powerOutputInt = -10;
                        if (((this.powerComp.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick) >= -this.properties.powerOutputDuringInitialization)
                            || (this.powerComp.PowerNet.CurrentStoredEnergy() > 0))
                        {
                            this.initializationElapsedTimeInTicks = 0;
                            this.forceFieldState = ForceFieldState.Initializing;
                        }
                        break;
                    case ForceFieldState.Initializing:
                        this.powerComp.powerOutputInt = this.properties.powerOutputDuringInitialization;
                        this.initializationElapsedTimeInTicks++;
                        if (this.initializationElapsedTimeInTicks >= this.properties.initializationDurationInTicks)
                        {
                            this.forceFieldState = ForceFieldState.Charging;
                            this.forceFieldCharge = (10f / 100) * this.properties.forceFieldMaxCharge;
                        }
                        break;
                    case ForceFieldState.Charging:
                        this.powerComp.powerOutputInt = this.properties.powerOutputDuringCharge;
                        this.forceFieldCharge += this.properties.forceFieldMaxCharge / (float)this.properties.chargeDurationInTicks;
                        if (this.forceFieldCharge >= this.properties.forceFieldMaxCharge)
                        {
                            this.forceFieldCharge = this.properties.forceFieldMaxCharge;
                            this.forceFieldState = ForceFieldState.Sustaining;
                        }
                        break;
                    case ForceFieldState.Sustaining:
                        this.powerComp.powerOutputInt = this.properties.powerOutputDuringSustain;
                        if (this.forceFieldCharge < this.properties.forceFieldMaxCharge)
                        {
                            this.forceFieldState = ForceFieldState.Charging;
                        }
                        break;
                    case ForceFieldState.Discharging:
                        this.powerComp.powerOutputInt = this.properties.powerOutputDuringDischarge;
                        if ((this.powerComp.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick) >= -this.properties.powerOutputDuringCharge)
                        {
                            this.forceFieldState = ForceFieldState.Charging;
                        }
                        else
                        {
                            this.forceFieldCharge -= this.properties.forceFieldMaxCharge / (float)this.properties.dischargeDurationInTicks;
                            if (this.forceFieldCharge <= 0)
                            {
                                this.forceFieldCharge = 0;
                                this.forceFieldState = ForceFieldState.Offline;
                            }
                        }
                        break;
                }
            }
            else // Power is off.
            {
                switch (this.forceFieldState)
                {
                    case ForceFieldState.Offline:
                        this.powerComp.powerOutputInt = -10;
                        break;
                    case ForceFieldState.Initializing:
                        this.forceFieldState = ForceFieldState.Offline;
                        break;
                    case ForceFieldState.Charging:
                    case ForceFieldState.Sustaining:
                        this.forceFieldState = ForceFieldState.Discharging;
                        break;
                    case ForceFieldState.Discharging:
                        this.powerComp.powerOutputInt = this.properties.powerOutputDuringDischarge;
                        this.forceFieldCharge -= this.properties.forceFieldMaxCharge / (float)this.properties.dischargeDurationInTicks;
                        if (this.forceFieldCharge <= 0)
                        {
                            this.forceFieldCharge = 0;
                            this.forceFieldState = ForceFieldState.Offline;
                        }
                        break;
                }
            }
            if (this.forceFieldCharge > 0)
            {
                TryAbsorbIncomingProjectiles();
            }

            ComputeDrawingParameters();
        }

        public void TryAbsorbIncomingProjectiles()
        {
            List<ProjectileWithAngle> incomingProjectiles = GetIncomingProjectiles();

            foreach (ProjectileWithAngle projectileWithAngle in incomingProjectiles)
            {
                if (projectileWithAngle.projectile.def.projectile.flyOverhead)
                {
                    continue;
                }
                if ((projectileWithAngle.projectile.def.defName == "Bullet_DoomsdayRocket")
                    || (projectileWithAngle.projectile.def.defName == "Bullet_Rocket"))
                {
                    TreatRocketProjectile(projectileWithAngle);
                }
                else if (projectileWithAngle.projectile is Projectile_Explosive)
                {
                    TreatExplosiveProjectile(projectileWithAngle);
                }
                else if ((this.forceFieldCharge > 0)
                    && (projectileWithAngle.projectile.def.defName.Contains("Seed") == false)
                    && (projectileWithAngle.projectile.def.defName != "Spark"))
                {
                    TreatStandardProjectile(projectileWithAngle);
                }
            }
        }

        public List<ProjectileWithAngle> GetIncomingProjectiles()
        {
            List<ProjectileWithAngle> incomingProjectiles = new List<ProjectileWithAngle>();
            foreach (IntVec3 cell in this.coveredCells)
            {
                List<Thing> thingsInCell = this.Map.thingGrid.ThingsListAt(cell);
                foreach (Thing thing in thingsInCell)
                {
                    if (thing is Projectile)
                    {
                        ProjectileWithAngle projectileWithAngle = new ProjectileWithAngle();
                        projectileWithAngle.projectile = thing as Projectile;
                        projectileWithAngle.incidentAngle = 0f;

                        if (IsProjectileIncoming(ref projectileWithAngle))
                        {
                            incomingProjectiles.Add(projectileWithAngle);
                        }
                    }
                }
            }
            return incomingProjectiles;
        }

        public bool IsProjectileIncoming(ref ProjectileWithAngle projectileWithAngle)
        {
            Vector3 generatorToProjectile = this.Position.ToVector3() - projectileWithAngle.projectile.ExactPosition;
            Quaternion forceFieldQuaternion = Quaternion.LookRotation(this.generatorForwardVector);
            projectileWithAngle.incidentAngle = Quaternion.Angle(projectileWithAngle.projectile.ExactRotation, forceFieldQuaternion);
            if (projectileWithAngle.incidentAngle > 90f)
            {
                return true;
            }
            return false;
        }

        public void TreatRocketProjectile(ProjectileWithAngle projectileWithAngle)
        {
            float rocketAbsorbtionCost = this.properties.forceFieldMaxCharge * this.properties.rocketAbsorbtionProportion;
            if (this.forceFieldCharge > rocketAbsorbtionCost)
            {
                this.forceFieldCharge -= rocketAbsorbtionCost;
                if (this.forceFieldCharge <= 0)
                {
                    this.forceFieldCharge = 0;
                    this.forceFieldState = ForceFieldState.Offline;
                }
                SoundInfo soundInfo = SoundInfo.InMap(new TargetInfo(projectileWithAngle.projectile.Position, this.Map), MaintenanceType.None);
                SoundDefOf.Thunder_OnMap.PlayOneShot(soundInfo);
                GenExplosion.DoExplosion(projectileWithAngle.projectile.Position, this.Map, 1.9f, DamageDefOf.Flame, projectileWithAngle.projectile);
                projectileWithAngle.projectile.Destroy();
                ActivateMatrixAbsorbtionEffect(projectileWithAngle.projectile.ExactPosition);
            }
            else
            {
                this.forceFieldCharge = 0;
                this.forceFieldState = ForceFieldState.Offline;
            }
        }

        public void TreatExplosiveProjectile(ProjectileWithAngle projectileWithAngle)
        {
            if (this.forceFieldCharge < this.properties.explosiveRepelCharge)
            {
                // Force field charge is too low to repel an explosive.
                return;
            }
            this.forceFieldCharge -= this.properties.explosiveRepelCharge;
            if (this.forceFieldCharge <= 0)
            {
                this.forceFieldCharge = 0;
                this.forceFieldState = ForceFieldState.Offline;
            }

            Projectile deflectedProjectile = ThingMaker.MakeThing(projectileWithAngle.projectile.def) as Projectile;
            GenSpawn.Spawn(deflectedProjectile, projectileWithAngle.projectile.Position, this.Map);

            float rebounceAngleInDegrees = (180f - projectileWithAngle.incidentAngle);
            float rebounceAngleInRadians = (float)(rebounceAngleInDegrees * Math.PI / 180f);
            float rebounceVectorMagnitude = Rand.Range(1.5f, 3.5f);
            Vector3 rebounceVector = new Vector3(0f, 0f, 0f);
            float xSign = +1f;
            float zSign = +1f;
            Thing projectileLauncher = ReflectionHelper.GetInstanceField(typeof(Projectile), projectileWithAngle.projectile, "launcher") as Thing;
            if (projectileLauncher == null)
            {
                Log.Warning("M&Co. ForceField mod: projectileLauncher is null!");
                return;
            }
            if (this.Rotation == Rot4.North)
            {
                if (projectileWithAngle.projectile.Position.x >= projectileLauncher.Position.x)
                {
                    xSign = +1f;
                }
                else
                {
                    xSign = -1f;
                }
                zSign = +1f;
            }
            else if (this.Rotation == Rot4.East)
            {
                if (projectileWithAngle.projectile.Position.z >= projectileLauncher.Position.z)
                {
                    zSign = +1f;
                }
                else
                {
                    zSign = -1f;
                }
                xSign = +1f;
            }
            else if (this.Rotation == Rot4.South)
            {
                if (projectileWithAngle.projectile.Position.x >= projectileLauncher.Position.x)
                {
                    xSign = +1f;
                }
                else
                {
                    xSign = -1f;
                }
                zSign = -1f;
            }
            else // West.
            {
                if (projectileWithAngle.projectile.Position.z >= projectileLauncher.Position.z)
                {
                    zSign = +1f;
                }
                else
                {
                    zSign = -1f;
                }
                xSign = -1f;
            }

            rebounceVector = new Vector3(xSign * rebounceVectorMagnitude * (float)Math.Sin(rebounceAngleInRadians), 0f, zSign * rebounceVectorMagnitude * (float)Math.Cos(rebounceAngleInRadians));
            LocalTargetInfo rebounceCell = new LocalTargetInfo((projectileWithAngle.projectile.ExactPosition + rebounceVector).ToIntVec3());
            deflectedProjectile.Launch(this, projectileWithAngle.projectile.ExactPosition, rebounceCell);
            projectileWithAngle.projectile.Destroy();
            ActivateMatrixAbsorbtionEffect(projectileWithAngle.projectile.ExactPosition);
        }

        public void TreatStandardProjectile(ProjectileWithAngle projectileWithAngle)
        {
            this.forceFieldCharge -= projectileWithAngle.projectile.def.projectile.damageAmountBase;
            if (this.forceFieldCharge <= 0)
            {
                this.forceFieldCharge = 0;
                this.forceFieldState = ForceFieldState.Offline;
            }
            projectileWithAngle.projectile.Destroy();
            ActivateMatrixAbsorbtionEffect(projectileWithAngle.projectile.ExactPosition);
        }


        public void ActivateMatrixAbsorbtionEffect(Vector3 absorbtionPosition)
        {
            if ((absorbtionPosition.ToIntVec3() == this.coveredCells[0])
                || (absorbtionPosition.ToIntVec3() == this.coveredCells[1]))
            {
                this.matrixIsStartingAbsorbion[0] = true;
            }
            else if ((absorbtionPosition.ToIntVec3() == this.coveredCells[2])
                    || (absorbtionPosition.ToIntVec3() == this.coveredCells[3]))
            {
                this.matrixIsStartingAbsorbion[1] = true;
            }
            else if ((absorbtionPosition.ToIntVec3() == this.coveredCells[4])
                    || (absorbtionPosition.ToIntVec3() == this.coveredCells[5]))
            {
                this.matrixIsStartingAbsorbion[2] = true;
            }
            else if ((absorbtionPosition.ToIntVec3() == this.coveredCells[6])
                    || (absorbtionPosition.ToIntVec3() == this.coveredCells[7]))
            {
                this.matrixIsStartingAbsorbion[3] = true;
            }
            else if ((absorbtionPosition.ToIntVec3() == this.coveredCells[8])
                    || (absorbtionPosition.ToIntVec3() == this.coveredCells[9]))
            {
                this.matrixIsStartingAbsorbion[4] = true;
            }
        }

        // ===================== Drawing functions =====================
        public static void DisplayCoveredCells(List<IntVec3> coverdCellsList)
        {
            GenDraw.DrawFieldEdges(coverdCellsList);
        }

        public void ComputeDrawingParameters()
        {
            const float fadingOffset = 0.3f;
            const float fadingVariable = 0.7f;

            if ((this.forceFieldState == ForceFieldState.Charging)
                || (this.forceFieldState == ForceFieldState.Sustaining)
                || (this.forceFieldState == ForceFieldState.Discharging))
            {
                // Standard matrix cyan effect.
                for (int matrixIndex = 0; matrixIndex < 5; matrixIndex++)
                {
                    this.matrixFadingCoefficient[matrixIndex] = fadingOffset + fadingVariable * (this.forceFieldCharge / this.properties.forceFieldMaxCharge);
                }

                // Additional lightning effect.
                if (Find.TickManager.TicksGame > this.nextSparkTick)
                {
                    this.nextSparkTick = Find.TickManager.TicksGame + Rand.RangeInclusive(150, 300);
                    int effectCellIndex = Rand.RangeInclusive(0, 4);
                    MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_ElectricalSpark"), null);
                    moteThrown.Scale = 0.8f;
                    moteThrown.exactRotation = Rand.Range(0f, 360f);
                    moteThrown.exactPosition = this.effectCells[effectCellIndex] + new Vector3(Rand.Range(-0.1f, 0.1f), 0f, Rand.Range(-0.1f, 0.1f));
                    GenSpawn.Spawn(moteThrown, moteThrown.exactPosition.ToIntVec3(), this.Map);
                }
            }

            for (int matrixIndex = 0; matrixIndex < 5; matrixIndex++)
            {
                // Absorbing matrix red effect.
                if (this.matrixIsStartingAbsorbion[matrixIndex])
                {
                    this.matrixAbsorbtionCounterInTicks[matrixIndex] = matrixAbsorbtionDurationInTicks;
                    this.matrixIsStartingAbsorbion[matrixIndex] = false;
                    MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_LightningGlow, null);
                    moteThrown.Scale = 4f;
                    moteThrown.exactRotation = Rand.Range(0f, 360f);
                    moteThrown.exactPosition = this.effectCells[matrixIndex] + new Vector3(Rand.Range(-0.2f, 0.2f), 0f, Rand.Range(-0.2f, 0.2f));
                    GenSpawn.Spawn(moteThrown, moteThrown.exactPosition.ToIntVec3(), this.Map);
                }
                if (this.matrixAbsorbtionCounterInTicks[matrixIndex] > 0)
                {
                    this.matrixAbsorbtionCounterInTicks[matrixIndex]--;
                    this.matrixAbsorbtionFadingCoefficient[matrixIndex] = 0.5f * ((float)this.matrixAbsorbtionCounterInTicks[matrixIndex] / (float)matrixAbsorbtionDurationInTicks);
                }
            }
        }

        public override void Draw()
        {
            base.Draw();

            if ((this.forceFieldState == ForceFieldState.Charging)
                || (this.forceFieldState == ForceFieldState.Sustaining)
                || (this.forceFieldState == ForceFieldState.Discharging))
            {
                for (int matrixIndex = 0; matrixIndex < 5; matrixIndex++)
                {
                    if (this.matrixAbsorbtionFadingCoefficient[matrixIndex] > 0f)
                    {
                        Graphics.DrawMesh(MeshPool.plane10, forceFieldMatrix, FadedMaterialPool.FadedVersionOf(forceFieldAbsorbtionTexture[matrixIndex], this.matrixAbsorbtionFadingCoefficient[matrixIndex]), 0);
                    }
                    else
                    {
                        Graphics.DrawMesh(MeshPool.plane10, forceFieldMatrix, FadedMaterialPool.FadedVersionOf(forceFieldTexture[matrixIndex], this.matrixFadingCoefficient[matrixIndex]), 0);
                    }

                }
            }

            GenDraw.FillableBarRequest chargeBar = default(GenDraw.FillableBarRequest);
            chargeBar.center = this.DrawPos + new Vector3(-0.3f, 0f, 0f).RotatedBy(this.Rotation.AsAngle) + Vector3.up * 0.1f;
            chargeBar.size = barSize;
            chargeBar.fillPercent = this.forceFieldCharge / this.properties.forceFieldMaxCharge;
            chargeBar.filledMat = barFilledColor;
            chargeBar.unfilledMat = barUnfilledColor;
            chargeBar.margin = 0.15f;
            Rot4 rotation = this.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            chargeBar.rotation = rotation;
            GenDraw.DrawFillableBar(chargeBar);

            if (Find.Selector.IsSelected(this))
            {
                GenDraw.DrawFieldEdges(this.coveredCells);
            }
        }
        
        // ===================== Inspection pannel functions =====================
        /// <summary>
        /// Get the string displayed in the inspection panel.
        /// </summary>
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.AppendLine();

            string stateAsString = GetStateAsString(this.forceFieldState);
            stringBuilder.AppendLine("Status: " + stateAsString);
            stringBuilder.Append("Force field charge: " + (int)this.forceFieldCharge + "/" + this.properties.forceFieldMaxCharge);

            return stringBuilder.ToString();
        }
        
        /// <summary>
        /// Gets the state as a string.
        /// </summary>
        public string GetStateAsString(ForceFieldState state)
        {
            string stateAsString = "";

            switch (state)
            {
                case ForceFieldState.Offline:
                    stateAsString = "offline";
                    break;
                case ForceFieldState.Initializing:
                    stateAsString = "initializing";
                    break;
                case ForceFieldState.Charging:
                    stateAsString = "charging";
                    break;
                case ForceFieldState.Sustaining:
                    stateAsString = "sustaining";
                    break;
                case ForceFieldState.Discharging:
                    stateAsString = "discharging";
                    break;
            }

            return (stateAsString);
        }
    }
}
