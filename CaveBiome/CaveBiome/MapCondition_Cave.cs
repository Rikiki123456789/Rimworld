using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace CaveBiome
{
    public class MapCondition_Cave : MapCondition
    {
		[Unsaved]
		private static List<Thing> filthToDelete = new List<Thing>();
		
		public override void MapConditionTick() {
			ScanForAndDeleteFilth();
			}
		
		public void ScanForAndDeleteFilth() {
			if((Find.TickManager.TicksGame % 331) == 0) { //(~5.5 seconds) using prime number to reduce convergent delays
				List<Thing> filthlist = Map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Filth));
				//Messages.Message("filth on map: " + filthlist.Count, MessageSound.Silent);
				if(filthlist.Count*filthlist.Count > Rand.Range(0, 300*300)) { //delete filth at maximum rate if >300 filth on map
					DeleteFilthAtCoordinate(filthlist[Rand.Range(0, filthlist.Count)].Position);
					}
				}
			}
		
		public void DeleteFilthAtCoordinate(IntVec3 position) {
			//Don't delete filth in player-built areas
			Room indoors = RoomQuery.RoomAt(position, Map);
			if(indoors is Room && !indoors.TouchesMapEdge) return;
						
			List<Thing> things = position.GetThingList(Map);
			foreach(Thing thing in things) { //yo dawg
				if(thing is Filth) {
					filthToDelete.Add(thing);
					}
				}
			if(filthToDelete.Count > 0) {
				//Messages.Message("Deleted filth at " + position.x + ", " + position.z, new GlobalTargetInfo(position, Map), MessageSound.Silent);
				for(int i = filthToDelete.Count-1; i >= 0; i--) filthToDelete[i].Destroy();
				filthToDelete.RemoveAll((x) => true);
				}
			}
		
        private int LerpTicks = 200;
		
		public override float PlantDensityFactor()
        {
            // To avoid getting plant seeds from map edges.
            return 0f;
        }

		private SkyColorSet caveSkyColors = new SkyColorSet(new Color(0.482f, 0.603f, 0.682f), Color.white, new Color(0.6f, 0.6f, 0.6f), 1f);

        public override float SkyTargetLerpFactor()
        {
            return MapConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, (float)this.LerpTicks, 1f);
        }
        public override SkyTarget? SkyTarget()
        {
			WeatherDef weather = WeatherDef.Named("CaveCalm");
			caveSkyColors.sky = weather.skyColorsDay.sky;
			caveSkyColors.shadow = weather.skyColorsDay.shadow;
			caveSkyColors.saturation = weather.skyColorsDay.saturation;
			caveSkyColors.overlay = weather.skyColorsDay.overlay;
            return new SkyTarget?(new SkyTarget(this.caveSkyColors));
        }
		
		public override bool AllowEnjoyableOutsideNow()
        {
            return false;
        }
    }
}
