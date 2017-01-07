using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace CaveworldFlora
{
    /// <summary>
    /// ClusterPlant_Gleamcap class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class ClusterPlant_BlackLotus : ClusterPlant
    {
        public const float poisonRadius = 7f;
        public const float minGrowthToPoison = 0.3f;

        public int nextLongTick = GenTicks.TickLongInterval;
        public static bool alertHasBeenSent = false;

        // ===================== Saving =====================
        /// <summary>
        /// Save and load internal state variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.nextLongTick, "nextLongTick");
            Scribe_Values.LookValue<bool>(ref ClusterPlant_BlackLotus.alertHasBeenSent, "alertHasBeenSent");
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// Main function:
        /// - 
        /// </summary>
        public override void TickRare()
        {
            if ((this.Growth >= minGrowthToPoison)
                && (this.Dying == false)
                && (this.isInCryostasis == false))
            {
                // Spawn toxic gas.
                ThrowPoisonSmoke();

                // Poison nearby pawns.
                List<Pawn> allPawnsSpawned = this.Map.mapPawns.AllPawnsSpawned;
				for (int pawnIndex = 0; pawnIndex < allPawnsSpawned.Count; pawnIndex++)
				{
					Pawn pawn = allPawnsSpawned[pawnIndex];
                    if (pawn.Position.InHorDistOf(this.Position, poisonRadius))
                    {
                        float num = 0.01f;
                        num *= pawn.GetStatValue(StatDefOf.ToxicSensitivity, true);
                        if (num != 0f)
                        {
                            Rand.PushSeed();
                            Rand.Seed = pawn.thingIDNumber * 74374237;
                            float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.Value);
                            Rand.PopSeed();
                            num *= num2;
                            HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, num);
                            if ((ClusterPlant_BlackLotus.alertHasBeenSent == false)
                                && pawn.IsColonist)
                            {
                                Find.LetterStack.ReceiveLetter("Black lotus", "One of your colonists has been intoxited by the effluvium of a black lotus. Beware, those emanations are extremely toxic.",
                                    LetterType.BadNonUrgent, new RimWorld.Planet.GlobalTargetInfo(pawn));
                                ClusterPlant_BlackLotus.alertHasBeenSent = true;
                            }
                        }
                    }
                }
            }            

            if (Find.TickManager.TicksGame >= this.nextLongTick)
            {
                this.nextLongTick = Find.TickManager.TicksGame + GenTicks.TickLongInterval;
                base.TickLong();
            }
            
        }

        public void ThrowPoisonSmoke()
        {
            Vector3 spawnPosition = this.Position.ToVector3Shifted() + Vector3Utility.RandomHorizontalOffset(3f);

            if (!spawnPosition.ShouldSpawnMotesAt(this.Map) || this.Map.moteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = ThingMaker.MakeThing(Util_CaveworldFlora.MotePoisonSmokeDef, null) as MoteThrown;
            moteThrown.Scale = 3f * this.Growth;
            moteThrown.rotationRate = (float)Rand.Range(-5, 5);
            moteThrown.exactPosition = spawnPosition;
            moteThrown.SetVelocity((float)Rand.Range(-20, 20), 0);
            GenSpawn.Spawn(moteThrown, spawnPosition.ToIntVec3(), this.Map);
        }
    }
}
