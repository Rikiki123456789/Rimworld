using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;

namespace Spaceship
{
    public static class Expedition
    {
        public enum ExpeditionKind
        {
            Geologists = 1,
            Miners = 2,
            OutpostSettlers = 3,
            Scouts = 4,
            Troopers = 5
        };

        public static Dictionary<PawnKindDef, int> expeditionGeologist = new Dictionary<PawnKindDef, int>()
        {
            {Util_PawnKindDefOf.Geologist, 4},
            {Util_PawnKindDefOf.Technician, 1},
            {Util_PawnKindDefOf.Scout, 1},
            {Util_PawnKindDefOf.Guard, 3},
            {Util_PawnKindDefOf.Officer, 1}
        };
        public static Dictionary<PawnKindDef, int> expeditionMiners = new Dictionary<PawnKindDef, int>()
        {
            {Util_PawnKindDefOf.Technician, 1},
            {Util_PawnKindDefOf.Miner, 4},
            {Util_PawnKindDefOf.Guard, 4},
            {Util_PawnKindDefOf.Officer, 1}
        };
        public static Dictionary<PawnKindDef, int> expeditionOutpostSettlers = new Dictionary<PawnKindDef, int>()
        {
            {Util_PawnKindDefOf.Technician, 6},
            {Util_PawnKindDefOf.Scout, 1},
            {Util_PawnKindDefOf.Guard, 4},
            {Util_PawnKindDefOf.Officer, 1}
        };
        public static Dictionary<PawnKindDef, int> expeditionScouts = new Dictionary<PawnKindDef, int>()
        {
            {Util_PawnKindDefOf.Scout, 3},
            {Util_PawnKindDefOf.Officer, 1}
        };
        public static Dictionary<PawnKindDef, int> expeditionTroopers = new Dictionary<PawnKindDef, int>()
        {
            {Util_PawnKindDefOf.Scout, 2},
            {Util_PawnKindDefOf.Guard, 8},
            {Util_PawnKindDefOf.ShockTrooper, 4},
            {Util_PawnKindDefOf.HeavyGuard, 2},
            {Util_PawnKindDefOf.Officer, 2}
        };

        public static List<Pawn> GenerateExpeditionPawns(Map map)
        {
            List<Pawn> expeditionPawns = new List<Pawn>();

            ExpeditionKind expeditionKind = (ExpeditionKind)Rand.RangeInclusive((int)ExpeditionKind.Geologists, (int)ExpeditionKind.Troopers);
            Dictionary<PawnKindDef, int> expedition = null;
            switch (expeditionKind)
            {
                case ExpeditionKind.Geologists:
                    expedition = expeditionGeologist;
                    break;
                case ExpeditionKind.Miners:
                    expedition = expeditionMiners;
                    break;
                case ExpeditionKind.OutpostSettlers:
                    expedition = expeditionOutpostSettlers;
                    break;
                case ExpeditionKind.Scouts:
                    expedition = expeditionScouts;
                    break;
                case ExpeditionKind.Troopers:
                    expedition = expeditionTroopers;
                    break;
                default:
                    Log.ErrorOnce("MiningCo. Spaceship: unhandled ExpeditionKind (" + expeditionKind.ToString() + ").", 123456782);
                    break;
            }
            if (expedition != null)
            {
                // Generate expedition pawns
                foreach (PawnKindDef pawnKind in expedition.Keys)
                {
                    for (int pawnIndex = 0; pawnIndex < expedition[pawnKind]; pawnIndex++)
                    {
                        Pawn pawn = MiningCoPawnGenerator.GeneratePawn(pawnKind, map);
                        expeditionPawns.Add(pawn);
                    }
                }
            }
            return expeditionPawns;
        }

        public static bool IsWeatherValidForExpedition(Map map)
        {
            float temperature = map.mapTemperature.SeasonalTemp;
            if ((temperature >= -20)
                && (temperature <= 50)
                && (map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) == false))
            {
                return true;
            }
            return false;
        }

        public static bool TryFindRandomExitSpot(Map map, IntVec3 startSpot, out IntVec3 exitSpot)
        {
            Predicate<IntVec3> validator = delegate(IntVec3 cell)
            {
                return ((cell.Fogged(map) == false)
                    && (map.roofGrid.Roofed(cell) == false)
                    && cell.Standable(map)
                    && map.reachability.CanReach(startSpot, cell, PathEndMode.Touch, TraverseMode.PassDoors, Danger.Some));
            };
            return CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Always, out exitSpot);
        }

        public static void RandomlyDamagePawn(Pawn pawn, int injuriesNumber, int damageAmount)
        {
            if (pawn.story.traits.HasTrait(TraitDef.Named("Wimp")))
            {
                // Do not hurt wimp pawns as they could be spawned as dead and break the lord behavior.
                return;
            }
            HediffSet hediffSet = pawn.health.hediffSet;
            int injuriesIndex = 0;
            while ((pawn.Dead == false)
                && (injuriesIndex < injuriesNumber)
                && HittablePartsViolence(hediffSet).Any<BodyPartRecord>())
            {
                injuriesIndex++;
                BodyPartRecord bodyPartRecord = HittablePartsViolence(hediffSet).RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
                DamageDef def;
                if (bodyPartRecord.depth == BodyPartDepth.Outside)
                {
                    def = HealthUtility.RandomViolenceDamageType();
                }
                else
                {
                    def = DamageDefOf.Blunt;
                }
                BodyPartRecord forceHitPart = bodyPartRecord;
                DamageInfo dinfo = new DamageInfo(def, damageAmount, 0f, -1f, null, forceHitPart, null, DamageInfo.SourceCategory.ThingOrUnknown);
                pawn.TakeDamage(dinfo);
            }
        }

        // Copied from Verse.HealthUtility.
        private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
        {
            return from x in bodyModel.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                   where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
                   select x;
        }
    }
}
