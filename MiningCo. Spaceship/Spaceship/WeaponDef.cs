using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound;   // Needed when you do something with the Sound

namespace Spaceship
{
    public class WeaponDef : Def
    {
        public ThingDef ammoDef = null;
        public SoundDef soundCastDef = null;
        public int ammoQuantity = 0;
        public int ticksBetweenShots = 0;
        public float startShootingDistance = 0f;
        public float ammoTravelDistance = 0f;
        public float ammoDispersion = 0f;
        public float targetAcquireRange = 0f;
        public bool isTwinGun = true; // Wether there is a gun on each side of the ship.
        public float horizontalPositionOffset = 0f;
        public float verticalPositionOffset = 0f;
        public int disableRainDurationInTicks = 0;
    }
}
