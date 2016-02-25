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

namespace OutpostGenerator
{
    /// <summary>
    /// ThinkNode_ConditionalMAndCoEmployee class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class ThinkNode_ConditionalMAndCoEmployee : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            Log.Message("Checking ThinkNode_ConditionalMAndCoEmployee for " + pawn.Name.ToStringShort);
            return pawn.IsColonist;
        }
    }
}
