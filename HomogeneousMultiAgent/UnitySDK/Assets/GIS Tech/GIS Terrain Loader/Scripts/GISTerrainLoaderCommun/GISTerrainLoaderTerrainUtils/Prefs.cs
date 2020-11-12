/*     Unity GIS Tech 2019-2020      */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class Prefs
    {
        public int detailResolution = 2048;
        public int resolutionPerPatch = 16;
        public int baseMapResolution = 1024;
        public int heightmapResolution = 128;
        public Vector2Int terrainCount;

        public Prefs(int detailresolution, int resolutionperPatch, int basemapresolution, int heightmapresolution, Vector2Int terraincount)
        {
            detailResolution = detailresolution;
            resolutionPerPatch = resolutionperPatch;
            baseMapResolution = basemapresolution;
            heightmapResolution = heightmapresolution;
            terrainCount = terraincount;
        }
    }
}