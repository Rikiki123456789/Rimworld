using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
using RimWorld.SquadAI;
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
        public override IEnumerable<Gizmo> GetGizmos()
        {
            return new List<Gizmo>();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            return new List<FloatMenuOption>();
        }
    }
}
