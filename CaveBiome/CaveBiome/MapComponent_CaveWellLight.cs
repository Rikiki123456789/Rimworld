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

        public const float lightRadiusCaveWellMin = 0f;
        public const float lightRadiusCaveWellMax = 10f;

        public static bool plantsMessageHasBeenSent = false;
        public static bool growingMessageHasBeenSent = false;

        public MapComponent_CaveWellLight(Map map) : base(map)
		{
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue<bool>(ref MapComponent_CaveWellLight.plantsMessageHasBeenSent, "plantsMessageHasBeenSent");
            Scribe_Values.LookValue<bool>(ref MapComponent_CaveWellLight.growingMessageHasBeenSent, "growingMessageHasBeenSent");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                MapComponentTick();
            }
        }

        public override void MapComponentTick()
        {   
            if (this.map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                return;
            }

            if (Find.TickManager.TicksGame >= this.nextLigthCheckTick)
            {
                this.nextLigthCheckTick = Find.TickManager.TicksGame + lightCheckPeriodInTicks;

                // Shut down light when there is an eclipse.
                /*if (this.map.mapConditionManager.ConditionIsActive(MapConditionDefOf.Eclipse))
                {
                    ... // TODO: eclipse.
                }*/

                int hour = GenLocalDate.HourOfDay(map);
                if ((hour >= sunriseBeginHour)
                    && (hour < sunriseEndHour))
                {
                    // Sunrise.
                    int currentDayTick = Find.TickManager.TicksAbs % GenDate.TicksPerDay;
                    int ticksSinceSunriseBegin = currentDayTick - sunriseBeginHourInTicks;
                    float sunriseProgress = (float)ticksSinceSunriseBegin / (float)sunriseDurationInTicks;
                    float caveWellLigthRadius = Mathf.Lerp(lightRadiusCaveWellMin, lightRadiusCaveWellMax, sunriseProgress);
                    List<Thing> caveWellsList = map.listerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
                    foreach (Thing caveWell in caveWellsList)
                    {
                        SetGlowRadius(caveWell, caveWellLigthRadius);
                    }
                }
                else if ((hour >= sunsetBeginHour)
                    && (hour < sunsetEndHour))
                {
                    // Sunset.
                    int currentDayTick = Find.TickManager.TicksAbs % GenDate.TicksPerDay;
                    int ticksSinceSunsetBegin = currentDayTick - sunsetBeginHourInTicks;
                    float sunsetProgress = 1f - ((float)ticksSinceSunsetBegin / (float)sunriseDurationInTicks);
                    float caveWellLigthRadius = Mathf.Lerp(lightRadiusCaveWellMin, lightRadiusCaveWellMax, sunsetProgress);
                    List<Thing> caveWellsList = map.listerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
                    foreach (Thing caveWell in caveWellsList)
                    {
                        SetGlowRadius(caveWell, caveWellLigthRadius);
                    }
                }

                if ((MapComponent_CaveWellLight.plantsMessageHasBeenSent == false)
                    && (hour >= sunriseBeginHour + 1))
                {
                    Find.LetterStack.ReceiveLetter("Cave plants", "In caves, most cave plants can be useful so look around!\n- some plants like giant leafs can be cooked,\n- others like cave vine are hard enough to be used like wood,\n- and some like devil's tongue provide useful fibrous material.\n\nBeware, though! Caves are a hard place to live and some plants may be dangerous.",
                        LetterType.Good);
                    MapComponent_CaveWellLight.plantsMessageHasBeenSent = true;
                }
                if ((MapComponent_CaveWellLight.growingMessageHasBeenSent == false)
                    && (hour >= sunriseBeginHour + 2))
                {
                    if (MapGenerator.PlayerStartSpot.IsValid
                        && (MapGenerator.PlayerStartSpot != IntVec3.Zero)) // Checking PlayerStartSpot validity will still raise an error message if it is invalid.
                    {
                        Find.LetterStack.ReceiveLetter("Growing in cave", "The sun cannot directly light the cave tunnels. You can however grow some plants in cave wells. Cave wells are natural openings to the surface.",
                            LetterType.Good, new RimWorld.Planet.GlobalTargetInfo(MapGenerator.PlayerStartSpot, this.map));
                    }
                    else
                    {
                        Find.LetterStack.ReceiveLetter("Growing in cave", "The sun cannot directly light the cave tunnels. You can however grow some plants in cave wells. Cave wells are natural openings to the surface.",
                            LetterType.Good);
                    }
                    MapComponent_CaveWellLight.growingMessageHasBeenSent = true;
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
                caveWell.Map.glowGrid.MarkGlowGridDirty(caveWell.Position);
            }
        }
    }
}
