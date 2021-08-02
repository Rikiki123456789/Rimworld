using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;

namespace Spaceship
{
    public static class Util_DownedPawn
    {
        public static Pawn GetRandomReachableDownedPawn(Pawn carrier)
        {
            if (carrier.Map == null)
            {
                return null;
            }
            foreach (Pawn downedPawn in carrier.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(carrier.Faction))
            {
                if (downedPawn.Downed)
                {
                    if (carrier.CanReserveAndReach(downedPawn, PathEndMode.OnCell, Danger.Some))
                    {
                        return downedPawn;
                    }
                }
            }
            return null;
        }

        public static Pawn GetNearestReachableDownedPawn(Pawn carrier)
        {
            if (carrier.Map == null)
            {
                return null;
            }
            Pawn nearestDownedPawn = null;
            float minDistance = 99999f;
            foreach (Pawn downedPawn in carrier.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(carrier.Faction))
            {
                if (downedPawn.Downed)
                {
                    if (carrier.CanReserveAndReach(downedPawn, PathEndMode.OnCell, Danger.Some))
                    {
                        float distance = IntVec3Utility.DistanceTo(carrier.Position, downedPawn.Position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestDownedPawn = downedPawn;
                        }
                    }
                }
            }
            return nearestDownedPawn;
        }
    }
}
