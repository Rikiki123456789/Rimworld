using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;    // Needed when you do something with the AI
using System.Xml;
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// PatchOperationCheckForCaveBiome class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    class PatchOperationCheckForCaveBiome : PatchOperation
    {
        protected override bool ApplyWorker(XmlDocument xml)
        {
            return ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "CaveBiome");
        }
    }
}
