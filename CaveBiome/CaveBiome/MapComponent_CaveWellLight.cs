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
        public const int lightCheckPeriodInTicks = GenDate.TicksPerHour;
        public int nextLigthCheckTick = 1;

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame >= nextLigthCheckTick)
            {
                nextLigthCheckTick = Find.TickManager.TicksGame + lightCheckPeriodInTicks;
                Log.Message("Checking light, hour = " + GenDate.HourOfDay);
                Thing caveWell = Find.ListerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef).First();
                if (caveWell != null)
                {
                    CompGlower glowerComp = caveWell.TryGetComp<CompGlower>();
                    if (glowerComp != null)
                    {
                        switch (GenDate.HourOfDay)
                        {
                            case 13:
                            case 5:
                                glowerComp.Props.glowRadius = 1f;
                                glowerComp.Props.overlightRadius = 0f;
                                Find.GlowGrid.MarkGlowGridDirty(caveWell.Position);
                                break;
                            case 14:
                            case 6:
                                glowerComp.Props.glowRadius = 1.5f;
                                glowerComp.Props.overlightRadius = 1f;
                                Find.GlowGrid.MarkGlowGridDirty(caveWell.Position);
                                break;
                            case 15:
                            case 7:
                                glowerComp.Props.glowRadius = 2f;
                                glowerComp.Props.overlightRadius = 1.5f;
                                Find.GlowGrid.MarkGlowGridDirty(caveWell.Position);
                                break;
                            case 16:
                            case 8:
                                glowerComp.Props.glowRadius = 2f;
                                glowerComp.Props.overlightRadius = 2f;
                                Find.GlowGrid.MarkGlowGridDirty(caveWell.Position);
                                break;
                            case 9:
                            case 18:
                                glowerComp.Props.glowRadius = 2f;
                                glowerComp.Props.overlightRadius = 1.5f;
                                Find.GlowGrid.MarkGlowGridDirty(caveWell.Position);
                                break;
                            case 10:
                            case 19:
                                glowerComp.Props.glowRadius = 1.5f;
                                glowerComp.Props.overlightRadius = 1f;
                                Find.GlowGrid.MarkGlowGridDirty(caveWell.Position);
                                break;
                            case 11:
                            case 20:
                                glowerComp.Props.glowRadius = 1f;
                                glowerComp.Props.overlightRadius = 0f;
                                Find.GlowGrid.MarkGlowGridDirty(caveWell.Position);
                                break;
                            case 12:
                            case 21:
                                glowerComp.Props.glowRadius = 0f;
                                glowerComp.Props.overlightRadius = 0f;
                                Find.GlowGrid.MarkGlowGridDirty(caveWell.Position);
                                break;
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.nextLigthCheckTick, "nextLigthCheckTick");
        }
    }
}
