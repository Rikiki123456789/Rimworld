using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
//using RimWorld.Planet;
using RimWorld.SquadAI;


namespace MechanoidTerraformer
{
    /// <summary>
    /// Order a pawn to go and secure the mechanoid terraformer.
    /// </summary>
    public class JobDriver_DisableBeacon : JobDriver
    {
        public const int chanceToSucceedPerResearchLevel = 6;

        public TargetIndex terraformerIndex = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(terraformerIndex);

            yield return Toils_Goto.GotoCell(terraformerIndex, PathEndMode.InteractionCell).FailOnDestroyed(terraformerIndex);

            yield return Toils_General.Wait(1500).FailOnDestroyed(terraformerIndex);

            Toil beaconDisablingResultToil = new Toil()
            {
                initAction = () =>
                {
                    string eventTitle = "";
                    string eventText = "";
                    float raidPointsFactor = 1f;
                    int dropsNumber = 0;
                    LetterType letterType = LetterType.BadUrgent;

                    Building_MechanoidTerraformer terraformer = this.TargetThingA as Building_MechanoidTerraformer;
                    terraformer.invasionIsDone = true;

                    string sheHeOrIt = "it";
                    string herHimOrIt = "it";
                    string herHisOrIts = "its";
                    if (pawn.gender == Gender.Female)
                    {
                        sheHeOrIt = "she";
                        herHisOrIts = "her";
                        herHimOrIt = "her";
                    }
                    else if (pawn.gender == Gender.Male)
                    {
                        sheHeOrIt = "he";
                        herHisOrIts = "his";
                        herHimOrIt = "him";
                    }

                    if ((this.pawn.skills.GetSkill(SkillDefOf.Research).TotallyDisabled == true)
                        || (this.pawn.skills.GetSkill(SkillDefOf.Research).level < 3))
                    {
                        eventTitle = "Invasion";
                        eventText = "   " + this.pawn.Name.ToStringShort + " has tried to disable the terraformer beacon but technology is not " + herHisOrIts + " big passion... "
                        + sheHeOrIt.CapitalizeFirst() + " just pressed on every button alerting by the way every nearby mechanoid shuttles.\n\n"
                        + "Be prepared to welcome some nasty and numerous visitors from nearby mechanoid hives!";

                        raidPointsFactor = 1.4f;
                        dropsNumber = 5;
                        letterType = LetterType.BadUrgent;
                    }
                    else if (this.pawn.skills.GetSkill(SkillDefOf.Research).level == 20)
                    {
                        eventTitle = "Beacon disabled";
                        eventText = "   " + this.pawn.Name.ToStringShort + " is a real crack in alien technology. Disabling the terraformer beacon was just another game for " + herHimOrIt + "\n\n"
                            + "You have nothing to fear from it anymore.";

                        raidPointsFactor = 0f;
                        dropsNumber = 0;
                        letterType = LetterType.Good;
                    }
                    else
                    {
                        float rand = Rand.Value * 100;
                        if (rand < this.pawn.skills.GetSkill(SkillDefOf.Research).level * chanceToSucceedPerResearchLevel)
                        {
                            // Disable sucessfull.
                            eventTitle = "Beacon disabled";
                            eventText = "   Even if " + this.pawn.Name.ToStringShort + " is not the best about alien technology, " + sheHeOrIt + " successfully disabled the terraformer beacon!\n\n"
                                + "You have nothing to fear from it anymore.";

                            raidPointsFactor = 0f;
                            dropsNumber = 0;
                            letterType = LetterType.Good;
                        }
                        else
                        {
                            // Bad luck.
                            eventTitle = "Invasion";
                            eventText = "   " + this.pawn.Name.ToStringShort + " has some knowledge about alien technology but " + sheHeOrIt + " still failed at properly disabling the terraformer beacon.\n\n"
                                + "Be prepared to welcome the incoming terrafomer defending force.";

                            raidPointsFactor = 0.4f;
                            dropsNumber = 2;
                            letterType = LetterType.BadUrgent;
                        }
                    }
                    terraformer.LaunchInvasion(eventTitle, eventText, raidPointsFactor, dropsNumber, letterType);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return beaconDisablingResultToil;

            yield return Toils_Reserve.Release(terraformerIndex);
        }
    }
}
