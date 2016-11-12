using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace CaveBiome
{
    /// <summary>
    /// Building_VillagerCorpsesGenerator class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_VillagerCorpsesGenerator : Building
    {
        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame == 1)
            {
                GenerateVillagerCorpses();
                this.Destroy();
            }
        }

        public void GenerateVillagerCorpses()
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Tribe);
            SpawnPawnCorpse(this.Position, PawnKindDef.Named("TribalChief"), faction, GenDate.TicksPerDay, true);
            SpawnPawnCorpse(this.Position + new IntVec3(2, 0, 2), PawnKindDef.Named("TribalWarrior"), faction, GenDate.TicksPerDay, true);
            SpawnPawnCorpse(this.Position + new IntVec3(2, 0, -2), PawnKindDef.Named("TribalWarrior"), faction, GenDate.TicksPerDay, true);
            SpawnPawnCorpse(this.Position + new IntVec3(-2, 0, -2), PawnKindDef.Named("TribalWarrior"), faction, GenDate.TicksPerDay, true);
            SpawnPawnCorpse(this.Position + new IntVec3(-2, 0, 2), PawnKindDef.Named("TribalWarrior"), faction, GenDate.TicksPerDay, true);
        }

        public static void SpawnPawnCorpse(IntVec3 spawnCell, PawnKindDef pawnKindDef, Faction faction, float rotProgressInTicks, bool removeEquipment = false)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
            GenSpawn.Spawn(pawn, spawnCell);
            if (removeEquipment)
            {
                pawn.equipment.DestroyAllEquipment();
                pawn.inventory.DestroyAll();
            }
            KillAndRotPawn(pawn, rotProgressInTicks);
        }

        public static void KillAndRotPawn(Pawn pawn, float rotProgressInTicks)
        {
            HealthUtility.GiveInjuriesToKill(pawn);
            foreach (Thing thing in pawn.Position.GetThingList())
            {
                if (thing.def.defName.Contains("Corpse"))
                {
                    CompRottable rotComp = thing.TryGetComp<CompRottable>();
                    if (rotComp != null)
                    {
                        rotComp.RotProgress = rotProgressInTicks;
                    }
                }
            }
        }
    }
}
