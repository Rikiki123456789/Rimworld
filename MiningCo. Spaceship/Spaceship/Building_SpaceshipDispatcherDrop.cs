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
    public class Building_SpaceshipDispatcherDrop : Building_SpaceshipDispatcher, IThingHolder
    {
        public bool teamIsDropped = false;
        public int teamDropTick = 0;

        public override bool takeOffRequestIsEnabled
        {
            get
            {
                return false;
            }
        }

        // ===================== Setup work =====================
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (respawningAfterLoad == false)
            {
                this.pawnsAboard.AddRange(Expedition.GenerateExpeditionPawns(this.Map));
            }
        }

        public void InitializeData_DispatcherDrop(Faction faction, int hitPoints, int landingDuration, SpaceshipKind spaceshipKind)
        {
            base.InitializeData(faction, hitPoints, landingDuration, spaceshipKind);
            this.teamDropTick = Find.TickManager.TicksGame + 5 * GenTicks.TicksPerRealSecond;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.teamIsDropped, "teamIsDropped");
            Scribe_Values.Look<int>(ref this.teamDropTick, "teamDropTick");
        }

        // ===================== Main function =====================
        public override void Tick()
        {
            base.Tick();

            if ((this.teamIsDropped == false)
                && (Find.TickManager.TicksGame >= this.teamDropTick)
                && Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer) == false)
            {
                DropTeam();
                this.teamIsDropped = true;
                this.takeOffTick = Find.TickManager.TicksGame + 10 * GenTicks.TicksPerRealSecond;
            }
        }

        public void DropTeam()
        {
            // Find exit spot.
            IntVec3 exitSpot = IntVec3.Invalid;
            bool exitSpotIsValid = Expedition.TryFindRandomExitSpot(this.Map, this.Position, out exitSpot);
            if (exitSpotIsValid)
            {
                // Spawn expedition pawns.
                List<Pawn> stayingAboardPawns = new List<Pawn>();
                List<Pawn> droppedPawns = new List<Pawn>();
                foreach (Pawn pawn in this.pawnsAboard)
                {
                    if (pawn.kindDef == Util_PawnKindDefOf.Pilot)
                    {
                        stayingAboardPawns.Add(pawn);
                    }
                    else
                    {
                        droppedPawns.Add(pawn);
                        GenSpawn.Spawn(pawn, this.Position + IntVec3Utility.RandomHorizontalOffset(3f), this.Map);
                    }
                }
                this.pawnsAboard = stayingAboardPawns;
                // Make lord.
                LordMaker.MakeNewLord(Util_Faction.MiningCoFaction, new LordJob_ExitMap(exitSpot), this.Map, droppedPawns);
                // Spawn payment.
                SpawnPayment(droppedPawns.Count);
                Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, droppedPawns.Count);
            }
        }
    }
}
