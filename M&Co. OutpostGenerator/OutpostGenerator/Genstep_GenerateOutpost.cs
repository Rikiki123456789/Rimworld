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
        public const int zoneSideSize = 11; // Side size of a zone.
        public const int zoneSideCenterOffset = 5;

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

            outpostData.areaSouthWestOrigin = Find.Map.Center + new IntVec3(-70, 0, 70);
            OG_BigOutpost.GenerateOutpost(outpostData);
            outpostData.areaSouthWestOrigin = Find.Map.Center + new IntVec3(70, 0, 70);
            OG_BigOutpost.GenerateOutpost(outpostData);
            outpostData.areaSouthWestOrigin = Find.Map.Center + new IntVec3(70, 0, -70);
            OG_BigOutpost.GenerateOutpost(outpostData);
            outpostData.areaSouthWestOrigin = Find.Map.Center + new IntVec3(-70, 0, -70);
            OG_BigOutpost.GenerateOutpost(outpostData);*/
            ////

            // Check this outpost can be spawned somewhere on the map.
            bool validSpawnPointIsFound = false;
            for (int tryIndex = 0; tryIndex < 15; tryIndex++)
            {
                if (CellFinderLoose.TryFindRandomNotEdgeCellWith(this.minEdgeDist, new Predicate<IntVec3>(this.CanScatterAt), out outpostData.areaSouthWestOrigin))
                {
                    // A valid spawn location has been found.
                    validSpawnPointIsFound = true;
                    break;
                }
                else
                {
                    Log.Message("tryIndex = " + tryIndex); // TODO: debug
                }
            }
            if (validSpawnPointIsFound == false)
            {
                Log.Message("Scatterer " + this.ToString() + " could not find an area to generate an outpost.");
                return;
            }
            
            // Generate the outpost.
            this.ScatterAt(outpostData.areaSouthWestOrigin);
        }

        protected override bool CanScatterAt(IntVec3 loc)
        {
            int mountainousBorderCells = 0;
            int mountainousBorderCellsThreshold = 0;
            int aquaticCells = 0;
            int aquaticCellsThreshold = 0;

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
                    minFreeAreaSideSize = OG_BigOutpost.areaSideLength;
                    break;
            }
            CellRect outpostArea = new CellRect(loc.x, loc.z, minFreeAreaSideSize, minFreeAreaSideSize);
            mountainousBorderCellsThreshold = (2 * outpostArea.Width + 2 * outpostArea.Height) / 4;
            aquaticCellsThreshold = outpostArea.Area / 100;
            foreach (IntVec3 cell in outpostArea)
            {
                // Only for border cells: too close from edge or crossing an existing structure (potentially a shrine).
                if ((cell.x == outpostArea.minX)
                    || (cell.x == outpostArea.maxX)
                    || (cell.z == outpostArea.minZ)
                    || (cell.z == outpostArea.maxZ))
                {
                    if (cell.CloseToEdge(20))
                    {
                        return false;
                    }

                    Building building = cell.GetEdifice();
                    if (building != null)
                    {
                        mountainousBorderCells++;
                        if (mountainousBorderCells > mountainousBorderCellsThreshold)
                        {
                            //Log.Message("Invalid spawn point. Covering too much mountainous cells (threshold = " + mountainousBorderCellsThreshold + ")."); // TODO. debug
                            return false;
                        }
                    }
                }
                TerrainDef terrain = Find.TerrainGrid.TerrainAt(cell);
                if (terrain == TerrainDef.Named("WaterDeep"))
                {
                    return false;
                }
                if ((terrain == TerrainDef.Named("Marsh"))
                    || (terrain == TerrainDef.Named("Mud"))
                    || (terrain == TerrainDef.Named("WaterShallow")))
                {
                    aquaticCells++;
                    if (aquaticCells > aquaticCellsThreshold)
                    {
                        //Log.Message("Invalid spawn point. Covering too much aquatic cells (threshold = " + aquaticCellsThreshold + ")."); // TODO. debug
                        return false;
                    }
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
            Log.Message("Valid spawn point found! Mountainous cells/threshold, aquatic cells/threshold = " + mountainousBorderCells + "/" + mountainousBorderCellsThreshold + ", " + aquaticCells + "/" + aquaticCellsThreshold); // TODO. debug
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
                    OG_BigOutpost.GenerateOutpost(outpostData);
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
            /*float outpostSizeSelector = Rand.Value;
            if (outpostSizeSelector < 0.2f)
            {
                // No outpost.
                outpostData.size = OG_OutpostSize.NoOutpost;
            }
            else if (outpostSizeSelector < 0.5f)
            {
                // Generate a big outpost.
                outpostData.size = OG_OutpostSize.BigOutpost;
            }
            else
            {
                // Generate a small outpost.
                outpostData.size = OG_OutpostSize.SmallOutpost;
            }*/
            // TODO: debug
            outpostData.size = OG_OutpostSize.BigOutpost;
        }

        protected void GetOutpostType(ref OG_OutpostData outpostData)
        {
            float outpostTypeSelector = Rand.Value;
            if (outpostTypeSelector < 0.25f)
            {
                outpostData.isMilitary = true;
                OG_Util.FactionOfMAndCo.RelationWith(Faction.OfColony).goodwill = OG_Util.FactionOfMAndCo.def.startingGoodwill.min;
                Faction.OfColony.RelationWith(OG_Util.FactionOfMAndCo).goodwill = OG_Util.FactionOfMAndCo.def.startingGoodwill.min;
                OG_Util.FactionOfMAndCo.RelationWith(Faction.OfColony).hostile = true;
                Faction.OfColony.RelationWith(OG_Util.FactionOfMAndCo).hostile = true;
            }
            else
            {
                outpostData.isMilitary = false;
                float goodwill = OG_Util.FactionOfMAndCo.def.startingGoodwill.RandomInRange;
                OG_Util.FactionOfMAndCo.RelationWith(Faction.OfColony).goodwill = goodwill;
                Faction.OfColony.RelationWith(OG_Util.FactionOfMAndCo).goodwill = goodwill;
                OG_Util.FactionOfMAndCo.RelationWith(Faction.OfColony).hostile = false;
                Faction.OfColony.RelationWith(OG_Util.FactionOfMAndCo).hostile = false;
            }
            Log.Message("Colony.RelationWithMAndCo goodwill/hostile = " + Faction.OfColony.RelationWith(OG_Util.FactionOfMAndCo).goodwill + "/" + Faction.OfColony.RelationWith(OG_Util.FactionOfMAndCo).hostile);
            Log.Message("MAndCo.RelationWithColony goodwill/hostile = " + OG_Util.FactionOfMAndCo.RelationWith(Faction.OfColony).goodwill + "/" + OG_Util.FactionOfMAndCo.RelationWith(Faction.OfColony).hostile);
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
