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

namespace OutpostGenerator
{
    /// <summary>
    /// OG_Inhabitants class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class OG_Inhabitants
    {
        public static void GenerateInhabitants(ref OG_OutpostData outpostData)
        {
            if (outpostData.isInhabited == false)
            {
                return;
            }

            // Create outpost allowed area.
            Area_Allowed outpostArea;
            Find.AreaManager.TryMakeNewAllowed(AllowedAreaMode.Humanlike, out outpostArea);
            outpostArea.SetLabel(OG_Util.OutpostAreaLabel);
            for (int xOffset = 0; xOffset < OG_BigOutpost.areaSideLength; xOffset++)
            {
                for (int zOffset = 0; zOffset < OG_BigOutpost.areaSideLength; zOffset++)
                {
                    outpostArea.Set(outpostData.areaSouthWestOrigin + new IntVec3(xOffset, 0, zOffset));
                }
            }
            for (int pawnIndex = 0; pawnIndex < 12; pawnIndex++)
            {
                // TODO: generate M&Co. pawns: mercenary and technicians.
                Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, OG_Util.FactionOfMAndCo);
                pawn.workSettings.EnableAndInitialize();
                GenSpawn.Spawn(pawn, outpostData.areaSouthWestOrigin + new IntVec3(OG_BigOutpost.areaSideLength / 2 + Rand.RangeInclusive(-5, 5), 0, OG_BigOutpost.areaSideLength / 2 + Rand.RangeInclusive(-5, 5)));
                pawn.playerSettings = new Pawn_PlayerSettings(pawn);
                pawn.playerSettings.AreaRestriction = outpostArea;
            }
        }
    }
}
