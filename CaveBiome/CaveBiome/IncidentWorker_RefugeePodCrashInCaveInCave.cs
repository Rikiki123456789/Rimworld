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
            if (Find.Map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                return base.TryExecute(parms);
            }
            IntVec3 intVec = IntVec3.Invalid;
            TryFindRefugeePodSpot(out intVec);
            if (intVec.IsValid == false)
            {
                return false;
            }
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction, PawnGenerationContext.NonPlayer, false, false, false, false, true, false, 20f, false, true, true, null, null, null, null, null, null);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            HealthUtility.GiveInjuriesToForceDowned(pawn);
            string label = "LetterLabelRefugeePodCrash".Translate();
            string text = "RefugeePodCrash".Translate();
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
            Find.LetterStack.ReceiveLetter(label, text, LetterType.BadNonUrgent, intVec, null);
            DropPodUtility.MakeDropPodAt(intVec, new DropPodInfo
            {
                SingleContainedThing = pawn,
                openDelay = 180,
                leaveSlag = true
            });
            return true;
        }

        public static void TryFindRefugeePodSpot(out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;
            List<Thing> caveWellsList = Find.ListerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
            foreach (Thing caveWell in caveWellsList.InRandomOrder())
            {
                if (IsValidPositionToSpawnRefugeePod(caveWell.Position))
                {
                    spawnCell = caveWell.Position;
                    return;
                }
            }
        }

        public static bool IsValidPositionToSpawnRefugeePod(IntVec3 position)
        {
            ThingDef chunkDef = ThingDefOf.ShipChunk;
            if ((position.InBounds() == false)
                || position.Fogged()
                || (position.Standable() == false)
                || (position.Roofed()
                    && position.GetRoof().isThickRoof))
            {
                return false;
            }
            return true;
        }
    }
}
