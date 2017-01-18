using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;
using RimWorld.Planet;

namespace CaveBiome
{
    public class GenStep_CaveRiver : GenStep
    {
        public const int OutOfBoundsOffset = 5;
        public const float RiverPartsGranularity = 5f;
        public const float RiverLateralFactor = 30f;
        public const float RiverWidthFactor = 10f;

        public override void Generate(Map map)
        {
            if (map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Nothing to do.
                return;
            }

            // Get river origin side.
            int riverEntrySideAsInt = Rand.RangeInclusive(2, 3);
            int riverExitSideAsInt = (riverEntrySideAsInt + 2) % 4;
            Rot4 riverEntrySide = new Rot4(riverEntrySideAsInt);
            Rot4 riverExitSide = new Rot4(riverExitSideAsInt);

            // Get river origin and end coordinates.
            Vector2 riverStart = Vector2.zero;
            Vector2 riverEnd = Vector2.zero;
            switch (riverEntrySideAsInt)
            {
                default:
                case 2: // South.
                    if ((riverEntrySideAsInt == 0)
                        || (riverEntrySideAsInt == 1))
                    {
                        Log.Warning("CaveBiome: river entry side (" + riverEntrySideAsInt + ") should not occur.");
                    }
                    riverStart = new Vector2(Rand.RangeInclusive((int)((float)(map.Size.x) * 0.25f), (int)((float)(map.Size.x) * 0.75f)), -OutOfBoundsOffset);
                    riverEnd = new Vector2(Rand.RangeInclusive((int)((float)(map.Size.x) * 0.25f), (int)((float)(map.Size.x) * 0.75f)), map.Size.z + OutOfBoundsOffset);
                    break;
                case 3: // West.
                    riverStart = new Vector2(-OutOfBoundsOffset, Rand.RangeInclusive((int)((float)(map.Size.z) * 0.25f), (int)((float)(map.Size.z) * 0.75f)));
                    riverEnd = new Vector2(map.Size.x + OutOfBoundsOffset, Rand.RangeInclusive((int)((float)(map.Size.z) * 0.25f), (int)((float)(map.Size.z) * 0.75f)));
                    break;
            }

            // Get straight river points.
            Vector2 riverVector = riverEnd - riverStart;
            int numberOfParts = (int)Math.Ceiling((double)(riverVector.magnitude / RiverPartsGranularity));
            Vector2 riverVectorNormalized = riverVector / (float)numberOfParts;
            List<Vector2> riverCoordinates = new List<Vector2>();
            Vector2 vector = riverStart;
            for (int partIndex = 0; partIndex < numberOfParts; partIndex++)
            {
                riverCoordinates.Add(vector);
                vector += riverVectorNormalized;
            }
            
            // Generate Perlin map and apply perturbations.
            Perlin perlinMap = new Perlin(0.05, 2.0, 0.5, 4, Rand.Range(0, 2147483647), QualityMode.High);
            List<Vector2> riverCoordinatesCopy = riverCoordinates.ListFullCopy<Vector2>();
            riverCoordinates.Clear();
            List<float> riverWidth = new List<float>();
            for (int coordinatesIndex = 0; coordinatesIndex < riverCoordinatesCopy.Count; coordinatesIndex++)
            {
                float perturbation = RiverLateralFactor * (float)perlinMap.GetValue((double)coordinatesIndex, 0.0, 0.0);
                Vector2 pointCoordinates = Vector2.zero;
                if (riverEntrySide == Rot4.South)
                {
                    pointCoordinates = riverCoordinatesCopy[coordinatesIndex] + perturbation * Vector2.right;
                }
                else
                {
                    pointCoordinates = riverCoordinatesCopy[coordinatesIndex] + perturbation * Vector2.down;
                }
                riverCoordinates.Add(pointCoordinates);

                float width = Mathf.Min(Mathf.Abs(RiverWidthFactor * (float)perlinMap.GetValue(0.0, 0.0, (double)coordinatesIndex)), 8f);
                riverWidth.Add(width);
            }

            // Generate river from coordinates.
            for (int coordinatesIndex = 0; coordinatesIndex < riverCoordinates.Count - 1; coordinatesIndex++)
            {
                float width = Mathf.Max(2.5f, riverWidth[coordinatesIndex]);
                Vector2 riverPart = riverCoordinates[coordinatesIndex + 1] - riverCoordinates[coordinatesIndex];
                Vector2 riverPartNormalized = riverPart / RiverPartsGranularity;
                for (int pointIndex = 0; pointIndex < RiverPartsGranularity; pointIndex++)
                {
                    IntVec3 point = new IntVec3(riverCoordinates[coordinatesIndex]) + new IntVec3(pointIndex * riverPartNormalized);
                    // Generate mud.
                    foreach (IntVec3 cell in GenRadial.RadialCellsAround(point, width + 1.9f, true))
                    {
                        if (cell.InBounds(map) == false)
                        {
                            continue;
                        }
                        Thing building = cell.GetEdifice(map);
                        if (building != null)
                        {
                            continue;
                        }
                        TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                        // Do not change stony terrains.
                        if ((terrain.defName.Contains("Rough") == false)
                            && (terrain != TerrainDefOf.WaterShallow)
                            && (terrain != TerrainDefOf.WaterDeep))
                        {
                            map.terrainGrid.SetTerrain(cell, TerrainDef.Named("Mud"));
                        }
                    }
                    // Generate shallow water and remove building/roof.
                    foreach (IntVec3 cell in GenRadial.RadialCellsAround(point, width, true))
                    {
                        if (cell.InBounds(map) == false)
                        {
                            continue;
                        }
                        Thing building = cell.GetEdifice(map);
                        if (building != null)
                        {
                            building.Destroy();
                        }
                        if (map.roofGrid.Roofed(cell))
                        {
                            map.roofGrid.SetRoof(cell, null);
                        }
                        TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                        if (terrain != TerrainDefOf.WaterDeep)
                        {
                            map.terrainGrid.SetTerrain(cell, TerrainDefOf.WaterShallow);
                        }
                    }
                    // Generate deep water.
                    if (width > 4)
                    {
                        foreach (IntVec3 cell in GenRadial.RadialCellsAround(point, width - 3.9f, true))
                        {
                            if (cell.InBounds(map) == false)
                            {
                                continue;
                            }
                            map.terrainGrid.SetTerrain(cell, TerrainDefOf.WaterDeep);
                        }
                    }
                }
            }
            IntVec3 offset = IntVec3.Zero;
            if (riverEntrySide == Rot4.South)
            {
                offset = new IntVec3(8, 0, 0);
            }
            else
            {
                offset = new IntVec3(0, 0, 8);
            }
            MapGenerator.PlayerStartSpot = new IntVec3(riverCoordinates[riverCoordinates.Count / 2]) + offset;
        }
    }
}
