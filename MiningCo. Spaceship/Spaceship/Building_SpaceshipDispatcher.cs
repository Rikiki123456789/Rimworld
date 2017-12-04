using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;    // Always needed
using RimWorld;       // RimWorld specific functions are found here
using Verse;          // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI
using Verse.AI.Group; // Needed when you do something with the group AI
using Verse.Sound;    // Needed when you do something with the Sound

namespace Spaceship
{
    public abstract class Building_SpaceshipDispatcher : Building_Spaceship, IThingHolder
    {
        public bool turretsAreDeployed = false;

        public const int turretsCount = 2;
        public IntVec3[] turretOffsetPositions = new IntVec3[turretsCount]
        {
            new IntVec3(-4, 0, -2),
            new IntVec3(4, 0, -2)
        };

        public Rot4[] turretOffsetRotations = new Rot4[turretsCount]
        {
            Rot4.West,
            Rot4.East
        };

        public override bool takeOffRequestIsEnabled
        {
            get
            {
                return true;
            }
        }

        // ===================== Setup work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (this.turretsAreDeployed == false)
            {
                // Spawn vulcan turrets.
                this.turretsAreDeployed = true;
                for (int turretIndex = 0; turretIndex < turretsCount; turretIndex++)
                {
                    Thing vulcanTurret = ThingMaker.MakeThing(Util_ThingDefOf.VulcanTurret, ThingDefOf.Plasteel);
                    vulcanTurret.SetFaction(Util_Faction.MiningCoFaction);
                    GenSpawn.Spawn(vulcanTurret, this.Position + this.turretOffsetPositions[turretIndex].RotatedBy(this.Rotation), this.Map, new Rot4(this.Rotation.AsInt + this.turretOffsetRotations[turretIndex].AsInt));
                    // Cannot set turret top rotation...
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // Despawn vulcan turrets.
            for (int turretIndex = 0; turretIndex < turretsCount; turretIndex++)
            {
                Thing vulcanTurret = (this.Position + this.turretOffsetPositions[turretIndex].RotatedBy(this.Rotation)).GetFirstThing(this.Map, Util_ThingDefOf.VulcanTurret);
                if (vulcanTurret != null)
                {
                    vulcanTurret.Destroy();
                }
            }
            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Boolean>(ref this.turretsAreDeployed, "turretsAreDeployed");
        }

        // ===================== Inspect panel =====================
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (Find.TickManager.TicksGame >= this.takeOffTick)
            {
                stringBuilder.Append("Taking off ASAP");
            }
            else
            {
                stringBuilder.Append("Planned take-off: " + Util_Misc.GetTicksAsStringInDaysHours(this.takeOffTick - Find.TickManager.TicksGame));
            }

            return stringBuilder.ToString();
        }
    }
}
