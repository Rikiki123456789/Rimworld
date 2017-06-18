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
    public class IncidentWorker_RefugeePodCrashInCave : IncidentWorker_RefugeePodCrash
    {
        private const float FogClearRadius = 4.5f;
        private const float RelationWithColonistWeight = 20f;
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                return base.TryExecute(parms);
            }
            IntVec3 intVec = IntVec3.Invalid;
            TryFindRefugeePodSpot(map, out intVec);
            if (intVec.IsValid == false)
            {
                return false;
            }
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 20f, false, true, true, false, false, null, null, null, null, null, null);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            HealthUtility.DamageUntilDowned(pawn);
            string label = "LetterLabelRefugeePodCrash".Translate();
            string text = "RefugeePodCrash".Translate();
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.BadNonUrgent, new GlobalTargetInfo(intVec, map, false), null);
            DropPodUtility.MakeDropPodAt(intVec, map, new ActiveDropPodInfo
            {
                SingleContainedThing = pawn,
                openDelay = 180,
                leaveSlag = true
            });
            return true;
        }

        public static void TryFindRefugeePodSpot(Map map, out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;
            List<Thing> caveWellsList = map.listerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
            foreach (Thing caveWell in caveWellsList.InRandomOrder())
            {
                if (IsValidPositionToSpawnRefugeePod(map, caveWell.Position))
                {
                    spawnCell = caveWell.Position;
                    return;
                }
            }
        }

        public static bool IsValidPositionToSpawnRefugeePod(Map map, IntVec3 position)
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
