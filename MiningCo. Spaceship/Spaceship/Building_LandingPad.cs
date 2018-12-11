using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{
    public class Building_LandingPad : Building
    {
        public bool isPrimary = true;
        public bool isReserved = false;

        // Blocked by things check (trees, building, ...).
        public const int blockCheckPerioInTicks = GenTicks.TicksPerRealSecond;
        public int nextBlockCheckTick = 0;
        public string blockingReasons = "";
        
        public bool isFree
        {
            get
            {
                return ((this.isReserved == false)
                    && (this.blockingReasons.Length == 0));
            }
        }
        public bool isFreeAndPowered
        {
            get
            {
                return (this.isFree
                    && this.powerComp.PowerOn);
            }
        }

        // Power comp.
        public CompPowerTrader powerComp = null;
        
        // Beacon lights parameters.
        public const int lightPeriodInTicks = 16 * GenTicks.TicksPerRealSecond;
        public const int lightInternalCrossDelayInTicks = GenTicks.TicksPerRealSecond / 2;
        public const int lightInternalCrossDurationInTicks = lightInternalCrossDelayInTicks;
        public const int lightExternalFrameDelayInTicks = GenTicks.TicksPerRealSecond / 4;
        public const int lightExternalFrameDurationInTicks = 7 * lightExternalFrameDelayInTicks;

        // ===================== Setup work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.powerComp.powerStartedAction = Notify_PowerStarted;
            this.powerComp.powerStoppedAction = Notify_PowerStopped;

            // Check if there is already another primary landing pad.
            if (this.Map.listerBuildings.ColonistsHaveBuilding(Util_ThingDefOf.LandingPad))
            {
                foreach (Building building in this.Map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPad))
                {
                    Building_LandingPad landingPad = building as Building_LandingPad;
                    if ((landingPad != this)
                        && landingPad.isPrimary)
                    {
                        this.isPrimary = false;
                        break;
                    }
                }
            }

            if (respawningAfterLoad == false)
            {
                // Only spawn beacons once.
                SpawnBeacons();
            }
        }

        public void SpawnBeacons()
        {
            // Internal cross: external green beacons.
            Building_LandingPadBeacon beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(0, 0, 3).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, 0);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(3, 0, 0).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, 0);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(0, 0, -3).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, 0);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(-3, 0, 0).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, 0);
            // Internal cross: middle white beacons.
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(0, 0, 2).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, lightInternalCrossDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(2, 0, 0).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, lightInternalCrossDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(0, 0, -2).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, lightInternalCrossDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(-2, 0, 0).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, lightInternalCrossDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(0, 0, 1).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, 2 * lightInternalCrossDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(1, 0, 0).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, 2 * lightInternalCrossDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(0, 0, -1).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, 2 * lightInternalCrossDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(-1, 0, 0).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.white, lightPeriodInTicks, lightInternalCrossDurationInTicks, 2 * lightInternalCrossDelayInTicks);
            // Internal cross: central green beacon.
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position, this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.green, lightPeriodInTicks, 2 * lightInternalCrossDurationInTicks, 3 * lightInternalCrossDelayInTicks);

            // Landing pad external frame: red beacons.
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(-2, 0, -8).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(2, 0, -8).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(-4, 0, -6).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(4, 0, -6).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(-4, 0, -3).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + 2 * lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(4, 0, -3).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + 2 * lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(-4, 0, 0).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + 3 * lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(4, 0, 0).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + 3 * lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(-4, 0, 3).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + 4 * lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(4, 0, 3).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + 4 * lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(-4, 0, 6).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + 5 * lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(4, 0, 6).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + 5 * lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(-1, 0, 9).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + 6 * lightExternalFrameDelayInTicks);
            beacon = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, this.Position + new IntVec3(1, 0, 9).RotatedBy(this.Rotation), this.Map) as Building_LandingPadBeacon;
            beacon.InitializeParameters(this, Color.red, lightPeriodInTicks, lightExternalFrameDurationInTicks, lightPeriodInTicks / 2 + 6 * lightExternalFrameDelayInTicks);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Map map = this.Map;

            // Destroy all landing pad beacons.
            foreach (IntVec3 cell in this.OccupiedRect().Cells)
            {
                Thing thing = cell.GetFirstThing(this.Map, Util_ThingDefOf.LandingPadBeacon);
                if (thing != null)
                {
                    thing.Destroy();
                }
            }
            base.Destroy(mode);
            Util_OrbitalRelay.TryUpdateLandingPadAvailability(map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.nextBlockCheckTick, "nextBlockCheckTick");
            Scribe_Values.Look<bool>(ref this.isPrimary, "isPrimary");
            Scribe_Values.Look<bool>(ref this.isReserved, "isReserved");
            Scribe_Values.Look<string>(ref this.blockingReasons, "blockingReasons");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();
            // Update blocking things.
            if (Find.TickManager.TicksGame >= this.nextBlockCheckTick)
            {
                this.nextBlockCheckTick = Find.TickManager.TicksGame + blockCheckPerioInTicks;
                CheckForBlockingThing();
            }
        }

        public void CheckForBlockingThing()
        {
            bool isUnroofed = true;

            this.blockingReasons = "";
            StringBuilder reasonsList = new StringBuilder();
            foreach (IntVec3 cell in this.OccupiedRect().Cells)
            {
                if (isUnroofed
                    && cell.Roofed(this.Map))
                {
                    isUnroofed = false; // To only append roof reason once.
                    reasonsList.AppendWithComma("roof");
                }
                Building building = cell.GetEdifice(this.Map);
                if ((building != null)
                    && (building.def != Util_ThingDefOf.VulcanTurret))
                {
                    reasonsList.AppendWithComma(building.Label);
                }
                Plant plant = cell.GetPlant(this.Map);
                if ((plant != null)
                    && plant.def.plant.IsTree)
                {
                    reasonsList.AppendWithComma(plant.Label);
                }
            }
            if (reasonsList.Length > 0)
            {
                this.blockingReasons = reasonsList.ToString();
            }
        }

        // ===================== Other functions =====================
        /// <summary>
        /// Notify beacons that powered is started.
        /// </summary>
        public void Notify_PowerStarted()
        {
            Util_OrbitalRelay.TryUpdateLandingPadAvailability(this.Map);
            foreach (Building building in this.Map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPadBeacon))
            {
                Building_LandingPadBeacon beacon = building as Building_LandingPadBeacon;
                if (beacon.landingPad == this)
                {
                    beacon.Notify_PowerStarted();
                }
            }
        }

        /// <summary>
        /// Notify beacons that powered is stopped.
        /// </summary>
        public void Notify_PowerStopped()
        {
            Util_OrbitalRelay.TryUpdateLandingPadAvailability(this.Map);
            foreach (Building building in this.Map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPadBeacon))
            {
                Building_LandingPadBeacon beacon = building as Building_LandingPadBeacon;
                if (beacon.landingPad == this)
                {
                    beacon.Notify_PowerStopped();
                }
            }
        }

        public void Notify_ShipLanding()
        {
            this.isReserved = true;
            Util_OrbitalRelay.TryUpdateLandingPadAvailability(this.Map);
        }

        public void Notify_ShipTakingOff()
        {
            this.isReserved = false;
            Util_OrbitalRelay.TryUpdateLandingPadAvailability(this.Map);
        }
        
        // ===================== Inspect panel =====================
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (this.isPrimary)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Primary landing pad");
            }
            if (this.blockingReasons.Length > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("Blocked by " + this.blockingReasons);
            }
            return stringBuilder.ToString();
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000105;

            Command_Action setTargetButton = new Command_Action();
            if (this.isPrimary)
            {
                setTargetButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/Commands_Primary");
                setTargetButton.defaultLabel = "Primary";
                setTargetButton.defaultDesc = "Spaceships will land there in priority if landing pad is clear. Otherwise, they will choose another.";
            }
            else
            {
                setTargetButton.icon = ContentFinder<Texture2D>.Get("Ui/Commands/Commands_Ancillary");
                setTargetButton.defaultLabel = "Ancillary";
                setTargetButton.defaultDesc = "Spaceships may only land there if primary landing pad is busy. Click to set it as primary.";
            }
            setTargetButton.activateSound = SoundDef.Named("Click");
            setTargetButton.action = new Action(SetAsPrimary);
            setTargetButton.groupKey = groupKeyBase + 1;
            buttonList.Add(setTargetButton);

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

        /// <summary>
        /// Set selected landing pad as primary.
        /// </summary>
        public void SetAsPrimary()
        {
            foreach (Building landingPad in this.Map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPad))
            {
                (landingPad as Building_LandingPad).isPrimary = false;
            }
            this.isPrimary = true;
        }
    }
}
