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
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    [StaticConstructorOnStartup]
    public class Building_LaserFencePylon : Building
    {
        public const int updatePeriodInTicks = 30;
        public int nextUpdateTick = 0;

        // Pylon state.
        public bool[] connectionIsAllowedByUser = new bool[4] { true, true, true, true };
        public bool[] cachedConnectionIsAllowedByUser = new bool[4] { true, true, true, true };
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

        // Draw.
        public static Material connectorNorth = MaterialPool.MatFrom("Things/Building/Security/LaserFencePylonConnectorNorth");
        public static Material connectorSouth = MaterialPool.MatFrom("Things/Building/Security/LaserFencePylonConnectorSouth");
        public static Material connectorEast = MaterialPool.MatFrom("Things/Building/Security/LaserFencePylonConnectorEast");
        public static Material connectorWest = MaterialPool.MatFrom("Things/Building/Security/LaserFencePylonConnectorWest");
        public Vector3 connectorScale = new Vector3(1f, 1f, 1f);
        public Matrix4x4 connectorMatrix = default(Matrix4x4);

        // ===================== Setup work =====================
        /// <summary>
        /// Used to initialize the pylon.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.powerComp = this.TryGetComp<CompPowerTrader>();
            connectorMatrix.SetTRS(base.DrawPos + Altitudes.AltIncVect, (0f).ToQuat(), connectorScale);
        }

        /// <summary>
        /// Called when pylon is minified for example.
        /// Deactivate all the fences and reset connection allowance to default.
        /// </summary>
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            DeactivateAllFences();
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                this.connectionIsAllowedByUser[directionAsInt] = true;
                this.cachedConnectionIsAllowedByUser[directionAsInt] = true;
            }
            base.DeSpawn();
        }

        /// <summary>
        /// Deactivate all the fences and destroy the pylon.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                if (this.fenceLength[directionAsInt] > 0)
                {
                    this.DeactivateFence(new Rot4(directionAsInt));
                }
            }
            base.Destroy(mode);
        }

        /// <summary>
        /// Save/load instance data values.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.nextUpdateTick, "nextUpdateTick");
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                Scribe_Values.Look<bool>(ref connectionIsAllowedByUser[directionAsInt], "connectionIsAllowedByUser" + directionAsInt, true);
                Scribe_Values.Look<bool>(ref cachedConnectionIsAllowedByUser[directionAsInt], "cachedConnectionIsAllowedByUser" + directionAsInt, true);
                //Scribe_References.Look<Building_LaserFencePylon>(ref linkedPylons[directionAsInt], "linkedPylon" + directionAsInt);
                Scribe_Values.Look<int>(ref fenceLength[directionAsInt], "fenceLength" + directionAsInt, 0);
            }
        }

        // ===================== Main function =====================
        /// <summary>
        /// When spawned:
        /// - if unpowered, deactivate all fences,
        /// - else, try to connect to nearby pylons.
        /// </summary>
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

        /// <summary>
        /// Deactivate all the fences in cardinal directions.
        /// </summary>
        public void DeactivateAllFences()
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                if (this.fenceLength[directionAsInt] > 0)
                {
                    this.DeactivateFence(new Rot4(directionAsInt));
                }
            }
        }

        /// <summary>
        /// Deactivate a fence in a given direction:
        /// - remove fence elements,
        /// - inform linked pylon, that it is deactivated.
        /// </summary>
        public void DeactivateFence(Rot4 direction)
        {
            if (this.fenceLength[direction.AsInt] > 0)
            {
                IntVec3 linkedPylonPosition = this.Position + new IntVec3(0, 0, this.fenceLength[direction.AsInt] + 1).RotatedBy(direction);
                Building_LaserFencePylon linkedPylon = linkedPylonPosition.GetFirstThing(this.Map, Util_LaserFence.LaserFencePylonDef) as Building_LaserFencePylon;
                if (linkedPylon != null)
                {
                    linkedPylon.RemoveFenceElements(direction.Opposite);
                }
                this.RemoveFenceElements(direction);
            }
        }

        /// <summary>
        /// Remove the fence elements in a given direction. Only north and east directions are treated. Other ones will be treated in a mirror way.
        /// </summary>
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

        /// <summary>
        /// Try to activate inactive fences in allowed cardinal directions.
        /// </summary>
        public void TryActivateInactiveFences()
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                if ((this.fenceLength[directionAsInt] == 0)
                    && this.connectionIsAllowedByUser[directionAsInt])
                {
                    this.LookForPylon(new Rot4(directionAsInt));
                }
            }
        }

        /// <summary>
        /// Look for a pylon to connect to in a given direction.
        /// </summary>
        public void LookForPylon(Rot4 direction, bool forceConnection = false)
        {
            for (int offset = 1; offset <= Settings.laserFenceMaxRange + 1; offset++)
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

        /// <summary>
        /// Connect to a given pylon if allowed or forced.
        /// </summary>
        public void TryConnectToPylon(Building_LaserFencePylon linkedPylon, Rot4 direction, int fenceLength, bool forceConnection)
        {
            // Check connection is allowed.
            if (forceConnection)
            {
                linkedPylon.connectionIsAllowedByUser[direction.Opposite.AsInt] = true;
                linkedPylon.cachedConnectionIsAllowedByUser[direction.Opposite.AsInt] = true;
            }
            if (linkedPylon.connectionIsAllowedByUser[direction.Opposite.AsInt] == false)
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
                this.ActivateFence(direction, fenceLength);
                linkedPylon.ActivateFence(direction.Opposite, fenceLength);
            }
        }

        /// <summary>
        /// Spawn the fence elements in a given direction. Only north and east directions are treated. Other ones will be treated in a mirror way.
        /// </summary>
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

        // ===================== Exported functions =====================
        /// <summary>
        /// Used by a fence element to notify a new building is blocking.
        /// </summary>
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

        /// <summary>
        /// Called when a pawn go to a pylon to take into account the player's cached configuration.
        /// </summary>
        public void Notify_PawnSwitchedLaserFence()
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                Rot4 direction = new Rot4(directionAsInt);
                this.connectionIsAllowedByUser[directionAsInt] = this.cachedConnectionIsAllowedByUser[directionAsInt];
                if (this.connectionIsAllowedByUser[directionAsInt])
                {
                    // Connection is allowed. Look for a pylon to connect to.
                    if (this.fenceLength[direction.AsInt] == 0)
                    {
                        this.LookForPylon(direction, true);
                    }
                }
                else
                {
                    // Connection is forbidden. Disconnect from linked pylon if necessary.
                    if (this.fenceLength[directionAsInt] > 0)
                    {
                        IntVec3 linkedPylonPosition = this.Position + new IntVec3(0, 0, this.fenceLength[direction.AsInt] + 1).RotatedBy(direction);
                        Building_LaserFencePylon linkedPylon = linkedPylonPosition.GetFirstThing(this.Map, Util_LaserFence.LaserFencePylonDef) as Building_LaserFencePylon;
                        if (linkedPylon != null)
                        {
                            linkedPylon.connectionIsAllowedByUser[direction.Opposite.AsInt] = false;
                            linkedPylon.cachedConnectionIsAllowedByUser[direction.Opposite.AsInt] = false;
                        }
                        this.DeactivateFence(direction);
                    }
                }
            }
        }

        // ===================== Gizmos =====================
        /// <summary>
        /// Get the buttons to toggle fences.
        /// </summary>
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
                northFenceGizmo.defaultDesc = "Click to deactivate north fence.";
            }
            else
            {
                northFenceGizmo.icon = northFenceInactive;
                northFenceGizmo.defaultDesc = "Click to activate north fence.";
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
                eastFenceGizmo.defaultDesc = "Click to deactivate east fence.";
            }
            else
            {
                eastFenceGizmo.icon = eastFenceInactive;
                eastFenceGizmo.defaultDesc = "Click to activate east fence.";
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
                southFenceGizmo.defaultDesc = "Click to deactivate south fence.";
            }
            else
            {
                southFenceGizmo.icon = southFenceInactive;
                southFenceGizmo.defaultDesc = "Click to activate south fence.";
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
                westFenceGizmo.defaultDesc = "Click to deactivate west fence.";
            }
            else
            {
                westFenceGizmo.icon = westFenceInactive;
                westFenceGizmo.defaultDesc = "Click to activate west fence.";
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
        /// <summary>
        /// Draw the fence connectors.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                if (this.fenceLength[directionAsInt] > 0)
                {
                    Material connectorTexture = connectorNorth;
                    if (directionAsInt == Rot4.North.AsInt)
                    {
                        connectorTexture = connectorNorth;
                    }
                    else if (directionAsInt == Rot4.East.AsInt)
                    {
                        connectorTexture = connectorEast;
                    }
                    else if (directionAsInt == Rot4.South.AsInt)
                    {
                        connectorTexture = connectorSouth;
                    }
                    else if (directionAsInt == Rot4.West.AsInt)
                    {
                        connectorTexture = connectorWest;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, connectorMatrix, connectorTexture, 0);
                }
            }
        }

        /// <summary>
        /// Highlight the cells that could be crossed by a fence.
        /// </summary>
        public static void DrawPotentialBuildCells(Map map, IntVec3 pylonPosition)
        {
            List<IntVec3> potentialBuildCells = new List<IntVec3>();
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                for (int offset = 1; offset <= Settings.laserFenceMaxRange; offset++)
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
