using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;

namespace PowerFist
{
    public class Verb_MeleeAttackWithRepel : Verb_MeleeAttack
    {
        private const float StunDurationFactor_Standard = 20f;
        private const float StunDurationFactor_EMP = 15f;
        
        protected override bool TryCastShot()
        {
            bool castIsSuccesful = base.TryCastShot();
            if (castIsSuccesful)
            {
                Pawn targetPawn = this.currentTarget.Thing as Pawn;
                if ((targetPawn != null)
                    && (!targetPawn.Dead)
                    && (!targetPawn.Downed))
                {
                    ThingDef_PowerFist repelProperties = this.ownerEquipment.def as ThingDef_PowerFist;
                    if (repelProperties == null)
                    {
                        Log.Warning("MiningCo. PowerFist: cannot cast ownerEquipment def into ThingDef_PowerFist.");
                        return castIsSuccesful;
                    }
                    bool casterWearsPowerArmor = IsCasterWearingPowerArmor();
                    int repelDurationInTicks = (int)repelProperties.repelDurationInTicks;
                    float maxRepelDistance = 0;
                    float crushDamageFactor = 0;
                    float electricDamageFactor = 0;
                    float stunDurationInTicks = 0;
                    float empDurationInTicks = 0;

                    // Compute repel max distance and stun duration according to the target size. Flying squirrels!!! :D
                    if (casterWearsPowerArmor)
                    {
                        crushDamageFactor = repelProperties.crushDamageFactorWithPowerArmor;
                        electricDamageFactor = repelProperties.electricDamageFactorWithPowerArmor;
                        empDurationInTicks = repelProperties.empDurationInTicks;
                        if (targetPawn.BodySize <= repelProperties.bodySizeSmall)
                        {
                            maxRepelDistance = repelProperties.repelDistanceFactorWithPowerArmor * repelProperties.repelDistanceLong;
                            stunDurationInTicks = repelProperties.stunDurationInTicksLong;
                        }
                        else if (targetPawn.BodySize <= repelProperties.bodySizeMedium)
                        {
                            maxRepelDistance = repelProperties.repelDistanceFactorWithPowerArmor * repelProperties.repelDistanceMedium;
                            stunDurationInTicks = repelProperties.stunDurationInTicksMedium;
                        }
                        else if (targetPawn.BodySize <= repelProperties.bodySizeBig)
                        {
                            maxRepelDistance = repelProperties.repelDistanceFactorWithPowerArmor * repelProperties.repelDistanceShort;
                            stunDurationInTicks = repelProperties.stunDurationInTicksShort;
                        }
                    }
                    else
                    {
                        crushDamageFactor = repelProperties.crushDamageFactor;
                        electricDamageFactor = repelProperties.electricDamageFactor;
                        if (targetPawn.BodySize <= repelProperties.bodySizeSmall)
                        {
                            maxRepelDistance = repelProperties.repelDistanceLong;
                            stunDurationInTicks = repelProperties.stunDurationInTicksMedium;
                        }
                        else if (targetPawn.BodySize <= repelProperties.bodySizeMedium)
                        {
                            maxRepelDistance = repelProperties.repelDistanceMedium;
                            stunDurationInTicks = repelProperties.stunDurationInTicksShort;
                        }
                    }
                    // Start repel if target is not too big.
                    IntVec3 vector = targetPawn.Position - this.CasterPawn.Position;
                    if (maxRepelDistance > 0)
                    {
                        float repelDistance = 0f;
                        ThingDef obstacleDef = null;
                        PowerFistRepeller repeller = GenSpawn.Spawn(Util_PowerFist.PowerFistRepellerDef, targetPawn.Position, targetPawn.Map) as PowerFistRepeller;
                        repeller.Notify_BeginRepel(targetPawn, vector, Mathf.FloorToInt(maxRepelDistance), repelDurationInTicks, out repelDistance, out obstacleDef);
                        if (obstacleDef != null)
                        {
                            Vector3 motePosition = targetPawn.Position.ToVector3Shifted() + vector.ToVector3() * repelDistance;
                            MoteMaker.ThrowText(motePosition, targetPawn.Map, "Crushed", Color.red);
                            int extraDamage = Mathf.RoundToInt(this.ownerEquipment.def.statBases.GetStatValueFromList(StatDefOf.MeleeWeapon_AverageDPS, 5f) * crushDamageFactor);
                            DamageInfo infos = new DamageInfo(DamageDefOf.Blunt, extraDamage, vector.AngleFlat, this.caster, null, obstacleDef);
                            targetPawn.TakeDamage(infos);
                        }
                    }
                    if ((targetPawn != null)
                       && (targetPawn.Dead
                        || targetPawn.Downed))
                    {
                        // Do not try to apply stun if pawn is dead/downed.
                        return true;
                    }
                    // Apply stun or EMP.
                    if (targetPawn.RaceProps.IsMechanoid)
                    {
                        if (casterWearsPowerArmor)
                        {
                            DamageInfo infos = new DamageInfo(DamageDefOf.EMP, Mathf.RoundToInt(empDurationInTicks / StunDurationFactor_EMP), vector.AngleFlat, this.caster, null, this.ownerEquipment.def);
                            targetPawn.stances.stunner.Notify_DamageApplied(infos, true);
                            MoteMaker.ThrowExplosionInteriorMote(targetPawn.Position.ToVector3Shifted(), targetPawn.Map, ThingDef.Named("Mote_ElectricalSpark"));

                            int extraDamage = Mathf.RoundToInt(this.ownerEquipment.def.statBases.GetStatValueFromList(StatDefOf.MeleeWeapon_AverageDPS, 5) * electricDamageFactor);
                            DamageInfo infos2 = new DamageInfo(Util_PowerFist.ElectricDamageDef, extraDamage, vector.AngleFlat, this.caster, null, this.ownerEquipment.def);
                            targetPawn.TakeDamage(infos2);
                        }
                    }
                    else if (stunDurationInTicks > 0)
                    {
                        DamageInfo infos = new DamageInfo(DamageDefOf.Stun, Mathf.RoundToInt(stunDurationInTicks / StunDurationFactor_Standard), vector.AngleFlat, this.caster, null, this.ownerEquipment.def);
                        targetPawn.stances.stunner.Notify_DamageApplied(infos, false);
                    }
                }
            }
            return castIsSuccesful;
        }

        protected bool IsCasterWearingPowerArmor()
        {
            foreach (Apparel apparel in this.CasterPawn.apparel.WornApparel)
            {
                if (apparel.def == ThingDef.Named("Apparel_PowerArmor"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
