using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Building_SupplyShip class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_SupplyShip : Building
    {
        private const int maxTicksToTakeOff = 2500;
        private int ticksToTakeOff = maxTicksToTakeOff;
        private Thing cryptosleepBay1 = null;
        private Thing cryptosleepBay2 = null;
        private Thing cargoBay1 = null;
        private Thing cargoBay2 = null;
        
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            // Spawn cryptosleep bays.
            if (this.cryptosleepBay1 == null)
            {
                cryptosleepBay1 = ThingMaker.MakeThing(OG_Util.SupplyShipCryptosleepBayLeftDef);
                cryptosleepBay1.SetFactionDirect(this.Faction);
                GenSpawn.Spawn(cryptosleepBay1, this.Position + new IntVec3(-4, 0, -2).RotatedBy(this.Rotation), this.Rotation);
            }
            if (this.cryptosleepBay2 == null)
            {
                cryptosleepBay2 = ThingMaker.MakeThing(OG_Util.SupplyShipCryptosleepBayRightDef);
                cryptosleepBay2.SetFactionDirect(this.Faction);
                GenSpawn.Spawn(cryptosleepBay2, this.Position + new IntVec3(3, 0, -2).RotatedBy(this.Rotation), this.Rotation);
            }
            // Spawn cargo bays.
            if (this.cargoBay1 == null)
            {
                cargoBay1 = ThingMaker.MakeThing(OG_Util.SupplyShipCargoBayLeftDef);
                cargoBay1.SetFactionDirect(this.Faction);
                GenSpawn.Spawn(cargoBay1, this.Position + new IntVec3(-4, 0, 1).RotatedBy(this.Rotation), this.Rotation);
            }
            if (this.cargoBay2 == null)
            {
                cargoBay2 = ThingMaker.MakeThing(OG_Util.SupplyShipCargoBayRightDef);
                cargoBay2.SetFactionDirect(this.Faction);
                GenSpawn.Spawn(cargoBay2, this.Position + new IntVec3(4, 0, 1).RotatedBy(this.Rotation), this.Rotation);
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            if (this.ticksToTakeOff == maxTicksToTakeOff)
            {
                // Only spawn reinforcement pawns, packaged meals and beer once.
                // TODO.
                /*DropPodInfo info = new DropPodInfo();
                Thing meals = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack);
                meals.stackCount = Find.ListerPawns.PawnsInFaction(OG_Util.FactionOfMAndCo).Count;
                meals.SetForbidden(true);
                info.SingleContainedThing = meals;
                DropPodUtility.MakeDropPodAt(this.dropZoneCenter + new IntVec3(Rand.RangeInclusive(-4, 4), 0, Rand.RangeInclusive(-4, 4)), info);*/

                Building_OrbitalRelay orbitalRelay = OG_Util.FindOrbitalRelay(this.Faction);
                if (orbitalRelay != null)
                {
                    List<PawnKindDef> reinforcementRequestsList;
                    orbitalRelay.GetAndClearReinforcementRequestsList(out reinforcementRequestsList);
                    foreach (PawnKindDef pawnType in reinforcementRequestsList)
                    {
                        // TODO: reuse OG_Inhabitants pawn generation algo.
                        Pawn pawn = PawnGenerator.GeneratePawn(pawnType, this.Faction);
                        GenSpawn.Spawn(pawn, this.Position);
                    }
                }
            }

            this.ticksToTakeOff--;
            if (this.ticksToTakeOff <= 0)
            {
                // TODO: supply ship taking off
                /*Thing cryptosleepBay = ThingMaker.MakeThing(OG_Util.SupplyShipCryptosleepBayDef);
                cryptosleepBay.SetFactionDirect(this.Faction);
                cryptosleepBay.Rotation = this.landingPadRotation;
                GenSpawn.Spawn(cryptosleepBay, this.landingPadPosition);*/
                this.Destroy();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (this.cryptosleepBay1 != null)
            {
                this.cryptosleepBay1.Destroy();
            }
            if (this.cryptosleepBay2 != null)
            {
                this.cryptosleepBay2.Destroy();
            }
            if (this.cargoBay1 != null)
            {
                this.cargoBay1.Destroy();
            }
            if (this.cargoBay2 != null)
            {
                this.cargoBay2.Destroy();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ticksToTakeOff, "ticksToTakeOff");
            Scribe_References.LookReference<Thing>(ref this.cryptosleepBay1, "cryptosleepBay1");
            Scribe_References.LookReference<Thing>(ref this.cryptosleepBay2, "cryptosleepBay2");
            Scribe_References.LookReference<Thing>(ref this.cargoBay1, "cargoBay1");
            Scribe_References.LookReference<Thing>(ref this.cargoBay2, "cargoBay2");
        }
    }
}
