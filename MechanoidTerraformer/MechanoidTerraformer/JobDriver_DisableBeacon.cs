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

            yield return Toils_Goto.GotoCell(terraformerIndex, PathEndMode.InteractionCell).FailOnDestroyedOrNull(terraformerIndex);

            yield return Toils_General.Wait(1500).FailOnDestroyedOrNull(terraformerIndex);

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

                    string sheHeOrIt = "it".Translate();
                    string herHimOrIt = "it".Translate();
                    string herHisOrIts = "its".Translate();
                    if (pawn.gender == Gender.Female)
                    {
                        sheHeOrIt = "she".Translate();
                        herHisOrIts = "her".Translate();
                        herHimOrIt = "her".Translate();
                    }
                    else if (pawn.gender == Gender.Male)
                    {
                        sheHeOrIt = "he".Translate();
                        herHisOrIts = "his".Translate();
                        herHimOrIt = "him".Translate();
                    }

                    if ((this.pawn.skills.GetSkill(SkillDefOf.Research).TotallyDisabled == true)
                        || (this.pawn.skills.GetSkill(SkillDefOf.Research).level < 3))
                    {
                        eventTitle = "Invasion".Translate();
                        eventText = string.Concat(new string[]
                        {
                            "   ",
                            this.pawn.Name.ToStringShort,
                            "try_to_diable".Translate(),
                            herHisOrIts,
                            "bigpassion".Translate(),
                            sheHeOrIt.CapitalizeFirst(),
                            "button_press".Translate()
                        });

                        raidPointsFactor = 1.4f;
                        dropsNumber = 5;
                        letterType = LetterType.BadUrgent;
                    }
                    else if (this.pawn.skills.GetSkill(SkillDefOf.Research).level == 20)
                    {
                        eventTitle = "Beacondisabled".Translate();


                        eventText = string.Concat(new string[]
                        {
                            "   ",
                            this.pawn.Name.ToStringShort,
                            "real_crack".Translate(),
                            herHimOrIt,
                            "no_fear".Translate()
                        });

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
                            eventTitle = "Beacondisabled".Translate();

                            eventText = string.Concat(new string[]
                            {
                                "Evenif".Translate(),
                                this.pawn.Name.ToStringShort,
                                "alien_about".Translate(),
                                sheHeOrIt,
                                "success_disabling".Translate()
                            });

                            raidPointsFactor = 0f;
                            dropsNumber = 0;
                            letterType = LetterType.Good;
                        }
                        else
                        {
                            // Bad luck.
                            eventTitle = "Invasion".Translate();
                            eventText = string.Concat(new string[]
                            {
                                "   ",
                                this.pawn.Name.ToStringShort,
                                "some_knowledge".Translate(),
                                sheHeOrIt,
                                "terr_beacon".Translate()
                            });
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
