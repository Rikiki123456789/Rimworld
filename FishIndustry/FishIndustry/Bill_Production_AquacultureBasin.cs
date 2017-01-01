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

namespace FishIndustry
{
    /// <summary>
    /// Bill_Production_AquacultureBasin class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Bill_Production_AquacultureBasin : Bill_Production
    {
        public Bill_Production_AquacultureBasin()
		{
		}
        public Bill_Production_AquacultureBasin(RecipeDef recipe)
            : base(recipe)
        {
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);

            Building_AquacultureBasin aquacultureBasin = this.billStack.billGiver as Building_AquacultureBasin;
            if (aquacultureBasin != null)
            {
                // Get the supplied species def and start a new breed cycle in the aquaculture basin.
                Corpse fishCorpse = ingredients.First() as Corpse;
                if (fishCorpse != null)
                {
                    PawnKindDef fishKind = null;
                    string fishCorpseAsString = fishCorpse.ToString();
                    if (fishCorpseAsString.Contains("Mashgon"))
                    {
                        fishKind = Util_FishIndustry.MashgonDef;
                    }
                    else if (fishCorpseAsString.Contains("Blueblade"))
                    {
                        fishKind = Util_FishIndustry.BluebladeDef;
                    }
                    else if (fishCorpseAsString.Contains("Tailteeth"))
                    {
                        fishKind = Util_FishIndustry.TailteethDef;
                    }
                    if (fishKind != null)
                    {
                        aquacultureBasin.StartNewBreedCycle(fishKind);
                    }
                    else
                    {
                        Log.Warning("FishIndustry: this fish is not handled for breeding (" + fishCorpseAsString + ").");
                    }
                }
            }
        }
    }
}
