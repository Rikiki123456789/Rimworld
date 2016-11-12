using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace CaveBiome
{
    /// <summary>
    /// MapComponent_CaveWellLight class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class MapComponent_CaveWellLight : MapComponent
    {
        public const int sunriseBeginHour = 6;
        public const int sunriseBeginHourInTicks = sunriseBeginHour * GenDate.TicksPerHour;
        public const int sunriseEndHour = 10;
        public const int sunriseEndHourInTicks = sunriseEndHour * GenDate.TicksPerHour;
        public const int sunriseDurationInTicks = sunriseEndHourInTicks - sunriseBeginHourInTicks;
        public const int sunsetBeginHour = 16;
        public const int sunsetBeginHourInTicks = sunsetBeginHour * GenDate.TicksPerHour;
        public const int sunsetEndHour = 20;
        public const int sunsetEndHourInTicks = sunsetEndHour * GenDate.TicksPerHour;
        public const int sunsetDurationInTicks = sunsetEndHourInTicks - sunsetBeginHourInTicks;
        public const int lightCheckPeriodInTicks = GenTicks.TicksPerRealSecond;
        public int nextLigthCheckTick = 1;

        public const float lightRadiusCaveWellMainMin = 0f;
        public const float lightRadiusCaveWellMainMax = 10f;
        public const float lightRadiusCaveWellAdditionalMin = 0f;
        public const float lightRadiusCaveWellAdditionalMax = 3f;

        public override void MapComponentTick()
        {
            if (Find.Map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                return;
            }

            if (Find.TickManager.TicksGame >= nextLigthCheckTick)
            {
                nextLigthCheckTick = Find.TickManager.TicksGame + lightCheckPeriodInTicks;
                /*Log.Message("TicksAbs = " + Find.TickManager.TicksAbs); // TODO: remove this.
                Log.Message("TicksGame = " + Find.TickManager.TicksGame);
                Log.Message("Checking light, hour = " + GenDate.HourOfDay);*/
                int hour = GenDate.HourOfDay;
                if ((hour >= sunriseBeginHour)
                    && (hour < sunriseEndHour))
                {
                    // Sunrise.
                    int currentDayTick = Find.TickManager.TicksAbs % GenDate.TicksPerDay;
                    //Log.Message("currentDayTick = " + currentDayTick);
                    int ticksSinceSunriseBegin = currentDayTick - sunriseBeginHourInTicks;
                    //Log.Message("ticksSinceSunriseBegin = " + ticksSinceSunriseBegin);
                    float sunriseProgress = (float)ticksSinceSunriseBegin / (float)sunriseDurationInTicks;
                    //Log.Message("sunriseProgress = " + sunriseProgress);
                    float caveWellMainLigthRadius = Mathf.Lerp(lightRadiusCaveWellMainMin, lightRadiusCaveWellMainMax, sunriseProgress);
                    //Log.Message("caveWellMainLigthRadius = " + caveWellMainLigthRadius);                    
                    List<Thing> caveWellMainList = Find.ListerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
                    foreach (Thing caveWellMain in caveWellMainList)
                    {
                        SetGlowRadius(caveWellMain, caveWellMainLigthRadius);
                    }
                }
                else if ((hour >= sunsetBeginHour)
                    && (hour < sunsetEndHour))
                {
                    // Sunset.
                    int currentDayTick = Find.TickManager.TicksAbs % GenDate.TicksPerDay;
                    //Log.Message("currentDayTick = " + currentDayTick);
                    int ticksSinceSunsetBegin = currentDayTick - sunsetBeginHourInTicks;
                    //Log.Message("ticksSinceSunsetBegin = " + ticksSinceSunsetBegin);
                    float sunsetProgress = 1f - ((float)ticksSinceSunsetBegin / (float)sunriseDurationInTicks);
                    //Log.Message("sunsetProgress = " + sunsetProgress);
                    float caveWellMainLigthRadius = Mathf.Lerp(lightRadiusCaveWellMainMin, lightRadiusCaveWellMainMax, sunsetProgress);
                    //Log.Message("caveWellMainLigthRadius = " + caveWellMainLigthRadius);
                    List<Thing> caveWellMainList = Find.ListerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
                    foreach (Thing caveWellMain in caveWellMainList)
                    {
                        SetGlowRadius(caveWellMain, caveWellMainLigthRadius);
                    }
                }
            }
        }
        
        public void SetGlowRadius(Thing caveWell, float glowradius)
        {
            CompGlower glowerComp = caveWell.TryGetComp<CompGlower>();
            if (glowerComp != null)
            {
                glowerComp.Props.glowRadius = glowradius;
                glowerComp.Props.overlightRadius = glowradius;
                Find.GlowGrid.MarkGlowGridDirty(caveWell.Position);
            }
        }
    }
}
