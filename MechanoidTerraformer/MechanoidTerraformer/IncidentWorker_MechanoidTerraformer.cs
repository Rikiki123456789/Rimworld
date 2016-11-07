using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
using Verse.Sound;   // Needed when you do something with the Sound

namespace MechanoidTerraformer
{
    /// <summary>
    /// IncidentWorker_MechanoidTerraformer class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class IncidentWorker_MechanoidTerraformer : IncidentWorker
    {
        private const int squareAreaRange = 7;

        protected override bool StorytellerCanUseNowSub()
        {
            return true;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Predicate<IntVec3> validator = delegate(IntVec3 testedCell)
            {
                if (testedCell.Fogged())
                {
                    return false;
                }
                IntVec2 freeSpaceSize = new IntVec2(16, 16);
                foreach (IntVec3 current in GenAdj.CellsOccupiedBy(testedCell, Rot4.North, freeSpaceSize))
                {
                    if (current.Walkable() == false)
                    {
                        bool result = false;
                        return result;
                    }                    
                    if (Find.RoofGrid.Roofed(current))
                    {
                        bool result = false;
                        return result;
                    }
                }
                return testedCell.CanReachColony();
            };
            IntVec3 landingCell;
            if (CellFinderLoose.TryFindRandomNotEdgeCellWith(20, validator, out landingCell) == false)
            {
                return false;
            }
            string eventText = "   You have detected a strange thing falling from the sky. It is quite bigger than a drop pod and does not emit the standard trading federation emergency call.\n"
                + "You should send someone to scout it but be careful, this stinks mechanoid technology...";
            Find.LetterStack.ReceiveLetter("Artifact", eventText, LetterType.BadNonUrgent, landingCell);
            Thing mechanoidTerraformerIncoming = ThingMaker.MakeThing(Util_MechanoidTerraformer.MechanoidTerraformerIncomingDef);
            mechanoidTerraformerIncoming.SetFactionDirect(Faction.OfMechanoids);
            GenSpawn.Spawn(mechanoidTerraformerIncoming, landingCell);

            return true;
        }
    }
}
