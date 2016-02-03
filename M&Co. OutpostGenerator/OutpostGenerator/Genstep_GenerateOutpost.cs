using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace OutpostGenerator
{
    /// <summary>
    /// Genstep_GenerateOutpost class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class Genstep_GenerateOutpost : Genstep_Scatterer
    {
        public static int zoneSideSize = 11; // Side size of a zone.
        public static int zoneSideCenterOffset = 5;

        OG_OutpostData outpostData = new OG_OutpostData();

        public override void Generate()
        {
            GenerateOutpostProperties(ref outpostData);
            if (outpostData.size == OG_OutpostSize.NoOutpost)
            {
                return;
            }

            //// TODO: debug, remove it.
            /*outpostData.battleOccured = true;
            Log.Message("outpostData:");
            Log.Message(" - size: " + outpostData.size.ToString());
            Log.Message(" - isMilitary: " + outpostData.isMilitary.ToString());
            Log.Message(" - battleOccured: " + outpostData.battleOccured.ToString());
            Log.Message(" - isRuined: " + outpostData.isRuined.ToString());
            Log.Message(" - isInhabited: " + outpostData.isInhabited.ToString());

            outpostData.areaSouthWestOrigin = Find.Map.Center + new IntVec3(-40, 0, 40);
            OG_SmallOutpost.GenerateOutpost(outpostData);
            outpostData.areaSouthWestOrigin = Find.Map.Center + new IntVec3(40, 0, 40);
            OG_SmallOutpost.GenerateOutpost(outpostData);
            outpostData.areaSouthWestOrigin = Find.Map.Center + new IntVec3(40, 0, -40);
            OG_SmallOutpost.GenerateOutpost(outpostData);
            outpostData.areaSouthWestOrigin = Find.Map.Center + new IntVec3(-40, 0, -40);
            OG_SmallOutpost.GenerateOutpost(outpostData);
            ////*/

            // Check this outpost can be spawned somewhere on the map.
            if (CellFinderLoose.TryFindRandomNotEdgeCellWith(this.minEdgeDist, new Predicate<IntVec3>(this.CanScatterAt), out outpostData.areaSouthWestOrigin) == false) // TODO: verify minEdgeDist is correctly used.
            {
                if (this.warnOnFail)
                {
                    Log.Message("Scatterer " + this.ToString() + " could not find an area to generate an outpost.");
                }
                return;
            }
            
            // Generate the outpost.
            this.ScatterAt(outpostData.areaSouthWestOrigin);
        }

        protected override bool CanScatterAt(IntVec3 loc)
        {
            if (base.CanScatterAt(loc) == false)
            {
                return false;
            }
            
            int minFreeAreaSideSize = 1;
            switch (this.outpostData.size)
            {
                case OG_OutpostSize.SmallOutpost:
                    minFreeAreaSideSize = OG_SmallOutpost.areaSideLength;
                    break;
                case OG_OutpostSize.BigOutpost:
                    // TODO: implement it.
                    //minFreeAreaSideSize = OG_BigOutpost.areaSideLength;
                    break;
            }
            CellRect outpostArea = new CellRect(loc.x, loc.z, minFreeAreaSideSize, minFreeAreaSideSize);
            foreach (IntVec3 cell in outpostArea)
            {
                if (cell.CloseToEdge(20))
                {
                    return false;
                }
                Building building = cell.GetEdifice();
                if (building != null)
                {
                    return false;
                }
                TerrainDef terrain = Find.TerrainGrid.TerrainAt(cell);
                if ((terrain == TerrainDef.Named("Marsh"))
                    || (terrain == TerrainDef.Named("Mud"))
                    || (terrain == TerrainDef.Named("WaterDeep"))
                    || (terrain == TerrainDef.Named("WaterShallow")))
                {
                    return false;
                }
                List<Thing> thingList = cell.GetThingList();
                foreach (Thing thing in thingList)
                {
                    if (thing.def.destroyable == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected override void ScatterAt(IntVec3 loc)
        {
            switch (this.outpostData.size)
            {
                case OG_OutpostSize.SmallOutpost:
                    OG_SmallOutpost.GenerateOutpost(outpostData);
                    break;
                case OG_OutpostSize.BigOutpost:
                    // TODO: implement it.
                    //OG_BigOutpost.GenerateOutpost(outpostData);
                    Log.Warning("Big outpost generation not yet implemented.");
                    break;
            }
        }

        protected void GenerateOutpostProperties(ref OG_OutpostData outpostData)
        {
            GetOutpostSize(ref outpostData);
            GetOutpostType(ref outpostData);
            GetBattleOccured(ref outpostData);
            GetIsRuined(ref outpostData);
            GetIsInhabited(ref outpostData);
        }

        protected void GetOutpostSize(ref OG_OutpostData outpostData)
        {
            // Get outpost size.
            // TODO: debug
            /*float outpostSizeSelector = Rand.Value;
            if (outpostSizeSelector < 0.2f)
            {
                // No outpost.
                outpostData.size = OutpostSize.NoOutpost;
            }
            else if (outpostSizeSelector < 0.5f)
            {
                // TODO: implement it.
                // Generate a big outpost.
                Log.Warning("Big outpost generation not yet implemented.");
                outpostData.size = OutpostSize.BigOutpost;
            }
            else*/
            {
                // Generate a small outpost.
                outpostData.size = OG_OutpostSize.SmallOutpost;
            }
        }

        protected void GetOutpostType(ref OG_OutpostData outpostData)
        {
            float outpostTypeSelector = Rand.Value;
            if (outpostTypeSelector < 0.25f)
            {
                outpostData.isMilitary = true;
            }
            else
            {
                outpostData.isMilitary = false;
            }
        }

        protected void GetBattleOccured(ref OG_OutpostData outpostData)
        {
            float battleThreshold = 0.33f;
            if (outpostData.isMilitary)
            {
                battleThreshold = 0.66f;
            }
            float battleOccuredSelector = Rand.Value;
            if (battleOccuredSelector < battleThreshold)
            {
                outpostData.battleOccured = true;
            }
            else
            {
                outpostData.battleOccured = false;
            }
        }

        protected void GetIsRuined(ref OG_OutpostData outpostData)
        {
            float ruinThreshold = 0.33f;
            if (outpostData.battleOccured)
            {
                ruinThreshold = 0.66f;
            }
            float ruinSelector = Rand.Value;
            if (ruinSelector < ruinThreshold)
            {
                outpostData.isRuined = true;
            }
            else
            {
                outpostData.isRuined = false;
            }
        }

        protected void GetIsInhabited(ref OG_OutpostData outpostData)
        {
            float inhabitedThreshold = 0.75f;
            if (outpostData.isRuined)
            {
                inhabitedThreshold = 0.25f;
            }
            float inhabitedSelector = Rand.Value;
            if (inhabitedSelector < inhabitedThreshold)
            {
                outpostData.isInhabited = true;
            }
            else
            {
                outpostData.isInhabited = false;
            }
        }
    }
}
