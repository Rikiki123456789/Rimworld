using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using RimWorld.SquadAI; // Needed when you do something with the squad AI
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
            // Generate technicians.
            for (int pawnIndex = 0; pawnIndex < 4; pawnIndex++)
            {
                // TODO: generate M&Co. pawns: mercenary and technicians.
                Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Slave, OG_Util.FactionOfMAndCo);
                pawn.workSettings.EnableAndInitialize();
                GenSpawn.Spawn(pawn, outpostData.areaSouthWestOrigin + new IntVec3(OG_BigOutpost.areaSideLength / 2 + Rand.RangeInclusive(-5, 5), 0, OG_BigOutpost.areaSideLength / 2 + Rand.RangeInclusive(-5, 5)));
                pawn.playerSettings = new Pawn_PlayerSettings(pawn);
                pawn.playerSettings.AreaRestriction = outpostArea;
            }

            // Generate outpost guards.
            List<Pawn> guardsList = new List<Pawn>();
            for (int pawnIndex = 0; pawnIndex < 8; pawnIndex++)
            {
                Pawn pawn = null;
                Color armyGreen = new Color(80f/255f, 130f/255f, 0);

                if (pawnIndex == 0)
                {
                    // Generate officer.
                    pawn = PawnGenerator.GeneratePawn(OG_Util.OutpostOfficerDef, OG_Util.FactionOfMAndCo);
                    Apparel pant = ThingMaker.MakeThing(ThingDef.Named("Apparel_Pants"), ThingDef.Named("Hyperweave")) as Apparel;
                    pant.SetColor(armyGreen);
                    pawn.apparel.Wear(pant);
                    Apparel shirt = ThingMaker.MakeThing(ThingDef.Named("Apparel_CollarShirt"), ThingDef.Named("Hyperweave")) as Apparel;
                    shirt.SetColor(armyGreen);
                    pawn.apparel.Wear(shirt);
                }
                else
                {
                    // Generate guard.
                    pawn = PawnGenerator.GeneratePawn(OG_Util.OutpostOfficerDef, OG_Util.FactionOfMAndCo);
                }
                pawn.workSettings.EnableAndInitialize();
                GenSpawn.Spawn(pawn, outpostData.areaSouthWestOrigin + new IntVec3(OG_BigOutpost.areaSideLength / 2 + Rand.RangeInclusive(-5, 5), 0, OG_BigOutpost.areaSideLength / 2 + Rand.RangeInclusive(-5, 5)));
                pawn.playerSettings = new Pawn_PlayerSettings(pawn);
                pawn.playerSettings.AreaRestriction = outpostArea;
                guardsList.Add(pawn);
            }
            // Affect squad brain to outpost guards.
            State_DefendOutpost stateDefend = new State_DefendOutpost(outpostData.areaSouthWestOrigin + new IntVec3(OG_BigOutpost.areaSideLength / 2, 0, OG_BigOutpost.areaSideLength / 2), OG_BigOutpost.areaSideLength * (3 / 4));
            StateGraph stateGraph = GraphMaker.SingleStateGraph(stateDefend);
            BrainMaker.MakeNewBrain(OG_Util.FactionOfMAndCo, stateGraph, guardsList);
        }
    }
}
