/*     Unity GIS Tech 2019-2020      */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class TerrainObject : MonoBehaviour
    {
        [HideInInspector]
        public string ElevationFilePath;
        [HideInInspector]
        public TerrainPrefs prefs;
        [HideInInspector]
        public TerrainContainerObject container;
        [HideInInspector]
        public Vector3 size;
        [HideInInspector]
        public Vector2Int Number;

        private Terrain _terrain;
        [HideInInspector]
        public Terrain terrain
        {
            get { return _terrain ?? (_terrain = GetComponent<Terrain>()); }
        }

        [HideInInspector]
        public TerrainData terrainData
        {
            get
            {
                return terrain.terrainData;
            }
        }
        [HideInInspector]
        public ElevationState m_ElevationState;
        [HideInInspector]
        public TextureState m_TextureState;
    }

}