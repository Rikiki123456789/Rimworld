using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace CaveBiome
{
    public class IncidentWorker_ResourcePodCrashInCave : IncidentWorker_ResourcePodCrash
    {
        private const int MaxStacks = 8;
        private const float MaxMarketValue = 40f;

        private static ThingDef RandomPodContentsDef()
        {
            Func<ThingDef, bool> isLeather = (ThingDef d) => d.category == ThingCategory.Item && d.thingCategories != null && d.thingCategories.Contains(ThingCategoryDefOf.Leathers);
            Func<ThingDef, bool> isMeat = (ThingDef d) => d.category == ThingCategory.Item && d.thingCategories != null && d.thingCategories.Contains(ThingCategoryDefOf.MeatRaw);
            int numLeathers = DefDatabase<ThingDef>.AllDefs.Where(isLeather).Count<ThingDef>();
            int numMeats = DefDatabase<ThingDef>.AllDefs.Where(isMeat).Count<ThingDef>();
            return (
                from d in DefDatabase<ThingDef>.AllDefs
                where d.category == ThingCategory.Item && d.tradeability == Tradeability.Stockable && d.equipmentType == EquipmentType.None && d.BaseMarketValue >= 1f && d.BaseMarketValue < MaxMarketValue && !d.HasComp(typeof(CompHatcher))
                select d).RandomElementByWeight(delegate(ThingDef d)
                {
                    float num = 100f;
                    if (isLeather(d))
                    {
                        num *= 5f / (float)numLeathers;
                    }
                    if (isMeat(d))
                    {
                        num *= 5f / (float)numMeats;
                    }
                    return num;
                });
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                return base.TryExecute(parms);
            }
            
            ThingDef thingDef = IncidentWorker_ResourcePodCrashInCave.RandomPodContentsDef();
            List<Thing> list = new List<Thing>();
            float num = (float)Rand.Range(150, 900);
            do
            {
                Thing thing = ThingMaker.MakeThing(thingDef, null);
                int num2 = Rand.Range(20, 40);
                if (num2 > thing.def.stackLimit)
                {
                    num2 = thing.def.stackLimit;
                }
                if ((float)num2 * thing.def.BaseMarketValue > num)
                {
                    num2 = Mathf.FloorToInt(num / thing.def.BaseMarketValue);
                }
                if (num2 == 0)
                {
                    num2 = 1;
                }
                thing.stackCount = num2;
                list.Add(thing);
                num -= (float)num2 * thingDef.BaseMarketValue;
            }
            while (list.Count < MaxStacks && num > thingDef.BaseMarketValue);
            IntVec3 intVec = IntVec3.Invalid;
            TryFindDropPodSpot(map, out intVec);
            if (intVec.IsValid)
            {
                DropPodUtility.DropThingsNear(intVec, map, list, 110, false, true, true);
                Find.LetterStack.ReceiveLetter("LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.Good, new GlobalTargetInfo(intVec, map), null);
                return true;
            }
            return false;
        }

        public void TryFindDropPodSpot(Map map, out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;
            List<Thing> caveWellsList = map.listerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
            foreach (Thing caveWell in caveWellsList.InRandomOrder())
            {
                if (IsValidPositionToSpawnDropPod(map, caveWell.Position))
                {
                    spawnCell = caveWell.Position;
                    return;
                }
            }
        }

        public static bool IsValidPositionToSpawnDropPod(Map map, IntVec3 position)
        {
            ThingDef chunkDef = ThingDefOf.ShipChunk;
            if ((position.InBounds(map) == false)
                || position.Fogged(map)
                || (position.Standable(map) == false)
                || (position.Roofed(map)
                    && position.GetRoof(map).isThickRoof))
            {
                return false;
            }
            return true;
        }
    }
}
