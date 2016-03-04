using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// Bullet_Rotating class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Bullet_Rotating : Bullet
    {
        public float angle = 0f;

        public override Quaternion ExactRotation
        {
            get
            {
                return angle.ToQuat();
            }
        }

        public override void Tick()
        {
            base.Tick();

            angle += 4f;
        }

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);

            MoteThrower.ThrowLightningGlow(this.ExactPosition, 0.5f);
        }
    }
}
