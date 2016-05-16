using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;

namespace OutpostGenerator
{
    internal static class DetoursOG
    {
        private static readonly NeedDef defComfort = DefDatabase<NeedDef>.GetNamed("Comfort");

        public static void InjectDetours()
        {
            Log.Message("InjectDetours");
            Detours.TryDetourFromTo(
                typeof (Pawn_NeedsTracker).GetMethod("ShouldHaveNeed", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof (DetoursOG).GetMethod("ShouldHaveNeed", BindingFlags.Static | BindingFlags.Public));
        }
        
        // Pawn_NeedsTracker
        public static bool ShouldHaveNeed(this Pawn_NeedsTracker _this, NeedDef nd)
        {
            Log.Message("Detoured ShouldHaveNeed");
            Pawn pawn = (Pawn)typeof(Pawn_NeedsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_this);
            if (pawn.RaceProps.intelligence < nd.minIntelligence)
            {
                return false;
            }
            if (nd == NeedDefOf.Food)
            {
                return pawn.RaceProps.EatsFood;
            }
            if (nd == NeedDefOf.Rest)
            {
                return pawn.RaceProps.needsRest;
            }
            return ((nd == NeedDefOf.Joy || nd == DetoursOG.defComfort) && (pawn.Faction != null && pawn.Faction == OG_Util.FactionOfMAndCo))
                || ((nd != NeedDefOf.Joy || pawn.HostFaction == null) && (!nd.colonistAndPrisonersOnly || (pawn.Faction != null && pawn.Faction.def == FactionDefOf.Colony) || (pawn.HostFaction != null && pawn.HostFaction == Faction.OfColony)));
        }
    }
}