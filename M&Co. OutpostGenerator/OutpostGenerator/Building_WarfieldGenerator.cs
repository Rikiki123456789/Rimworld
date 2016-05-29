using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Building_WarfieldGenerator class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_WarfieldGenerator : Building
    {
        public int battleZoneAbs;
        public int battleZoneOrd;
        public OG_OutpostData outpostData;

        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame == 1)
            {
                GenerateWarfield(this.battleZoneAbs, this.battleZoneOrd, this.outpostData);
                this.Destroy(DestroyMode.Vanish);
            }
        }

        public void GenerateWarfield(int battleZoneAbs, int battleZoneOrd, OG_OutpostData outpostData)
        {
            // Get a random hostile faction.
            int securityForcesCorpseNumber = Rand.Range(2, 4);
            int hostilesCorpseNumber = 0;
            FactionDef hostileFactionDef = null;
            Faction hostileFaction = null;
            float hostileFactionSelector = Rand.Value;
            if (hostileFactionSelector < 0.25f)
            {
                hostileFactionDef = FactionDefOf.Tribe;
                hostileFaction = Find.FactionManager.FirstFactionOfDef(hostileFactionDef);
                hostilesCorpseNumber = Rand.Range(6, 8);
            }
            else if (hostileFactionSelector < 0.5f)
            {
                hostileFactionDef = FactionDefOf.Pirate;
                hostileFaction = Find.FactionManager.FirstFactionOfDef(hostileFactionDef);
                hostilesCorpseNumber = Rand.Range(3, 5);
            }
            else if (hostileFactionSelector < 0.75f)
            {
                hostileFactionDef = FactionDefOf.SpacerHostile;
                hostileFaction = Find.FactionManager.FirstFactionOfDef(hostileFactionDef);
                hostilesCorpseNumber = Rand.Range(3, 5);
            }
            else
            {
                hostileFactionDef = FactionDefOf.Mechanoid;
                hostileFaction = Find.FactionManager.FirstFactionOfDef(hostileFactionDef);
                hostilesCorpseNumber = Rand.Range(1, 3);
            }
            // Spawn corpses.
            IntVec3 zoneOrigin = Zone.GetZoneOrigin(outpostData.areaSouthWestOrigin, battleZoneAbs, battleZoneOrd);
            for (int corpseIndex = 0; corpseIndex < securityForcesCorpseNumber + hostilesCorpseNumber; corpseIndex++)
            {
                int tries = 3; // Max 3 tries per corpse.
                bool validPositionIsFound = false;
                IntVec3 corpsePosition = new IntVec3();
                for (int tryIndex = 0; tryIndex < tries; tryIndex++)
                {
                    corpsePosition = zoneOrigin + new IntVec3(Rand.Range(1, Genstep_GenerateOutpost.zoneSideSize - 1), 0, Rand.Range(1, Genstep_GenerateOutpost.zoneSideSize - 1));
                    if (corpsePosition.GetEdifice() == null)
                    {
                        validPositionIsFound = true;
                        break;
                    }
                }
                if (validPositionIsFound == false)
                {
                    continue;
                }
                // Generate the corpse according to the faction.
                Pawn corpse = null;
                if (corpseIndex < securityForcesCorpseNumber)
                {
                    corpse = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, OG_Util.FactionOfMAndCo);
                }
                else
                {
                    float pawnKindSelector = Rand.Value;
                    PawnKindDef corpseKindDef = null;
                    if (hostileFactionDef == FactionDefOf.Tribe)
                    {
                        if (pawnKindSelector < 0.4f)
                            corpseKindDef = PawnKindDef.Named("TribalWarrior");
                        else if (pawnKindSelector < 0.8f)
                            corpseKindDef = PawnKindDef.Named("TribalArcher");
                        else
                            corpseKindDef = PawnKindDef.Named("TribalChief");
                    }
                    else if (hostileFactionDef == FactionDefOf.Pirate)
                    {
                        if (pawnKindSelector < 0.25f)
                            corpseKindDef = PawnKindDef.Named("Drifter");
                        else if (pawnKindSelector < 0.50f)
                            corpseKindDef = PawnKindDef.Named("Scavenger");
                        else if (pawnKindSelector < 0.75f)
                            corpseKindDef = PawnKindDef.Named("Thrasher");
                        else
                            corpseKindDef = PawnKindDef.Named("Pirate");
                    }
                    else if (hostileFactionDef == FactionDefOf.SpacerHostile)
                    {
                        if (pawnKindSelector < 0.25f)
                            corpseKindDef = PawnKindDef.Named("SpaceSoldier");
                        else if (pawnKindSelector < 0.50f)
                            corpseKindDef = PawnKindDef.Named("MercenaryGunner");
                        else if (pawnKindSelector < 0.75f)
                            corpseKindDef = PawnKindDef.Named("GrenadierDestructive");
                        else
                            corpseKindDef = PawnKindDef.Named("MercenaryElite");
                    }
                    else if (hostileFactionDef == FactionDefOf.Mechanoid)
                    {
                        if (pawnKindSelector < 0.6f)
                            corpseKindDef = PawnKindDef.Named("Scyther");
                        else
                            corpseKindDef = PawnKindDef.Named("Centipede");
                    }
                    corpse = PawnGenerator.GeneratePawn(corpseKindDef, hostileFaction);
                }
                GenSpawn.Spawn(corpse, corpsePosition);
                // Damage the weapon so the warfield effect is not too exploitable (otherwise, player can get good guns at game start).
                if (corpse.equipment.Primary != null)
                {
                    corpse.equipment.Primary.HitPoints = (int)(Rand.Range(0.05f, 0.30f) * corpse.equipment.Primary.MaxHitPoints);
                }
                // "Kill the corpse".
                HealthUtility.GiveInjuriesToKill(corpse);
                // Make it rotten if outpost is abandonned.
                if (this.outpostData.isInhabited == false)
                {
                    List<Thing> thingsList = corpsePosition.GetThingList();
                    foreach (Thing thing in thingsList)
                    {
                        if (thing.def.defName.Contains("Corpse"))
                        {
                            CompRottable rotComp = thing.TryGetComp<CompRottable>();
                            if (rotComp != null)
                            {
                                rotComp.rotProgress = 20f * GenDate.TicksPerDay; // 20 days so the corpse is dessicated.
                            }
                        }
                    }
                }
            }

            // Destroy some sandbags in the zone.
            List<Thing> sandbagsList = Find.ListerThings.ThingsOfDef(ThingDefOf.Sandbags);
            for (int sandbagIndex = sandbagsList.Count - 1; sandbagIndex >= 0; sandbagIndex--)
            {
                Thing sandbag = sandbagsList[sandbagIndex];
                if (sandbag.Position.InHorDistOf(this.Position, Genstep_GenerateOutpost.zoneSideSize / 2f)
                    && (Rand.Value < 0.1f))
                {
                    // Manually spawn sandbag rubble and use Vanish instead of Kill to avoid spawning ugly metal remains.
                    GenSpawn.Spawn(ThingDef.Named("SandbagRubble"), sandbag.Position);
                    sandbag.Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}
