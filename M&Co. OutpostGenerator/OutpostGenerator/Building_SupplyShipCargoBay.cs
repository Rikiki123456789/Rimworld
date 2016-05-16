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
    /// Building_SupplyShipCargoBay class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Building_SupplyShipCargoBay : Building_Storage
    {
        // WARNING: there is a known bug when several pawns from different factions (Colony and M&Co. for example) try to reserve the same cargo bay spot.
        // This can only be avoided by the player by setting proper authorized zones.

        public override void Notify_ReceivedThing(Thing newItem)
        {
            base.Notify_ReceivedThing(newItem);
            // TODO: remove this. The troops review will perform a more precise report.
            /*Corpse corpse = newItem as Corpse;
            if (corpse != null)
            {
                PawnKindDef pawnType = corpse.innerPawn.kindDef;
                Log.Message("Loaded " + pawnType.ToString() + " into cargo bay.");
                List<Thing> orbitalRelayList = Find.ListerThings.ThingsOfDef(OG_Util.OrbitalRelayDef);
                if (orbitalRelayList.Count > 0)
                {
                    Building_OrbitalRelay orbitalRelay = orbitalRelayList.First() as Building_OrbitalRelay;
                    orbitalRelay.RequestReinforcement(pawnType);
                }
            }*/
            newItem.Destroy();
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            // TODO: unforbid every gun/corpse/apparel in the outpost so it is carried to the cargo bay?
        }
    }
}
