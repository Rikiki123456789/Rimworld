using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;      // Needed when you do something with the AI
//using RimWorld.SquadAI;
//using Verse.Sound; // Needed when you do something with the Sound

namespace LaserFence
{
    /// <summary>
    /// Building_LaserFencePylon class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    [StaticConstructorOnStartup]
    public class Building_LaserFencePylon : Building
    {
        public const int pylonMaxDistance = 5;
        public const int updatePeriodInTicks = 30;
        public int nextUpdateTick = 0;

        // Pylon state.
        public bool[] connectionIsAllowedByUser = new bool[4] { true, true, true, true };
        public bool[] cachedConnectionIsAllowedByUser = new bool[4] { true, true, true, true };
        public Building_LaserFencePylon[] linkedPylons = new Building_LaserFencePylon[4] { null, null, null, null };
        public int[] fenceLength = new int[4] { 0, 0, 0, 0 };

        public bool manualSwitchIsPending
        {
            get
            {
                for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
                {
                    if (this.connectionIsAllowedByUser[directionAsInt] != this.cachedConnectionIsAllowedByUser[directionAsInt])
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        // Power component.
        public CompPowerTrader powerComp = null;
        
        // Gizmo textures.
        public static Texture2D northFenceActive = ContentFinder<Texture2D>.Get("Ui/Commands/NorthFenceActive");
        public static Texture2D northFenceInactive = ContentFinder<Texture2D>.Get("Ui/Commands/NorthFenceInactive");
        public static Texture2D eastFenceActive = ContentFinder<Texture2D>.Get("Ui/Commands/EastFenceActive");
        public static Texture2D eastFenceInactive = ContentFinder<Texture2D>.Get("Ui/Commands/EastFenceInactive");
        public static Texture2D southFenceActive = ContentFinder<Texture2D>.Get("Ui/Commands/SouthFenceActive");
        public static Texture2D southFenceInactive = ContentFinder<Texture2D>.Get("Ui/Commands/SouthFenceInactive");
        public static Texture2D westFenceActive = ContentFinder<Texture2D>.Get("Ui/Commands/WestFenceActive");
        public static Texture2D westFenceInactive = ContentFinder<Texture2D>.Get("Ui/Commands/WestFenceInactive");

        // ===================== Setup work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.powerComp = this.TryGetComp<CompPowerTrader>();
            this.nextUpdateTick = Find.TickManager.TicksGame + Rand.Range(0, updatePeriodInTicks);
        }

        public override void DeSpawn()
        {
            DeactivateAllFences();
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                this.connectionIsAllowedByUser[directionAsInt] = true;
                this.cachedConnectionIsAllowedByUser[directionAsInt] = true;
            }
            base.DeSpawn();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                if (this.linkedPylons[directionAsInt] != null)
                {
                    this.DeactivateFence(new Rot4(directionAsInt));
                }
            }
            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                Scribe_Values.Look<bool>(ref connectionIsAllowedByUser[directionAsInt], "connectionIsAllowedByUser" + directionAsInt, true);
                Scribe_Values.Look<bool>(ref cachedConnectionIsAllowedByUser[directionAsInt], "cachedConnectionIsAllowedByUser" + directionAsInt, true);
                Scribe_References.Look<Building_LaserFencePylon>(ref linkedPylons[directionAsInt], "linkedPylon" + directionAsInt);
                Scribe_Values.Look<int>(ref fenceLength[directionAsInt], "fenceLength" + directionAsInt, 0);
            }
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();

            if (this.Spawned == false)
            {
                return;
            }
            if (this.powerComp.PowerOn == false)
            {
                this.DeactivateAllFences();
            }
            else
            {
                if (Find.TickManager.TicksGame >= this.nextUpdateTick)
                {
                    this.nextUpdateTick = Find.TickManager.TicksGame + updatePeriodInTicks;
                    this.TryActivateInactiveFences();
                }
            }
        }

        public void DeactivateAllFences()
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                if (this.linkedPylons[directionAsInt] != null)
                {
                    this.DeactivateFence(new Rot4(directionAsInt));
                }
            }
        }

        public void DeactivateFence(Rot4 direction)
        {
            if (this.linkedPylons[direction.AsInt] != null)
            {
                this.RemoveFenceElements(direction);
                Rot4 linkedPylonDirection = direction;
                linkedPylonDirection.Rotate(RotationDirection.Clockwise); // Rotate 2 times to get the opposite direction.
                linkedPylonDirection.Rotate(RotationDirection.Clockwise);
                this.linkedPylons[direction.AsInt].RemoveFenceElements(linkedPylonDirection);
                this.linkedPylons[direction.AsInt].linkedPylons[linkedPylonDirection.AsInt] = null;
                this.linkedPylons[direction.AsInt] = null;
            }
        }

        public void RemoveFenceElements(Rot4 direction)
        {
            // Remove north or east laser fences.
            if ((direction == Rot4.North)
                || (direction == Rot4.East))
            {
                for (int offset = 1; offset <= this.fenceLength[direction.AsInt]; offset++)
                {
                    IntVec3 checkedPosition = this.Position + new IntVec3(0, 0, offset).RotatedBy(direction);
                    foreach (Thing thing in checkedPosition.GetThingList(this.Map))
                    {
                        if ((thing.def == Util_LaserFence.LaserFenceDef)
                            && (thing.Rotation == direction))
                        {
                            thing.Destroy();
                            break;
                        }
                    }
                }
            }
            this.fenceLength[direction.AsInt] = 0;
        }

        public void TryActivateInactiveFences()
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                if ((this.linkedPylons[directionAsInt] == null)
                    && this.connectionIsAllowedByUser[directionAsInt])
                {
                    this.LookForPylon(new Rot4(directionAsInt));
                }
            }
        }

        public void LookForPylon(Rot4 direction, bool forceConnection = false)
        {
            for (int offset = 1; offset <= pylonMaxDistance; offset++)
            {
                IntVec3 checkedPosition = this.Position + new IntVec3(0, 0, offset).RotatedBy(direction);
                foreach (Thing thing in checkedPosition.GetThingList(this.Map))
                {
                    if (thing is Building_LaserFencePylon)
                    {
                        this.TryConnectToPylon(thing as Building_LaserFencePylon, direction, offset - 1, forceConnection);
                        return;
                    }
                    if (thing is Pawn)
                    {
                        // Avoid connecting when an ally is in the path.
                        Pawn pawn = thing as Pawn;
                        if ((this.Faction != null)
                            && (pawn.HostileTo(this.Faction) == false))
                        {
                            return;
                        }
                    }
                }
                if (checkedPosition.GetEdifice(this.Map) != null)
                {
                    return;
                }
            }
        }

        public void TryConnectToPylon(Building_LaserFencePylon linkedPylon, Rot4 direction, int fenceLength, bool forceConnection)
        {
            Rot4 linkedPylonDirection = direction;
            linkedPylonDirection.Rotate(RotationDirection.Clockwise); // Rotate 2 times to get the opposite direction.
            linkedPylonDirection.Rotate(RotationDirection.Clockwise);
            // Check connection is allowed.
            if (forceConnection)
            {
                linkedPylon.connectionIsAllowedByUser[linkedPylonDirection.AsInt] = true;
                linkedPylon.cachedConnectionIsAllowedByUser[linkedPylonDirection.AsInt] = true;
            }
            if (linkedPylon.connectionIsAllowedByUser[linkedPylonDirection.AsInt] == false)
            {
                this.connectionIsAllowedByUser[direction.AsInt] = false;
                this.cachedConnectionIsAllowedByUser[direction.AsInt] = false;
                return;
            }
            // Check linkedPylon is powered.
            CompPowerTrader linkedPowerComp = linkedPylon.TryGetComp<CompPowerTrader>();
            if ((linkedPowerComp != null)
                && linkedPowerComp.PowerOn)
            {
                if (linkedPylon.linkedPylons[linkedPylonDirection.AsInt] != null)
                {
                    // If linkedPylon is already connected to a third pylon, first disconnect from it.
                    linkedPylon.DeactivateFence(linkedPylonDirection);
                }
                this.linkedPylons[direction.AsInt] = linkedPylon;
                this.ActivateFence(direction, fenceLength);
                linkedPylon.linkedPylons[linkedPylonDirection.AsInt] = this;
                linkedPylon.ActivateFence(linkedPylonDirection, fenceLength);
            }
        }

        public void ActivateFence(Rot4 direction, int fenceLength)
        {
            this.fenceLength[direction.AsInt] = fenceLength;
            if ((direction == Rot4.North)
                || (direction == Rot4.East))
            {
                // Spawn laser fences.
                for (int offset = 1; offset <= this.fenceLength[direction.AsInt]; offset++)
                {
                    IntVec3 fencePosition = this.Position + new IntVec3(0, 0, offset).RotatedBy(direction);
                    Building_LaserFence laserFence = ThingMaker.MakeThing(Util_LaserFence.LaserFenceDef) as Building_LaserFence;
                    laserFence.pylon = this;
                    GenSpawn.Spawn(laserFence, fencePosition, this.Map, direction);
                }
            }
        }

        public void Notify_EdificeIsBlocking()
        {
            this.DeactivateAllFences();
            this.TryActivateInactiveFences();
        }

        public void ToggleNorthFenceStatus()
        {
            int directionAsInt = Rot4.North.AsInt;
            this.cachedConnectionIsAllowedByUser[directionAsInt] = !this.cachedConnectionIsAllowedByUser[directionAsInt];
        }

        public void ToggleEastFenceStatus()
        {
            int directionAsInt = Rot4.East.AsInt;
            this.cachedConnectionIsAllowedByUser[directionAsInt] = !this.cachedConnectionIsAllowedByUser[directionAsInt];
        }

        public void ToggleSouthFenceStatus()
        {
            int directionAsInt = Rot4.South.AsInt;
            this.cachedConnectionIsAllowedByUser[directionAsInt] = !this.cachedConnectionIsAllowedByUser[directionAsInt];
        }

        public void ToggleWestFenceStatus()
        {
            int directionAsInt = Rot4.West.AsInt;
            this.cachedConnectionIsAllowedByUser[directionAsInt] = !this.cachedConnectionIsAllowedByUser[directionAsInt];
        }

        // Called when a pawn go to a pylon to take into account the player cached configuration.
        public void SwitchLaserFence()
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                Rot4 direction = new Rot4(directionAsInt);
                this.connectionIsAllowedByUser[directionAsInt] = this.cachedConnectionIsAllowedByUser[directionAsInt];
                if (this.connectionIsAllowedByUser[directionAsInt])
                {
                    this.LookForPylon(direction, true);
                }
                else
                {
                    if (this.linkedPylons[directionAsInt] != null)
                    {
                        Rot4 linkedPylonDirection = direction;
                        linkedPylonDirection.Rotate(RotationDirection.Clockwise); // Rotate 2 times to get the opposite direction.
                        linkedPylonDirection.Rotate(RotationDirection.Clockwise);
                        this.linkedPylons[directionAsInt].connectionIsAllowedByUser[linkedPylonDirection.AsInt] = false;
                        this.linkedPylons[directionAsInt].cachedConnectionIsAllowedByUser[linkedPylonDirection.AsInt] = false;
                        this.DeactivateFence(direction);
                    }
                }
            }
        }

        // ===================== Gizmos =====================
        public override IEnumerable<Gizmo> GetGizmos()
        {
            int groupKeyBase = 700000100;
            Rot4 direction = Rot4.North;

            List<Gizmo> gizmoList = new List<Gizmo>();
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                gizmoList.Add(gizmo);
            }
            if (this.Faction != Faction.OfPlayer)
            {
                return gizmoList;
            }

            Command_Action northFenceGizmo = new Command_Action();
            direction = Rot4.North;
            if (this.cachedConnectionIsAllowedByUser[direction.AsInt])
            {
                northFenceGizmo.icon = northFenceActive;
                northFenceGizmo.defaultDesc = "Deactivate north fence.";
            }
            else
            {
                northFenceGizmo.icon = northFenceInactive;
                northFenceGizmo.defaultDesc = "Activate north fence.";
            }
            northFenceGizmo.defaultLabel = "North fence: " + GetFenceStatusAsString(direction.AsInt) + ".";
            northFenceGizmo.activateSound = SoundDef.Named("Click");
            northFenceGizmo.action = new Action(ToggleNorthFenceStatus);
            northFenceGizmo.groupKey = groupKeyBase + 1;
            gizmoList.Add(northFenceGizmo);

            Command_Action eastFenceGizmo = new Command_Action();
            direction = Rot4.East;
            if (this.cachedConnectionIsAllowedByUser[direction.AsInt])
            {
                eastFenceGizmo.icon = eastFenceActive;
                eastFenceGizmo.defaultDesc = "Deactivate east fence.";
            }
            else
            {
                eastFenceGizmo.icon = eastFenceInactive;
                eastFenceGizmo.defaultDesc = "Activate east fence.";
            }
            eastFenceGizmo.defaultLabel = "East fence: " + GetFenceStatusAsString(direction.AsInt) + ".";
            eastFenceGizmo.activateSound = SoundDef.Named("Click");
            eastFenceGizmo.action = new Action(ToggleEastFenceStatus);
            eastFenceGizmo.groupKey = groupKeyBase + 2;
            gizmoList.Add(eastFenceGizmo);

            Command_Action southFenceGizmo = new Command_Action();
            direction = Rot4.South;
            if (this.cachedConnectionIsAllowedByUser[direction.AsInt])
            {
                southFenceGizmo.icon = southFenceActive;
                southFenceGizmo.defaultDesc = "Deactivate south fence.";
            }
            else
            {
                southFenceGizmo.icon = southFenceInactive;
                southFenceGizmo.defaultDesc = "Activate south fence.";
            }
            southFenceGizmo.defaultLabel = "South fence: " + GetFenceStatusAsString(direction.AsInt) + ".";
            southFenceGizmo.activateSound = SoundDef.Named("Click");
            southFenceGizmo.action = new Action(ToggleSouthFenceStatus);
            southFenceGizmo.groupKey = groupKeyBase + 3;
            gizmoList.Add(southFenceGizmo);

            Command_Action westFenceGizmo = new Command_Action();
            direction = Rot4.West;
            if (this.cachedConnectionIsAllowedByUser[direction.AsInt])
            {
                westFenceGizmo.icon = westFenceActive;
                westFenceGizmo.defaultDesc = "Deactivate west fence.";
            }
            else
            {
                westFenceGizmo.icon = westFenceInactive;
                westFenceGizmo.defaultDesc = "Activate west fence.";
            }
            westFenceGizmo.defaultLabel = "West fence: " + GetFenceStatusAsString(direction.AsInt) + ".";
            westFenceGizmo.activateSound = SoundDef.Named("Click");
            westFenceGizmo.action = new Action(ToggleWestFenceStatus);
            westFenceGizmo.groupKey = groupKeyBase + 4;
            gizmoList.Add(westFenceGizmo);

            return gizmoList;
        }

        public string GetFenceStatusAsString(int directionAsInt)
        {
            if (this.cachedConnectionIsAllowedByUser[directionAsInt])
            {
                return "activated";
            }
            else
            {
                return "deactivated";
            }
        }

        // ===================== Draw =====================
        public static void DrawPotentialBuildCells(Map map, IntVec3 pylonPosition)
        {
            List<IntVec3> potentialBuildCells = new List<IntVec3>();
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                for (int offset = 1; offset <= 5; offset++)
                {
                    potentialBuildCells.Add(pylonPosition + new IntVec3(offset, 0, 0).RotatedBy(new Rot4(directionAsInt)));
                }
                if (potentialBuildCells.NullOrEmpty() == false)
                {
                    GenDraw.DrawFieldEdges(potentialBuildCells);
                }
            }
        }
    }
}
