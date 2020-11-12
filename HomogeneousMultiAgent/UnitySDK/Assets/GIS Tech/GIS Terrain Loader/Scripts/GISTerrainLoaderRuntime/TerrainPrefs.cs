/*     Unity GIS Tech 2019-2020      */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class TerrainPrefs : MonoSingleton<TerrainPrefs>
    {
        [Header("Terrain prefs Classe")]
        public TerrainElevation TerrainElevation = TerrainElevation.RealWorldElevation;
        public int detailResolution = 2048;
        public int resolutionPerPatch = 16;
        public int baseMapResolution = 1024;
        public int heightmapResolution = 128;
        public int BaseMapDistance = 2000;

        [Space(3)]
        public float TerrainExaggeration;
        [HideInInspector]
        public Vector2Int terrainCount = Vector2Int.one;
        public Vector3 terrainScale = Vector3.one;
        [HideInInspector]
        public Vector2 terrainDimensions;
        [Header("Textures prefs")]
        [Space(2)]
        public TextureMode textureMode = TextureMode.WithTexture;
        public int textureHeight = 1024;
        public int textureWidth = 1024;
        public Color textureEmptyColor = Color.white;


    }
}