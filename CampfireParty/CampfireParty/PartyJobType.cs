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
    /// PartyJobType enum.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public enum PartyJobType
    {
        Undefined = 0,
        WanderAroundPyre = 1,
        PlayTheGuitar = 2,
        Dance = 3,
        RunInCircleAroundPyre = 4,
        DrinkBeer = 5,
        DropClothes = 6,
        ShootUpInTheAir = 7
    }
}
