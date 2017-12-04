using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;    // Always needed
using RimWorld;       // RimWorld specific functions are found here
using Verse;          // RimWorld universal objects are here
using Verse.AI;       // Needed when you do something with the AI
using Verse.AI.Group; // Needed when you do something with the group AI
using Verse.Sound;    // Needed when you do something with the Sound

namespace Spaceship
{
    public class Building_SpaceshipDispatcherPick : Building_SpaceshipDispatcher, IThingHolder
    {
        public override bool takeOffRequestIsEnabled
        {
            get
            {
                return true;
            }
        }

        // ===================== Setup work =====================
        public void InitializeData_DispatcherPick(Faction faction, int hitPoints, int landingDuration, SpaceshipKind spaceshipKind)
        {
            base.InitializeData(faction, hitPoints, landingDuration, spaceshipKind);
        }
        
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode != DestroyMode.KillFinalize)
            {
                // Spawn payments according to number of picked pawns.
                if (this.pawnsAboard.Count > pilotsNumber)
                {
                    int pickedPawnNumber = this.pawnsAboard.Count - pilotsNumber;
                    SpawnPayment(pickedPawnNumber);
                    Util_Faction.AffectFactionGoodwillWithOther(Util_Faction.MiningCoFaction, Faction.OfPlayer, pickedPawnNumber);
                }
            }
            base.Destroy(mode);
        }

        // ===================== Other functions =====================
        public override void Notify_PawnBoarding(Pawn pawn, bool isLastLordPawn)
        {
            base.Notify_PawnBoarding(pawn, isLastLordPawn);
            if (isLastLordPawn)
            {
                this.takeOffTick = Find.TickManager.TicksGame + 10 * GenTicks.TicksPerRealSecond;
            }
        }
    }
}
