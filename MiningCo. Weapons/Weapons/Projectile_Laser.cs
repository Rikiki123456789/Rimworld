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

namespace Weapons
{
    /// <summary>
    /// Projectile_Laser class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    public class Projectile_Laser : Projectile
    {
        // ===================== Main Work Function =====================
        /// <summary>
        /// Check for interception on whole trajectory in one tick then spawn projectile drawer.
        /// </summary>
        public override void Tick()
        {
            while (this.DestroyedOrNull() == false)
            {
                base.Tick();
            }
        }

        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(base.launcher, hitThing, intendedTarget.Thing, base.equipmentDef, def, targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                DamageDef damageDef = def.projectile.damageDef;
                float amount = base.DamageAmount;
                float armorPenetration = base.ArmorPenetration;
                Vector3 eulerAngles = ExactRotation.eulerAngles;
                float y = eulerAngles.y;
                Thing launcher = base.launcher;
                ThingDef equipmentDef = base.equipmentDef;
                DamageInfo dinfo = new DamageInfo(damageDef, amount, armorPenetration, y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                Pawn pawn = hitThing as Pawn;
                if (pawn != null && pawn.stances != null && pawn.BodySize <= def.projectile.StoppingPower + 0.001f)
                {
                    pawn.stances.StaggerFor(95);
                }
            }

            // Spawn projectile drawer.
            Vector3 impactPosition;
            if (hitThing != null)
            {
                impactPosition = new Vector3(hitThing.DrawPos.x, 0f, hitThing.DrawPos.z);
            }
            else
            {
                impactPosition = new Vector3(ExactPosition.x, 0f, ExactPosition.z);
                MoteMaker.ThrowMicroSparks(impactPosition, map);
            }
            Projectile_LaserDrawer projectileDrawer = ThingMaker.MakeThing(ThingDef.Named("LaserRay")) as Projectile_LaserDrawer;
            projectileDrawer.Initialize(this.def, new Vector3(this.origin.x, 0f, this.origin.z), impactPosition);
            GenSpawn.Spawn(projectileDrawer, (impactPosition - this.origin).ToIntVec3(), map);
        }

        public override void Draw()
        {
            // Do nothing.
        }
    }
}