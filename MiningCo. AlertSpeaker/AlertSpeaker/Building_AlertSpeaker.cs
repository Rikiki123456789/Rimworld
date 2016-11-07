using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace AlertSpeaker
{
    /// <summary>
    /// AlertSpeaker class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    class Building_AlertSpeaker : Building
    {
        // Used by Destroy function.
        public static int numberOfAlertSpeakers = 0;

        // Constants.
        public const float alertSpeakerMaxRange = 6.9f;
        public const int earlyPhaseTicksThreshold = 60000; // Maximum duration the adrenaline boost can last: 1 day.
        public const int latePhaseTicksThreshold = 120000; // A very prolonged alert may generate stress: 2 days.

        // Danger phase.
        public static int lastUpdateTick = 0;
        public static int alertStartTick = 0;
        public static StoryDanger previousDangerRate = StoryDanger.None;
        public static StoryDanger currentDangerRate = StoryDanger.None;

        // Drawing parameters.
        public static int lowDangerDrawingTick = 0;
        public static int highDangerDrawingTick = 0;
        public CompGlower glowerComp;
        public static float glowRadius = 0f;
        public static ColorInt glowColor = new ColorInt(0, 0, 0, 255);

        // Sound parameters.
        public static bool soundIsActivated = true;
        public static int alarmSoundTick = 0;
        public const int alarmSoundPeriod = 1200;  // 20 s.
        public static SoundDef lowDangerAlarmSound = SoundDef.Named("LowDangerAlarm");
        public static SoundDef highDangerAlarmSound = SoundDef.Named("HighDangerAlarm");

        // Other variables.
        public CompPowerTrader powerComp;

        // Icons texture.
        public Texture2D sirenSoundEnabledIcon;
        public Texture2D sirenSoundDisabledIcon;
                
        // ===================== Setup Work =====================
        /// <summary>
        /// Initializes instance variables.
        /// </summary>
        /// 
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            numberOfAlertSpeakers++;
            glowerComp = base.GetComp<CompGlower>();
            powerComp = base.GetComp<CompPowerTrader>();

            sirenSoundEnabledIcon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_SirenSoundEnabled");
            sirenSoundDisabledIcon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_SirenSoundDisabled");
        }

        /// <summary>
        /// Saves and loads variables (stored in savegame data).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref lastUpdateTick, "lastUpdateTick", 0);
            Scribe_Values.LookValue<int>(ref alertStartTick, "alertStartTick", 0);
            Scribe_Values.LookValue<StoryDanger>(ref previousDangerRate, "previousDangerRate", StoryDanger.None);
            Scribe_Values.LookValue<StoryDanger>(ref currentDangerRate, "currentDangerRate", StoryDanger.None);
            Scribe_Values.LookValue<int>(ref lowDangerDrawingTick, "lowDangerDrawingTick", 0);
            Scribe_Values.LookValue<int>(ref highDangerDrawingTick, "highDangerDrawingTick", 0);
            Scribe_Values.LookValue<int>(ref alarmSoundTick, "alarmSoundTick", 0);
        }
        
        // ===================== Destroy =====================
        /// <summary>
        /// Destroys the alert speaker and remove any bonus/malus i fthere are no more on the map.
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy();

            if (mode == DestroyMode.Deconstruct)
            {
                List<ThingCount> costList = this.def.costList;
                foreach (ThingCount cost in costList)
                {
                    Thing deconstructedResource = ThingMaker.MakeThing(cost.thingDef);
                    deconstructedResource.stackCount = cost.count;
                    GenSpawn.Spawn(deconstructedResource, this.Position);
                }
            }

            numberOfAlertSpeakers--;
            if (numberOfAlertSpeakers == 0)
            {
                RemoveAnyStatBonusFromAllColonists();
                RemoveAnyStatMalusFromAllColonists();
                RemoveAnyThoughtBonusFromAllColonists();
                RemoveAnyThoughtMalusFromAllColonists();
            }
        }
        
        /// <summary>
        /// Checks if the wall supporting the alert speaker is still alive.
        /// </summary>
        public static bool CheckIfSupportingWallIsAlive(IntVec3 alertSpeakerPosition, Rot4 alertSpeakerRotation)
        {
            IntVec3 wallPosition = alertSpeakerPosition + new IntVec3(0, 0, -1).RotatedBy(alertSpeakerRotation);
            
            // Built wall.
            if (Find.ThingGrid.ThingAt(wallPosition, ThingDefOf.Wall) != null)
            {
                return true;
            }

            // Natural block.
            Thing potentialWall = Find.ThingGrid.ThingAt(wallPosition, ThingCategory.Building);
            if (potentialWall != null)
            {
                if ((potentialWall as Building).def.building.isNaturalRock)
                {
                    return true;
                }
            }
            // No wall.
            return false;
        }
        
        /// <summary>
        /// Get the effect zone cells.
        /// </summary>
        public static List<IntVec3> GetEffectZoneCells(IntVec3 alertSpeakerPosition)
        {
            IEnumerable<IntVec3> cellsInRange = GenRadial.RadialCellsAround(alertSpeakerPosition, Building_AlertSpeaker.alertSpeakerMaxRange, true);
            List<IntVec3> effectZoneCells = new List<IntVec3>();
            foreach (IntVec3 cell in cellsInRange)
            {
                if (cell.GetRoom() == alertSpeakerPosition.GetRoom())
                {
                    effectZoneCells.Add(cell);
                }
            }
            return effectZoneCells;
        }

        // ===================== Main Work Function =====================
        /// <summary>
        /// - Checks if the supporting wall is still alive.
        /// - Checks current threat level.
        /// - Performs adequate treatment when a danger level transition occurs.
        /// - Applies an adrenaline bonus to nearby colonists according to current danger rate.
        /// </summary>
        public override void Tick()
        {
            if (CheckIfSupportingWallIsAlive(this.Position, this.Rotation) == false)
            {
                this.Destroy(DestroyMode.Deconstruct);
            }
            base.Tick();

            int tickCounter = Find.TickManager.TicksGame;
            if (lastUpdateTick != tickCounter)
            {
                // The following treatment is performed only once per tick (static treatment).
                lastUpdateTick = tickCounter;
                if ((tickCounter % (2 * GenTicks.TicksPerRealSecond)) == 0)
                {
                    DisplayActiveMote();
                }

                if ((tickCounter % GenTicks.TicksPerRealSecond) == 0)
                {
                    previousDangerRate = currentDangerRate;
                    currentDangerRate = Find.StoryWatcher.watcherDanger.DangerRating;
                    PerformTreatmentOnDangerRateTransition();
                }
                PerformSoundTreatment();
                ComputeDrawingParameters();
            }

            // Update for each speaker.
            if ((tickCounter % GenTicks.TicksPerRealSecond) == 0)
            {
                if (powerComp.PowerOn)
                {
                    PerformTreatmentDuringAlert();
                }
            }
            PerformDrawingTreatment();
        }
        
        /// <summary>
        /// Displays the active mote when applicable.   
        /// </summary>
        public void DisplayActiveMote()
        {
            foreach (Pawn colonist in Find.MapPawns.FreeColonists)
            {
                MoteAttached bonusMalusMote = null;
                if (CheckIfColonistHasASmallAdrenalineBoostBonus(colonist))
                {
                    bonusMalusMote = ThingMaker.MakeThing(Util_AlertSpeaker.SmallAdrenalineBoostMoteDef) as MoteAttached;
                    bonusMalusMote.AttachTo(colonist);
                    GenSpawn.Spawn(bonusMalusMote, colonist.Position);
                }
                else if (CheckIfColonistHasAMediumAdrenalineBoostBonus(colonist))
                {
                    bonusMalusMote = ThingMaker.MakeThing(Util_AlertSpeaker.MediumAdrenalineBoostMoteDef) as MoteAttached;
                    bonusMalusMote.AttachTo(colonist);
                    GenSpawn.Spawn(bonusMalusMote, colonist.Position);
                }
                else if (CheckIfColonistHasASmallStressMalus(colonist))
                {
                    bonusMalusMote = ThingMaker.MakeThing(Util_AlertSpeaker.SmallStressMoteDef) as MoteAttached;
                    bonusMalusMote.AttachTo(colonist);
                    GenSpawn.Spawn(bonusMalusMote, colonist.Position);
                }
            }
        }

        /// <summary>
        /// Check if a danger rate transition is occuring and perform the corresponding treatment.
        /// Transitions management:
        /// Transition 1: beginning alert.
        ///   => set the alert start tick.
        /// Transition 2: finished alert.
        ///   => remove any stat bonus/malus and malus thought.
        ///   => add a bonus thought.
        /// Transition 3: increased danger rating.
        ///   => convert the small adrenaline boost into medium adrenaline boost.
        /// Transition 4: decreased danger rating.
        ///   => keep the medium adrenaline boost (nothing to do).
        /// </summary>
        public void PerformTreatmentOnDangerRateTransition()
        {
            // Transition 1: beginning alert.
            if ((previousDangerRate == StoryDanger.None)
                && ((currentDangerRate == StoryDanger.Low)
                || (currentDangerRate == StoryDanger.High)))
            {
                // Set the alert start tick.
                alertStartTick = Find.TickManager.TicksGame;
                RemoveAnyThoughtBonusFromAllColonists();
                TryApplyColonyIsThreatenedThoughtToAllColonists();
                if (currentDangerRate == StoryDanger.Low)
                {
                    lowDangerDrawingTick = 0;
                    PlayOneLowDangerAlarmSound();
                    alarmSoundTick = 0;
                }
                else
                {
                    highDangerDrawingTick = 0;
                    PlayOneHighDangerAlarmSound();
                    alarmSoundTick = 0;
                }
            }
            // Transition 2: finished alert.
            else if (((previousDangerRate == StoryDanger.Low)
                || (previousDangerRate == StoryDanger.High))
                && (currentDangerRate == StoryDanger.None))
            {
                RemoveAnyStatBonusFromAllColonists();
                RemoveAnyStatMalusFromAllColonists();
                RemoveAnyThoughtBonusFromAllColonists();
                RemoveAnyThoughtMalusFromAllColonists();
                TryApplyThreatIsFinishedThoughtToAllColonists();
            }
            // Transition 3: increased danger rating.
            else if ((previousDangerRate == StoryDanger.Low)
                && (currentDangerRate == StoryDanger.High))
            {
                highDangerDrawingTick = 0;
                PlayOneHighDangerAlarmSound();
                alarmSoundTick = 0;

                // Convert the small adrenaline boost into medium adrenaline boost.
                IEnumerable<Pawn> colonistList = Find.MapPawns.FreeColonists;
                foreach (Pawn colonist in colonistList)
                {
                    foreach (Apparel apparel in colonist.apparel.WornApparel)
                    {
                        if (apparel.def == Util_AlertSpeaker.SmallAdrenalineBoostStatBonusDef)
                        {
                            Apparel unusedDestroyedApparel;
                            colonist.apparel.TryDrop(apparel, out unusedDestroyedApparel, colonist.Position);
                            colonist.apparel.Wear((Apparel)ThingMaker.MakeThing(Util_AlertSpeaker.MediumAdrenalineBoostStatBonusDef), true);

                            IEnumerable<Thought> thoughts = colonist.needs.mood.thoughts.ThoughtsOfDef(Util_AlertSpeaker.SmallAdrenalineBoostThoughtDef);
                            if (thoughts.Count() != 0)
                            {
                                (thoughts.First() as Thought_Memory).age = thoughts.First().def.DurationTicks;
                            }
                            colonist.needs.mood.thoughts.TryGainThought(Util_AlertSpeaker.MediumAdrenalineBoostThoughtDef);
                            break;
                        }
                    }
                }
            }
            // Transition 4: decreased danger rating.
            else if ((previousDangerRate == StoryDanger.High)
                && (currentDangerRate == StoryDanger.Low))
            {
                lowDangerDrawingTick = 0;
                alarmSoundTick = 0;
            }
        }


        /// <summary>
        /// Performs the treatments during an alert according to the current danger rate.
        ///   o danger rate == None => no bonus.
        ///   o danger rate == Low  =>
        ///      * early phase: small adrenaline boost.
        ///      * middle and late phase: no bonus.
        ///   o danger rate == High =>
        ///      * early phase: medium adrenaline boost.
        ///      * middle phase: no bonus.
        ///      * late phase: small stress malus.
        /// </summary>
        public void PerformTreatmentDuringAlert()
        {
            int tickCounter = Find.TickManager.TicksGame;

            if (currentDangerRate == StoryDanger.None)
            {
                // TODO: add music when peaceful.
                return;
            }
            
            List<Pawn> colonistList = new List<Pawn>();
            foreach (Pawn colonist in Find.MapPawns.FreeColonists)
            {
                if ((colonist.GetRoom() == this.GetRoom())
                    && colonist.Position.InHorDistOf(this.Position, alertSpeakerMaxRange))
                {
                    colonistList.Add(colonist);
                }
            }

            if (tickCounter <= alertStartTick + earlyPhaseTicksThreshold)
            {
                if (colonistList.Count == 0)
                    return;
                if (currentDangerRate == StoryDanger.Low)
                {
                    TryApplySmallAdrenalineBoostBonusToColonists(colonistList);
                }
                else if (currentDangerRate == StoryDanger.High)
                {
                    TryApplyMediumAdrenalineBoostBonusToColonists(colonistList);
                }
            }
            else if (tickCounter <= alertStartTick + latePhaseTicksThreshold)
            {
                RemoveAnyStatBonusFromAllColonists();
                RemoveAnyThoughtBonusFromAllColonists();
            }
            else
            {
                if (currentDangerRate == StoryDanger.High)
                {
                    TryApplySmallStressMalusToAllColonists();
                }
            }
        }

        // ===================== Stat and thought bonus/malus functions =====================

        /// <summary>
        /// Check if the given colonist has a small adrenaline boost bonus.
        /// </summary>
        public bool CheckIfColonistHasASmallAdrenalineBoostBonus(Pawn colonist)
        {
            foreach (Apparel apparel in colonist.apparel.WornApparel)
            {
                if (apparel.def == Util_AlertSpeaker.SmallAdrenalineBoostStatBonusDef)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the given colonist has a medium adrenaline boost bonus.
        /// </summary>
        public bool CheckIfColonistHasAMediumAdrenalineBoostBonus(Pawn colonist)
        {
            foreach (Apparel apparel in colonist.apparel.WornApparel)
            {
                if (apparel.def == Util_AlertSpeaker.MediumAdrenalineBoostStatBonusDef)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the given colonist has a small stress malus.
        /// </summary>
        public bool CheckIfColonistHasASmallStressMalus(Pawn colonist)
        {
            foreach (Apparel apparel in colonist.apparel.WornApparel)
            {
                if (apparel.def == Util_AlertSpeaker.SmallStressStatMalusDef)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Try to apply a small adrenaline boost to each colonist if he has not already a small or a medium one.
        /// </summary>
        public void TryApplySmallAdrenalineBoostBonusToColonists(List<Pawn> colonistList)
        {
            foreach (Pawn colonist in colonistList)
            {
                bool colonistHasAnAdrenalineBoostBonus = CheckIfColonistHasASmallAdrenalineBoostBonus(colonist) || CheckIfColonistHasAMediumAdrenalineBoostBonus(colonist);
                if (colonistHasAnAdrenalineBoostBonus == false)
                {
                    colonist.apparel.Wear((Apparel)ThingMaker.MakeThing(Util_AlertSpeaker.SmallAdrenalineBoostStatBonusDef));
                    colonist.needs.mood.thoughts.TryGainThought(Util_AlertSpeaker.SmallAdrenalineBoostThoughtDef);
                }
            }
        }

        /// <summary>
        /// Try to apply a medium adrenaline boost bonus to each colonist if he has not already one.
        /// </summary>
        public void TryApplyMediumAdrenalineBoostBonusToColonists(List<Pawn> colonistList)
        {
            foreach (Pawn colonist in colonistList)
            {
                if (CheckIfColonistHasAMediumAdrenalineBoostBonus(colonist) == false)
                {
                    colonist.apparel.Wear((Apparel)ThingMaker.MakeThing(Util_AlertSpeaker.MediumAdrenalineBoostStatBonusDef));
                    colonist.needs.mood.thoughts.TryGainThought(Util_AlertSpeaker.MediumAdrenalineBoostThoughtDef);
                }
            }
        }

        /// <summary>
        /// Try to apply a small stress malus to all colonists.
        /// </summary>
        public void TryApplySmallStressMalusToAllColonists()
        {
            IEnumerable<Pawn> colonistList = Find.MapPawns.FreeColonists;
            foreach (Pawn colonist in colonistList)
            {
                if (CheckIfColonistHasASmallStressMalus(colonist) == false)
                {
                    colonist.apparel.Wear((Apparel)ThingMaker.MakeThing(Util_AlertSpeaker.SmallStressStatMalusDef));
                    colonist.needs.mood.thoughts.TryGainThought(Util_AlertSpeaker.ColonyIsUnderPressureThoughtDef);
                }
            }
        }

        /// <summary>
        /// Try to apply the "colony is threathened" malus thought to all colonists.
        /// </summary>
        public void TryApplyColonyIsThreatenedThoughtToAllColonists()
        {
            IEnumerable<Pawn> colonistList = Find.MapPawns.FreeColonists;
            foreach (Pawn colonist in colonistList)
            {
                colonist.needs.mood.thoughts.TryGainThought(Util_AlertSpeaker.ColonyIsThreatenedThoughtDef);
            }
        }
        
        /// <summary>
        /// Try to apply the "threat is finished" thought to all colonists.
        /// </summary>
        public void TryApplyThreatIsFinishedThoughtToAllColonists()
        {
            IEnumerable<Pawn> colonistList = Find.MapPawns.FreeColonists;
            foreach (Pawn colonist in colonistList)
            {
                colonist.needs.mood.thoughts.TryGainThought(Util_AlertSpeaker.ThreatIsFinishedThoughtDef);
            }
        }

        /// <summary>
        /// Remove any stat bonus from all colonists.
        /// </summary>
        public void RemoveAnyStatBonusFromAllColonists()
        {
            IEnumerable<Pawn> colonistList = Find.MapPawns.FreeColonists;
            foreach (Pawn colonist in colonistList)
            {
                foreach (Apparel apparel in colonist.apparel.WornApparel)
                {
                    if ((apparel.def == Util_AlertSpeaker.SmallAdrenalineBoostStatBonusDef)
                        || (apparel.def == Util_AlertSpeaker.MediumAdrenalineBoostStatBonusDef))
                    {
                        Apparel unusedDestroyedApparel;
                        colonist.apparel.TryDrop(apparel, out unusedDestroyedApparel, colonist.Position);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Remove any stat malus from all colonists.
        /// </summary>
        public void RemoveAnyStatMalusFromAllColonists()
        {
            IEnumerable<Pawn> colonistList = Find.MapPawns.FreeColonists;
            foreach (Pawn colonist in colonistList)
            {
                foreach (Apparel apparel in colonist.apparel.WornApparel)
                {
                    if (apparel.def == Util_AlertSpeaker.SmallStressStatMalusDef)
                    {
                        Apparel unusedDestroyedApparel;
                        colonist.apparel.TryDrop(apparel, out unusedDestroyedApparel, colonist.Position);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Remove any thought bonus from all colonists.
        /// </summary>
        public void RemoveAnyThoughtBonusFromAllColonists()
        {
            IEnumerable<Pawn> colonistList = Find.MapPawns.FreeColonists;
            foreach (Pawn colonist in colonistList)
            {
                IEnumerable<Thought> thoughts = colonist.needs.mood.thoughts.ThoughtsOfDef(Util_AlertSpeaker.SmallAdrenalineBoostThoughtDef);
                if (thoughts.Count() != 0)
                {
                    (thoughts.First() as Thought_Memory).age = thoughts.First().def.DurationTicks;
                }
                thoughts = colonist.needs.mood.thoughts.ThoughtsOfDef(Util_AlertSpeaker.MediumAdrenalineBoostThoughtDef);
                if (thoughts.Count() != 0)
                {
                    (thoughts.First() as Thought_Memory).age = thoughts.First().def.DurationTicks;
                }
                thoughts = colonist.needs.mood.thoughts.ThoughtsOfDef(Util_AlertSpeaker.ThreatIsFinishedThoughtDef);
                if (thoughts.Count() != 0)
                {
                    (thoughts.First() as Thought_Memory).age = thoughts.First().def.DurationTicks;
                }
            }
        }

        /// <summary>
        /// Remove any thought malus from all colonists.
        /// </summary>
        public void RemoveAnyThoughtMalusFromAllColonists()
        {
            IEnumerable<Pawn> colonistList = Find.MapPawns.FreeColonists;
            foreach (Pawn colonist in colonistList)
            {
                IEnumerable<Thought> thoughts = colonist.needs.mood.thoughts.ThoughtsOfDef(Util_AlertSpeaker.ColonyIsThreatenedThoughtDef);
                if (thoughts.Count() != 0)
                {
                    (thoughts.First() as Thought_Memory).age = thoughts.First().def.DurationTicks;
                }
                thoughts = colonist.needs.mood.thoughts.ThoughtsOfDef(Util_AlertSpeaker.ColonyIsUnderPressureThoughtDef);
                if (thoughts.Count() != 0)
                {
                    (thoughts.First() as Thought_Memory).age = thoughts.First().def.DurationTicks;
                }
            }
        }

        // ===================== Sound functions =====================

        /// <summary>
        /// Performs the sound treatment.
        ///   o danger rate == None => no sound.
        ///   o danger rate == Low  => plays low danger alarm sound
        ///   o danger rate == High => plays high danger alarm sound
        /// </summary>
        public void PerformSoundTreatment()
        {
            int localAlarmSoundPeriod = alarmSoundPeriod * (int)Find.TickManager.CurTimeSpeed;
            switch (currentDangerRate)
            {
                case StoryDanger.Low:
                    if (alarmSoundTick >= localAlarmSoundPeriod)
                    {
                        PlayOneLowDangerAlarmSound();
                        alarmSoundTick = 0;
                    }
                    break;
                case StoryDanger.High:
                    if (alarmSoundTick >= localAlarmSoundPeriod)
                    {
                        PlayOneHighDangerAlarmSound();
                        alarmSoundTick = 0;
                    }
                    break;
            }
            alarmSoundTick++;
        }

        /// <summary>
        /// Play one low danger alarm sound.
        /// </summary>
        public void PlayOneLowDangerAlarmSound()
        {
            if (soundIsActivated)
            {
                lowDangerAlarmSound.PlayOneShotOnCamera();
            }
        }

        /// <summary>
        /// Play one high danger alarm sound.
        /// </summary>
        public void PlayOneHighDangerAlarmSound()
        {
            if (soundIsActivated)
            {
                highDangerAlarmSound.PlayOneShotOnCamera();
            }
        }

        // ===================== Drawing functions =====================

        /// <summary>
        /// Compute the global drawing parameters.
        ///   o danger rate == None => small green light.
        ///   o danger rate == Low  => medium yellow light ramping up and down.
        ///   o danger rate == High => big flashing red light.
        /// </summary>
        public void ComputeDrawingParameters()
        {
            const int lowDangerHalfPeriod = 60;
            const int highDangerHalfPeriod = 10;
            const int highDangerQuietPeriod = 40;
            const float noDangerGlowRadiusOffset = 2f;
            const float lowDangerGlowRadiusOffset = 1f;
            const float lowDangerGlowRadiusDynamic = 4f;
            const float highDangerGlowRadiusOffset = 1f;
            const float highDangerGlowRadiusDynamic = 6f;
            switch (currentDangerRate)
            {
                case StoryDanger.None:
                    glowRadius = noDangerGlowRadiusOffset;
                    glowColor.r = 0;
                    glowColor.g = 220;
                    glowColor.b = 0;
                    break;
                case StoryDanger.Low:
                    glowColor.r = 242;
                    glowColor.g = 185;
                    glowColor.b = 0;
                    if (lowDangerDrawingTick < lowDangerHalfPeriod)
                    {
                        glowRadius = lowDangerGlowRadiusOffset + lowDangerGlowRadiusDynamic * ((float)lowDangerDrawingTick / (float)lowDangerHalfPeriod);
                    }
                    else if (lowDangerDrawingTick < 2 * lowDangerHalfPeriod)
                    {
                        glowRadius = lowDangerGlowRadiusOffset + lowDangerGlowRadiusDynamic * (1f - ((float)(lowDangerDrawingTick - lowDangerHalfPeriod) / (float)lowDangerHalfPeriod));
                    }
                    else
                    {
                        lowDangerDrawingTick = 0;
                    }
                    lowDangerDrawingTick++;
                    break;
                case StoryDanger.High:
                    glowColor.r = 220;
                    glowColor.g = 0;
                    glowColor.b = 0;
                    if (highDangerDrawingTick < highDangerHalfPeriod)
                    {
                        glowRadius = highDangerGlowRadiusOffset + highDangerGlowRadiusDynamic * ((float)highDangerDrawingTick / (float)highDangerHalfPeriod);
                    }
                    else if (highDangerDrawingTick < 2 * highDangerHalfPeriod)
                    {
                        glowRadius = highDangerGlowRadiusOffset + highDangerGlowRadiusDynamic * (1 - ((float)(highDangerDrawingTick - highDangerHalfPeriod) / (float)highDangerHalfPeriod));
                    }
                    else if (highDangerDrawingTick < highDangerQuietPeriod)
                    {
                        glowRadius = 0;
                    }
                    else
                    {
                        highDangerDrawingTick = 0;
                    }
                    highDangerDrawingTick++;
                    break;
            }
        }

        /// <summary>
        /// Performs the drawing treatment. Applies the drawing parameters.
        /// </summary>
        public void PerformDrawingTreatment()
        {
            if ((glowerComp.Props.glowRadius != glowRadius)
                || (glowerComp.Props.glowColor != glowColor))
            {
                glowerComp.Props.glowRadius = glowRadius;
                glowerComp.Props.glowColor = glowColor;
                Find.MapDrawer.MapMeshDirty(this.Position, MapMeshFlag.Things);
                Find.GlowGrid.MarkGlowGridDirty(this.Position);
            }
        }

        /// <summary>
        /// Performs the drawing treatment. Applies the drawing parameters.
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            if (Find.Selector.IsSelected(this))
            {
                List<IntVec3> cellsInEffectZone = Building_AlertSpeaker.GetEffectZoneCells(this.Position);
                GenDraw.DrawFieldEdges(cellsInEffectZone);
            }
        }

        ///<summary>
        ///This creates the command buttons to control the alert speaker sound activation.
        ///</summary>
        ///<returns>The list of command buttons to display.</returns>
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> buttonList = new List<Gizmo>();
            int groupKeyBase = 700000100;

            Command_Action soundActivationButton = new Command_Action();
            if (soundIsActivated)
            {
                soundActivationButton.icon = sirenSoundEnabledIcon;
                soundActivationButton.defaultDesc = "Deactivate siren sound.";
                soundActivationButton.defaultLabel = "Siren sound activated.";
            }
            else
            {
                soundActivationButton.icon = sirenSoundDisabledIcon;
                soundActivationButton.defaultDesc = "Activate siren sound.";
                soundActivationButton.defaultLabel = "Siren sound deactivated.";
            }
            soundActivationButton.activateSound = SoundDef.Named("Click");
            soundActivationButton.action = new Action(PerformSirenSoundAction);
            soundActivationButton.groupKey = groupKeyBase + 1;
            buttonList.Add(soundActivationButton);

            IEnumerable<Gizmo> resultButtonList;
            IEnumerable<Gizmo> basebuttonList = base.GetGizmos();
            if (basebuttonList != null)
            {
                resultButtonList = buttonList.AsEnumerable<Gizmo>().Concat(basebuttonList);
            }
            else
            {
                resultButtonList = buttonList.AsEnumerable<Gizmo>();
            }
            return (resultButtonList);
        }

        /// <summary>
        /// Activates/deactivates siren sound.
        /// </summary>
        public void PerformSirenSoundAction()
        {
            soundIsActivated = !soundIsActivated;
        }

    }
}
