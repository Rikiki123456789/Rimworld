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

namespace Projector
{
    /// <summary>
    /// Building_FixedProjector class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class Building_FixedProjector : Building
    {
        public const int lineOfSightCheckPeriodInTicks = 30;
        public int nextLineOfSightCheckTick = 0;

        // Components references.
        public CompPowerTrader powerComp;

        // Light references.
        public Thing light = null;

        // Projector range.
        public const int projectorRange = 2;


        // ===================== Setup Work =====================
        /// <summary>
        /// Initialize instance variables.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.powerComp = base.GetComp<CompPowerTrader>();
            this.nextLineOfSightCheckTick = Find.TickManager.TicksGame + Rand.Range(0, lineOfSightCheckPeriodInTicks);

            this.powerComp.powerStartedAction = OnPoweredOn;
            this.powerComp.powerStoppedAction = OnPoweredOff;
        }

        /// <summary>
        /// Remove the light and destroy the object.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            SwitchOffLight();
            base.Destroy(mode);
        }

        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.nextLineOfSightCheckTick, "nextLineOfSightCheckTick");
            Scribe_References.Look<Thing>(ref this.light, "light");
        }


        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - look for a target,
        /// - light it if it exists,
        /// - otherwise, idle turn.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (powerComp.PowerOn
                && (Find.TickManager.TicksGame >= this.nextLineOfSightCheckTick))
            {
                this.nextLineOfSightCheckTick = Find.TickManager.TicksGame + lineOfSightCheckPeriodInTicks;

                IntVec3 lightCenter;
                bool lightCenterIsValid = GetLightCenterPosition(this.Map, this.Position, this.Rotation, out lightCenter);
                if (lightCenterIsValid
                    && GenSight.LineOfSight(this.Position, lightCenter, this.Map))
                {
                    SwitchOnLight(lightCenter);
                }
                else
                {
                    SwitchOffLight();
                }
            }
        }

        // ===================== Utility Function =====================
        /// <summary>
        /// Get the light center position according to the projector position and rotation.
        /// </summary>
        public static bool GetLightCenterPosition(Map map, IntVec3 projectorPosition, Rot4 projectorRotation, out IntVec3 lightCenterPosition)
        {
            bool targetIsValid = true;
            lightCenterPosition = projectorPosition + new IntVec3(0, 0, projectorRange).RotatedBy(projectorRotation);
            if (lightCenterPosition.InBounds(map) == false)
            {
                lightCenterPosition = projectorPosition;
                targetIsValid = false;
            }
            return targetIsValid;
        }

        /// <summary>
        /// Action when powered on.
        /// </summary>
        public void OnPoweredOn()
        {
            IntVec3 target = this.Position + new IntVec3(0, 0, projectorRange).RotatedBy(this.Rotation);
            if (GenSight.LineOfSight(this.Position, target, this.Map))
            {
                SwitchOnLight(target);
            }
        }

        /// <summary>
        /// Action when powered off.
        /// </summary>
        public void OnPoweredOff()
        {
            SwitchOffLight();
        }

        /// <summary>
        /// Power off the light.
        /// </summary>
        public void SwitchOffLight()
        {
            if (this.light.DestroyedOrNull() == false)
            {
                this.light.Destroy();
            }
            this.light = null;
        }

        /// <summary>
        /// Light an area at given position.
        /// </summary>
        public void SwitchOnLight(IntVec3 position)
        {
            if (this.light.DestroyedOrNull())
            {
                // Note: we could forbid several lights on the same spot but as glowers stack, it is visually better.
                /*Thing potentialLight = position.GetFirstThing(this.Map, Util_Projector.ProjectorLightDef);
                if (potentialLight == null)*/
                {
                    this.light = GenSpawn.Spawn(Util_Projector.FixedProjectorLightDef, position, this.Map);
                }
            }
        }

        /// <summary>
        /// Get the list of light cells.
        /// </summary>
        public static List<IntVec3> GetLightedCells(Map map, IntVec3 position, Rot4 rotation)
        {
            List<IntVec3> lightedCellsList = new List<IntVec3>();

            IntVec3 lightCenter;
            bool lightCenterIsValid = GetLightCenterPosition(map, position, rotation, out lightCenter);
            Room room = position.GetRoom(map);
            if (lightCenterIsValid
                && (room != null))
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(lightCenter, 1.5f, true))
                {
                    if (cell.InBounds(map)
                        && (room == cell.GetRoom(map))
                        && GenSight.LineOfSight(position, cell, map))
                    {
                        lightedCellsList.Add(cell);
                    }
                }
            }
            return lightedCellsList;
        }

        // ===================== Draw =====================
        /// <summary>
        /// Draw the projector and a line to the targeted pawn.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            if (Find.Selector.IsSelected(this))
            {
                List<IntVec3> lightedCellsList = Building_FixedProjector.GetLightedCells(this.Map, this.Position, this.Rotation);
                GenDraw.DrawFieldEdges(lightedCellsList);
            }
        }
    }
}