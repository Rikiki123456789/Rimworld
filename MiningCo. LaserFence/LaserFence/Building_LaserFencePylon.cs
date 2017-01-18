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
        // Pylon state.
        private bool[] connectionIsAllowedByUser = new bool[4] { true, true, true, true };
        private bool[] cachedConnectionIsAllowedByUser = new bool[4] { true, true, true, true };
        private Building_LaserFencePylon[] linkedPylons = new Building_LaserFencePylon[4] { null, null, null, null };
        private int[] fenceLength = new int[4] { 0, 0, 0, 0 };
        private CompPowerTrader powerComp = null;
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

        // Textures.
        private static Material fenceTexture = MaterialPool.MatFrom("Effects/LaserFence", ShaderDatabase.Transparent);
        private Vector3 northFenceScale = new Vector3(1f, 1f, 1f);
        private Matrix4x4 northFenceMatrix = default(Matrix4x4);
        private Vector3 eastFenceScale = new Vector3(1f, 1f, 1f);
        private Matrix4x4 eastFenceMatrix = default(Matrix4x4);

        // Gizmo textures.
        private static Texture2D northFenceActive = ContentFinder<Texture2D>.Get("Ui/Commands/NorthFenceActive");
        private static Texture2D northFenceInactive = ContentFinder<Texture2D>.Get("Ui/Commands/NorthFenceInactive");
        private static Texture2D eastFenceActive = ContentFinder<Texture2D>.Get("Ui/Commands/EastFenceActive");
        private static Texture2D eastFenceInactive = ContentFinder<Texture2D>.Get("Ui/Commands/EastFenceInactive");
        private static Texture2D southFenceActive = ContentFinder<Texture2D>.Get("Ui/Commands/SouthFenceActive");
        private static Texture2D southFenceInactive = ContentFinder<Texture2D>.Get("Ui/Commands/SouthFenceInactive");
        private static Texture2D westFenceActive = ContentFinder<Texture2D>.Get("Ui/Commands/WestFenceActive");
        private static Texture2D westFenceInactive = ContentFinder<Texture2D>.Get("Ui/Commands/WestFenceInactive");

        // ######## Spawn setup ######## //

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);

            this.powerComp = this.TryGetComp<CompPowerTrader>();
        }

        // ######## Tick ######## //

        private bool refreshAfterLoading = true;
        public override void Tick()
        {
            base.Tick();

            if (refreshAfterLoading)
            {
                this.DeactivateAllFences();
                this.TryToActivateInactiveFences();
                refreshAfterLoading = false;
            }
            if (this.powerComp.PowerOn == false)
            {
                this.DeactivateAllFences();
            }
            else
            {
                if ((Find.TickManager.TicksGame % 30) == 0)
                {
                    this.TryToActivateInactiveFences();
                }
            }
        }

        // ######## Destroy ######## //

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);

            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                if (this.linkedPylons[directionAsInt] != null)
                {
                    this.DisconnectFromPylon(new Rot4(directionAsInt));
                }
            }
        }

        // ######## ExposeData ######## //

        public override void ExposeData()
        {
            base.ExposeData();

            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                Scribe_Values.LookValue<bool>(ref connectionIsAllowedByUser[directionAsInt], "connectionIsAllowedByUser" + directionAsInt, true);
                Scribe_Values.LookValue<bool>(ref cachedConnectionIsAllowedByUser[directionAsInt], "cachedConnectionIsAllowedByUser" + directionAsInt, true);
                Scribe_References.LookReference<Building_LaserFencePylon>(ref linkedPylons[directionAsInt], "linkedPylons" + directionAsInt);
                Scribe_Values.LookValue<int>(ref fenceLength[directionAsInt], "fenceLength" + directionAsInt, 0);
            }
        }

        // ######## Gizmos ######## //

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

        // ######## Draw ######## //

        public override void Draw()
        {
            base.Draw();

            if (this.linkedPylons[Rot4.North.AsInt] != null)
            {
                Graphics.DrawMesh(MeshPool.plane10, northFenceMatrix, fenceTexture, 0);
            }
            if (this.linkedPylons[Rot4.East.AsInt] != null)
            {
                Graphics.DrawMesh(MeshPool.plane10, eastFenceMatrix, fenceTexture, 0);
            }
        }

        // ######## Other functions ######## //

        private void DeactivateAllFences()
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                if (this.linkedPylons[directionAsInt] != null)
                {
                    this.DisconnectFromPylon(new Rot4(directionAsInt));
                }
            }
        }

        private void TryToActivateInactiveFences()
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                if ((this.linkedPylons[directionAsInt] == null)
                    && this.connectionIsAllowedByUser[directionAsInt])
                {
                    this.LookForPylonInDirection(new Rot4(directionAsInt));
                }
            }
        }

        private void LookForPylonInDirection(Rot4 direction, bool forceConnection = false)
        {
            this.DisconnectFromPylon(direction);
            for (int offset = 1; offset <= 5; offset++)
            {
                IntVec3 checkedPosition = this.Position + new IntVec3(0, 0, offset).RotatedBy(direction);
                foreach (Thing thing in checkedPosition.GetThingList(this.Map))
                {
                    if (thing is Building_LaserFencePylon)
                    {
                        this.TryToConnectToPylon(thing as Building_LaserFencePylon, direction, offset - 1, forceConnection);
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

        private void DisconnectFromPylon(Rot4 direction)
        {
            if (this.linkedPylons[direction.AsInt] != null)
            {
                this.DeactivateFence(direction);
                Rot4 linkedPylonDirection = direction;
                linkedPylonDirection.Rotate(RotationDirection.Clockwise); // Rotate 2 times to get the opposite direction.
                linkedPylonDirection.Rotate(RotationDirection.Clockwise);
                this.linkedPylons[direction.AsInt].DeactivateFence(linkedPylonDirection);
                this.linkedPylons[direction.AsInt].linkedPylons[linkedPylonDirection.AsInt] = null;
                this.linkedPylons[direction.AsInt] = null;
            }
        }

        private void TryToConnectToPylon(Building_LaserFencePylon linkedPylon, Rot4 direction, int fenceLength, bool forceConnection)
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
                    linkedPylon.DisconnectFromPylon(linkedPylonDirection);
                }
                this.linkedPylons[direction.AsInt] = linkedPylon;
                this.ActivateFence(direction, fenceLength);
                linkedPylon.linkedPylons[linkedPylonDirection.AsInt] = this;
                linkedPylon.ActivateFence(linkedPylonDirection, fenceLength);
            }
        }

        private void DeactivateFence(Rot4 direction)
        {
            // Remove north or east laser fences.
            if ((direction == Rot4.North)
                || (direction == Rot4.East))
            {
                for (int offset = 1; offset <= this.fenceLength[direction.AsInt]; offset++)
                {
                    IntVec3 checkedPosition = this.Position + new IntVec3(0, 0, offset).RotatedBy(direction);
                    List<Thing> thingList = checkedPosition.GetThingList(this.Map);
                    for (int thingIndex = thingList.Count - 1; thingIndex >= 0; thingIndex--)
                    {
                        Thing thing = thingList[thingIndex];
                        if (thing is Building_LaserFence)
                        {
                            thing.Destroy(DestroyMode.Vanish); // Only destroy 1 fence at this position as 2 fences may cross.
                            break;
                        }
                    }
                }
            }
            this.fenceLength[direction.AsInt] = 0;
        }

        private void ActivateFence(Rot4 direction, int fenceLength)
        {
            this.fenceLength[direction.AsInt] = fenceLength;

            if ((direction == Rot4.North)
                || (direction == Rot4.East))
            {
                // Spawn laser fences.
                for (int offset = 1; offset <= this.fenceLength[direction.AsInt]; offset++)
                {
                    IntVec3 fencePosition = this.Position + new IntVec3(0, 0, offset).RotatedBy(direction);
                    Building_LaserFence laserFence = ThingMaker.MakeThing(ThingDef.Named("LaserFence")) as Building_LaserFence;
                    laserFence.pylon = this;
                    GenSpawn.Spawn(laserFence, fencePosition, this.Map);
                }
                // Drawing parameters.
                Vector3 fenceScale = new Vector3(fenceLength, 1f, 1f);
                if (direction == Rot4.North)
                {
                    northFenceScale = fenceScale;
                    northFenceMatrix.SetTRS(base.DrawPos + new Vector3(0f, 0f, 0.5f + (float)fenceLength / 2f) + Altitudes.AltIncVect, Quaternion.AngleAxis(90f, Vector3.up), northFenceScale);
                }
                else if (direction == Rot4.East)
                {
                    eastFenceScale = fenceScale;
                    eastFenceMatrix.SetTRS(base.DrawPos + new Vector3(0.5f + (float)fenceLength / 2f, 0f, 0f) + Altitudes.AltIncVect, Quaternion.identity, eastFenceScale);
                }
            }
        }

        public void InformEdificeIsBlocking()
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                this.DeactivateAllFences();
                this.TryToActivateInactiveFences();
            }
        }

        private string GetFenceStatusAsString(int directionAsInt)
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

        public void SwitchLaserFence()
        {
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                Rot4 direction = new Rot4(directionAsInt);
                this.connectionIsAllowedByUser[directionAsInt] = this.cachedConnectionIsAllowedByUser[directionAsInt];
                if (this.connectionIsAllowedByUser[directionAsInt])
                {
                    this.LookForPylonInDirection(direction, true);
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
                        this.DisconnectFromPylon(direction);
                    }
                }
            }
        }
                
        public static bool CanPlaceNewPylonHere(Map map, IntVec3 testedPosition, out string reason)
        {
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(new TargetInfo(testedPosition, map)))
            {
                Building building = cell.GetEdifice(map);
                if ((building != null)
                    && (building.def.building.isNaturalRock))
                {
                    reason = "Pylon cannot be built near a natural rock.";
                    return false;
                }
                TerrainDef terrain = cell.GetTerrain(map);
                if ((terrain == TerrainDef.Named("WaterDeep"))
                    || (terrain == TerrainDef.Named("WaterShallow")))
                {
                    reason = "Pylon cannot be built near water.";
                    return false;
                }
            }
            reason = "";
            return true;
        }
        
        public static void DrawPotentialPlacePositions(Map map, IntVec3 pylonPosition)
        {
            List<IntVec3> potentialPlacingPositionsList = new List<IntVec3>();
            for (int directionAsInt = 0; directionAsInt < 4; directionAsInt++)
            {
                for (int offset = 1; offset <= 5; offset++)
                {
                    string unusedReason = "";
                    IntVec3 testedPosition = pylonPosition + new IntVec3(offset, 0, 0).RotatedBy(new Rot4(directionAsInt));
                    if (Building_LaserFencePylon.CanPlaceNewPylonHere(map, testedPosition, out unusedReason))
                    {
                        potentialPlacingPositionsList.Add(testedPosition);
                    }
                }
                if (potentialPlacingPositionsList.NullOrEmpty() == false)
                {
                    GenDraw.DrawFieldEdges(potentialPlacingPositionsList);
                }
            }
        }
    }
}
