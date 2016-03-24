using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace MechanoidTerraformer
{
    /// <summary>
    /// MechanoidTerraformerIncoming class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class MechanoidTerraformerIncoming : Thing
    {
        public const int diagonalTrajectoryDurationInTicks = 240;
        public const int verticalTrajectoryDurationInTicks = 240;
        public int ticksToImpact = diagonalTrajectoryDurationInTicks + verticalTrajectoryDurationInTicks;

        // Thrust effect.
        public static readonly Material shadowTexture = MaterialPool.MatFrom("Things/Special/DropPodShadow", ShaderDatabase.Transparent);

        // Sound.
        public static readonly SoundDef preLandingSound = SoundDef.Named("DropPodFall");
        public static readonly SoundDef landingSound = SoundDef.Named("MechanoidTerraformerLanding");
        public const int soundAnticipationTicks = 60;

        public override Vector3 DrawPos
        {
            get
            {
                Vector3 result = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem);
                if (this.ticksToImpact > verticalTrajectoryDurationInTicks)
                {
                    float coefficient = (float)(this.ticksToImpact - verticalTrajectoryDurationInTicks);
                    float num = coefficient * coefficient * 0.001f;
                    result.x -= num * 0.4f;
                    result.z += num * 0.6f + 3f;
                }
                else
                {
                    result.z += ((float)this.ticksToImpact / verticalTrajectoryDurationInTicks) * 3f;
                }
                return result;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            if (Find.RoofGrid.Roofed(base.Position))
            {
                Log.Warning("Mechanoid terraformer dropped on roof at " + base.Position);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
        }

        public override void Tick()
        {
            this.ticksToImpact--;
            if (this.ticksToImpact <= 0)
            {
                bool pawnOrBuildingHasBeenCrushed = false;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (IntVec3 landingCell in GenAdj.CellsOccupiedBy(this))
                {
                    foreach (Thing thing in Find.ThingGrid.ThingsAt(landingCell))
                    {
                        if ((thing is Mote) || (thing.Faction == null))
                        {
                            continue;
                        }
                        if ((thing != this) && (thing.Faction == Faction.OfColony))
                        {
                            if (((thing is Building)
                                || (thing is Pawn))
                                && (pawnOrBuildingHasBeenCrushed == false))
                            {
                                pawnOrBuildingHasBeenCrushed = true;
                                stringBuilder.AppendLine("This strange building has crushed anything under it upon landing.");
                                stringBuilder.AppendLine();
                                stringBuilder.AppendLine("The following things have been crushed:");
                                stringBuilder.AppendLine();
                            }
                            if (thing is Pawn)
                            {
                                Pawn crushedPawn = thing as Pawn;
                                string herHimOrIt = "it";
                                string sheHeOrIt = "it";
                                if (crushedPawn.gender == Gender.Female)
                                {
                                    herHimOrIt = "her";
                                    sheHeOrIt = "she";
                                }
                                else if (crushedPawn.gender == Gender.Male)
                                {
                                    herHimOrIt = "him";
                                    sheHeOrIt = "he";
                                }
                                stringBuilder.AppendLine("- Poor " + crushedPawn.Name + "'. Don't bother looking for " + herHimOrIt + ", " + sheHeOrIt + " is already six feet under. RIP.");

                            }
                            else if (thing is Building)
                            {
                                Building crushedBuilding = thing as Building;
                                stringBuilder.AppendLine("- " + crushedBuilding.Label);
                            }
                            thing.Destroy(DestroyMode.Vanish);
                        }
                    }
                }
                if (pawnOrBuildingHasBeenCrushed)
                {
                    Find.LetterStack.ReceiveLetter("Hurting landing", stringBuilder.ToString(), LetterType.BadNonUrgent, this.Position);
                }

                Thing mechanoidTerraformer = ThingMaker.MakeThing(Util_MechanoidTerraformer.MechanoidTerraformerDef);
                mechanoidTerraformer.SetFactionDirect(Faction.OfMechanoids);
                GenSpawn.Spawn(mechanoidTerraformer, this.Position);
                this.Destroy();
            }
            else if (this.ticksToImpact <= verticalTrajectoryDurationInTicks)
            {
                for (int dustMoteIndex = 0; dustMoteIndex < 2; dustMoteIndex++)
                {
                    Vector3 dustMotePosition = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead) + Gen.RandomHorizontalVector(4f);
                    MoteThrower.ThrowDustPuff(dustMotePosition, 1.2f);
                }
            }
            if (this.ticksToImpact == soundAnticipationTicks + verticalTrajectoryDurationInTicks)
            {
                MechanoidTerraformerIncoming.preLandingSound.PlayOneShot(base.Position);
            }
            if (this.ticksToImpact == verticalTrajectoryDurationInTicks)
            {
                MechanoidTerraformerIncoming.landingSound.PlayOneShot(base.Position);
            }
        }

        public override void DrawAt(Vector3 drawLoc)
        {
            base.DrawAt(drawLoc);
            float num = 5f + (float)this.ticksToImpact / 100f;
            Vector3 scale = new Vector3(num, 1f, num);
            Matrix4x4 matrix = default(Matrix4x4);
            drawLoc.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
            matrix.SetTRS(this.TrueCenter(), base.Rotation.AsQuat, scale);
            Graphics.DrawMesh(MeshPool.plane10Back, matrix, MechanoidTerraformerIncoming.shadowTexture, 0);
        }
    }
}
