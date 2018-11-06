using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
//using VerseBase;   // Material/Graphics handling functions are found here
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// Designator_ZoneAdd_Fishing class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.</permission>
    class Designator_ZoneAdd_Fishing : Designator_ZoneAdd
    {
		protected override string NewZoneLabel
		{
			get
			{
                return "FishIndustry.FishingZone".Translate();
			}
		}

        public Designator_ZoneAdd_Fishing()
		{
			this.zoneTypeToPlace = typeof(Zone_Fishing);
            this.defaultLabel = "FishIndustry.FishingZone".Translate();
            this.defaultDesc = "FishIndustry.DesignatorFishingZoneDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("Ui/Designators/ZoneCreate_Fishing", true);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!base.CanDesignateCell(c).Accepted)
			{
				return false;
            }
            if (Util_PlaceWorker.IsNearFishingPier(this.Map, c, Util_PlaceWorker.minDistanceBetweenTwoFishingSpots))
            {
                return false;
            }
			if (Util_Zone_Fishing.IsAquaticTerrain(this.Map, c)
                && c.Walkable(this.Map))
			{
				return true;
			}
			return false;
		}

		protected override Zone MakeNewZone()
		{
			return new Zone_Fishing(Find.CurrentMap.zoneManager);
		}
    }
}
