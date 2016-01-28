using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound
using RimWorld;      // RimWorld specific functions are found here
//using RimWorld.SquadAI;
namespace CampfireParty
{
    /// <summary>
    /// CampfireParty utility class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public static class Util_CampfireParty
    {
        // Defs.
        public static ThingDef Def_PyreFire
        {
            get
            {
                return ThingDef.Named("PyreFire");
            }
        }

        // Jobs.
        public static JobDef Job_StartCampfireParty
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_StartCampfireParty");
            }
        }
        public static JobDef Job_WanderAroundPyre
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_WanderAroundPyre");
            }
        }
        public static JobDef Job_PlayTheGuitar
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_PlayTheGuitar");
            }
        }
        public static JobDef Job_Dance
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_Dance");
            }
        }
        public static JobDef Job_IngestBeer
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_IngestBeer");
            }
        }
        public static JobDef Job_DropClothes
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_DropClothes");
            }
        }
        public static JobDef Job_ShootUpinTheAir
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_ShootUpinTheAir");
            }
        }
        public static JobDef Job_UpdateThought
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("JobDef_UpdateThought");
            }
        }

        // Thought.
        public static ThoughtDef Thought_HadCampfireParty1
        {
            get
            {
                return (ThoughtDef.Named("HadCampfireParty1"));
            }
        }
        public static ThoughtDef Thought_HadCampfireParty2
        {
            get
            {
                return (ThoughtDef.Named("HadCampfireParty2"));
            }
        }
        public static ThoughtDef Thought_HadCampfireParty3
        {
            get
            {
                return (ThoughtDef.Named("HadCampfireParty3"));
            }
        }
        public static ThoughtDef Thought_HadCampfireParty4
        {
            get
            {
                return (ThoughtDef.Named("HadCampfireParty4"));
            }
        }
        public static ThoughtDef Thought_HadCampfirePartyPsychopaths
        {
            get
            {
                return (ThoughtDef.Named("HadCampfirePartyPsychopaths"));
            }
        }    
    
        // Motes.
        public static ThingDef Mote_BeerAvailable
        {
            get
            {
                return (ThingDef.Named("Mote_BeerAvailable"));
            }
        }
        public static ThingDef Mote_BeerUnavailable
        {
            get
            {
                return (ThingDef.Named("Mote_BeerUnavailable"));
            }
        }
        public static ThingDef Mote_Guitar
        {
            get
            {
                return (ThingDef.Named("Mote_Guitar"));
            }
        }
        public static ThingDef Mote_MusicNote
        {
            get
            {
                return (ThingDef.Named("Mote_MusicNote"));
            }
        }

        // JoyKindDef.
        public static JoyKindDef JoyKindDefOf_Social
        {
            get
            {
                return DefDatabase<JoyKindDef>.GetNamed("Social");
            }
        }
    }
}
