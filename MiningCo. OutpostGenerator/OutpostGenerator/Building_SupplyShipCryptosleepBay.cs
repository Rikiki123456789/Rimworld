using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Building_ShipCryptosleepBay class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_SupplyShipCryptosleepBay : Building_CryptosleepCasket
    {
        public override void Tick()
        {
            base.Tick();
            // Remove potential open designation.
            Designation designation = Find.DesignationManager.DesignationOn(this, DesignationDefOf.Open);
            if (designation != null)
            {
                designation.Delete();
            }

            Thing thing = this.ContainedThing;
            if (thing != null)
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null)
                {
                    SoundDef.Named("CryptosleepCasketEject").PlayOneShot(base.Position);
                }
                this.container.ClearAndDestroyContents();
            }
        }
        
        // Disable Gizmos.
        public override IEnumerable<Gizmo> GetGizmos()
        {
            return new List<Gizmo>();
        }

        // Disable float menu options (so colonists cannot be forced to enter the supply ship). Mmmh, clandestine passengers could be cool though...
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            return new List<FloatMenuOption>();
        }
    }
}
