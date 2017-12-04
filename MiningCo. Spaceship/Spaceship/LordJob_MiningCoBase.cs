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
using Verse.AI.Group;

namespace Spaceship
{
    public abstract class LordJob_MiningCoBase : LordJob
    {
        public const int pawnExitedGoodwillImpact = 1;
        public const int pawnLostGoodwillImpact = -3;

        public IntVec3 targetDestination;

        public LordJob_MiningCoBase()
        {
        }

        public LordJob_MiningCoBase(IntVec3 targetDestination)
		{
            this.targetDestination = targetDestination;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntVec3>(ref this.targetDestination, "targetDestination");
        }

        public override StateGraph CreateGraph()
        {
            return null;
        }

        public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
        {
            base.Notify_PawnLost(p, condition);
            if (condition == PawnLostCondition.IncappedOrKilled)
            {
                Util_Faction.AffectFactionGoodwillWithOther(Util_Faction.MiningCoFaction, Faction.OfPlayer, pawnLostGoodwillImpact);
                Messages.Message("A MiningCo. employee has been injured or killed in your area. MiningCo. goodwill toward you decreased: " + pawnLostGoodwillImpact + ".", new TargetInfo(p.Position, this.Map), MessageTypeDefOf.NegativeHealthEvent);
            }
            else if (condition == PawnLostCondition.ExitedMap)
            {
                Util_Faction.AffectFactionGoodwillWithOther(Util_Faction.MiningCoFaction, Faction.OfPlayer, pawnExitedGoodwillImpact);
            }
        }
    }
}