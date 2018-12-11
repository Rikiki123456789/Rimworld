using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;    // Always needed
using RimWorld;       // RimWorld specific functions are found here
using Verse;          // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI
using Verse.AI.Group; // Needed when you do something with the AI group.

namespace Spaceship
{
    public class IncidentWorker_DispatcherPick : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (base.CanFireNowSub(parms) == false)
            {
                return false;
            }
            if (Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer))
            {
                return false;
            }
            Map map = (Map)parms.target;
            if (Expedition.IsWeatherValidForExpedition(map))
            {
                Building_OrbitalRelay orbitalRelay = Util_OrbitalRelay.GetOrbitalRelay(map);
                if ((orbitalRelay != null)
                    && (orbitalRelay.powerComp.PowerOn))
                {
                    Building_LandingPad landingPad = Util_LandingPad.GetBestAvailableLandingPadReachingMapEdge(map);
                    if (landingPad != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Building_LandingPad landingPad = Util_LandingPad.GetBestAvailableLandingPadReachingMapEdge(map);
            if (landingPad == null)
            {
                // Should not happen if CanFireNowSub returned true.
                return false;
            }

            // Spawn landing dispatcher spaceship.
            FlyingSpaceshipLanding dispatcherSpaceship = Util_Spaceship.SpawnLandingSpaceship(landingPad, SpaceshipKind.DispatcherPick);

            // Spawn expedition team.
            List<Pawn> teamPawns = Expedition.GenerateExpeditionPawns(map);
            if (Rand.Value < 0.2f)
            {
                ApplyInjuriesOrIllnessToTeam(map, teamPawns);
            }
            return SpawnTeamOnMapEdge(landingPad.Position, map, teamPawns);
        }
        
        public void ApplyInjuriesOrIllnessToTeam(Map map, List<Pawn> teamPawns)
        {
            if ((map.Biome == BiomeDefOf.TropicalRainforest)
                || (map.Biome == BiomeDef.Named("TropicalSwamp")))
            {
                // Apply malaria/sleeping sickness to all team pawns in tropical or swamp biomes.
                HediffDef illness = null;
                if (Rand.Value < 0.5f)
                {
                    illness = HediffDef.Named("SleepingSickness");
                }
                else
                {
                    illness = HediffDefOf.Malaria;
                }
                foreach (Pawn pawn in teamPawns)
                {
                    pawn.health.AddHediff(illness);
                }
            }
            else
            {
                // Randomly damage some pawns.
                int injuredPawnsNumber = Mathf.RoundToInt(Rand.Range(0.25f, 0.5f) * teamPawns.Count);
                injuredPawnsNumber = Mathf.Clamp(injuredPawnsNumber, 1, teamPawns.Count - 1);
                for (int pawnIndex = 0; pawnIndex < injuredPawnsNumber; pawnIndex++)
                {
                    Expedition.RandomlyDamagePawn(teamPawns[pawnIndex], Rand.Range(1, 2), Rand.Range(12, 20));
                }
            }
        }

        public bool SpawnTeamOnMapEdge(IntVec3 targetDestination, Map map, List<Pawn> teamPawns)
        {
            // Find entry cell.
            IntVec3 entryCell = IntVec3.Invalid;
            bool entryCellIsValid = Expedition.TryFindRandomExitSpot(map, targetDestination, out entryCell);
            if (entryCellIsValid)
            {
                // Spawn expedition pawns.
                foreach (Pawn pawn in teamPawns)
                {
                    IntVec3 cell = CellFinder.RandomSpawnCellForPawnNear(entryCell, map, 5);
                    GenSpawn.Spawn(pawn, cell, map);
                }
                // Make lord.
                Lord lord = LordMaker.MakeNewLord(Util_Faction.MiningCoFaction, new LordJob_BoardSpaceship(targetDestination), map, teamPawns);
            }
            return entryCellIsValid;
        }
    }
}
